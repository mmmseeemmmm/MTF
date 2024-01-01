using MTFApp.UIHelpers;

namespace MTFApp.ExportSequence
{
    public class SequenceExportWrapper : NotifyPropertyBase
    {
        private bool enableExport;

        public ExportSetting ExportSetting { get; set; }
        public bool IsMain { get; set; }

        public bool EnableExport
        {
            get { return enableExport; }
            set
            {
                enableExport = value;
                NotifyPropertyChanged();
            }
        }

        public bool Export
        {
            get { return ExportSetting.Export; }
            set
            {
                ExportSetting.Export = value;
                NotifyPropertyChanged();
            }
        }


        public SequenceExportWrapper(ExportSetting exportSetting)
        {
            ExportSetting = exportSetting;
            EnableExport = false;
        }
    }
}