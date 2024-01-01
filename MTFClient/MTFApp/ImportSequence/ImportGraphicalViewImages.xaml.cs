using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.Import;

namespace MTFApp.ImportSequence
{
    /// <summary>
    /// Interaction logic for ImportGraphicalViewImages.xaml
    /// </summary>
    public partial class ImportGraphicalViewImages : ImportSequenceBase
    {
        private readonly ObservableCollection<ImageImportWrapper> images = new ObservableCollection<ImageImportWrapper>();
        private bool allowImport = true;
        private bool allowOverride;
        private bool canNext;

        public ImportGraphicalViewImages(ImportSharedData sharedData)
            : base(sharedData)
        {
            InitializeComponent();
            DataContext = this;
        }

        public override string Title => LanguageHelper.GetString("Mtf_Import_ImagesTitle");

        public override string Description => LanguageHelper.GetString("Mtf_Import_ImagesDesc");

        public ObservableCollection<ImageImportWrapper> Images => images;

        public override bool Skip => SharedData.Images == null || !SharedData.Images.Any();

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

        public override bool CanNext => canNext;

        private void SetOverrideState(bool value)
        {
            foreach (var item in Images)
            {
                item.Conflict.OverrideOriginal = value;
            }
        }

        private void SetImportState(bool value)
        {
            foreach (var item in Images)
            {
                item.Conflict.EnableImport = value;
            }
        }

        protected override void OnShow()
        {
            if (SharedData.Images != null)
            {
                if (!images.Any())
                {
                    foreach (var img in SharedData.Images)
                    {
                        var importConflict = new ImportConflict(Guid.Empty, img.FileName, img.Name, false, false);
                        SharedData.ImportSetting.ImagesSetting.Add(importConflict);
                        images.Add(new ImageImportWrapper(importConflict, img.Data));
                    }
                }

                AssignConflictAsync();
            }
        }

        private async void AssignConflictAsync()
        {
            canNext = false;
            NotifyPropertyChanged(nameof(CanNext));

            var names = await Task.Run(() => MTFClient.GetGraphicalViewImageNames());

            if (names != null)
            {
                foreach (var img in images)
                {
                    img.Conflict.IsConflict = names.Contains(img.Conflict.FileName);
                }
            }

            canNext = true;
            NotifyPropertyChanged(nameof(CanNext));
        }
    }
}