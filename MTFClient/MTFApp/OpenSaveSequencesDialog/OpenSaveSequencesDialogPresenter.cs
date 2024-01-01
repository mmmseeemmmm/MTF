using MTFApp.UIHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;


namespace MTFApp.OpenSaveSequencesDialog
{
    public delegate void PahtChangedEventHandler();

    class OpenSaveSequencesDialogPresenter : PresenterBase
    {
        public event PahtChangedEventHandler PathChanged;

        #region fields

        private string rootPath;
        private readonly bool useMtfBaseDirectory;
        private readonly bool allowBrowseServer;
        private List<NameStructure> fileExtension;
        private ObservableCollection<SequenceFileContainer> remoteItems;
        private SequenceFileContainer selectedItem;
        private ICommand renameFromContextMenu;
        private ICommand deleteFromContextMenu;
        private ObservableCollection<NameStructure> filePath = new ObservableCollection<NameStructure>();
        private readonly List<string> currentExtensions = new List<string>();

        #endregion


        public OpenSaveSequencesDialogPresenter(DialogTypeEnum dialogType, string rootPath, List<string> fileExtensions, bool useMtfBaseDirectory,
            bool allowBrowseServer)
        {
            this.rootPath = rootPath;
            this.useMtfBaseDirectory = useMtfBaseDirectory;
            this.allowBrowseServer = allowBrowseServer;
            supplementFileExtension(fileExtensions);
            renameFromContextMenu = new Command(param => RefreshDirectoryToEdit(param as SequenceFileContainer), () => SelectedItem != null);
            deleteFromContextMenu = new Command(PerformDeleteItem, () => SelectedItem != null);
            this.DialogType = dialogType;
            if (!allowBrowseServer)
            {
                LoadRemoteItemsAsync(string.Empty);
            }
            else
            {
                LoadRemoteItemsWithBrowsingAsync(rootPath);
            }
            FillFileExtension();
        }

        private void supplementFileExtension(List<string> fileExtensions)
        {
            if (fileExtensions != null)
            {
                foreach (var fileExtension in fileExtensions)
                {
                    var fileExtensionToUpper = fileExtension.ToUpper();
                    currentExtensions.Add(fileExtension != null && !fileExtension.StartsWith(".") ? string.Format(".{0}", fileExtension) : fileExtension);
                    currentExtensions.Add(fileExtensionToUpper != null && !fileExtensionToUpper.StartsWith(".") ? string.Format(".{0}", fileExtensionToUpper) : fileExtensionToUpper);
                }
            }
        }
        
        protected virtual void OnPathChanged()
        {
            if (PathChanged != null)
            {
                PathChanged();
            }
        }

        private async void LoadRemoteItemsWithBrowsingAsync(string path)
        {
            if (useMtfBaseDirectory)
            {
                await Task.Run(() => path = MTFClient.GetServerFullDirectoryPath(path));
            }
            if (!string.IsNullOrEmpty(path))
            {
                foreach (var s in path.Split(Path.DirectorySeparatorChar))
                {
                    UpdatePath(s.EndsWith(":") ? s + Path.DirectorySeparatorChar : s);
                }
            }
            SetNewItems(path);
        }

        private async void LoadRemoteItemsAsync(string path)
        {
            await Task.Run(() => remoteItems = GetItemsFromServer(path));
            NotifyPropertyChanged("RemoteItems");
        }

        #region commands

        public ICommand ChangeLocationFolder
        {
            get { return new Command(ChangeLocation); }
        }

        public ICommand RenameItem
        {
            get { return new Command(PerformRenameItem); }
        }

        public ICommand RenameFromContextMenu
        {
            get { return renameFromContextMenu; }
        }

        public ICommand DeleteItem
        {
            get { return deleteFromContextMenu; }
        }

        public ICommand CreateDirectory
        {
            get { return new Command(createDirectory); }
        }

        #endregion

        #region properties

        public ObservableCollection<SequenceFileContainer> RemoteItems
        {
            get { return remoteItems; }
            set { remoteItems = value; }
        }

