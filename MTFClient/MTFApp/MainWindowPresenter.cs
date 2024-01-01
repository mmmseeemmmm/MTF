using MTFApp.SequenceExecution;
using MTFApp.ComponentConfig;
using MTFApp.SequenceEditor;
using MTFApp.UIHelpers;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using MTFClientServerCommon.MTFAccessControl;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using MTFApp.AccessKeyDialog;
using MTFApp.Managers.Components;
using MTFApp.ReportViewer;
using MTFApp.ServerService;
using MTFApp.ServerService.Components;
using MTFApp.Settings;
using MTFApp.WarningsViewer;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.MTFAccessControl.AnetAccessKey;
using MTFClientServerCommon.MTFAccessControl.USBAccessKey;

namespace MTFApp
{
    class MainWindowPresenter : PresenterBase
    {
        private readonly Action<Guid> updateLanguageAction;
        //private MTFCore.Core mmCore;
        public enum MainContentEnum { ComponentConfig, SequenceEdtior, SequenceExecution, ResultViewer, EditorTest, Settings, Warnings };
        private MTFUserControl componentConfigControl;
        private MTFUserControl sequenceEditorControl;
        private MTFUserControl sequenceExecutionControl;
        private MTFUserControl reportViewerControl;
        private MTFUserControl editorTestControl;
        private MTFUserControl settingsControl;
        private MTFUserControl warningsControl;

        private List<MTFUserControl> openedControls = new List<MTFUserControl>();
        private Control mainContent;
        SettingsClass setting;
        private Command loginCommand;
        private Command logoutCommand;
        private Command openAccessKeyInfoCommand;
        private double scale;
        private bool isLandscape;
        private IList<AccessKeyProvider> accessKeyProviders;
        public List<IMainCommands> CollectionControls = new List<IMainCommands>();
        private bool showMainMenu;
        private bool autoHideMainMenu;

        private Command openComponetConfigurationCommand;
        private Command openSequenceEditorCommand;
        private Command openSequenceExecutionCommand;
        private Command openResultViewerCommand;
        private Command openEditorCommand;
        private Command openSettingsCommand;
        private Command openWarningsCommand;
        private Command jumpToActivityCommand;

        private readonly ComponentsClient componentsClient;

        public MainWindowPresenter(Action<Guid> updateLanguageAction)
        {
            componentsClient = ServiceClientsContainer.Get<ComponentsClient>();
            this.updateLanguageAction = updateLanguageAction;
            InitCommands();

#if !DEBUG
            EnvironmentHelper.IsProductionMode = true;
#endif
            Warning.Instance.PropertyChanged += (sender, args) => { NotifyPropertyChanged("Warning"); NotifyPropertyChanged("WarningsVisibility"); };

            loginCommand = new Command(login, () => true);
            logoutCommand = new Command(logout, () => true);
            openAccessKeyInfoCommand = new Command(openAccessKeyInfo);

            setting = StoreSettings.GetInstance.SettingsClass;
            setting.OnLanguageChanged += LanguageChanged;
            scale = setting.AppScale;

            MTFClient.SequenceExecutionStateChanged += MTFClient_SequenceExecutionStateChanged;

            //if (EnvironmentHelper.IsProductionMode)
            //{
            //    OpenSequenceExecution.Execute(null);
            //}
#if DEBUG
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += timer_Tick;
            timer.Start();
#endif
        }

        private void LanguageChanged(object sender, LanguageChangeEventArgs e)
        {
            NotifyPropertyChanged("Validity");
        }

