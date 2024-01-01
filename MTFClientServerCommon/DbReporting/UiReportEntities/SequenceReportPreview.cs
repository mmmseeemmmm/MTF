using System;
using MTFClientServerCommon.Helpers;

namespace MTFClientServerCommon.DbReporting.UiReportEntities
{
    [Serializable]
    public class SequenceReportPreview
    {
        public int Id { get; set; }
        public string CycleName { get; set; }
        public bool? SequenceStatus { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? StopTime { get; set; }
        public string GraphicalViews { get; set; }
        public string VariantGsDut { get; set; }
        public string VariantLightDistribution { get; set; }
        public string VariantMountingSide { get; set; }
        public string VariantVersion { get; set; }


        public double Duration
        {
            get
            {
                if (StopTime.HasValue)
                {
                    return (StopTime.Value - StartTime).TotalMilliseconds;
                }
                return 0;
            }

        }

        public string SequenceVariant =>
            SequenceVariantHelper.CombineVariant(VariantVersion, VariantLightDistribution, VariantMountingSide, VariantGsDut);
        
    }
}
