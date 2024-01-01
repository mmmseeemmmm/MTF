using System;

namespace MTFClientServerCommon.DbReporting
{
    [Serializable]
    public class ReportFilter : ICloneable
    {
        public string SequenceName { get; set; }
        public string CycleName { get; set; }
        public DateTime? StartTimeFrom { get; set; }
        public DateTime? StartTimeTo { get; set; }
        public bool Last24Hours{ get; set; }
        public bool LastWeek{ get; set; }
        public bool? ReportStatus{ get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }

        public object Clone()
        {
            return new ReportFilter
            {
                SequenceName = SequenceName,
                CycleName = CycleName,
                StartTimeFrom = StartTimeFrom,
                StartTimeTo = StartTimeTo,
                Last24Hours = Last24Hours,
                LastWeek = LastWeek,
                ReportStatus = ReportStatus,
                PageSize = PageSize,
                PageNumber = PageNumber,
            };
        }
    }
}
