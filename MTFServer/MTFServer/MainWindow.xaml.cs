using MTFClientServerCommon;
using MTFCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Linq;
using System.IO;
using System.Windows.Markup;
using ALControls;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.SequenceLocalization;
using MTFCore.DbReporting.DbReportingWcf;
using MessageBox = System.Windows.MessageBox;

namespace MTFServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private TcpSingletonMTFCore tcpSingleton;
        private bool ServerIsRunning = false;
        private System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();
        private System.Windows.Forms.MenuItem startStopMenuItem;

        private const int MessageMaxCount = 100;
        private const string ErrorIcon = @"Resources\ErrorIcon.png";
        private const string WarningIcon = @"Resources\WarningIcon.png";
        private const string InfoIcon = @"Resources\InfoIcon.png";
        private const string QuestionIcon = @"Resources\QuestionIcon.png";

        public MainWindow(string port)
        {
            if (Environment.GetCommandLineArgs().Any(i => i.ToUpper().StartsWith("-STARTCLIENT=")))
            {
                var clientPath = Environment.GetCommandLineArgs().First(i => i.ToUpper().StartsWith("-STARTCLIENT=")).Substring(13);
                try
                {
                    System.Diagnostics.ProcessStartInfo pi = new System.Diagnostics.ProcessStartInfo();
                    pi.WorkingDirectory = Path.GetDirectoryName(clientPath);
                    pi.Arguments = port;
                    pi.FileName = Path.GetFileName(clientPath);

                    System.Diagnostics.Process.Start(pi);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Starting client {clientPath} failed. Inner exception: {e.ToString()}");
                }
            }

            Messages = new ObservableCollection<MessageObject>();

            Addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(ip => ip.ToString()).ToList<string>();
            Addresses.Add("localhost");

            DataContext = this;

            InitializeComponent();

            createNotifiIcon();
            Start();
        }

        private void createNotifiIcon()
        {
            startStopMenuItem = new System.Windows.Forms.MenuItem("Stop", (s, e) =>
            {
                if (ServerIsRunning)
                {
                    this.Stop();
                }
                else
                {
                    this.Start();
                }
            });

            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new[] {
                startStopMenuItem,
                new System.Windows.Forms.MenuItem("Settings", (s,e)=>{
                    this.Show();
                    this.Activate();
                }),
                new System.Windows.Forms.MenuItem("Exit MTF server", (s,e)=>{this.CloseMTFServerApp();})
            });

            notifyIcon.Icon = Resource.MTFIcon_32;
            notifyIcon.Visible = true;
            notifyIcon.Click += (s, e) =>
            {
                if (((System.Windows.Forms.MouseEventArgs)e).Button == System.Windows.Forms.MouseButtons.Left)
                {
                    if (this.Visibility != System.Windows.Visibility.Visible)
                    {
                        this.Show();
                        this.Activate();
                    }
                    else
                    {
                        this.Hide();
                    }
                }
                else if (((System.Windows.Forms.MouseEventArgs)e).Button == System.Windows.Forms.MouseButtons.Right)
                {
                }
            };
        }

        private void CloseMTFServerApp()
        {
            if (MessageBox.Show("Do you really want close MTF server application?",
                "AL Main Testing Framework Server", MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                CloseMTFServerAppImmediately();
            }
        }

        private void CloseMTFServerAppImmediately()
        {
            tcpSingleton?.SaveServerSettings();

            App.Current.Shutdown();
        }

        WindowState lastWindowState = WindowState.Normal;
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState == WindowState.Minimized)
            {
                WindowState = lastWindowState;
                this.Hide();
            }

            lastWindowState = WindowState;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            this.Hide();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Stop();

            notifyIcon.Dispose();
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Start()
        {
            try
            {
                SequenceLocalizationHelper.Load();
                btnStop.IsEnabled = true;
                btnStart.IsEnabled = false;
                startStopMenuItem.Text = "Stop";
                tcpSingleton = new TcpSingletonMTFCore();
                tcpSingleton.OnMessage += tcpSingleton_OnMessage;
                tcpSingleton.RequestServerStop += tcpSingleton_RequestServerStop;
                //tcpSingleton.DefaultSequenceName = this.SequenceName;
                //tcpSingleton.StartSequenceOnStart = this.StartSequenceOnServerStart;
                tcpSingleton.ServerSettingsChanged += tcpSingleton_ServerSettingsChanged;
                if (tcpSingleton.ServerSettings != null)
                {
                    this.Settings = tcpSingleton.ServerSettings;
                }
                else
                {
                    this.Settings = new ServerSettings();
                    appendMessages("Loading configuration",
                        "Configuration file not found, default configuration(" + Settings.MTFServerAddress + ":" + Settings.MTFServerPort + ") will be used.",
                        WarningIcon);
                }
                tcpSingleton.Start(Settings.MTFServerAddress, Settings.MTFServerPort);
                StringBuilder sb = new StringBuilder("Server successfully started on ");
                sb.Append(Settings.MTFServerAddress).Append(":").Append(Settings.MTFServerPort);
                appendMessages("MTF Server started", sb.ToString(), InfoIcon);
                //notifyIcon.ShowBalloonTip(1, "MTF Server", sb.ToString(), System.Windows.Forms.ToolTipIcon.Info);
                ServerIsRunning = true;

                StartReportingServer();
                InitLanguage();
            }
            catch (Exception e)
            {
                appendMessages(e);
                notifyIcon.ShowBalloonTip(1, "MTF Server", "Server isn't started", System.Windows.Forms.ToolTipIcon.Error);
            }
        }

        private void StartReportingServer()
        {
            try
            {
                TcpSingletonReporting.Instance.Start(Settings.MTFServerAddress, Settings.MTFServerPort);

                StringBuilder sb = new StringBuilder("MTF Reporting Server successfully started on ");
                sb.Append(Settings.MTFServerAddress).Append(":").Append(Settings.MTFServerReportingPort);
                appendMessages("MTF Reporting Server started", sb.ToString(), InfoIcon);
            }
            catch (Exception e)
            {
                appendMessages(e);
                notifyIcon.ShowBalloonTip(1, "MTF Reporting Server", "Reporting Server isn't started", System.Windows.Forms.ToolTipIcon.Error);
            }
        }

        private void InitLanguage()
        {
            //setting.OnLanguageChanged += LanguageChanged;
            LanguageHelper.Initialize("MTFServer", "MTFServer.Localizations", "UIStrings");
            SetLanguage(Settings.Language);
        }

        private void SetLanguage(string language)
        {
            LanguageHelper.ChangeLanguage(language);
            Application.Current.Dispatcher?.Invoke(() => Language = XmlLanguage.GetLanguage(language));
            RefreshLanguage();
        }

        private void RefreshLanguage()
        {
            Dispatcher.Invoke(SetLocalization);
        }

        void tcpSingleton_ServerSettingsChanged(ServerSettings settings)
        {
            if (settings != null && this.Settings != null && settings.Language != this.Settings.Language)
            {
                SetLanguage(settings.Language);
                SequenceLocalizationHelper.Load();
            }
            this.Settings = settings;

        }

        void tcpSingleton_RequestServerStop()
        {
            this.Dispatcher.Invoke(() => this.CloseMTFServerAppImmediately());
        }

        void tcpSingleton_OnMessage(string header, string message, int level)
        {
            string iconName;
            switch (level)
            {
                case 0: iconName = InfoIcon; break;
                case 1: iconName = WarningIcon; break;
                case 2: iconName = ErrorIcon; break;
                default: iconName = QuestionIcon; break;
            }
            appendMessagesDispatcher("MTF Server: " + header, message, iconName);
        }

        private void Stop()
        {
            try
            {
                btnStop.IsEnabled = false;
                btnStart.IsEnabled = true;
                startStopMenuItem.Text = "Start";
                tcpSingleton.Stop();

                StringBuilder sb = new StringBuilder("Server on ");
                sb.Append(tcpSingleton.Address).Append(":").Append(tcpSingleton.Port).Append(" successfully stopped.");
                tcpSingleton = null;
                appendMessages("MTF Server stopped", sb.ToString(), InfoIcon);
                notifyIcon.ShowBalloonTip(1, "MTF Server", sb.ToString(), System.Windows.Forms.ToolTipIcon.Info);

                StopReportingServer();
            }
            catch (Exception e)
            {
                appendMessages(e);
                notifyIcon.ShowBalloonTip(1, "MTF Server", "Server stopped with errors", System.Windows.Forms.ToolTipIcon.Error);
            }
            finally
            {
                ServerIsRunning = false;
            }
        }

        private void StopReportingServer()
        {
            try
            {
                TcpSingletonReporting.Instance.Stop();
                StringBuilder sb = new StringBuilder("Reporting Server on ");
                sb.Append(Settings.MTFServerAddress).Append(":").Append(Settings.MTFServerReportingPort).Append(" successfully stopped.");
                appendMessages("MTF Reporting Server stopped", sb.ToString(), InfoIcon);
            }
            catch (Exception e)
            {
                appendMessages(e);
                notifyIcon.ShowBalloonTip(1, "MTF Reporting Server", "Reporting Server stopped with errors", System.Windows.Forms.ToolTipIcon.Error);
            }

        }

        private void appendMessages(Exception e)
        {
            appendMessages(e.Message, e.ToString(), ErrorIcon);
        }

        private void appendMessagesDispatcher(string header, string text, string iconName)
        {
            try
            {
                Dispatcher.Invoke(() => { appendMessages(header, text, iconName); });
            }
            catch
            {
            }
        }

        private void appendMessages(string header, string text, string iconName)
        {
            if (Messages.Count >= MessageMaxCount)
            {
                Messages.RemoveAt(Messages.Count - 1);
            }

            Messages.Insert(0, new MessageObject
            {
                Header = header,
                Message = text,
                IconName = iconName
            });
        }

        public List<string> Addresses
        {
            get;
            set;
        }

        //public string MTFServerAddress
        //{
        //    get;
        //    set;
        //}

        //public string MTFServerPort
        //{
        //    get;
        //    set;
        //}

        //public bool StartSequenceOnServerStart
        //{
        //    get;
        //    set;
        //}

        //public string SequenceName
        //{
        //    get;
        //    set;
        //}

        public ObservableCollection<MessageObject> Messages
        {
            get;
            set;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private ServerSettings settings;

        public ServerSettings Settings
        {
            get { return settings; }
            set
            {
                settings = value;
                NotifyPropertyChanged();
            }
        }
    }

    public class MessageObject
    {
        public string Header { get; set; }
        public string Message { get; set; }
        public string IconName { get; set; }
    }
}
