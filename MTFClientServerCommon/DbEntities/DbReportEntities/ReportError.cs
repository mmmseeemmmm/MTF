using System;
using MTFClientServerCommon.DbEntities.DbEnums;

namespace MTFClientServerCommon.DbEntities.DbReportEntities
{
    public class ReportError : DbEntity
    {
        public DateTime TimeStamp { get; set; }
        public string ActivityName { get; set; }
        public string Message { get; set; }
        public DbErrorTypes ErrorType { get; set; }

        public int SequenceReportId { get; set; }
        public SequenceReport SequenceReport { get; set; }
    }
}