using System;
using System.Collections.Generic;
using MTFCommon;

namespace MTFClientServerCommon.DbReporting.UiReportEntities
{
    [Serializable]
    public class SequenceReportValidationTableRowDetail
    {
        public string ActualValue { get; set; }
        public string RoundedValue { get; set; }
        public double GsPercentage { get; set; }
        public bool? HasImage { get; set; }
        public bool? IsHidden { get; set; }
        public string Name { get; set; }
        public int NumberOfRepetition { get; set; }
        public MTFValidationTableStatus Status { get; set; }
        public List<ReportValidationTableColumnDetail> Columns { get; set; }
    }
}