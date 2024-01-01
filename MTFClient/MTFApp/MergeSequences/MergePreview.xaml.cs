namespace MTFApp.MergeSequences
{
    /// <summary>
    /// Interaction logic for MergePreview.xaml
    /// </summary>
    public partial class MergePreview : MergeSequencesBase
    {
        private MergeSharedData sharedData;

        public MergePreview(MergeSharedData sharedData)
        {
            InitializeComponent();
            this.sharedData = sharedData;
            this.root.DataContext = this;
        }

        public override string Title => "Merge preview";

        public override string Description => "Content of selected sequence.";

        public override bool IsFirstControl => true;

        public MTFClientServerCommon.MTFSequence Sequence => sharedData.MergedSequence;
    }
}
