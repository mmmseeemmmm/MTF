using MTFClientServerCommon.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MTFClientServerCommon.Import;

namespace MTFApp.ImportSequence
{
    /// <summary>
    /// Interaction logic for ImportSequenceConflicts.xaml
    /// </summary>
    public partial class ImportSequenceConflicts : ImportSequenceBase
    {
        private ObservableCollection<ImportConflict> items;
        private string destinationFolder;
        private bool reload;
        private bool allowImport = true;
        private bool allowOverride;

        public ImportSequenceConflicts(ImportSharedData sharedData)
            : base(sharedData)
        {
            InitializeComponent();
            DataContext = this;
            destinationFolder = sharedData.SequenceDestinationPath;
            DataContext = this;
        }


        public ObservableCollection<ImportConflict> Items
        {
            get { return items; }
            set
            {
                items = value;
                NotifyPropertyChanged();
            }
        }

        public string DestinationFolder
        {
            get { return Path.Combine(LanguageHelper.GetString("OpenDialog_Root"), SharedData.SequenceDestinationPath) + "\\"; }
        }

        public override string Title
        {
            get { return LanguageHelper.GetString("Mtf_Import_SeqConflictTitle"); }
        }

        public override string Description
        {
            get { return LanguageHelper.GetString("Mtf_Import_SeqConflictDesc"); }
        }

        public bool AllowImport
        {
            get { return allowImport; }
            set
            {
                allowImport = value;
                SetImportState(value);
            }
        }


        public bool AllowOverride
        {
            get { return allowOverride; }
            set
            {
                allowOverride = value;
                SetOverrideState(value);
            }
        }

        protected override void OnShow()
        {
            if (destinationFolder != SharedData.SequenceDestinationPath)
            {
                reload = true;
                destinationFolder = SharedData.SequenceDestinationPath;
            }
            else
            {
                reload = false;
            }
            SolveConflicts(SharedData.ImportSetting.Sequences);
            NotifyPropertyChanged("DestinationFolder");
        }

        private void SolveConflicts(List<ImportConflict> sequenceNames)
        {
            if (items == null || reload)
            {
                foreach (var item in sequenceNames)
                {
                    item.StorePath = GetFullPath(SharedData.SequenceDestinationPath, item.FullFileName, SharedData.MetaData);
                    item.IsConflict = MTFClient.ExistSequence(item.StorePath);
                    item.OverrideOriginal = false;
                }
                Items = new ObservableCollection<ImportConflict>(sequenceNames);
            }
        }

        private string GetFullPath(string sequenceDestination, string fileName, List<SequenceMetaData> metaData)
        {
            var fileMetaData = metaData.FirstOrDefault(x => x.StoredName == fileName);
            if (fileMetaData != null)
            {
                if (!string.IsNullOrEmpty(fileMetaData.MainSequenceName))
                {
                    var mainSequenceName = fileMetaData.MainSequenceName;
                    var parentMetaData = metaData.FirstOrDefault(x => x.Path == mainSequenceName);
                    if (parentMetaData != null)
                    {
                        var path = fileMetaData.Path ?? Path.GetFileName(fileMetaData.OriginalName);
                        return fileMetaData.PathForImport = DirectoryPathHelper.GetFullPathFromRelative(parentMetaData.PathForImport, path);
                    }
                }
                return fileMetaData.PathForImport = Path.Combine(sequenceDestination, fileMetaData.Path);
            }
            return Path.Combine(sequenceDestination, fileName);
        }


        private void SetOverrideState(bool value)
        {
            foreach (var item in Items.Where(x=>x.EnableImport))
            {
                item.OverrideOriginal = value;
            }
        }

        private void SetImportState(bool value)
        {
            foreach (var item in Items)
            {
                item.EnableImport = value;
            }
        }
    }
}