        public void InitAccessKeyProviders()
        {
            accessKeyProviders = new List<AccessKeyProvider>
            {
                new USBAccessKeyProvider {IsActive = true, Name = "USB Access Key"},
                new AnetAccessKeyProvider {IsActive = false, Name = "ANET Access Key"},
            };

            if (StoreSettings.GetInstance.SettingsClass.AccessKeyProviderSettingses == null)
            {
                StoreSettings.GetInstance.SettingsClass.AccessKeyProviderSettingses = new List<AccessKeyProviderSettings>();
            }

            foreach (var accessKeyProvider in accessKeyProviders)
            {
                var accessKeySettings = StoreSettings.GetInstance.SettingsClass.AccessKeyProviderSettingses.FirstOrDefault(kp => kp.Name == accessKeyProvider.Name && kp.TypeName == accessKeyProvider.GetType().FullName);
                if (accessKeySettings == null)
                {
                    accessKeySettings = new AccessKeyProviderSettings
                    {
                        IsActive = accessKeyProvider is USBAccessKeyProvider,
                        Name = accessKeyProvider.Name,
                        TypeName = accessKeyProvider.GetType().FullName,
                        Parameters = accessKeyProvider.Parameters == null ? null : accessKeyProvider.Parameters.Select(p => new AccessKeyProviderParameter { Name = p.Name, Value = p.Value }).ToList(),
                    };
                    StoreSettings.GetInstance.SettingsClass.AccessKeyProviderSettingses.Add(accessKeySettings);
                }
                accessKeySettings.HasConfigControl = accessKeyProvider.HasConfigControl;
                accessKeyProvider.IsActive = accessKeySettings.IsActive;
                accessKeyProvider.Parameters = accessKeySettings.Parameters == null ? null : accessKeySettings.Parameters.Select(p => new MTFClientServerCommon.MTFAccessControl.AccessKeyProviderParameter { Name = p.Name, Value = p.Value }).ToList();
                accessKeyProvider.ShowMessage = (header, message, questionTypeMessage) =>
                    MTFMessageBox.Show(header, message, questionTypeMessage ? MTFMessageBoxType.Question : MTFMessageBoxType.Error,
                    questionTypeMessage ? MTFMessageBoxButtons.YesNo : MTFMessageBoxButtons.Ok, null, false) == MTFMessageBoxResult.Yes;
                accessKeyProvider.OpenConfigControl = false;

                if (accessKeyProvider.IsActive)
                {
                    accessKeyProvider.NewAccessKey += AccessKeyProviderOnNewAccessKey;
                    accessKeyProvider.OnError += AccessKeyProviderOnOnError;
                    try
                    {
                        accessKeyProvider.Init();
                    }
                    catch (Exception e)
                    {
                        //System.Windows.Window splash = null;
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            System.Windows.MessageBox.Show(((MainWindow)System.Windows.Application.Current.MainWindow).Splash,
                                string.Format(LanguageHelper.GetString("Msg_Body_AccessInitCrash"), accessKeyProvider.Name, e.Message, Environment.NewLine),
                                LanguageHelper.GetString("Msg_Header_AccessInitCrash"));
                        });
                    }
                }
            }
        }

        public IList<AccessKeyProvider> AccessKeyProviders
        {
            get { return accessKeyProviders; }
        }

        private void AccessKeyProviderOnOnError(object sender, string message)
        {
            //MTFMessageBox.Show(((AccessKeyProvider)sender).Name, message, MTFMessageBoxType.Info, MTFMessageBoxButtons.Ok);
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(((MainWindow)Application.Current.MainWindow).Splash, message, ((AccessKeyProvider)sender).Name);
            });
        }

        private void AccessKeyProviderOnNewAccessKey(object sender, AccessKey accessKey)
        {
            AccessKey = accessKey;
        }

        private IEnumerable<Command> mainWindowCommands;
        private void InitCommands()
        {
            openComponetConfigurationCommand = new Command(() => SwitchMainContent(MainContentEnum.ComponentConfig), () => !EnvironmentHelper.IsProductionMode && !this.IsRunningSequence) { KeyShortucuts = new[] { new CommandShortcut { Key = Key.F1, Modifier = ModifierKeys.Alt } } };
            openSequenceEditorCommand = new Command(() => SwitchMainContent(MainContentEnum.SequenceEdtior), () => !EnvironmentHelper.IsProductionMode && !this.IsRunningSequence) { KeyShortucuts = new[] { new CommandShortcut { Key = Key.F2, Modifier = ModifierKeys.Alt } } };
            openSequenceExecutionCommand = new Command(() => SwitchMainContent(MainContentEnum.SequenceExecution)) { KeyShortucuts = new[] { new CommandShortcut { Key = Key.F3, Modifier = ModifierKeys.Alt } } };
            openResultViewerCommand = new Command(() => SwitchMainContent(MainContentEnum.ResultViewer), () => !EnvironmentHelper.IsProductionMode);
            openEditorCommand = new Command(() => SwitchMainContent(MainContentEnum.EditorTest));
            openSettingsCommand = new Command(() => SwitchMainContent(MainContentEnum.Settings), () => !EnvironmentHelper.IsProductionMode) { KeyShortucuts = new[] { new CommandShortcut { Key = Key.F4, Modifier = ModifierKeys.Alt } } };
            openWarningsCommand = new Command(() => SwitchMainContent(MainContentEnum.Warnings));

            mainWindowCommands = new[]
            {
                openComponetConfigurationCommand, openSequenceEditorCommand, openSequenceExecutionCommand,
                openResultViewerCommand, openEditorCommand, openSettingsCommand, openWarningsCommand
            };

            jumpToActivityCommand = new Command((param) =>
                                            {
                                                if (!EnvironmentHelper.IsProductionMode)
                                                {
                                                    SwitchMainContent(MainContentEnum.SequenceEdtior);
                                                    var activity = param as MTFSequenceActivity;
                                                    if (SequenceEditorPresenter != null)
                                                    {
                                                        if (activity != null)
                                                        {
                                                            SequenceEditorPresenter.NavigateToActivity(activity, true);
                                                        }
                                                        else
                                                        {
                                                            SequenceEditorPresenter.SelectActivityById(param as List<Guid>, true);
                                                        }
                                                    }
                                                }
                                            }, () => !this.IsRunningSequence);
        }

        public IEnumerable<Command> MainWindowCommands => mainWindowCommands;

        public void DownloadClientAssembly(ClientAssemblyInfo assemblyInfo)
        {
            var asmStream = componentsClient.DownloadClientAssembly(assemblyInfo);
            var fullPath = Path.Combine(BaseConstants.ClientControlAssemblyClientCachePath, assemblyInfo.Path);
            FileHelper.CreateDirectory(fullPath);
        
            using (var fs = File.Create(Path.Combine(fullPath, assemblyInfo.Name)))
            {
                asmStream.CopyTo(fs);
            }

            FileHelper.SetFileForEveryone(Path.Combine(fullPath, assemblyInfo.Name));

            asmStream.Dispose();
        }

        public IEnumerable<ClientAssemblyInfo> AssembliesToDownload()
        {
            var serverAssemblies = componentsClient.GetClientAssemblies();
            return serverAssemblies.Where(a => !File.Exists(ClientUICacheName(a)) || new FileInfo(ClientUICacheName(a)).Length != a.Size);
        }

        private string ClientUICacheName(ClientAssemblyInfo assemblyInfo) => Path.Combine(BaseConstants.ClientControlAssemblyClientCachePath, assemblyInfo.Path, assemblyInfo.Name);

        private void timer_Tick(object sender, EventArgs e)
        {
            NotifyPropertyChanged("AssemblyVersion");
        }

        private void openAccessKeyInfo()
        {
            IsDarken = true;
            PopupWindow.PopupWindow pw = null;
            App.Current.Dispatcher.Invoke(() =>
            {
                pw = new PopupWindow.PopupWindow(
                    new AccessKeyInfoControl { DataContext = AccessKey }
                    );
                pw.CanClose = true;
                pw.Title = LanguageHelper.GetString("Access_Header");
                pw.ShowDialog();
            });
            IsDarken = false;
        }

        private AccessKey accessKey;
        public AccessKey AccessKey
        {
            get { return accessKey; }
            set
            {
                accessKey = value;

                Task.Run(() => { AccessKeyChanged(); });

                NotifyPropertyChanged("UserFirstName");
                NotifyPropertyChanged("UserLastName");
                NotifyPropertyChanged("Validity");
                NotifyPropertyChanged("AccessKey");

                if (AccessKey != null && !AccessKey.IsValid)
                {
                    if (!AccessKey.IsDateValid)
                    {
                        MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_AccessError"),
                            LanguageHelper.GetString("Msg_Body_AccessExpired"), MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                    }
                    if (!AccessKey.IsMachineValid)
                    {
                        MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_AccessError"),
                            LanguageHelper.GetString("Msg_Body_AccessValid"), MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                    }
                    if (!AccessKey.IsUsbIdValid)
                    {
                        MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_AccessError"),
                            LanguageHelper.GetString("Msg_Body_AccessAnotherUsb"), MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                    }
                }
                else if (AccessKey != null)
                {
                    double daysLeft = Math.Truncate((AccessKey.Expiration - DateTime.Now).TotalDays) + 1;
                    if (daysLeft < 14)
                    {
                        MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_AccessError"),
                            string.Format(LanguageHelper.GetString("Msg_Body_AccessDays"), daysLeft), MTFMessageBoxType.Warning, MTFMessageBoxButtons.Ok);
                    }
                }

                MTFClient.LogedUserChanged(accessKey == null ? null : accessKey.KeyOwnerFirstName + " " + accessKey.KeyOwnerLastName);
            }
        }

        private void AccessKeyChanged()
        {
#if !DEBUG
            setProductionMode(accessKey == null || !accessKey.IsValid);
#endif
            EnvironmentHelper.CurrentAccessKey = accessKey;

            foreach (var control in openedControls)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (control.DataContext is IAccess)
                    {
                        ((IAccess)control.DataContext).AccesskeyChanged(accessKey);
                    }
                });
            }

            NotifyPropertyChanged("TestVisibility");

        }

        public string UserFirstName
        {
            get
            {
                if (accessKey == null)
                {
                    return string.Empty;
                }
                return accessKey.KeyOwnerFirstName;
            }
        }

        public string UserLastName
        {
            get
            {
                if (accessKey == null || string.IsNullOrEmpty(accessKey.KeyOwnerLastName))
                {
                    return string.Empty;
                }
                return accessKey.KeyOwnerLastName.ToUpper();
            }
        }

        public string Validity
        {
            get
            {
                if (accessKey == null)
                {
                    return null;
                }
                if (!accessKey.IsMachineValid)
                {
                    return LanguageHelper.GetString("Access_WronMachine");
                }
                else if (!accessKey.IsUsbIdValid)
                {
                    return LanguageHelper.GetString("Access_Corrupted");
                }
                else if (!accessKey.IsDateValid)
                {
                    return LanguageHelper.GetString("Access_Expired");
                }

                return string.Format("{1} {0}", accessKey.Expiration.ToString("dd-MM-yyyy"), LanguageHelper.GetString("Access_ValidTo"));
            }
        }

        private bool isDarken;
        public bool IsDarken
        {
            get { return isDarken; }
            set { isDarken = value; NotifyPropertyChanged(); }
        }

        public bool IsLandscape
        {
            get { return isLandscape; }
            set
            {
                isLandscape = value;
                NotifyPropertyChanged();
            }
        }

        public SettingsClass Setting
        {
            get { return setting; }
        }

        public ICommand OpenComponentConfiguration
        {
            get { return openComponetConfigurationCommand; }
        }

        public ICommand OpenSequenceEditor
        {
            get { return openSequenceEditorCommand; }
        }

        public ICommand OpenSequenceExecution
        {
            get { return openSequenceExecutionCommand; }
        }

        public ICommand OpenResultViewer
        {
            get { return openResultViewerCommand; }
        }

        public ICommand OpenEditorTest
        {
            get { return openEditorCommand; }
        }

        public ICommand OpenSettings
        {
            get { return openSettingsCommand; }
        }

        public ICommand OpenWarnings
        {
            get { return openWarningsCommand; }
        }

        public ICommand Login
        {
            get { return loginCommand; }
        }

        public ICommand Logout
        {
            get { return logoutCommand; }
        }

        public ICommand OpenAccessKeyInfoCommand
        {
            get { return openAccessKeyInfoCommand; }
        }

        public ICommand CreateLicenseRequestCommand
        {
            get { return new Command(CreateLicenseRequest); }
        }

        private void CreateLicenseRequest()
        {
            IsDarken = true;
            PopupWindow.PopupWindow pw;
            Application.Current.Dispatcher.Invoke(() =>
            {
                pw = new PopupWindow.PopupWindow(new AccessKeyRequest())
                     {
                         CanClose = true,
                         Title = LanguageHelper.GetString("Access_Request_Header")
                     };
                pw.ShowDialog();
            });
            IsDarken = false;
        }

        private bool isInfoActive;
        public bool IsInfoActive
        {
            get { return isInfoActive; }
            set { isInfoActive = value; NotifyPropertyChanged(); }
        }

        private bool isSequenceEditorActive;
        public bool IsSequenceEditorActive
        {
            get { return isSequenceEditorActive; }
            set { isSequenceEditorActive = value; NotifyPropertyChanged(); }
        }

        private bool isComponentConfigActive;
        public bool IsComponentConfigActive
        {
            get { return isComponentConfigActive; }
            set { isComponentConfigActive = value; NotifyPropertyChanged(); }
        }

        private bool isSequenceExecutionActive;
        public bool IsSequenceExecutionActive
        {
            get { return isSequenceExecutionActive; }
            set { isSequenceExecutionActive = value; NotifyPropertyChanged(); }
        }

        private bool isResultViewerActive;
        public bool IsResultViewerActive
        {
            get { return isResultViewerActive; }
            set { isResultViewerActive = value; NotifyPropertyChanged(); }
        }

        private bool isWarningsViewerActive;
        public bool IsWarningsViewerActive
        {
            get { return isWarningsViewerActive; }
            set { isWarningsViewerActive = value; NotifyPropertyChanged(); }
        }

        private bool isTestingActive;
        public bool IsTestingActive
        {
            get { return isTestingActive; }
            set { isTestingActive = value; NotifyPropertyChanged(); }
        }

        private bool isSettingsActive;
        public bool IsSettingsActive
        {
            get { return isSettingsActive; }
            set { isSettingsActive = value; NotifyPropertyChanged(); }
        }

        private void setProductionMode(bool isProductionMode)
        {
            EnvironmentHelper.IsProductionMode = isProductionMode;
            NotifyPropertyChanged("MenuVisibility");
            //NotifyPropertyChanged("NotMenuVisibility");
            NotifyPropertyChanged("WarningsVisibility");

            App.Current.Dispatcher.Invoke(() =>
            {
                openComponetConfigurationCommand.RaiseCanExecuteChanged();
                openSequenceEditorCommand.RaiseCanExecuteChanged();
                openSequenceExecutionCommand.RaiseCanExecuteChanged();
                openResultViewerCommand.RaiseCanExecuteChanged();
                openSettingsCommand.RaiseCanExecuteChanged();
            });

            if (sequenceExecutionControl != null)
            {
                App.Current.Dispatcher.Invoke((() =>
                    { ((SequenceExecutionPresenter)sequenceExecutionControl.DataContext).RefreshMenuVisibility(); NotifyPropertyChanged("MainCommands"); }));
            }
            if (isProductionMode)
            {
                App.Current.Dispatcher.Invoke((() =>
                    SwitchMainContent(MainContentEnum.SequenceExecution)));
            }
        }

        private void logout()
        {
            AccessKey = null;
        }

        private void login()
        {
            PopupWindow.PopupWindow pw = new PopupWindow.PopupWindow(new LoginControl.LoginControl()) { Title = LanguageHelper.GetString("MTF_Login_Login"), CanClose = false };
            pw.ShowDialog();
            if (pw.MTFDialogResult.Result == MTFClientServerCommon.MTFDialogResultEnum.Ok)
            {
                AccessKey = new AccessKey("Super", "User", "System", DateTime.MaxValue,
                    new List<AccessMachine>
                    {
                        new AccessMachine("default",
                            new List<AccessRole>
                            {
                                new AccessRole("debug"),
                                new AccessRole("service"),
                                new AccessRole("teach")
                            }, new List<AccessSequence>())
                    },
                    "SYS", "1.0.0.0", "Key-Generated-By-Application-159a-adfe-eghz-547ydf-8jk-58rtuj-vluyj59", "");
            }
        }

        public SequenceExecutionPresenter SequenceExecutionPresenter
        {
            get
            {
                if (sequenceExecutionControl == null)
                {
                    return null;
                }

                return sequenceExecutionControl.DataContext as SequenceExecutionPresenter;
            }
        }

        public SequenceEditorPresenter SequenceEditorPresenter
        {
            get
            {
                if (sequenceEditorControl == null)
                {
                    return null;
                }

                return sequenceEditorControl.DataContext as SequenceEditorPresenter;
            }
        }

        public List<MTFUserControl> GetOpenedControls
        {
            get
            {
                return openedControls;
            }
        }

        public ICommand JumpToActivityCommand
        {
            get { return jumpToActivityCommand; }
        }

        private void CloseDebugSetupWindows()
        {
            if (SequenceExecutionPresenter != null)
            {
                SequenceExecutionPresenter.CloseExtendedMode();
            }
        }

        public void SwitchMainContent(MainContentEnum content)
        {
            CloseDebugSetupWindows();
            if (content == MainContentEnum.ComponentConfig)
            {
                if (componentConfigControl == null)
                {
                    componentConfigControl = new ComponentConfigControl();
                    componentConfigControl.Loaded += MainControlLoaded;
                    var dataContext = new ComponentConfigPresenter();
                    CollectionControls.Add(dataContext);
                    dataContext.PropertyChanged += MainWindowPresenter_PropertyChanged;
                    componentConfigControl.DataContext = dataContext;
                    openedControls.Add(componentConfigControl);
                }
                MainContent = componentConfigControl;
                ((PresenterBase)componentConfigControl.DataContext).Activated();
                setMainContentFlags(content);
            }
            else if (content == MainContentEnum.SequenceEdtior)
            {
                if (sequenceEditorControl == null)
                {
                    sequenceEditorControl = new SequenceEditorControl();
                    sequenceEditorControl.Loaded += MainControlLoaded;
                    var dataContext = new SequenceEditorPresenter();
                    CollectionControls.Add(dataContext);
                    dataContext.PropertyChanged += MainWindowPresenter_PropertyChanged;
                    sequenceEditorControl.DataContext = dataContext;
                    ((SequenceEditorControl)sequenceEditorControl).SetDataContext();
                    openedControls.Add(sequenceEditorControl);
                }
                MainContent = sequenceEditorControl;
                ((PresenterBase)sequenceEditorControl.DataContext).Activated();
                setMainContentFlags(content);
            }
            else if (content == MainContentEnum.SequenceExecution)
            {
                if (sequenceExecutionControl == null)
                {
                    sequenceExecutionControl = new SequenceExecutionControl();
                    sequenceExecutionControl.Loaded += MainControlLoaded;
                    var dataContext = new SequenceExecutionPresenter();
                    CollectionControls.Add(dataContext);
                    dataContext.PropertyChanged += MainWindowPresenter_PropertyChanged;
                    sequenceExecutionControl.DataContext = dataContext;
                    openedControls.Add(sequenceExecutionControl);
                }
                MainContent = sequenceExecutionControl;
                ((PresenterBase)sequenceExecutionControl.DataContext).Activated();
                setMainContentFlags(content);
            }
            else if (content == MainContentEnum.ResultViewer)
            {
                if (reportViewerControl == null)
                {
                    reportViewerControl = new ReportViewerControl();
                    reportViewerControl.Loaded += MainControlLoaded;
                    var dataContext = new ReportViewerPresenter();
                    CollectionControls.Add(dataContext);
                    dataContext.PropertyChanged += MainWindowPresenter_PropertyChanged;
                    reportViewerControl.DataContext = dataContext;
                    openedControls.Add(reportViewerControl);
                }
                MainContent = reportViewerControl;
                setMainContentFlags(content);
            }
            else if (content == MainContentEnum.EditorTest)
            {
                if (editorTestControl == null)
                {
                    editorTestControl = new EditorTest.EditorTestControl();
                    var dataContext = new EditorTest.EditorTestPresenter();
                    dataContext.PropertyChanged += MainWindowPresenter_PropertyChanged;
                    editorTestControl.DataContext = dataContext;
                    openedControls.Add(editorTestControl);
                }
                MainContent = editorTestControl;
                setMainContentFlags(content);
            }
            else if (content == MainContentEnum.Settings)
            {
                if (settingsControl == null)
                {
                    settingsControl = new SettingsControl();
                    var dataContext = new SettingsPresenter();
                    dataContext.PropertyChanged += MainWindowPresenter_PropertyChanged;
                    settingsControl.DataContext = dataContext;
                    openedControls.Add(settingsControl);
                }
                ((SettingsPresenter)settingsControl.DataContext).Activated();
                MainContent = settingsControl;
                setMainContentFlags(content);
            }
            else if (content == MainContentEnum.Warnings)
            {
                if (warningsControl == null)
                {
                    warningsControl = new WarningsViewerControl();
                    warningsControl.Loaded += MainControlLoaded;
                    var dataContext = new WarningsViewerPresenter();
                    dataContext.PropertyChanged += MainWindowPresenter_PropertyChanged;
                    warningsControl.DataContext = dataContext;
                    openedControls.Add(warningsControl);
                }
                MainContent = warningsControl;
                setMainContentFlags(content);
            }

            else
            {
                MainContent = null;
            }

            //LoadUIParams(MainContent as MTFUserControl);

            NotifyPropertyChanged("MainContent");
        }

        //private void LoadUIParams(MTFUserControl mtfUserControl)
        //{
        //    if (mtfUserControl != null)
        //    {
        //        mtfUserControl.LoadCfg();
        //    }
        //}

        private void MainControlLoaded(object sender, RoutedEventArgs e)
        {
            var fe = (FrameworkElement)sender;
            var presenter = fe.DataContext as PresenterBase;
            if (presenter != null)
            {
                updateLanguageAction(presenter.Id);
            }
        }

        private void setMainContentFlags(MainContentEnum content)
        {
            IsInfoActive = false;
            IsSequenceEditorActive = content == MainContentEnum.SequenceEdtior;
            IsComponentConfigActive = content == MainContentEnum.ComponentConfig;
            IsSequenceExecutionActive = content == MainContentEnum.SequenceExecution;
            IsResultViewerActive = content == MainContentEnum.ResultViewer;
            IsTestingActive = content == MainContentEnum.EditorTest;
            IsSettingsActive = content == MainContentEnum.Settings;
            IsWarningsViewerActive = content == MainContentEnum.Warnings;
        }

        public System.Windows.Visibility MenuVisibility
        {
            get
            {
                return EnvironmentHelper.IsProductionMode || EnvironmentHelper.HasAccessKeyRole("hideCommands", false) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            }
        }

        //public System.Windows.Visibility NotMenuVisibility
        //{
        //    get
        //    {
        //        return EnvironmentHelper.IsProductionMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        //    }
        //}

        public System.Windows.Visibility TestVisibility
        {
            get
            {
                return EnvironmentHelper.HasAccessKeyRole("developertestmode") ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        public System.Windows.Visibility WarningsVisibility
        {
            get
            {
                return !EnvironmentHelper.IsProductionMode && Warning.Count > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        public bool IsSequenceModified
        {
            get { return SequenceIsModifiedByEditor || SequenceIsModifiedByExecution; }
        }

        public bool SequenceIsModifiedByEditor
        {
            get
            {
                var editor = SequenceEditorPresenter;
                if (editor != null && editor.MainSequence != null)
                {
                    return editor.IsModified;
                }
                return false;
            }
        }

        public bool SequenceIsModifiedByExecution
        {
            get { return SequenceExecutionPresenter != null && SequenceExecutionPresenter.SequenceIsModified; }
        }

        public Task<string> OnClosing()
        {
            CloseDebugSetupWindows();
            if (MTFClient != null && StoreSettings.GetInstance.SettingsClass.StopMTFServer)
            {
                return Task.Run(() =>
                                {
                                    try
                                    {
                                        return MTFClient.RequestStopServer();
                                    }
                                    catch (Exception ex)
                                    {
                                        SystemLog.LogException(ex);
                                        return string.Empty;
                                    }
                                });
            }

            if(MTFClient != null)
            {
                MTFClient.SequenceExecutionStateChanged -= MTFClient_SequenceExecutionStateChanged;
            }

            return Task.FromResult(string.Empty);
        }

        public Control MainContent
        {
            get { return mainContent; }
            private set
            {
                mainContent = value;
                if (mainContent != null)
                {
                    setMainControlStatus(mainContent.DataContext);
                }
                NotifyPropertyChanged("MainCommands");
                NotifyPropertyChanged("HeaderText");
            }
        }

        void MainWindowPresenter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HeaderText")
            {
                NotifyPropertyChanged("HeaderText");
            }
            else if (e.PropertyName == "Commands")
            {
                NotifyPropertyChanged("MainCommands");
            }
            else if (e.PropertyName == "ControlStatus" && sender is IControlStatus)
            {
                MainControlStatus = ((IControlStatus)sender).ControlStatus;
            }
        }

        public IEnumerable<Command> MainCommands
        {
            get { return (mainContent != null && mainContent.DataContext is IMainCommands) ? ((IMainCommands)mainContent.DataContext).Commands() : null; }
        }

        public string HeaderText
        {
            get
            {
                return (mainContent != null && mainContent.DataContext is IHeaderText) ? ((IHeaderText)mainContent.DataContext).HeaderText : string.Empty;
            }
        }

        public string AssemblyVersion
        {
            get
            {
#if DEBUG
                return string.Format("{0:0.00} MB{1}ver. {2} {3} DEBUG",
                    ((double)System.Diagnostics.Process.GetCurrentProcess().WorkingSet64) / 1024 / 1024,
                    Environment.NewLine, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version,
                    (IntPtr.Size == 8) ? "64-bit" : "32-bit");
#else
                return string.Format("ver. {0} {1}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version, 
                    (IntPtr.Size == 8) ? "64-bit" : "32-bit");
#endif
            }
        }

        private ControlStatus mainControlStatus;
        public ControlStatus MainControlStatus
        {
            get { return mainControlStatus; }
            set
            {
                mainControlStatus = value;
                NotifyPropertyChanged();
            }
        }

        private void setMainControlStatus(object presenter)
        {
            MainControlStatus = !(presenter is IControlStatus) ? ControlStatus.None : ((IControlStatus)presenter).ControlStatus;
        }

        public void AppClosing()
        {
            foreach (var ap in accessKeyProviders)
            {
                ap.Dispose();
            }
        }

        public double Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                setting.AppScale = value;
                NotifyPropertyChanged();
            }
        }

        public Warning Warning
        {
            get
            {
                return Warning.Instance;
            }
        }

        public bool ShowMainMenu
        {
            get { return showMainMenu; }
            set
            {
                showMainMenu = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand ShowMenuCommand
        {
            get { return new Command(() => ShowMainMenu = !ShowMainMenu); }
        }

        private bool isRunningSequence;
        public bool IsRunningSequence
        {
            get { return this.isRunningSequence; }
            set
            {
                this.isRunningSequence = value;
                NotifyPropertyChanged();
            }
        }

        void MTFClient_SequenceExecutionStateChanged(MTFSequenceExecutionState newState)
        {
            if (newState == MTFSequenceExecutionState.Pause
                || newState == MTFSequenceExecutionState.DebugGoToNewPosition
                || newState == MTFSequenceExecutionState.DebugGoToTopPosition
                || newState == MTFSequenceExecutionState.Executing
                || newState == MTFSequenceExecutionState.ExecutionPreparation
                || newState == MTFSequenceExecutionState.Stopping
                || newState == MTFSequenceExecutionState.Aborting
                || newState == MTFSequenceExecutionState.AborSubSequence)
            {
                this.IsRunningSequence = true;
            }
            else
            {
                this.IsRunningSequence = false;
            }

            if (Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                                                      {
                                                          this.openComponetConfigurationCommand.RaiseCanExecuteChanged();
                                                          this.openSequenceEditorCommand.RaiseCanExecuteChanged();
                                                          this.jumpToActivityCommand.RaiseCanExecuteChanged();
                                                      });
            }
        }
    }
}
