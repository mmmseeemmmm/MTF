using System.Collections.Generic;
using System.Linq;
using MTFClientServerCommon.Helpers;

namespace MTFApp.ExportSequence
{
    /// <summary>
    /// Interaction logic for ExportSummary.xaml
    /// </summary>
    public partial class ExportSummary : ExportSequenceBase
    {
        public ExportSummary(ExportSharedData sharedData)
            : base(sharedData)
        {
            InitializeComponent();
            DataContext = this;
        }


        public override bool IsLastControl
        {
            get { return true; }
        }

        public IEnumerable<ExportSetting> ExportedSequence
        {
            get { return SharedData.SequencesToExport.Where(x => x.Export); }
        }

        public IEnumerable<ExportSetting> DontExportedSequence
        {
            get { return SharedData.SequencesToExport.Where(x => !x.Export); }
        }

        public IEnumerable<ExportSetting> ExportedConfigs
        {
            get { return SharedData.ConfigsToExport.Where(x => x.Export); }
        }

        public IEnumerable<ExportSetting> DontExportedConfigs
        {
            get { return SharedData.ConfigsToExport.Where(x => !x.Export); }
        }

        public IEnumerable<ExportSetting> ExportedImages
        {
            get { return SharedData.ImagesToExport.Where(x => x.ExportSetting.Export).Select(x => x.ExportSetting); }
        }

        public IEnumerable<ExportSetting> DontExportedImages
        {
            get { return SharedData.ImagesToExport.Where(x => !x.ExportSetting.Export).Select(x => x.ExportSetting); }
        }

        public bool ExistExport
        {
            get { return ExistExportSequence || ExistExportConfigs || ExistExportImages; }
        }

        public bool ExistNoExport
        {
            get { return ExistNoExportConfigs || ExistNoExportSequence || ExistNoExportImages; }
        }

        public bool ExistExportSequence
        {
            get { return ExportedSequence.Any(); }
        }

        public bool ExistExportConfigs
        {
            get { return ExportedConfigs.Any(); }
        }

        public bool ExistExportImages
        {
            get { return ExportedImages.Any(); }
        }

        public bool ExistNoExportSequence
        {
            get { return DontExportedSequence.Any(); }
        }

        public bool ExistNoExportConfigs
        {
            get { return DontExportedConfigs.Any(); }
        }

        public bool ExistNoExportImages
        {
            get { return DontExportedImages.Any(); }
        }


        protected override void OnShow()
        {
            NotifyPropertyChanged("ExportedSequence");
            NotifyPropertyChanged("DontExportedSequence");
            NotifyPropertyChanged("ExportedConfigs");
            NotifyPropertyChanged("DontExportedConfigs");
            NotifyPropertyChanged("ExportedImages");
            NotifyPropertyChanged("DontExportedImages");

            NotifyPropertyChanged("ExistExportSequence");
            NotifyPropertyChanged("ExistExportConfigs");
            NotifyPropertyChanged("ExistExportImages");
            NotifyPropertyChanged("ExistNoExportSequence");
            NotifyPropertyChanged("ExistNoExportConfigs");
            NotifyPropertyChanged("ExistNoExportImages");

            NotifyPropertyChanged("ExistExport");
            NotifyPropertyChanged("ExistNoExport");
        }

        public override string Title
        {
            get { return LanguageHelper.GetString("Mtf_Export_SummaryTitle"); }
        }

        public override string Description
        {
            get { return LanguageHelper.GetString("Mtf_Export_SummaryDesc"); }
        }
    }
}