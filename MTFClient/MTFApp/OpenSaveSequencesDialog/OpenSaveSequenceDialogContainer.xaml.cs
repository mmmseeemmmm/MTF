using MTFApp.UIHelpers;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MTFClientServerCommon;
using System.Collections.Generic;
using MTFClientServerCommon.Helpers;

namespace MTFApp.OpenSaveSequencesDialog
{
    /// <summary>
    /// Interaction logic for OpenSaveSequenceDialogContainer.xaml
    /// </summary>
    public partial class OpenSaveSequenceDialogContainer : UserControl
    {
        private OpenSaveSequencesDialogControl body;
        private readonly OpenSaveSequencesDialogPresenter presenter;

        public OpenSaveSequenceDialogContainer(DialogTypeEnum dialogType, string rootPath, List<string> fileExtensions, bool useMtfBaseDirectory, bool allowBrowseServer)
        {
            InitializeComponent();
            this.Body = new OpenSaveSequencesDialogControl(dialogType, rootPath, fileExtensions, useMtfBaseDirectory, allowBrowseServer);
            presenter = body.DataContext as OpenSaveSequencesDialogPresenter;
            this.DataContext = presenter;
        }

        public OpenSaveSequencesDialogControl Body
        {
            get { return body; }
            set { body = value; }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseWindow(false);
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }

        private void ButtonStorno_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Window.GetWindow(this).Close();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            string parameter = ((Button)sender).CommandParameter.ToString();
            if (!string.IsNullOrEmpty(parameter))
            {
                Save(parameter); 
            }
        }

        private void ButtonOpen_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string parameter = ((Button)sender).CommandParameter.ToString();
            Open(parameter);
        }

        private void Save(string parameter)
        {
            var selectedItem = presenter.RemoteItems.FirstOrDefault(x => x.Name == parameter && x.SequenceFileType == MTFDialogItemTypes.File);
            if (selectedItem == null)
            {
                selectedItem = new SequenceFileContainer(parameter, presenter.CheckFileExtensoin(parameter), MTFDialogItemTypes.File);
                selectedItem.FullName = Path.Combine(presenter.LastFullName, selectedItem.FullName);
            }
            body.PerformSaveAction(selectedItem);
        }

        private void Open(string parameter)
        {
            var selectedItem = presenter.RemoteItems.FirstOrDefault(x => x.Name == parameter);
            if (selectedItem == null)
            {
                MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"),
                    string.Format(LanguageHelper.GetString("OpenDialog_NotFound"), parameter), MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                return;
            }
            if (selectedItem.CanChangeLocation)
            {
                presenter.ChangeLocationFolder.Execute(selectedItem);
            }
            else
            {
                body.SetSelectedItemAndClose(selectedItem);
            }
        }

        private void saveTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string parameter = (sender as TextBox).Text;
                if (!string.IsNullOrEmpty(parameter))
                {
                    Save(parameter);
                }
            }
        }

        private void openTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string parameter = (sender as TextBox).Text;
                Open(parameter);
            }
        }

        private void CloseWindow(bool dialogResult)
        {
            Window.GetWindow(this).DialogResult = dialogResult;
            Window.GetWindow(this).Close();
        }

        private void ButtonSelect_Click(object sender, RoutedEventArgs e)
        {
           CloseWindow(true);
        }
    }
}
