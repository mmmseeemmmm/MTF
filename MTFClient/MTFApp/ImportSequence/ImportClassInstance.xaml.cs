using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MTFApp.ServerService;
using MTFApp.ServerService.Components;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.Import;

namespace MTFApp.ImportSequence
{
    /// <summary>
    /// Interaction logic for ImportClassInstance.xaml
    /// </summary>
    public partial class ImportClassInstance : ImportSequenceBase
    {
        private ObservableCollection<ImportConflict> items;
        private bool allowImport = true;
        private bool allowOverride;
        private bool canNext;


        public ImportClassInstance(ImportSharedData sharedData)
            : base(sharedData)
        {
            InitializeComponent();
            DataContext = this;
        }

        public override string Title => LanguageHelper.GetString("Mtf_Import_ComponentConflictTitle");

        public ObservableCollection<ImportConflict> Items
        {
            get => items;
            set
            {
                items = value;
                NotifyPropertyChanged();
            }
        }


        public bool AllowImport
        {
            get => allowImport;
            set
            {
                allowImport = value;
                SetImportState(value);
            }
        }

        public bool AllowOverride
        {
            get => allowOverride;
            set
            {
                allowOverride = value;
                SetOverrideState(value);
            }
        }

        public override string Description => LanguageHelper.GetString("Mtf_Import_ComponentConflictDesc");


        public override bool Skip => !SharedData.Configurations.Any();

        public override bool CanNext => canNext;

        protected override async void OnShow()
        {
            if (items == null)
            {
                canNext = false;
                NotifyPropertyChanged(nameof(CanNext));
                var fileNames = SharedData.ImportSetting.ClassInstances.Select(x => x.FullFileName).Distinct().ToList();
                var classInstanceConfigurations = await Task.Run(() => ServiceClientsContainer.Get<ComponentsClient>().ClassInstanceConfigurationsByNames(fileNames));
                foreach (var item in SharedData.ImportSetting.ClassInstances)
                {
                    var conflictItem = classInstanceConfigurations.FirstOrDefault(x => x.Id == item.Id);
                    if (conflictItem != null)
                    {
                        item.IsConflict = true;
                        item.ConflictItem = conflictItem.Name;
                    }
                }

                Items = new ObservableCollection<ImportConflict>(SharedData.ImportSetting.ClassInstances);
                canNext = true;
                NotifyPropertyChanged(nameof(CanNext));
            }
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