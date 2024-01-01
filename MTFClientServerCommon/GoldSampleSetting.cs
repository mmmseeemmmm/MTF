using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using MTFClientServerCommon.Constants;

namespace MTFClientServerCommon
{
    [Serializable]
    public class GoldSampleSetting : MTFDataTransferObject
    {
        public GoldSampleSetting()
        {

        }

        public GoldSampleSetting(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public void CreateGoldSampleSelector(IList<SequenceVariantGroup> variantGroups)
        {
            GoldSampleSelector = new SequenceVariant();
            var group = variantGroups.FirstOrDefault(g => g.Name == SequenceVariantConstants.GsDutCategory);
            var index = variantGroups.IndexOf(group);
            GoldSampleSelector.SetVariant(SequenceVariantConstants.GsDutCategory, index, group.Values.Where(v => v.Name == SequenceVariantConstants.GoldSample));
        }


        public GoldSampleValidationMode GoldSampleValidationMode
        {
            get { return GetProperty<GoldSampleValidationMode>(); }
            set { SetProperty(value); }
        }

        public int GoldSampleCount
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int GoldSampleMinutes
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public bool UseGoldSample
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public int GoldSampleAfterStartInMinutes
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public bool AllowGoldSampleAfterStart
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool AllowGoldSampleAfterInactivity
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public int GoldSampleAfterInactivityInMinutes
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public bool GoldSampleAfterVariantChanged
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public int VariantChangedAfterSamplesCount
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int GoldSampleRequiredAfterShiftStartInMinutes
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public MTFObservableCollection<GoldSampleShift> GoldSampleShifts
        {
            get { return GetProperty<MTFObservableCollection<GoldSampleShift>>(); }
            set { SetProperty(value); }
        }

        public string GoldSampleDataFile
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public SequenceVariant GoldSampleSelector
        {
            get { return GetProperty<SequenceVariant>(); }
            set { SetProperty(value); }
        }

        public bool MoreGoldSamples
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public List<GoldSampleShift> GetSortedShift()
        {
            var originalList = GoldSampleShifts.ToList();
            for (int i = 0; i < originalList.Count - 1; i++)
            {
                for (int j = 0; j < originalList.Count - i - 1; j++)
                {
                    if (originalList[j + 1] < originalList[j])
                    {
                        var tmp = originalList[j + 1];
                        originalList[j + 1] = originalList[j];
                        originalList[j] = tmp;
                    }
                }
            }
            return originalList;
        }
    }

    public enum GoldSampleValidationMode
    {
        [Description("Sample count")]
        Count,
        [Description("Time")]
        Time,
        [Description("Shift")]
        Shift
    }

    [Serializable]
    public class GoldSampleShift : MTFDataTransferObject
    {
        public GoldSampleShift()
        {
        }

        public GoldSampleShift(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public byte ShiftBeginningHour
        {
            get { return GetProperty<byte>(); }
            set { SetProperty(value); }
        }

        public byte ShiftBeginningMinute
        {
            get { return GetProperty<byte>(); }
            set { SetProperty(value); }
        }

        public static bool operator <(GoldSampleShift s1, GoldSampleShift s2)
        {
            return s1.ShiftBeginningHour < s2.ShiftBeginningHour  ||
                   (s1.ShiftBeginningHour == s2.ShiftBeginningHour && s1.ShiftBeginningMinute < s2.ShiftBeginningMinute);
        }

        public static bool operator >(GoldSampleShift s1, GoldSampleShift s2)
        {
            return s1.ShiftBeginningHour > s2.ShiftBeginningHour ||
                   (s1.ShiftBeginningHour == s2.ShiftBeginningHour && s1.ShiftBeginningMinute > s2.ShiftBeginningMinute);
        }
    }
}
