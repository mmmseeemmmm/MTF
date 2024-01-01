using System;

namespace MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport
{
    [Serializable]
    public class SummaryReportData
    {
        public int Count { get; set; }
        public int Ok { get; set; }
        public int Nok { get; set; }
        public DateTime BeginInterval { get; set; }
    }
}
