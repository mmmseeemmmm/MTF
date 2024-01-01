using System.ComponentModel.DataAnnotations.Schema;

namespace MTFClientServerCommon.DbEntities.DbReportEntities
{
    public class ReportRoundingRules : DbEntity
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int Digits { get; set; }
        public int SequenceRunId { get; set; }
        [ForeignKey("SequenceRunId")]
        public virtual ReportSequenceRun SequenceRun { get; set; }
    }
}