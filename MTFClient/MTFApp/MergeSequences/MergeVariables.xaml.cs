using MTFClientServerCommon;
using System.Collections.Generic;

namespace MTFApp.MergeSequences
{
    /// <summary>
    /// Interaction logic for MergeVariables.xaml
    /// </summary>
    public partial class MergeVariables : MergeSequencesBase
    {
        private MergeSharedData sharedData;

        public MergeVariables(MergeSharedData sharedData)
        {
            InitializeComponent();
            this.sharedData = sharedData;
            this.root.DataContext = this;
        }

        public override string Title => "Merge variables";

        public override string Description => "Select variables which you want to add to sequence or replace by variables from original sequence.";

        public IEnumerable<MTFVariable> OriginalComponents => sharedData.OriginalSequence.MTFVariables;

        public IEnumerable<MergeSetting> Variables => sharedData.MergeVariablesSetting;
    }
}
