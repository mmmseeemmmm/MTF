using System;

namespace MTFClientServerCommon.DbReporting.UiReportEntities
{
    [Serializable]
    public class SequenceReportMessageDetail
    {
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}