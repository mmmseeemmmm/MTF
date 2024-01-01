using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.Import;


namespace MTFApp.ImportSequence
{
    /// <summary>
    /// Interaction logic for ImportSummary.xaml
    /// </summary>
    public partial class ImportSummary : ImportSequenceBase
    {
        private readonly MTFImportSetting importSetting;

        public ImportSummary(ImportSharedData sharedData):base(sharedData)
        {
            InitializeComponent();
            importSetting = sharedData.ImportSetting;
            DataContext = this;
        }

        public override string Title
        {
            get { return LanguageHelper.GetString("Mtf_Import_SummaryTitle"); }
        }

        public override string Description
        {
            get { return LanguageHelper.GetString("Mtf_Import_SummaryDesc"); }
        }

        public override bool IsLastControl
        {
            get { return true; }
        }

        public string DestinationFolder
        {
            get { return Path.Combine(LanguageHelper.GetString("OpenDialog_Root"), SharedData.SequenceDestinationPath) + "\\"; }
        }

        public IEnumerable<ImportConflict> ImportedSequence
        {
            get { return importSetting.Sequences.Where(x => x.EnableImport && (!x.IsConflict || (x.IsConflict && x.OverrideOriginal))); }
        }

        public IEnumerable<ImportConflict> DontImportedSequence
        {
            get { return importSetting.Sequences.Where(x => !x.EnableImport || (x.IsConflict && !x.OverrideOriginal)); }
        }

        public IEnumerable<ImportConflict> ImportedConfigs
        {
            get { return importSetting.ClassInstances.Where(x => x.EnableImport && (!x.IsConflict || (x.IsConflict && x.OverrideOriginal))); }
        }

        public IEnumerable<ImportConflict> DontImportedConfigs
        {
            get { return importSetting.ClassInstances.Where(x => !x.EnableImport || (x.IsConflict && !x.OverrideOriginal)); }
        }

        public IEnumerable<ImportConflict> ImportedImages
        {
            get { return importSetting.ImagesSetting.Where(x => x.EnableImport && (!x.IsConflict || (x.IsConflict && x.OverrideOriginal))); }
        }

        public IEnumerable<ImportConflict> DontImportedImages
        {
            get { return importSetting.ImagesSetting.Where(x => !x.EnableImport || (x.IsConflict && !x.OverrideOriginal)); }
        }

        public bool ExistImport
        {
            get { return ExistImportSequence || ExistImportConfigs || ExistImportImages; }
        }

        public bool ExistNoImport
        {
            get { return ExistNoImportConfigs || ExistNoImportSequence || ExistNoImportImages; }
        }

        public bool ExistImportSequence
        {
            get { return ImportedSequence.Any(); }
        }

        public bool ExistImportConfigs
        {
            get { return ImportedConfigs.Any(); }
        }

        public bool ExistImportImages
        {
            get { return ImportedImages.Any(); }
        }

        public bool ExistNoImportSequence
        {
            get { return DontImportedSequence.Any(); }
        }

        public bool ExistNoImportConfigs
        {
            get { return DontImportedConfigs.Any(); }
        }

        public bool ExistNoImportImages
        {
            get { return DontImportedImages.Any(); }
        }


        protected override void OnShow()
        {
            NotifyPropertyChanged("ImportedSequence");
            NotifyPropertyChanged("DontImportedSequence");
            NotifyPropertyChanged("ImportedConfigs");
            NotifyPropertyChanged("DontImportedConfigs");
            NotifyPropertyChanged("ImportedImages");
            NotifyPropertyChanged("DontImportedImages");

            NotifyPropertyChanged("ExistImportSequence");
            NotifyPropertyChanged("ExistImportConfigs");
            NotifyPropertyChanged("ExistNoImportSequence");
            NotifyPropertyChanged("ExistNoImportConfigs");
            NotifyPropertyChanged("ExistImportImages");
            NotifyPropertyChanged("ExistNoImportImages");

            NotifyPropertyChanged("ExistImport");
            NotifyPropertyChanged("ExistNoImport");
        }

        
    }
}