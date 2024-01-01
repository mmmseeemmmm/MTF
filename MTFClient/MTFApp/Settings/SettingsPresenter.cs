using MTFApp.UIHelpers;
using MTFClientServerCommon.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using IWshRuntimeLibrary;
using MTFApp.OpenSaveSequencesDialog;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;

namespace MTFApp.Settings
{
    class SettingsPresenter : PresenterBase, IHeaderText, IMainCommands
    {
        private Command removeConnection;
        private Command openAccessKeyProviderConfigControlCommand;
        private ServerSettings serverSettings;
        private bool allowButtons = true;
        private const string DataFileParameter = "Data file";
        private bool enableAll;
        private bool canActivated = false;
        private bool startMTFClientAfterLogin;
        private readonly SettingsClass setting;

        #region Fields

        private int connectionDelay = StoreSettings.GetInstance.SettingsClass.ConnectionDelay;

        #endregion Fields

        #region Properties

        public int ConnectionDelay
        {
            get => connectionDelay;
            set
            {
                connectionDelay = value;
                NotifyPropertyChanged();
            }
        }

        #endregion Properties

        public SettingsPresenter()
        {
            setting = StoreSettings.GetInstance.SettingsClass;
            setting.OnLanguageChanged += LanguageChanged;

            startMTFClientAfterLogin = MTFShortcutExists;

            prepaireAccessKeyProversSettingses();
            ((MainWindowPresenter)Application.Current.MainWindow.DataContext).PropertyChanged += SettingsPresenter_PropertyChanged;
            LoadSettingsAsync();
            ApplicationInfo = MTFClientServerCommon.ComputerInfo.ComputerInfo.GetApplicationInfo();
            Task.Run(() =>
                     {
                         OsInfo = MTFClientServerCommon.ComputerInfo.ComputerInfo.GetOsInfo();
                         Application.Current.Dispatcher.Invoke(() => NotifyPropertyChanged("OsInfo"));

                         CpuInfo = MTFClientServerCommon.ComputerInfo.ComputerInfo.GetCpuInfo();
                         Application.Current.Dispatcher.Invoke(() => NotifyPropertyChanged("CpuInfo"));
                     });

            removeConnection = new Command(() =>
                                           {
                                               Connections.RemoveAt(SelectedConnectionIndex);
                                               if (Connections.Any())
                                               {
                                                   SelectedConnectionIndex = 0;
                                               }
                                           }, () => SelectedConnectionIndex > -1);
            openAccessKeyProviderConfigControlCommand = new Command(openAccessKeyConfigControl);
            mtfEditorIsCollapsed = setting.CollapsedParametersInEditor;
            allowDragDrop = setting.AllowDragDrop;

            DispatcherTimer timer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 3)};
            timer.Tick += cyclicUpdate;
            timer.Start();
        }

        private void LanguageChanged(object sender, EventArgs e)
        {
        }

       
        private async void LoadSettingsAsync()
        {
            EnableAll = false;
            if (serverSettings!=null)
            {
                serverSettings.PropertyChanged -= serverSettings_PropertyChanged;
            }
            await Task.Run(() => serverSettings = MTFClient.GetServerSettings());
            serverSettings.PropertyChanged += serverSettings_PropertyChanged;
            NotifyPropertyChanged("ServerSettings");
            EnableAll = true;
            canActivated = true;
        }

        void serverSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Task.Run(() => MTFClient.UpdateServerSettings(serverSettings));
        }

        void SettingsPresenter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Scale")
            {
                zoom = ((MainWindowPresenter)App.Current.MainWindow.DataContext).Scale;
                NotifyPropertyChanged("Zoom");
            }
        }

        public bool DoCyclicUpdate { get; set; }

        void cyclicUpdate(object sender, EventArgs e)
        {
            if (DoCyclicUpdate)
            {
                MTFClientServerCommon.ComputerInfo.ComputerInfo.UpdateApplicationInfo(ApplicationInfo);
            }
        }

        public MTFClientServerCommon.ComputerInfo.OSInfo OsInfo { get; set; }

        public MTFClientServerCommon.ComputerInfo.CPUInfo CpuInfo { get; set; }

        public MTFClientServerCommon.ComputerInfo.ApplicationInfo ApplicationInfo { get; set; }

        public string HeaderText => "MainMenu_Setting";

        public IEnumerable<Command> Commands()
        {
            return new Command[]
                   {
                       new Command(() => save())
                       {
                           Icon = AutomotiveLighting.MTFCommon.MTFIcons.SaveFile,
                           Name = "MainCommand_Save",
                           KeyShortucuts =
                               new CommandShortcut[] {new CommandShortcut {Key = Key.S, Modifier = ModifierKeys.Control}}
                       }
                   };
        }

        private void save()
        {
            setting.StartMTFServer = StartMTFServer;
            setting.StopMTFServer = StopMTFServer;
            setting.MTFServerPath = MTFServerPath;
            setting.ConnectToServerAutomatically = ConnectToServerAutomatically;
            setting.ConnectionDelay = this.ConnectionDelay;
            setting.SelectedConnectionIndex = SelectedConnectionIndex;

            setting.OpenControlOnStart = OpenControlOnStart;
            setting.SequenceExecutionViewType = SequenceExecutionDefaultViewType;
            setting.IsTableCollapsed = SequenceExecutionTableDefaultCollapsed;

            ((MainWindowPresenter)Application.Current.MainWindow.DataContext).Scale = zoom;
            setting.AppScale = zoom;
            setting.DialogScale = dialogZoom;
            setting.StartSequenceAfterStartDelay = StartSequenceDelay;

            setting.AllowOpenSequenceInSequenceExecution = AllowOpenSequenceInSequenceExecution;
            setting.AllowStartSequenceInSequenceExecution = AllowStartSequenceInSequenceExecution;
            setting.AllowStopSequenceInSequenceExecution = AllowStopSequenceInSequenceExecution;
            setting.AllowPauseSequenceInSequenceExecution = AllowPauseSequenceInSequenceExecution;
            setting.HideAllCommandsInSequenceExecution = HideAllCommandsInSequenceExecution;

            setting.BackupEnabled = BackupEnabled;
            setting.BackupPeriod = BackupPeriod;
            setting.DeleteBackupPeriod = DeleteBackupPeriod;
            setting.ShowTreeWithFullImage = ShowTreeWithFullImage;

            setting.AccessKeyProviderSettingses = AccessKeyProviderSettingses;
            setting.HideMainMenu = HideMainMenu;
            setting.AutoHideMainMenu = AutoHideMainMenu;
            setting.MoreClients = MoreClients;
            setting.Language = Language;
            setting.Connections = Connections.ToList();

            StoreSettings.GetInstance.Save();

            //server settings
            serverSettings.Language = Language;
            MTFClient.UpdateServerSettings(serverSettings);
            try
            {
                MTFClient.SaveServerSettings();
            }
            catch (Exception ex)
            {
                MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), ex.Message, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
            }
        }

        private bool startMTFServer = StoreSettings.GetInstance.SettingsClass.StartMTFServer;

        public bool StartMTFServer
        {
            get => startMTFServer;
            set
            {
                startMTFServer = value;
                NotifyPropertyChanged();
            }
        }

        private bool stopMTFServer = StoreSettings.GetInstance.SettingsClass.StopMTFServer;

        public bool StopMTFServer
        {
            get => stopMTFServer;
            set
            {
                stopMTFServer = value;
                NotifyPropertyChanged();
            }
        }

        private string mtfServerPath = StoreSettings.GetInstance.SettingsClass.MTFServerPath;

        public string MTFServerPath
        {
            get => mtfServerPath;
            set
            {
                mtfServerPath = value;
                NotifyPropertyChanged();
            }
        }

        private bool moreClients = StoreSettings.GetInstance.SettingsClass.MoreClients;

        public bool MoreClients
        {
            get => moreClients;
            set
            {
                moreClients = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<MTFApp.ConnectionDialog.ConnectionContainer> connections =
            new ObservableCollection<ConnectionDialog.ConnectionContainer>(StoreSettings.GetInstance.SettingsClass.Connections);

        public ObservableCollection<MTFApp.ConnectionDialog.ConnectionContainer> Connections => connections;

        private int selectedConnectionIndex = StoreSettings.GetInstance.SettingsClass.SelectedConnectionIndex;

        public int SelectedConnectionIndex
        {
            get => selectedConnectionIndex;
            set
            {
                selectedConnectionIndex = value;
                NotifyPropertyChanged();
                removeConnection.RaiseCanExecuteChanged();
            }
        }

        public Command AddConnection
        {
            get
            {
                return new Command(() =>
                                   {
                                       Connections.Add(new ConnectionDialog.ConnectionContainer
                                                       {
                                                           Alias =
                                                               LanguageHelper.GetString(
                                                                   "Setting_Connection_NewAlias")
                                                       });
                                       SelectedConnectionIndex = connections.Count - 1;
                                   });
            }
        }

        public Command RemoveConnection => removeConnection;

        private bool connectToServerAutomatically = StoreSettings.GetInstance.SettingsClass.ConnectToServerAutomatically;

        public bool ConnectToServerAutomatically
        {
            get => connectToServerAutomatically;
            set
            {
                connectToServerAutomatically = value;
                NotifyPropertyChanged();
            }
        }

        public Command BrowseMTFServer => new Command(browseMTFServer);

        private void browseMTFServer()
        {
            string serverPath = (!string.IsNullOrEmpty(MTFServerPath) &&
                                 System.IO.Directory.Exists(MTFServerPath.Remove(MTFServerPath.LastIndexOf(System.IO.Path.DirectorySeparatorChar))))
                ? MTFServerPath
                : AppDomain.CurrentDomain.BaseDirectory;

            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog
                                                 {
                                                     Filter = "MTF Server|MTFServer.exe",
                                                     InitialDirectory = serverPath,
                                                 };

            ((MainWindowPresenter)App.Current.MainWindow.DataContext).IsDarken = true;
            if (ofd.ShowDialog() == true)
            {
                MTFServerPath = ofd.FileName;
            }
            ((MainWindowPresenter)App.Current.MainWindow.DataContext).IsDarken = false;
        }

        public Command BrowseSequence => new Command(browseSeqeunce);

        private void browseSeqeunce()
        {
            var openDialog = new OpenSaveSequenceDialogContainer(DialogTypeEnum.OpenDialog,
                BaseConstants.SequenceBasePath, new List<string>() { BaseConstants.SequenceExtension }, true, false);
            PopupWindow.PopupWindow pw = new PopupWindow.PopupWindow(openDialog);
            pw.Title = LanguageHelper.GetString("OpenDialog_OpenSeq");
            if (pw.ShowDialog() == true)
            {
                if (serverSettings != null)
                {
                    serverSettings.SequenceName = ((OpenSaveSequencesDialogPresenter)openDialog.DataContext).SelectedItem.FullName;
                }
            }
        }

        public IEnumerable<EnumValueDescription> Controls => EnumHelper.GetAllValuesAndDescriptions<Controls>().ToList();

        private Controls openControlOnStart = StoreSettings.GetInstance.SettingsClass.OpenControlOnStart;

        public Controls OpenControlOnStart
        {
            get => openControlOnStart;
            set
            {
                openControlOnStart = value;
                NotifyPropertyChanged();
            }
        }

        private bool hideMainMenu = StoreSettings.GetInstance.SettingsClass.HideMainMenu;

        public bool HideMainMenu
        {
            get => hideMainMenu;
            set
            {
                hideMainMenu = value;
                NotifyPropertyChanged();
            }
        }

        private bool autoHideMainMenu = StoreSettings.GetInstance.SettingsClass.AutoHideMainMenu;

        public bool AutoHideMainMenu
        {
            get => autoHideMainMenu;
            set
            {
                autoHideMainMenu = value;
                NotifyPropertyChanged();
            }
        }

        private string language = StoreSettings.GetInstance.SettingsClass.Language;
        public string Language
        {
            get => language;
            set
            {
                language = value;
                NotifyPropertyChanged();
            }
        }

        public IEnumerable<EnumValueDescription> SequenceExecutionViewTypes => GenerateViewTypes();

        public IEnumerable<EnumValueDescription> ErrorMessageTypes => EnumHelper.GetAllValuesAndDescriptions<ErrorMessageType>().ToList();

        private IEnumerable<EnumValueDescription> GenerateViewTypes()
        {
            var views = EnumHelper.GetAllValuesAndDescriptions(new [] {SequenceExecutionViewType.TimeView, }).ToList();
#if !DEBUG
            if (!EnvironmentHelper.HasAccessKeyRole("service"))
            {
                var serviceView = views.FirstOrDefault(x => (SequenceExecutionViewType)x.Value == SequenceExecutionViewType.Service);
                views.Remove(serviceView);
            }
#endif
            return views;
        }

        private SequenceExecutionViewType sequenceExecutionDefaultViewType = StoreSettings.GetInstance.SettingsClass.SequenceExecutionViewType;

        public SequenceExecutionViewType SequenceExecutionDefaultViewType
        {
            get => sequenceExecutionDefaultViewType;
            set
            {
                sequenceExecutionDefaultViewType = value;
                NotifyPropertyChanged();
            }
        }

        private bool sequenceExecutionTableDefaultCollapsed = StoreSettings.GetInstance.SettingsClass.IsTableCollapsed;

        public bool SequenceExecutionTableDefaultCollapsed
        {
            get => sequenceExecutionTableDefaultCollapsed;
            set
            {
                sequenceExecutionTableDefaultCollapsed = value;
                NotifyPropertyChanged();
            }
        }

        public ServerSettings ServerSettings => serverSettings;


        private int startSequenceDelay = StoreSettings.GetInstance.SettingsClass.StartSequenceAfterStartDelay;

        public int StartSequenceDelay
        {
            get => startSequenceDelay;
            set
            {
                startSequenceDelay = value;
                NotifyPropertyChanged();
            }
        }


        private double zoom = StoreSettings.GetInstance.SettingsClass.AppScale;

        public int Zoom
        {
            get => (int)Math.Round(zoom * 100);
            set
            {
                zoom = (double)value / 100;
                NotifyPropertyChanged();
            }
        }

        private double dialogZoom = StoreSettings.GetInstance.SettingsClass.DialogScale;

        public int DialogZoom
        {
            get => (int)Math.Round(dialogZoom * 100);
            set
            {
                dialogZoom = (double)value / 100;
                NotifyPropertyChanged();
            }
        }

        private bool allowOpenSequenceInSequenceExecution = StoreSettings.GetInstance.SettingsClass.AllowOpenSequenceInSequenceExecution;

        public bool AllowOpenSequenceInSequenceExecution
        {
            get => allowOpenSequenceInSequenceExecution;
            set => allowOpenSequenceInSequenceExecution = value;
        }

        private bool allowStartSequenceInSequenceExecution = StoreSettings.GetInstance.SettingsClass.AllowStartSequenceInSequenceExecution;

        public bool AllowStartSequenceInSequenceExecution
        {
            get => allowStartSequenceInSequenceExecution;
            set => allowStartSequenceInSequenceExecution = value;
        }

        private bool allowStopSequenceInSequenceExecution = StoreSettings.GetInstance.SettingsClass.AllowStopSequenceInSequenceExecution;

        public bool AllowStopSequenceInSequenceExecution
        {
            get => allowStopSequenceInSequenceExecution;
            set => allowStopSequenceInSequenceExecution = value;
        }

        private bool allowPauseSequenceInSequenceExecution = StoreSettings.GetInstance.SettingsClass.AllowPauseSequenceInSequenceExecution;

        public bool AllowPauseSequenceInSequenceExecution
        {
            get => allowPauseSequenceInSequenceExecution;
            set => allowPauseSequenceInSequenceExecution = value;
        }

        private bool hideAllCommandsInSequenceExecution = StoreSettings.GetInstance.SettingsClass.HideAllCommandsInSequenceExecution;

        public bool HideAllCommandsInSequenceExecution
        {
            get => hideAllCommandsInSequenceExecution;
            set => hideAllCommandsInSequenceExecution = value;
        }

        private bool backupEnabled = StoreSettings.GetInstance.SettingsClass.BackupEnabled;

        public bool BackupEnabled
        {
            get => backupEnabled;
            set => backupEnabled = value;
        }

        private int backupPeriod = StoreSettings.GetInstance.SettingsClass.BackupPeriod;

        public int BackupPeriod
        {
            get => backupPeriod;
            set => backupPeriod = value;
        }

        private int deleteBackupPeriod = StoreSettings.GetInstance.SettingsClass.DeleteBackupPeriod;

        public int DeleteBackupPeriod
        {
            get => deleteBackupPeriod;
            set => deleteBackupPeriod = value;
        }

        private bool showTreeWithFullImage = StoreSettings.GetInstance.SettingsClass.ShowTreeWithFullImage;

        public bool ShowTreeWithFullImage
        {
            get => showTreeWithFullImage;
            set => showTreeWithFullImage = value;
        }

        private void prepaireAccessKeyProversSettingses()
        {
            AccessKeyProviderSettingses = setting.AccessKeyProviderSettingses.Select(ps => ps.Clone() as AccessKeyProviderSettings).ToList();
        }

        public List<AccessKeyProviderSettings> AccessKeyProviderSettingses { get; set; }

        private void openAccessKeyConfigControl(object param)
        {
            var mainWindowPresenter = App.Current.MainWindow.DataContext as MainWindowPresenter;
            if (mainWindowPresenter == null)
            {
                return;
            }

            var accessKeyProviderSettings = param as AccessKeyProviderSettings;
            if (accessKeyProviderSettings == null)
            {
                return;
            }

            var accessKeyProvider =  mainWindowPresenter.AccessKeyProviders.FirstOrDefault(ap => ap.Name == accessKeyProviderSettings.Name && ap.GetType().FullName == accessKeyProviderSettings.TypeName);
            if (accessKeyProvider == null)
            {
                return;
            }

            accessKeyProvider.Parameters = accessKeyProviderSettings.Parameters == null ? 
                null : accessKeyProviderSettings.Parameters.Select(p => new MTFClientServerCommon.MTFAccessControl.AccessKeyProviderParameter { Name = p.Name, Value = p.Value }).ToList();
            
            var filePath = accessKeyProvider.Parameters.First(x => x.Name == DataFileParameter).Value;
            if (!string.IsNullOrEmpty(filePath))
            {
                if (System.IO.File.Exists(filePath))
                {
                    accessKeyProvider.OpenConfigControl = true;
                    PopupWindow.PopupWindow pw = new PopupWindow.PopupWindow(accessKeyProvider.ConfigControl, true);
                    pw.ShowDialog();
                }
                else 
                {
                    if (MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Question_CreateNew"),
                        string.Format(LanguageHelper.GetString("Msg_Body_DataFileDoesntExist"),filePath),
                        MTFMessageBoxType.Question, MTFMessageBoxButtons.YesNo) == MTFMessageBoxResult.Yes)
                    {
                        accessKeyProvider.OpenConfigControl = true;
                        PopupWindow.PopupWindow pw = new PopupWindow.PopupWindow(accessKeyProvider.ConfigControl, true);
                        pw.ShowDialog();
                    }
                }
            }
            else 
            {
                MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"),
                    LanguageHelper.GetString("Setting_KeyProviders_MsgNoDataFile"), MTFMessageBoxType.Error,
                    MTFMessageBoxButtons.Ok);
            }
        }

        public ICommand OpenAccesKeyConfigControlCommand => openAccessKeyProviderConfigControlCommand;

        public ICommand ReconnectCommand => new Command(Reconnect);

        private void Reconnect(object param)
        {
            var mainWindowPresenter = App.Current.MainWindow.DataContext as MainWindowPresenter;
            if (mainWindowPresenter == null)
            {
                return;
            }
            var providerSettings = param as AccessKeyProviderSettings;
            if (providerSettings!=null)
            {
                var accessKeyProvider = mainWindowPresenter.AccessKeyProviders.FirstOrDefault(ap => ap.Name == providerSettings.Name && ap.GetType().FullName == providerSettings.TypeName);
                if (accessKeyProvider != null)
                {
                    accessKeyProvider.Reconnect();
                }
            }
        }


        private static bool mtfEditorIsCollapsed;
        public static bool MtfEditorIsCollapsed
        {
            get => mtfEditorIsCollapsed;
            set
            {
                mtfEditorIsCollapsed = value;
                StoreSettings.GetInstance.SettingsClass.CollapsedParametersInEditor = value;
            }
        }

        private static bool allowDragDrop;
        public static bool AllowDragDrop
        {
            get => allowDragDrop;
            set
            {
                allowDragDrop = value;
                StoreSettings.GetInstance.SettingsClass.AllowDragDrop = value;
            }
        }

        public bool EnableAll
        {
            get => enableAll;
            set
            {
                enableAll = value;
                NotifyPropertyChanged();
            }
        }

        public void Activated()
        {
            if (canActivated)
            {
                LoadSettingsAsync(); 
            }
        }


        public bool StartMTFClientAfterLogin
        {
            get => startMTFClientAfterLogin;
            set
            {
                startMTFClientAfterLogin = value;
                if (startMTFClientAfterLogin)
                {
                    CreateMTFShortcut();
                }
                else
                {
                    DeleteMTFShortcut();
                }
                NotifyPropertyChanged();
            }
        }

        private void CreateMTFShortcut()
        {
            WshShell wsh = new WshShell();
            IWshShortcut shortcut = wsh.CreateShortcut(MTFShortcutPath) as IWshShortcut;
            shortcut.Arguments = string.Empty;
            shortcut.TargetPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            shortcut.WindowStyle = 1;
            shortcut.Description = App.Current.MainWindow.Title;
            shortcut.WorkingDirectory = Environment.CurrentDirectory + @"\";
            shortcut.Save();
        }

        private string MTFShortcutPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "MTFApp.lnk");

        private bool MTFShortcutExists => System.IO.File.Exists(MTFShortcutPath);

        private void DeleteMTFShortcut()
        {
            if (MTFShortcutExists)
            {
                System.IO.File.Delete(MTFShortcutPath);
            }
        }
    }
}