using MTFClientServerCommon;
using System.Collections.ObjectModel;
using MTFApp.UIHelpers;

namespace MTFApp.MergeSequences
{
    public class MergeSharedData
    {
        public MTFSequence OriginalSequence { get; set; }

        public MTFSequence MergedSequence { get; set; }

        private ObservableCollection<MergeSetting> mergeComponentsSetting;

        public ObservableCollection<MergeSetting> MergeComponentsSetting
        {
            get => mergeComponentsSetting;
            set => mergeComponentsSetting = value;
        }

        private ObservableCollection<MergeSetting> mergeVariablesSetting;

        public ObservableCollection<MergeSetting> MergeVariablesSetting
        {
            get => mergeVariablesSetting;
            set => mergeVariablesSetting = value;
        }

        public MergeSharedData(MTFSequence originalSequence, MTFSequence mergedSequence)
        {
            this.OriginalSequence = originalSequence;
            this.MergedSequence = mergedSequence;
            this.mergeComponentsSetting = new ObservableCollection<MergeSetting>();
            this.mergeVariablesSetting = new ObservableCollection<MergeSetting>();
            foreach (var item in MergedSequence.MTFSequenceClassInfos)
            {
                this.mergeComponentsSetting.Add(new MergeSetting(item, false));
            }
            foreach (var item in MergedSequence.MTFVariables)
            {
                this.mergeVariablesSetting.Add(new MergeSetting(item, false));
            }
        }



    }

    public class MergeSetting : NotifyPropertyBase
    {
        public MTFDataTransferObject MergedComponent { get; set; }
        private bool replace;

        public bool Replace
        {
            get => replace;
            set
            {
                replace = value;
                NotifyPropertyChanged();
            }
        }
        public MTFDataTransferObject OriginalComponent { get; set; }

        public MergeSetting(MTFDataTransferObject mergedComponent, bool replace)
        {
            this.MergedComponent = mergedComponent;
            this.replace = replace;
        }
    }
}
