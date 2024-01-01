using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFClientServerCommon.Helpers;

namespace MTFApp.ConnectionDialog
{
    /// <summary>
    /// Interaction logic for ConnectionDialogControl.xaml
    /// </summary>
    public partial class ConnectionDialogControl : UserControl
    {
        ConnectionDialogPresenter connectionDialogPresenter = new ConnectionDialogPresenter();
        public ConnectionDialogControl()
        {
            InitializeComponent();
            DataContext = connectionDialogPresenter;
        }

        private async void runButton_Click(object sender, RoutedEventArgs e)
        {
            var currentCursor = Cursor;
            try
            {
                ((ConnectionDialogPresenter)DataContext).IsEnableButtons = false;
                Window parentWindow = Window.GetWindow(this);
                this.Cursor = Cursors.Wait;
                await ((ConnectionDialogPresenter)DataContext).Connect();
                if (((ConnectionDialogPresenter)DataContext).IsConnected)
                {
                    parentWindow.Close();
                }
            }
            catch (Exception ex)
            {
                if (((ConnectionDialogPresenter)DataContext).SelectedItem.Host == "localhost" || ((ConnectionDialogPresenter)DataContext).SelectedItem.Host == "127.0.0.1")
                {
                    var result = MessageBox.Show(
                        string.Format("{0}{1}{1}{2}", ex.Message, Environment.NewLine, LanguageHelper.GetString("MTF_Connection_RunServer")),
                        LanguageHelper.GetString("MTF_Connection_Error"), MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        OpenFileDialog dialog = new OpenFileDialog();
                        dialog.Filter = string.Format("{0}  (*.exe)|*.exe", LanguageHelper.GetString("MTF_Connection_ExeFiles"));
                        var mtfDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "Server"));
                        dialog.InitialDirectory = Directory.Exists(mtfDirectory) ? mtfDirectory : Environment.CurrentDirectory;
                        if (dialog.ShowDialog() == true)
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                WorkingDirectory = System.IO.Path.GetDirectoryName(dialog.FileName),
                                FileName = System.IO.Path.GetFileName(dialog.FileName),
                                Arguments = ((ConnectionDialogPresenter) DataContext).SelectedItem.Port
                            });


                            runButton_Click(sender, e);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(ex.Message, LanguageHelper.GetString("MTF_Connection_Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                this.Cursor = currentCursor;
                ((ConnectionDialogPresenter)DataContext).IsEnableButtons = true;
            }
        }

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                ((Button)sender).Focus();
            }
        }
    }
}
