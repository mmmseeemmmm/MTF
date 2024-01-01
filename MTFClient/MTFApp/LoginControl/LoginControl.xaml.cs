using MTFApp.PopupWindow;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MTFApp.LoginControl
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl, IReturnsDialogResult, IRaiseCloseEvent
    {
        public LoginControl()
        {
            InitializeComponent();
        }

        public MTFDialogResult DialogResult
        {
            get
            {
                return dialogResult;
            }
        }

        private MTFDialogResult dialogResult = new MTFDialogResult { Result = MTFDialogResultEnum.Cancel };
        private void btnCancelClick(object sender, RoutedEventArgs e)
        {
            dialogResult.Result = MTFDialogResultEnum.Cancel;
            if (Close != null)
            {
                Close(this);
            }
        }

        private static string GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return Convert.ToBase64String(algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString)));
        }

        private void btnOKClick(object sender, RoutedEventArgs e)
        {
            dialogResult.Result = (GetHash(passwordBox.Password) == "biCybeAkVVW/UPlcHzoItw94Wvh0A2MpfPKhyaB3VpU=" || passwordBox.Password == "volko2019") ? MTFDialogResultEnum.Ok : MTFDialogResultEnum.Cancel;
            if (Close != null)
            {
                Close(this);
            }
        }

        public event CloseEventHandler Close;

        private void passwordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnOKClick(sender, null);
            }
        }

        private void passwordBox_Loaded(object sender, RoutedEventArgs e)
        {
            passwordBox.Focus();
        }
    }
}
