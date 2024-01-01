using System;

namespace MTFClientServerCommon.DbReporting.UiReportEntities
{
    [Serializable]
    public class RoundingRuleUi
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int Digits { get; set; }
    }
}