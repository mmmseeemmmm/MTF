using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using System.Collections.Generic;


namespace MTFApp.OpenSaveSequencesDialog
{
    /// <summary>
    /// Interaction logic for OpenSaveSequencesDialogControl.xaml
    /// </summary>
    public partial class OpenSaveSequencesDialogControl : UserControl
    {
        private readonly OpenSaveSequencesDialogPresenter presenter;
        private readonly DialogTypeEnum dialogType;


        public OpenSaveSequencesDialogControl(DialogTypeEnum dialogType, string rootPath, List<string> fileExtensions, bool useMtfBaseDirectory, bool allowBrowseServer)
        {
            InitializeComponent();
            this.dialogType = dialogType;
            presenter = new OpenSaveSequencesDialogPresenter(dialogType, rootPath, fileExtensions, useMtfBaseDirectory, allowBrowseServer);
            this.DataContext = presenter;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Back && presenter.RemoteItems!=null && !presenter.IsEditableMode)
            {
                var upItem = presenter.RemoteItems.FirstOrDefault(x => x.SequenceFileType == MTFDialogItemTypes.Up);
                ChangeLocation(upItem);
                e.Handled = true;
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!presenter.IsEditableMode)
            {
                if (e == null || e.ChangedButton == MouseButton.Left)
                {
                    SequenceFileContainer selectedItem = ((ListBox)sender).SelectedItem as SequenceFileContainer;
                    if (selectedItem != null)
                    {
                        PerformAction(selectedItem);
                    }
                }
            }
        }

        private void ListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            var selectedItem = listBox.SelectedItem as SequenceFileContainer;
            if (selectedItem == null)
            {
                return;
            }
            if (e.Key == Key.Enter)
            {
                PerformAction(selectedItem);
            }
            else if (e.Key == Key.Back)
            {
                if (listBox.Items.Count > 0 && listBox.Items[0] != null && ((SequenceFileContainer)listBox.Items[0]).SequenceFileType == MTFDialogItemTypes.Up)
                {
                    ChangeLocation(listBox.Items[0]);
                }
            }
            else if (e.Key == Key.F2)
            {
                presenter.RefreshDirectoryToEdit(selectedItem);
            }
            else if (e.Key == Key.Delete)
            {
                presenter.DeleteItem.Execute(selectedItem);
            }
        }

        private void ListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = UIHelper.GetObjectDataFromPoint(sender as ListBox, e.GetPosition(sender as ListBox));
            if (!(item is SequenceFileContainer))
            {
                presenter.UnselectItem();
            }
        }


        



        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox_LostFocus(sender, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                presenter.EditModeSwitchOff();
                e.Handled = true;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (presenter.IsEditableMode)
            {
                TextBox t = sender as TextBox;
                object o = t.Tag;
                try
                {
                    presenter.RenameItem.Execute(new NameStructure(t.Text, t.Tag.ToString()));
                }
                catch (Exception ex)
                {
                    MTFMessageBox.Show("Error", ex.Message, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                }
            }
        }

        private void TextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock t = sender as TextBlock;
            string item = t.Tag.ToString();
            presenter.ChangeLocationFolderFromAddress(item);
        }


        private void PerformAction(SequenceFileContainer selectedItem)
        {
            if (selectedItem == null)
            {
                return;
            }
            if (selectedItem.CanChangeLocation)
            {
                ChangeLocation(selectedItem);
            }
            else
            {
                if (dialogType == DialogTypeEnum.SaveDialog)
                {
                    PerformSaveAction(selectedItem);
                }
                else
                {
                    CloseWindow(true); 
                }
            }
        }

        public void PerformSaveAction(SequenceFileContainer selectedItem)
        {
            if (!presenter.IsIllegalChar(selectedItem.Name))
            {
                if (presenter.VerifyNameSave(selectedItem))
                {
                    SetSelectedItemAndClose(selectedItem);
                }
                else
                {
                    var result = MTFMessageBox.Show("Question", selectedItem.Name + " already exists.\nDo you want to replace it?",
                         MTFMessageBoxType.Question, MTFMessageBoxButtons.YesNo);

                    if (result == MTFMessageBoxResult.Yes)
                    {
                        SetSelectedItemAndClose(selectedItem);
                    }
                }
            }
            else
            {
                MTFMessageBox.Show("Error", "Illegal character in name", MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
            }
        }

        

        private void ChangeLocation(object param)
        {
            presenter.ChangeLocationFolder.Execute(param);
        }

        public void SetSelectedItemAndClose(SequenceFileContainer selectedItem)
        {
            presenter.SelectedItem = selectedItem;
            CloseWindow(true);
        }

        private void CloseWindow(bool dialogResult)
        {
            if (dialogType!= DialogTypeEnum.InnerDialog)
            {
                Window.GetWindow(this).DialogResult = dialogResult;
                Window.GetWindow(this).Close(); 
            }
        }

        private void ListBoxTouchDown(object sender, TouchEventArgs e)
        {
            var item = UIHelper.GetObjectDataFromPoint(sender as ListBox, e.GetTouchPoint(sender as ListBox).Position);
            if (item is SequenceFileContainer)
            {
                presenter.SelectedItem = item as SequenceFileContainer;
                PerformAction(item as SequenceFileContainer);
            }
        }
        


       

        

        

        

        





    }
}
