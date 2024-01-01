using System;

namespace MTFClientServerCommon.DbReporting.UiReportEntities
{
    [Serializable]
    public class ReportValidationTableColumnDetail
    {
        public string Value { get; set; }
        public bool Status { get; set; }
        public bool CanRound { get; set; }
    }
}