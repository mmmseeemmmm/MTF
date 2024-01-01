using MTFApp.UIHelpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MTFApp.ServerService;


namespace MTFApp.ConnectionDialog
{
    class ConnectionDialogPresenter : PresenterBase
    {
        #region Fields

        private ObservableCollection<ConnectionContainer> sourceList;
        private ConnectionContainer selectedItem;
        private StoreSettings setting = StoreSettings.GetInstance;
        private bool isConnected;
        private TemplateStyleEnum templateStyle;
        private Command runCommand = null;
        private Command exitCommand = null;

        #endregion


        #region Properties

        private bool isEnableButtons;

        public bool IsEnableButtons
        {
            get { return isEnableButtons; }
            set
            {
                isEnableButtons = value;
                runCommand.RaiseCanExecuteChanged();
                exitCommand.RaiseCanExecuteChanged();
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<ConnectionContainer> SourceList
        {
            get { return sourceList; }
            set
            {
                sourceList = value;
                NotifyPropertyChanged();
            }
        }
        public ConnectionContainer SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (value != null)
                {
                    setting.SettingsClass.SelectedConnectionIndex = sourceList.IndexOf(value);
                }
                selectedItem = value;
                NotifyPropertyChanged();
            }
        }
        public bool IsConnected
        {
            get { return isConnected; }
            set { isConnected = value; }
        }
        public string AssemblyVersion
        {
            get
            {
#if DEBUG
                return string.Format("ver. {0} {1} DEBUG", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version, (IntPtr.Size == 8) ? "64-bit" : "32-bit");
#else
                return string.Format("ver. {0} {1}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version, (IntPtr.Size == 8) ? "64-bit" : "32-bit");
#endif
            }
        }
        public TemplateStyleEnum TemplateStyle
        {
            get { return templateStyle; }
            set { templateStyle = value; }
        }
        public ConnectionContainer NewItem { get; set; }

        #endregion


        #region ctor

        public ConnectionDialogPresenter()
        {
            templateStyle = TemplateStyleEnum.Login;
            sourceList = new ObservableCollection<ConnectionContainer>();
            runCommand = new Command(() => setting.Save(false), () => IsEnableButtons);
            exitCommand = new Command(() =>
            {
                Application.Current.Shutdown();
                setting.Save(false);
            }, () => IsEnableButtons);
            FillList();
            IsEnableButtons = true;
        }

        #endregion


        #region Commands

        public ICommand ExitCommand
        {
            get { return exitCommand; }
        }

        public ICommand RunCommand
        {
            get { return runCommand; }
        }

        public ICommand SwitchTemplateCommand
        {
            get { return new Command(SwitchTemplate); }
        }

        public ICommand RemoveItemCommand
        {
            get { return new Command(RemoveItem); }
        }

        public ICommand AddItemCommand
        {
            get { return new Command(AddItem); }
        }

        public ICommand SaveSettingCommand
        {
            get { return new Command(SaveSetting); }
        }

        #endregion


        #region Public Methods

        public async Task Connect()
        {
            SettingsClass.SelectedConnection = SelectedItem;
            isConnected = false;
            await Task.Run(() =>
            {
                ServiceClientsContainer.ConnectAll();
                isConnected = ServiceClientsContainer.AllIsConnected;
            });
        }

        #endregion


        #region Private Methods

        private void SaveSetting()
        {
            selectedItem.Alias = NewItem.Alias;
            selectedItem.Host = NewItem.Host;
            selectedItem.Port = NewItem.Port;
            NotifyPropertyChanged("SelectedItem");
            setting.SettingsClass.Connections = sourceList.ToList();
            SwitchTemplate(TemplateStyleEnum.Login);
        }

        private void AddItem(object param)
        {
            sourceList.Add(param as ConnectionContainer);
            setting.SettingsClass.Connections = sourceList.ToList();
            selectedItem = sourceList.Last() as ConnectionContainer;
            NotifyPropertyChanged("SelectedItem");
            SwitchTemplate(TemplateStyleEnum.Login);
        }

        private void RemoveItem(object param)
        {
            if (param == selectedItem)
            {
                sourceList.Remove(param as ConnectionContainer);
                if (sourceList.Count > 0)
                {
                    selectedItem = sourceList.First();
                }
            }
            else
            {
                sourceList.Remove(param as ConnectionContainer);
            }
            setting.SettingsClass.Connections = sourceList.ToList();
            NotifyPropertyChanged("SelectedItem");
        }

        private void SwitchTemplate(object param)
        {
            templateStyle = (TemplateStyleEnum)Enum.Parse(typeof(TemplateStyleEnum), param.ToString());
            NotifyPropertyChanged("TemplateStyle");
            if (templateStyle == TemplateStyleEnum.Add)
            {
                NewItem = new ConnectionContainer();
            }
            if (templateStyle == TemplateStyleEnum.Setting && selectedItem != null)
            {
                NewItem = new ConnectionContainer() { Alias = selectedItem.Alias, Host = selectedItem.Host, Port = selectedItem.Port };
            }
        }

        private void FillList()
        {
            //sourceList.Add(new ConnectionContainer { Alias = "PC 1", Host = "192.168.1.0", Port = "2442" });
            //sourceList.Add(new ConnectionContainer { Alias = "PC 2", Host = "192.168.0.254", Port = "8080" });
            //sourceList.Add(new ConnectionContainer { Alias = "PC 3", Host = "10.0.0.1", Port = "1234" });
            //sourceList.Add(new ConnectionContainer { Alias = "PC 4", Host = "10.0.0.2", Port = "7890" });
            //sourceList.Add(new ConnectionContainer { Alias = "PC 5", Host = "127.0.0.1", Port = "1111" });
            //setting.SettingsClass.Connections = sourceList.ToList();
            sourceList = new ObservableCollection<ConnectionContainer>(setting.SettingsClass.Connections);
            int selectedIndex = setting.SettingsClass.SelectedConnectionIndex;
            if (selectedIndex < sourceList.Count)
            {
                SelectedItem = sourceList[selectedIndex];
            }
            NotifyPropertyChanged("SourceList");
        }

        #endregion
    }

    public enum TemplateStyleEnum
    {
        Login,
        Add,
        Setting
    }


}
