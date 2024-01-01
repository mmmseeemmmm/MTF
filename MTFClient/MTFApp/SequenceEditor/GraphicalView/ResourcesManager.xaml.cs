using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using MTFApp.UIHelpers;
using MTFApp.UIHelpers.DragAndDrop;
using MTFApp.UIHelpers.LongTask;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.GraphicalView;
using MTFClientServerCommon.Helpers;

namespace MTFApp.SequenceEditor.GraphicalView
{
    /// <summary>
    /// Interaction logic for ResourcesControl.xaml
    /// </summary>
    public partial class ResourcesManager : UserControl, INotifyPropertyChanged
    {
        private GraphicalViewImg selectedItem;
        private readonly Command renameCommand;
        private readonly Command removeCommand;
        private readonly MTFClient mtfClient = MTFClient.GetMTFClient();
        private ObservableCollection<GraphicalViewImg> imageCollection;
        private readonly List<string> allowdExtensions = new List<string> { "*.png", "*.jpeg", "*.jpg", "*.bmp" };
        private readonly SettingsClass setting;

        public event PropertyChangedEventHandler PropertyChanged;


        public ResourcesManager()
        {
            InitializeComponent();
            ResourcesControlRoot.DataContext = this;
            IsEnabled = false;
            setting = StoreSettings.GetInstance.SettingsClass;

            GetImageResourcesAsync();

            renameCommand = new Command(Rename, () => SelectedItem != null);
            removeCommand = new Command(Remove, () => SelectedItem != null);
        }

        #region Commands

        public ICommand AddCommand
        {
            get { return new Command(AddResource); }
        }

        public Command RenameCommand
        {
            get { return renameCommand; }
        }

        public Command RemoveCommand
        {
            get { return removeCommand; }
        }

        #endregion

        #region Poperties

        public ObservableCollection<GraphicalViewImg> ImageCollection
        {
            get { return imageCollection; }
        }

        public GraphicalViewImg SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                NotifyPropertyChanged();
                UpdateCommands();
            }
        }

        #endregion

        private void Remove()
        {
            if (SelectedItem != null)
            {
                if (MTFMessageBox.ShowConfirmRemoveItem(SelectedItem.Name) == MTFMessageBoxResult.Yes)
                {
                    mtfClient.RemoveFile(string.Format("{0}{1}", SelectedItem.FileName, BaseConstants.GraphicalViewImageExtension),
                        BaseConstants.GraphicalViewSources, true);
                    ImageCollection.Remove(SelectedItem);
                }
            }
        }

        private async void GetImageResourcesAsync()
        {
            imageCollection = await GraphicalViewResourcesHelper.Instance.GetResources();
            NotifyPropertyChanged("ImageCollection");
            IsEnabled = true;
        }

        private void Rename()
        {
            if (SelectedItem != null)
            {
                var renameControl = new RenameControl(SelectedItem, "Name");
                var popup = new PopupWindow.PopupWindow(renameControl, true);
                popup.ShowDialog();
                if (popup.MTFDialogResult != null && popup.MTFDialogResult.Result == MTFDialogResultEnum.Ok)
                {
                    LongTask.Do(() =>
                                {
                                    mtfClient.RemoveFile(
                                        string.Format("{0}{1}", SelectedItem.FileName, BaseConstants.GraphicalViewImageExtension),
                                        BaseConstants.GraphicalViewSources, true);
                                    SaveImages(new List<GraphicalViewImg> { SelectedItem });
                                }, LanguageHelper.GetString("Mtf_LongTask_Saving"));
                }
            }
        }

        private void UpdateCommands()
        {
            removeCommand.RaiseCanExecuteChanged();
            renameCommand.RaiseCanExecuteChanged();
        }

        private void AddResource()
        {
            var filter = string.Join(";", allowdExtensions);
            var dialog = new OpenFileDialog
                         {
                             Multiselect = true,
                             Filter = string.Format("{0} ({1})|{1}",
                                 LanguageHelper.GetString("Editor_GraphicalView_ResourceManager_ImageFiles"),
                                 filter)
                         };

            if (dialog.ShowDialog() == true)
            {
                LongTask.Do(() => UploadImages(dialog.FileNames), LanguageHelper.GetString("Mtf_LongTask_Saving"));
                SelectedItem = ImageCollection.LastOrDefault();
                ScrollToEnd();
            }
        }

        private void UploadImages(IEnumerable<string> imagePaths)
        {
            var list = imagePaths.Select(fileName => new GraphicalViewImg
                                                     {
                                                         Data = File.ReadAllBytes(fileName),
                                                         Name = Path.GetFileNameWithoutExtension(fileName)
                                                     }).ToList();

            var result = SaveImages(list);
            if (result != null)
            {
                foreach (var img in result)
                {
                    ImageCollection.Add(img);
                }
            }
        }

        private List<GraphicalViewImg> SaveImages(List<GraphicalViewImg> images)
        {
            return UIHelper.HandleWcfCallWithErrorMsg(() => mtfClient.SaveGraphicalViewImages(images),
                LanguageHelper.GetString("Editor_GraphicalView_ResourceManager_SaveError"));
        }


        private void ResourcesPreviewMouseWhell(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = UIHelper.FindChild<ScrollViewer>(sender as DependencyObject);

            if (scrollViewer == null)
            {
                return;
            }

            if (e.Delta < 0)
            {
                scrollViewer.LineRight();
            }
            else
            {
                scrollViewer.LineLeft();
            }

            e.Handled = true;
        }

        private void ScrollToEnd()
        {
            var scrollViewer = UIHelper.FindChild<ScrollViewer>(ResourcesListBox);
            if (scrollViewer != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => scrollViewer.ScrollToRightEnd()), DispatcherPriority.ApplicationIdle);
            }
        }

        private void ResourcesListBoxKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    Remove();
                    break;
                case Key.F2:
                    Rename();
                    break;
            }
        }

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ResourcesListBoxOnPreviewDrop(object sender, DragEventArgs e)
        {
            if (setting.AllowDragDrop && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null)
                {
                    UploadImages(files.Where(file => allowdExtensions.Any(ex => string.Equals(ex, string.Format("*{0}", Path.GetExtension(file)), StringComparison.CurrentCultureIgnoreCase))));
                }
            }
        }

        private void ResourcesListBoxOnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!setting.AllowDragDrop)
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var item = UIHelper.FindParent<ListBoxItem>(e.MouseDevice.DirectlyOver as DependencyObject);
                if (item != null)
                {
                    var data = new DataObject(DragAndDropTypes.ResourceManagerImage, item.Content);
                    DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
                }
            }
        }
    }
}