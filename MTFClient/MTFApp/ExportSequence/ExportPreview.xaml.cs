using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MTFClientServerCommon.Helpers;
using Version = MTFClientServerCommon.Version;

namespace MTFApp.ExportSequence
{
    /// <summary>
    /// Interaction logic for ExportPreview.xaml
    /// </summary>
    public partial class ExportPreview : ExportSequenceBase
    {
        private readonly Version mtfVersion;

        public ExportPreview(ExportSharedData sharedData)
            : base(sharedData)
        {
            InitializeComponent();
            DataContext = this;
            mtfVersion = new Version(Assembly.GetExecutingAssembly().GetName().Version);
        }

        public IEnumerable<ExportSetting> Components
        {
            get { return SharedData.ConfigsToExport; }
        }

        public bool HasComponents
        {
            get { return Components.Any(); }
        }

        public IEnumerable<ExportSetting> Sequences
        {
            get { return SharedData.SequencesToExport; }
        }

        public bool HasSequences
        {
            get { return Sequences.Any(); }
        }

        public IEnumerable<ExportSetting> Images
        {
            get { return SharedData.ImagesToExport.Select(x => x.ExportSetting); }
        }

        public bool HasImages
        {
            get { return Images.Any(); }
        }


        public override string Title
        {
            get { return LanguageHelper.GetString("Mtf_Export_PreviewTitle"); }
        }

        public override bool IsFirstControl
        {
            get { return true; }
        }

        public override string Description
        {
            get { return LanguageHelper.GetString("Mtf_Export_PreviewDesc"); }
        }

        public Version MTFVersion
        {
            get { return mtfVersion; }
        }
    }
}