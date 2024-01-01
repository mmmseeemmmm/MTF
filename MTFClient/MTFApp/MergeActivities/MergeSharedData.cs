using System.Collections.Generic;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.MergeActivities
{
    public class MergeSharedData
    {
        public IEnumerable<MergeActivitiesData<MTFSequenceClassInfo>> ComponentsMapping { get; set; }
        public IEnumerable<MergeActivitiesData<MTFVariable>> VariablesMapping { get; set; }
        public IEnumerable<MTFSequenceActivity> AllActivities { get; private set; }

        public MergeSharedData(IEnumerable<MTFSequenceActivity> activities)
        {
            AllActivities = MTFSequenceHelper.GetAllActivitiesAsPlainCollection(activities);
        }
    }

    public class MergeActivitiesData<T> : NotifyPropertyBase
        where T : MTFDataTransferObject
    {
        private T newData;
        public T OriginalData { get; set; }

        public T NewData
        {
            get { return newData; }
            set
            {
                newData = value;
                NotifyPropertyChanged();
            }
        }

        public IList<T> AllowedData { get; set; }

        public bool IsAssigned
        {
            get { return NewData != null; }
        }
    }

}
