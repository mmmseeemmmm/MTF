using MTFClientServerCommon;
using System.Collections.Generic;

namespace MTFApp.MergeSequences
{
    /// <summary>
    /// Interaction logic for MergeComponents.xaml
    /// </summary>
    public partial class MergeComponents : MergeSequencesBase
    {
        private MergeSharedData sharedData;

        public MergeComponents(MergeSharedData sharedData)
        {
            InitializeComponent();
            this.sharedData = sharedData;
            this.root.DataContext = this;

        }

        public override string Title => "Merge components configuration";

        public override string Description => "Select components which you want to add to sequence or replace by components from original sequence.";

        public IEnumerable<MTFSequenceClassInfo> OriginalComponents => sharedData.OriginalSequence.MTFSequenceClassInfos;

        public IEnumerable<MergeSetting> Components => sharedData.MergeComponentsSetting;
    }
}
