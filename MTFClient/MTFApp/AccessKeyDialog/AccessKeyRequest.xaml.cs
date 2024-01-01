using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using MTFApp.PopupWindow;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.RestApi;
using UsbDrive;

namespace MTFApp.AccessKeyDialog
{
    /// <summary>
    /// Interaction logic for AccessKeyRequest.xaml
    /// </summary>
    public partial class AccessKeyRequest : UserControl, IRaiseCloseEvent, INotifyPropertyChanged
    {
        private Command sendCommand;
        private Command cancelCommand;
        private Command refreshCommand;
        private readonly List<TextBox> validationElements = new List<TextBox>();
        private bool isOnline;
        private bool usbLoaded;
        private string firstName;
        private string lastName;
        private string email;
        private string reason;
        private string serverState = "Access_Request_Connecting";
        private ObservableCollection<UsbKey> availableDisks;
        private UsbKey selectedUsb;
        private bool usbError;

        public event PropertyChangedEventHandler PropertyChanged;
        public event CloseEventHandler Close;

        public AccessKeyRequest()
        {
            InitializeComponent();
            DataContext = this;
            InitCommands();
            FillValidationElements();
            FillAvailableDisksAsync();
            TestIsOnlineAsync();
        }


        public Command SendCommand
        {
            get { return sendCommand; }
        }

        public Command CancelCommand
        {
            get { return cancelCommand; }
        }

        public Command RefreshCommand
        {
            get { return refreshCommand; }
        }

        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        public string Reason
        {
            get { return reason; }
            set { reason = value; }
        }

        public bool CanSend
        {
            get { return isOnline && usbLoaded; }
        }

        public string ServerState
        {
            get { return serverState; }
            set
            {
                serverState = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<UsbKey> AvailableDisks
        {
            get { return availableDisks; }
            set
            {
                availableDisks = value;
                NotifyPropertyChanged();
            }
        }

        public UsbKey SelectedUsb
        {
            get { return selectedUsb; }
            set
            {
                selectedUsb = value;
                UsbError = value == null;
            }
        }

        public bool UsbError
        {
            get { return usbError; }
            set
            {
                usbError = value;
                NotifyPropertyChanged();
            }
        }

        private void InitCommands()
        {
            sendCommand = new Command(Send, () => CanSend);
            cancelCommand = new Command(CloseDialog);
            refreshCommand = new Command(FillAvailableDisksAsync);
        }

        private async void TestIsOnlineAsync()
        {
            await Task.Run(() =>
                           {
                               try
                               {
                                   RestApiClient.MakeRequest("http://czjih1weeg1/accesslicenses/api/request/test", RestApiMethod.Get);
                                   isOnline = true;
                                   ServerState = "Access_Request_Online";
                               }
                               catch (Exception)
                               {
                                   isOnline = false;
                                   ServerState = "Access_Request_Offline";
                               }
                           });

            UpdateCommands();
        }

        private async void FillAvailableDisksAsync()
        {
            usbLoaded = false;
            AvailableDisks = await Task.Run(() => UsbManager.GetAvailableDisks());
            usbLoaded = true;
            UpdateCommands();
        }

        private void FillValidationElements()
        {
            validationElements.Add(FirstNameTextBlock);
            validationElements.Add(LastNameTextBlock);
            validationElements.Add(EmailTextBlock);
            validationElements.Add(ReasonTextBox);
        }

        private void CloseDialog()
        {
            if (Close != null)
            {
                Close(this);
            }
        }

        private void Send()
        {
            var hasError = false;

            foreach (var validationElement in validationElements)
            {
                // ReSharper disable PossibleNullReferenceException
                validationElement.GetBindingExpression(TextBox.TextProperty).UpdateSource();

                if (!hasError)
                {
                    hasError = Validation.GetHasError(validationElement);
                }

                UsbError = SelectedUsb == null;
            }

            if (!hasError && !UsbError)
            {
                SendToServer();
            }
        }

        private void UpdateCommands()
        {
            sendCommand.RaiseCanExecuteChanged();
        }


        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void SendToServer()
        {
            var endPoint = "http://czjih1weeg1/accesslicenses/api/request/create/";
            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            var obj = new LicenseAccessRequest()
                      {
                          FirstName = FirstName,
                          LastName = LastName,
                          Email = Email,
                          HwId =
                              AccessRequestSerializer.Encrypt(
                                  new MTFClientServerCommon.MTFAccessControl.UsbDrive.AccessRequest(SelectedUsb.DeviceID,
                              Assembly.GetExecutingAssembly().GetName().Version.ToString(), SelectedUsb.HardwareID)),
                          Reason = Reason,
                          Creator = userName
                      };

            try
            {
                var json = JsonSerializer.Serialize(obj);
                RestApiClient.MakeRequest(endPoint, RestApiMethod.Post, json);
                MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Info"),
                    LanguageHelper.GetString("Access_Request_Success"), MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                CloseDialog();
            }
            catch (Exception ex)
            {
                MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), ex.Message, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
            }
        }
    }
}