using System.Collections.Generic;
using MTFClientServerCommon.Helpers;

namespace MTFApp.ExportSequence
{
    /// <summary>
    /// Interaction logic for ExportImages.xaml
    /// </summary>
    public partial class ExportImages : ExportSequenceBase
    {
        private bool allowExport = true;

        public ExportImages(ExportSharedData data)
            : base(data)
        {
            InitializeComponent();
            DataContext = this;
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

        public IList<ImageExportWrapper> Images
        {
            get { return SharedData.ImagesToExport; }
        }

        public override string Title
        {
            get { return LanguageHelper.GetString("Mtf_Export_ImagesTitle"); }
        }

        public override string Description
        {
            get { return LanguageHelper.GetString("Mtf_Export_ImagesDesc"); }
        }

        public override bool Skip
        {
            get { return Images.Count == 0; }
        }

        private void SetExportState(bool value)
        {
            foreach (var wrapper in Images)
            {
                wrapper.ExportSetting.Export = value;
            }
        }
    }
}