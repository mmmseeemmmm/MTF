using System.Collections.Generic;
using System.Linq;

namespace MTFApp.MergeSequences
{
    /// <summary>
    /// Interaction logic for MergeSummary.xaml
    /// </summary>
    public partial class MergeSummary : MergeSequencesBase
    {
        private MergeSharedData sharedData;

        public MergeSummary(MergeSharedData sharedData)
        {
            InitializeComponent();
            this.sharedData = sharedData;
            this.root.DataContext = this;
        }

        public override string Title => "Merge summary";

        public override string Description => "Summary of merge.";

        public override bool IsLastControl => true;

        public IEnumerable<MergeSetting> AddedComponents
        {
            get
            {
                return sharedData.MergeComponentsSetting.Where(x => !x.Replace);
            }
        }

        public IEnumerable<MergeSetting> ReplacedComponents
        {
            get
            {
                return sharedData.MergeComponentsSetting.Where(x => x.Replace);
            }
        }

        public IEnumerable<MergeSetting> AddedVariables
        {
            get
            {
                return sharedData.MergeVariablesSetting.Where(x => !x.Replace);
            }
        }

        public IEnumerable<MergeSetting> ReplacedVariables
        {
            get
            {
                return sharedData.MergeVariablesSetting.Where(x => x.Replace);
            }
        }

        protected override void OnShow()
        {
            NotifyPropertyChanged("AddedComponents");
            NotifyPropertyChanged("ReplacedComponents");
            NotifyPropertyChanged("AddedVariables");
            NotifyPropertyChanged("ReplacedVariables");
        }
    }
}
