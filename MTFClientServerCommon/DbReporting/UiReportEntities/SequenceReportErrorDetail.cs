using System;
using MTFCommon;

namespace MTFClientServerCommon.DbReporting.UiReportEntities
{
    [Serializable]
    public class SequenceReportErrorDetail
    {
        public string ActivityName { get; set; }
        public ErrorTypes ErrorType { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}