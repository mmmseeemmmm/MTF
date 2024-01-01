using System;

namespace MTFClientServerCommon.DbEntities.DbReportEntities
{
    public class ReportMessage : DbEntity
    {
        public ReportMessage()
        {
            TimeStamp = DateTime.Now;
        }

        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
        public int SequenceReportId { get; set; }
        public SequenceReport SequenceReport { get; set; }
    }
}