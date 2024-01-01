using System.Collections.Generic;
using MTFClientServerCommon.Helpers;

namespace MTFApp.ExportSequence
{
    /// <summary>
    /// Interaction logic for ExportClassInstances.xaml
    /// </summary>
    public partial class ExportClassInstances : ExportSequenceBase
    {
        private bool allowExport = true;

        public ExportClassInstances(ExportSharedData sharedData)
            : base(sharedData)
        {
            InitializeComponent();
            DataContext = this;
        }

        public IList<ExportSetting> ConfigsToExport
        {
            get { return SharedData.ConfigsToExport; }
        }

        public bool AllowExport
        {
            get { return allowExport; }
            set
            {
                allowExport = value;
                SetExportState(value);
            }
        }

        private void SetExportState(bool value)
        {
            foreach (var item in SharedData.ConfigsToExport)
            {
                item.Export = value;
            }
        }

        public override string Title
        {
            get { return LanguageHelper.GetString("Mtf_Export_ComponentsTitle"); }
        }

        public override string Description
        {
            get { return LanguageHelper.GetString("Mtf_Export_ComponentsDesc"); }
        }

        public override bool Skip
        {
            get { return ConfigsToExport.Count == 0; }
        }
    }
}