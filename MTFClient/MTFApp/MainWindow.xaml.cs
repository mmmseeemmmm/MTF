using MTFApp.UIHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Markup;
using ALControls;
using MTFApp.ServerService;
using MTFApp.UIHelpers.Converters;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.SequenceLocalization;
using MessageBox = System.Windows.MessageBox;
using MTFApp.MessageBoxes;

namespace MTFApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase, INotifyPropertyChanged
    {
        MainWindowPresenter dataContext;
        public PopupWindow.SplashScreen Splash = null;
        private readonly ModifierKeys shortcutDisplaykey = ModifierKeys.Alt;
        internal static Mutex MTFMutex;
        private readonly SettingsClass setting;
        private bool languageWasChanged;
        private readonly List<Guid> localizedPresenters = new List<Guid>();
        readonly LicenseDestroyManager licenceDestroy = new LicenseDestroyManager();
        private bool serverResult;

        public MainWindow()
        {
            setting = StoreSettings.GetInstance.SettingsClass;
            InitLanguage();
            bool isAlreadyRunning = CheckMutex();
            Task.Run(() => InitMainWindow(isAlreadyRunning));
            Visibility = Visibility.Hidden;
            licenceDestroy.OnRemove += (s, a) =>
                                       {
                                           if (dataContext != null)
                                           {
                                               dataContext.AccessKey = null;
                                           }
                                       };
        }

        private void InitLanguage()
        {
            setting.OnLanguageChanged += LanguageChanged;
            LanguageHelper.Initialize("MTFApp", "MTFApp.Localizations", "UIStrings");
            SetLanguage(setting.Language);
            SequenceLocalizationHelper.Load();
        }

        private void InitMainWindow(bool isAlreadyRunning)
        {
            //MTFMutex = new Mutex(false, "a495301b-bb39-45e8-b232-3c0e754208de1");
            Application.Current.Dispatcher.Invoke(() => Splash = new PopupWindow.SplashScreen { DataContext = this });

            SourceInitialized += InitializeWindowSource;

            var canRunMoreTimes = setting.MoreClients;

            if (!canRunMoreTimes)
            {
                if (isAlreadyRunning)
                {
                    MessageBox.Show(LanguageHelper.GetString("Msg_Body_NoMoreClients"),
                        LanguageHelper.GetString("Msg_Header_Error"), MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Environment.Exit(0);
                }
            }

            if (setting.StartMTFServer)
            {
                var serverpath = setting.MTFServerPath;
                Application.Current.Dispatcher.Invoke(Splash.Show);
                SplashInfo = LanguageHelper.GetString("Mtf_StartingServer");
                try
                {
                    var pi = new System.Diagnostics.ProcessStartInfo
                    {
                        WorkingDirectory = Path.GetDirectoryName(serverpath),
                        FileName = Path.GetFileName(serverpath)
                    };

                    System.Diagnostics.Process.Start(pi);
                }
                catch (Exception)
                {
                    Application.Current.Dispatcher.Invoke(Splash.Hide);

                    MessageBox.Show(string.Format(LanguageHelper.GetString("Mtf_StartingNotStart"), serverpath, Environment.NewLine),
                        LanguageHelper.GetString("Msg_Header_Error"),
                        MessageBoxButton.OK, MessageBoxImage.Error);

                    if (!openConnectionDialog())
                    {
                        Environment.Exit(0);
                    }
                }
            }

            //if (!EnvironmentHelper.IsProductionMode)
            if (!setting.ConnectToServerAutomatically)
            {
                Application.Current.Dispatcher.Invoke(Splash.Hide);
                if (!openConnectionDialog())
                {
                    Environment.Exit(0);
                }
            }
            else
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(Splash.Show);
                    SplashInfo = LanguageHelper.GetString("Mtf_Connect");

                    SettingsClass.SelectedConnection = setting.Connections[setting.SelectedConnectionIndex];

                    Thread.Sleep(TimeSpan.FromSeconds(setting.ConnectionDelay));

                    int tries = 4;
                    for (int i = 0; i < tries; i++)
                    {
                        try
                        {
                            ServiceClientsContainer.ConnectAll();
                        }
                        catch
                        {
                            SplashInfo = string.Format("{0}{1}", LanguageHelper.GetString("Mtf_ConnectNextTry"), new string('.', i));
                            Thread.Sleep(1000);
                        }
                    }
                    if (!ServiceClientsContainer.AllIsConnected)
                    {
                        HandleConnectionError();
                    }
                }
                catch
                {
                    HandleConnectionError();
                }
            }

            InitializeComponent();
            SplashInfo = LanguageHelper.GetString("Mtf_Init_Ui");
            dataContext = new MainWindowPresenter(UpdateLanguage);
            Application.Current.Dispatcher.Invoke(() => DataContext = dataContext);
            Application.Current.Dispatcher.Invoke(() => ApplyBasicSetting(dataContext));

            SplashInfo = LanguageHelper.GetString("Mtf_Init_Access");
            dataContext.InitAccessKeyProviders();

            DownloadClientAssemblies();

            CheckDataMigration();

            //if (EnvironmentHelper.IsProductionMode)
            //{
            //    this.ResizeMode = System.Windows.ResizeMode.NoResize;
            //    this.WindowState = System.Windows.WindowState.Maximized;
            //}

            Application.Current.Dispatcher.Invoke(Splash.Close);
            Application.Current.Dispatcher.Invoke(() => Visibility = Visibility.Visible);

            SizeChanged += (s, e) => { RecalculateLandscape(); };
            Application.Current.Dispatcher.Invoke(() =>
                                                  {
                                                      switch (setting.OpenControlOnStart)
                                                      {
                                                          case Controls.ComponentsConfiguration:
                                                              dataContext.SwitchMainContent(MainWindowPresenter.MainContentEnum.ComponentConfig);
                                                              break;
                                                          case Controls.SequenceEditor:
                                                              dataContext.SwitchMainContent(MainWindowPresenter.MainContentEnum.SequenceEdtior);
                                                              break;
                                                          case Controls.SequenceExecution:
                                                              dataContext.SwitchMainContent(MainWindowPresenter.MainContentEnum.SequenceExecution);
                                                              break;
                                                          case Controls.Settings:
                                                              dataContext.SwitchMainContent(MainWindowPresenter.MainContentEnum.Settings);
                                                              break;
                                                      }
                                                  });
        }

        private void HandleConnectionError()
        {
            Application.Current.Dispatcher.Invoke(Splash.Hide);
            if (!openConnectionDialog())
            {
                Environment.Exit(0);
            }
        }

        private void UpdateLanguage(Guid presenterId)
        {
            if (languageWasChanged && !localizedPresenters.Contains(presenterId))
            {
                localizedPresenters.Add(presenterId);
                RefreshLanguage();
            }
        }

        private void LanguageChanged(object sender, LanguageChangeEventArgs e)
        {
            languageWasChanged = true;
            localizedPresenters.Clear();
            SetLanguage(e.Language);
            SequenceLocalizationHelper.Load();
        }

        private void SetLanguage(string language)
        {
            LanguageHelper.ChangeLanguage(language);
            Application.Current.Dispatcher?.Invoke(() => Language = XmlLanguage.GetLanguage(language));
            RefreshLanguage();
        }

        private void RefreshLanguage()
        {
            Application.Current.Dispatcher.Invoke(SetLocalization);
        }

        private bool CheckMutex()
        {
            bool isAlreadyRunning = false;
            MTFMutex = new Mutex(false, "a495301b-bb39-45e8-b232-3c0e754208dee");
            try
            {
                isAlreadyRunning = !MTFMutex.WaitOne(TimeSpan.Zero, true);
            }
            catch (AbandonedMutexException)
            {
                try
                {
                    MTFMutex.ReleaseMutex();
                    isAlreadyRunning = !MTFMutex.WaitOne(TimeSpan.Zero, true);
                }
                catch (Exception ex)
                {
                    SystemLog.LogException(ex);
                }
            }
            return isAlreadyRunning;
        }

        private void DownloadClientAssemblies()
        {
            var assembliesToDownload = dataContext.AssembliesToDownload();
            if (assembliesToDownload == null || !assembliesToDownload.Any())
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(Splash.Show);

            SizeToHumanReadableConverter humanReadableConverter = new SizeToHumanReadableConverter();
            foreach (var asmInfo in assembliesToDownload)
            {
                SplashInfo = string.Format("{0} {1} ({2})", LanguageHelper.GetString("Mtf_Init_Download"),
                    Path.GetFileNameWithoutExtension(asmInfo.Name), humanReadableConverter.Convert(asmInfo.Size, null, null, null));
                dataContext.DownloadClientAssembly(asmInfo);
            }
        }

        private void CheckDataMigration()
        {
            var dataMigrationInfo = MTFClient.GetMTFClient().CheckPossibleDataMigration();

            if (dataMigrationInfo.MigrationPossible)
            {
                DataMigrationType result = DataMigrationType.DoNothing;
                Dispatcher.Invoke(() =>
                {
                    SplashInfo = LanguageHelper.GetString("Mtf_Init_DataMigration");
                    MessageBoxDataMigration messageBoxDataMigration = new MessageBoxDataMigration(dataMigrationInfo.PreviousMTFVersion);
                    messageBoxDataMigration.ShowDialog();
                    result = messageBoxDataMigration.Result;
                });
                Dispatcher.Invoke(Splash.Show);

                MTFClient.GetMTFClient().DoDataMigration(result).Wait();
            }
        }

        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            licenceDestroy.Set(e.Timestamp);
            base.OnPreviewTouchDown(e);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            licenceDestroy.Set(e.Timestamp);
            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && UIHelper.FindParent<UIHelpers.Editors.MTFTermDesigner>(e.MouseDevice.DirectlyOver as DependencyObject) != null)
            {
                e.Handled = false;
                return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                dataContext.Scale = UIHelper.GetNewScale(e.Delta, dataContext.Scale);
                e.Handled = true;
            }
            else
            {
                base.OnPreviewMouseWheel(e);
            }
        }

        private bool openingKeyShortcutsInProgress = false;
        private object openingKeyShortcutsInProgressLock = new object();
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            licenceDestroy.Set(e.Timestamp);
            if (Keyboard.Modifiers == ModifierKeys.Control && (e.Key == Key.NumPad0 || e.Key == Key.D0))
            {
                dataContext.Scale = 1;
                e.Handled = true;
            }
            e.Handled = e.Handled || handleMainCommandsShortcuts(e.Key, e.SystemKey, Keyboard.Modifiers);

            //show keyboard shortcuts
            if (Keyboard.Modifiers == shortcutDisplaykey && this.IsActive)
            {
                lock (openingKeyShortcutsInProgressLock)
                {
                    if (!openingKeyShortcutsInProgress && !e.Handled)
                    {
                        openingKeyShortcutsInProgress = true;
                        Task.Run(() =>
                        {
                            Thread.Sleep(2000);
                            if (openingKeyShortcutsInProgress)
                            {
                                App.Current.Dispatcher.Invoke(() => ShortcutDisplay(true));
                            }
                            //openingKeyShortcutsInProgress = false;
                        });
                    }
                }
            }

            base.OnPreviewKeyDown(e);
        }

        void App_Deactivated(object sender, EventArgs e)
        {
            if (openingKeyShortcutsInProgress)
            {
                lock (openingKeyShortcutsInProgressLock)
                {
                    openingKeyShortcutsInProgress = false;
                    ShortcutDisplay(false);
                }
            }
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            if (openingKeyShortcutsInProgress)
            {
                lock (openingKeyShortcutsInProgressLock)
                {
                    openingKeyShortcutsInProgress = false;
                    ShortcutDisplay(false);
                }
            }
            base.OnPreviewKeyUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            KeyboardHandler.Instance.RaiseEvent(e);

            if (e.Key == Key.System && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        //private bool isShortcutShown = false;
        private void ShortcutDisplay(bool isShown)
        {
            //foreach (var command in dataContext.MainCommands != null ? dataContext.MainWindowCommands.Concat(dataContext.MainCommands) : dataContext.MainWindowCommands)
            foreach (var command in dataContext.MainWindowCommands)
            {
                command.ShowKeyShortcut = isShown;
            }

            foreach (var context in dataContext.CollectionControls)
            {
                foreach (var command in context.Commands())
                {
                    command.ShowKeyShortcut = isShown;
                }
            }
        }

        private bool handleMainCommandsShortcuts(Key key, Key systemKey, ModifierKeys modifier)
        {
            bool commandExecuted = false;
            var keyInternal = key;
            if (key == Key.System)
            {
                keyInternal = systemKey;
            }

            foreach (var command in dataContext.MainCommands != null ? dataContext.MainWindowCommands.Concat(dataContext.MainCommands) : dataContext.MainWindowCommands)
            {
                if (executeCommand(command, keyInternal, modifier))
                {
                    commandExecuted = true;
                }
            }

            return commandExecuted;
        }

        private bool executeCommand(Command command, Key key, ModifierKeys modifier)
        {
            if (command.KeyShortucuts != null && command.KeyShortucuts.Any(s => s.Key == key && s.Modifier == modifier) && command.CanExecute(null))
            {
                command.Execute(null);
                return true;
            }
            return false;
        }

        private void RecalculateLandscape()
        {
            if (dataContext != null)
            {
                dataContext.IsLandscape = Width > Height;
            }
        }

        private bool openConnectionDialog()
        {
            var ocdResult = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                Window connectionWindow = new ConnectionDialog.ConnectionDialog();
                connectionWindow.ShowDialog();
                ocdResult = connectionWindow.DialogResult == true;
            });
            return ocdResult;
        }

        private void ApplyBasicSetting(MainWindowPresenter dataContext)
        {

            WindowState = dataContext.Setting.WindowState;

            if (!double.IsNaN(dataContext.Setting.WindowLocation.X) &&
                !double.IsNaN(dataContext.Setting.WindowLocation.Y) &&
                !double.IsNaN(dataContext.Setting.WindowSize.Width) &&
                !double.IsNaN(dataContext.Setting.WindowSize.Height))
            {
                //out of visible bounds check
                if (!((dataContext.Setting.WindowLocation.X <= SystemParameters.VirtualScreenLeft - dataContext.Setting.WindowSize.Width) ||
                    (dataContext.Setting.WindowLocation.Y <= SystemParameters.VirtualScreenTop - dataContext.Setting.WindowSize.Height) ||
                    (SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth <= dataContext.Setting.WindowLocation.X) ||
                    (SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight <= dataContext.Setting.WindowLocation.Y)))
                {
                    Left = dataContext.Setting.WindowLocation.X;
                    Top = dataContext.Setting.WindowLocation.Y;
                    Width = dataContext.Setting.WindowSize.Width;
                    Height = dataContext.Setting.WindowSize.Height;
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            serverResult = false;
#if !DEBUG
            if (IsInitialized)
            {
                if (dataContext != null && dataContext.IsSequenceModified)
                {
                    System.Text.StringBuilder msg = new System.Text.StringBuilder(LanguageHelper.GetString("Msg_Body_SequenceNotSaved"));
                    if (dataContext.SequenceIsModifiedByEditor)
                    {
                        msg.Append(" ");
                        msg.Append(LanguageHelper.GetString("Msg_Body_EditorModified"));
                    }
                    else if (dataContext.SequenceIsModifiedByExecution)
                    {
                        msg.Append(" ");
                        msg.Append(LanguageHelper.GetString("Msg_Body_ExecutionModified"));
                    }

                    var result = MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Exit"),
                        msg.Append(Environment.NewLine).Append(Environment.NewLine).Append(LanguageHelper.GetString("Msg_Body_SaveSequence")).ToString(),
                        MTFMessageBoxType.Question, MTFMessageBoxButtons.YesNoCancel);

                    if (result == MTFMessageBoxResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    if (result == MTFMessageBoxResult.Yes)
                    {
                        if (dataContext.SequenceIsModifiedByEditor)
                        {
                            if (dataContext.SequenceEditorPresenter != null && dataContext.SequenceEditorPresenter.SaveAll().Result == false)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }
                        else if (dataContext.SequenceIsModifiedByExecution && dataContext.SequenceExecutionPresenter != null)
                        {
                            dataContext.SequenceExecutionPresenter.SaveSequence();
                        }
                    }
                }
                else if (dataContext != null)
                {
                    var messageInfo = new MessageInfo
                                      {
                                          Text = LanguageHelper.GetString("Msg_Body_Exit"),
                                          Type = SequenceMessageType.Question,
                                          Buttons = MessageButtons.YesNo,
                                      };
                    PopupWindow.PopupWindow pw = new PopupWindow.PopupWindow(new MessageBoxControl.MessageBoxControl(messageInfo));
                    pw.CanClose = false;
                    pw.Title = LanguageHelper.GetString("Msg_Header_Exit");
                    pw.ShowDialog();

                    if (pw.MTFDialogResult == null || pw.MTFDialogResult.Result != MTFDialogResultEnum.Yes)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
#endif
            if (dataContext != null)
            {
                ClosingRequest(e);

                if (!serverResult)
                {
                    e.Cancel = true;
                }
            }
            base.OnClosing(e);
        }


        private async void ClosingRequest(CancelEventArgs e)
        {
            var closingError = await dataContext.OnClosing();

            serverResult = true;

            if (!string.IsNullOrEmpty(closingError))
            {
                var messageInfo = new MessageInfo
                {
                    Text = closingError,
                    Type = SequenceMessageType.Error,
                    Buttons = MessageButtons.Ok,
                };
                PopupWindow.PopupWindow pw = new PopupWindow.PopupWindow(new MessageBoxControl.MessageBoxControl(messageInfo))
                {
                    CanClose = false,
                                                     Title = LanguageHelper.GetString("Msg_Header_Exit")
                };
                pw.ShowDialog();
                e.Cancel = true;
                return;
            }

            StoreSettings.GetInstance.Save();
            dataContext.AppClosing();
            dataContext = null;

            if (e.Cancel)
            {
                Close();
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            RecalculateLandscape();
        }

        private void closeButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void minimizeButtonClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void maximizeButtonClick(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && Cursor == Cursors.Arrow)
            {
                DragMove();
            }

            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                if (WindowState == WindowState.Normal)
                {
                    WindowState = WindowState.Maximized;
                    Cursor = Cursors.Arrow;
                }
                else
                {
                    WindowState = WindowState.Normal;
                }
            }
        }

        private void powerOffButtonClick(object sender, RoutedEventArgs e)
        {
            var messageInfo = new MessageInfo
                              {
                                  Text = LanguageHelper.GetString("Msg_Body_Shutdown"),
                                  Type = SequenceMessageType.ImportantQuestion,
                                  Buttons = MessageButtons.YesNo,
                              };
            PopupWindow.PopupWindow pw = new PopupWindow.PopupWindow(new MessageBoxControl.MessageBoxControl(messageInfo));
            pw.CanClose = false;
            pw.Title = LanguageHelper.GetString("Msg_Header_Shutdown");
            pw.ShowDialog();
            if (pw.MTFDialogResult.Result == MTFDialogResultEnum.Yes)
            {
                var psi = new System.Diagnostics.ProcessStartInfo("shutdown", "/s /t 0");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                System.Diagnostics.Process.Start(psi);
            }
        }

        #region Main window resizing
        private const int resizeBorder = 10;
        private const int resizeCornerBorder = 25;
        private ResizeDirection resizeMode = ResizeDirection.NoResize;
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (/*!EnvironmentHelper.IsProductionMode && */WindowState != WindowState.Maximized)
            {
                if (e.LeftButton == MouseButtonState.Pressed && resizeMode != ResizeDirection.NoResize)
                {
                    ResizeWindow(resizeMode);
                }

                resizeMode = GetBorderPosition(e.GetPosition((IInputElement)sender));
            }
        }

        private ResizeDirection GetBorderPosition(Point position)
        {
            ResizeDirection mouseBorderPosition;

            //left top corner
            if ((position.X < resizeBorder && position.Y < resizeCornerBorder) || (position.Y < resizeBorder && position.X < resizeCornerBorder))
            {
                Cursor = Cursors.SizeNWSE;
                mouseBorderPosition = ResizeDirection.TopLeft;
            }
            //right top corner
            else if ((position.X > Width - resizeBorder && position.Y < resizeCornerBorder) || (position.X > Width - resizeCornerBorder && position.Y < resizeBorder))
            {
                Cursor = Cursors.SizeNESW;
                mouseBorderPosition = ResizeDirection.TopRight;
            }
            //left bottom corner
            else if (position.X < resizeBorder && position.Y > Height - resizeCornerBorder || position.X < resizeCornerBorder && position.Y > Height - resizeBorder)
            {
                Cursor = Cursors.SizeNESW;
                mouseBorderPosition = ResizeDirection.BottomLeft;
            }
            //right bottom corner
            else if (position.X > Width - resizeCornerBorder && position.Y > Height - resizeBorder || position.X > Width - resizeBorder && position.Y > Height - resizeCornerBorder)
            {
                Cursor = Cursors.SizeNWSE;
                mouseBorderPosition = ResizeDirection.BottomRight;
            }
            //left or right column
            else if (position.X < resizeBorder)
            {
                Cursor = Cursors.SizeWE;
                mouseBorderPosition = ResizeDirection.Left;
            }
            //right column
            else if (position.X > Width - resizeBorder)
            {
                Cursor = Cursors.SizeWE;
                mouseBorderPosition = ResizeDirection.Right;
            }
            //top row
            else if (position.Y < resizeBorder)
            {
                Cursor = Cursors.SizeNS;
                mouseBorderPosition = ResizeDirection.Top;
            }
            //bottom row
            else if (position.Y > Height - resizeBorder)
            {
                Cursor = Cursors.SizeNS;
                mouseBorderPosition = ResizeDirection.Bottom;
            }
            else
            {
                Cursor = Cursors.Arrow;
                mouseBorderPosition = ResizeDirection.NoResize;
            }

            return mouseBorderPosition;
        }

        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
            NoResize = 100,
        }

        private const int WM_SYSCOMMAND = 0x112;
        private HwndSource hwndSource;

        private void InitializeWindowSource(object sender, EventArgs e)
        {
            hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            hwndSource.AddHook(new HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            return IntPtr.Zero;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        #endregion Main window resizing

        private void AlLogo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (EnvironmentHelper.IsProductionMode)
                {
                    dataContext.Login.Execute(null);
                }
                else
                {
                    dataContext.Logout.Execute(null);
                }


            }
        }

        private string splashInfo;
        public string SplashInfo
        {
            get { return splashInfo; }
            set
            {
                splashInfo = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }

        private void ShowMenuClick(object sender, MouseButtonEventArgs e)
        {
            dataContext?.ShowMenuCommand.Execute(null);
        }

        private void MainMenuLeave(object sender, MouseEventArgs e)
        {
            if (dataContext != null && dataContext.Setting.AutoHideMainMenu && dataContext.ShowMainMenu)
            {
                dataContext.ShowMenuCommand.Execute(null);
            }
        }
    }
}
