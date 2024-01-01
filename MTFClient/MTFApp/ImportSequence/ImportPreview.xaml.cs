using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.GraphicalView;
using Version = MTFClientServerCommon.Version;

namespace MTFApp.ImportSequence
{
    /// <summary>
    /// Interaction logic for ImportPreview.xaml
    /// </summary>
    public partial class ImportPreview : ImportSequenceBase
    {
        private readonly Version currentMtfVersion;

        public ImportPreview(ImportSharedData sharedData)
            : base(sharedData)
        {
            InitializeComponent();
            DataContext = this;
            currentMtfVersion = new Version(Assembly.GetExecutingAssembly().GetName().Version);
        }


        public string FileName => SharedData.FileName;

        public IEnumerable<SequencesInZip> Sequences => SharedData.Sequences;

        public Dictionary<string, List<ComponentConfigInfo>> Configurations => SharedData.Configurations;

        public bool HasConfigurations => Configurations.Any();

        public IList<GraphicalViewImg> Images => SharedData.Images;

        public bool HasImages => Images.Any();

        public override string Title => LanguageHelper.GetString("Mtf_Import_PreviewTitle");

        public override bool IsFirstControl => true;

        public override string Description => LanguageHelper.GetString("Mtf_Import_PreviewDesc");

        public override bool CanNext => CanImport && base.CanNext;

        public Version MTFVersion => SharedData.OriginalMtfVersion;

        public bool CanImport => MTFVersion == null || MTFVersion <= currentMtfVersion;
    }
}