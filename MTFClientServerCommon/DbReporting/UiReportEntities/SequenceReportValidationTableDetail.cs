using System;
using System.Collections.Generic;
using MTFClientServerCommon.MTFValidationTable;
using MTFCommon;

namespace MTFClientServerCommon.DbReporting.UiReportEntities
{
    [Serializable]
    public class SequenceReportValidationTableDetail
    {
        public string Name { get; set; }
        public List<SequenceReportValidationTableRowDetail> Rows { get; set; }
        public MTFValidationTableStatus TableStatus { get; set; }
        public MTFValidationTableExecutionMode ValidationMode { get; set; }
        public DateTime ValidationTime { get; set; }
        public List<string> Columns { get; set; }
    }
}