        public SequenceFileContainer SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                NotifyPropertyChanged();
            }
        }

        public List<NameStructure> FileExtension
        {
            get { return fileExtension; }
            set { fileExtension = value; }
        }

        public NameStructure SelectedFileExtension { get; set; }

        public DialogTypeEnum DialogType { get; set; }

        public bool IsEditableMode
        {
            get
            {
                return remoteItems.Any(x => { return (x.IsEditable == true); });
            }
        }

        public ObservableCollection<NameStructure> FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        public string LastFullName
        {
            get
            {
                return filePath.Count < 1 ? string.Empty : filePath.Last().FullName;
            }
        }

        #endregion

        #region public methods

        public void RefreshDirectoryToEdit(SequenceFileContainer selectedItem)
        {
            selectedItem.IsEditable = !selectedItem.IsEditable;
        }

        public void EditModeSwitchOff()
        {
            foreach (var item in remoteItems)
            {
                if (item.IsEditable)
                {
                    item.IsEditable = false;
                }
            }
        }

        public void UnselectItem()
        {
            SelectedItem = null;
        }

        public void ChangeLocationFolderFromAddress(string fullName)
        {
            string newFullName = fullName;
            int index = filePath.IndexOf(filePath.FirstOrDefault(x => x.FullName == fullName));
            if (index == -1)
            {
                newFullName = string.Empty;
                filePath.Clear();
            }
            else
            {
                int count = filePath.Count() - 1;
                for (int i = index; i < count; i++)
                {
                    RemoveLastPath();
                }
            }
            LoadRemoteItemsAsync(newFullName);
            if (filePath.Count > 0)
            {
                remoteItems.Insert(0, new SequenceFileContainer(MTFDialogItemTypes.Up));
            }
            NotifyPropertyChanged("RemoteItems");
            OnPathChanged();
        }

        #endregion

        #region private methods

        private void FillFileExtension()
        {
            fileExtension = new List<NameStructure>() { new NameStructure(BaseConstants.SequenceExtension, string.Format("{0} (*{1})", LanguageHelper.GetString("OpenDialog_TypeSequence"), BaseConstants.SequenceExtension)) };
            SelectedFileExtension = fileExtension[0];
        }

        private ObservableCollection<SequenceFileContainer> GetItemsFromServer(string path)
        {
            var items = new ObservableCollection<SequenceFileContainer>();
            if (path == null)
            {
                path = string.Empty;
            }
            try
            {
                var serverData = allowBrowseServer
                    ? MTFClient.GetServerFileAndFolders(path, currentExtensions.Count > 0)
                    : MTFClient.GetSequencesInfo(rootPath ?? string.Empty, path);

                serverData.ForEach(x =>
                                   {
                                       if (x.Type == MTFDialogItemTypes.File)
                                       {
                                           var extension = Path.GetExtension(x.Name);
                                           if (currentExtensions.Contains(extension))
                                           {
                                               items.Add(new SequenceFileContainer(Path.GetFileNameWithoutExtension(x.Name),
                                                   Path.Combine(path, x.Name), x.Type));
                                           }
                                       }
                                       else
                                       {
                                           items.Add(new SequenceFileContainer(x.Name, Path.Combine(path, x.Name), x.Type));
                                       }
                                   });
            }
            catch (Exception)
            {
                items.Clear();
            }
            //UpdatePath("");
            return items;
        }

        private void ChangeLocation(object param)
        {
            SequenceFileContainer actualItem;
            if (param == null)
            {
                return;
            }
            actualItem = param as SequenceFileContainer;
            if (actualItem != null)
            {
                SetNewItems(actualItem);
            }
        }

        private void SetNewItems(SequenceFileContainer actualItem)
        {
            if (actualItem.CanBrowse)
            {
                UpdatePath(actualItem.Name);
                SetNewItems(LastFullName);
            }
            if (actualItem.SequenceFileType == MTFDialogItemTypes.Up)
            {
                RemoveLastPath();
                SetNewItems(LastFullName);
            }
            OnPathChanged();
        }

        private async void SetNewItems(string fullName)
        {
            ObservableCollection<SequenceFileContainer> newItems = null;
            await Task.Run(() => newItems = GetItemsFromServer(fullName));
            if (filePath.Count > 0)
            {
                newItems.Insert(0, new SequenceFileContainer(MTFDialogItemTypes.Up));
            }
            remoteItems = newItems;
            NotifyPropertyChanged("RemoteItems");
        }

        private void UpdatePath(string name)
        {
            if (name != "")
            {
                filePath.Add(new NameStructure(name, Path.Combine(LastFullName, name)));
            }
            NotifyPropertyChanged("Path");
        }

        private void RemoveLastPath()
        {
            if (filePath.Count > 0)
            {
                filePath.RemoveAt(filePath.Count - 1);
                NotifyPropertyChanged("Path");
            }
        }

        public bool VerifyNameSave(SequenceFileContainer newItem)
        {
            foreach (SequenceFileContainer item in remoteItems)
            {
                if (item.FullName == newItem.FullName && item.SequenceFileType == newItem.SequenceFileType)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsIllegalChar(string name)
        {
            return BaseConstants.IllegalFileNameCharacters.Any(name.Contains);
        }

        public string CheckFileExtensoin(string parameter)
        {
            if (parameter.EndsWith(SelectedFileExtension.Name))
            {
                return parameter;
            }
            else return parameter + SelectedFileExtension.Name;
        }

        private void PerformRenameItem(object param)
        {
            NameStructure item = param as NameStructure;
            if (item == null)
            {
                return;
            }
            if (selectedItem != null)
            {
                if (string.Equals(selectedItem.Name, item.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    EditModeSwitchOff();
                    return;
                }
            }
            MTFClient.RenameItem(item.Name, item.FullName, rootPath, useMtfBaseDirectory);
            var selItem = remoteItems.FirstOrDefault(x => x.FullName == item.FullName);
            if (selItem != null)
            {
                selItem.Name = item.Name;
                selItem.FullName = SetFullName(item, selItem.SequenceFileType);
            }
            NotifyPropertyChanged("SelectedItem");
            EditModeSwitchOff();
        }

        private string SetFullName(NameStructure item, MTFDialogItemTypes sequenceFileType)
        {
            var path = Path.GetDirectoryName(item.FullName);
            var fileName = Path.GetFileNameWithoutExtension(item.FullName);
            var newName = Path.Combine(path, item.Name);
            if (sequenceFileType == MTFDialogItemTypes.File)
            {
                newName = CheckFileExtensoin(newName);
            }
            return newName;
        }

        private void PerformDeleteItem(object param)
        {
            var item = param as SequenceFileContainer;
            if (item != null)
            {
                var result = MTFMessageBox.ShowConfirmRemoveItem(item.Name);
                if (result == MTFMessageBoxResult.Yes)
                {
                    if (item.SequenceFileType == MTFDialogItemTypes.Folder)
                    {
                        MTFClient.RemoveDirectory(item.FullName, rootPath, useMtfBaseDirectory);
                        remoteItems.Remove(item);
                        NotifyPropertyChanged("RemoteItems");
                    }
                    else if (item.SequenceFileType == MTFDialogItemTypes.File)
                    {
                        MTFClient.RemoveFile(item.FullName, rootPath, useMtfBaseDirectory);
                        remoteItems.Remove(item);
                        NotifyPropertyChanged("RemoteItems");
                    }
                }
            }
        }

        private async void createDirectory()
        {
            string path = LastFullName;
            string name = MTFClient.CreateDirectory(path, LanguageHelper.GetString("Buttons_NewFolder"), useMtfBaseDirectory);
            await Task.Run(() => remoteItems = GetItemsFromServer(path));
            if (filePath.Count > 0)
            {
                remoteItems.Insert(0, new SequenceFileContainer(MTFDialogItemTypes.Up));
            }
            var newFolder = remoteItems.FirstOrDefault(x => x.Name == name);
            if (newFolder != null)
            {
                RefreshDirectoryToEdit(newFolder);
            }
            NotifyPropertyChanged("RemoteItems");
        }


        #endregion

    }

}
