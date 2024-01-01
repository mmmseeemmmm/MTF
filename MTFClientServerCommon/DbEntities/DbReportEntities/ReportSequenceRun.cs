using System;
using System.Collections.Generic;

namespace MTFClientServerCommon.DbEntities.DbReportEntities
{
    public class ReportSequenceRun : DbEntity
    {
        public ReportSequenceRun()
        {
            StartTime = DateTime.Now;
            RoundingRules = new List<ReportRoundingRules>();
        }

        public string Machine { get; set; }
        public string WinUser { get; set; }
        public string SequenceName { get; set; }
        public DateTime StartTime { get; set; }

        public DateTime? StopTime { get; set; }

        public virtual List<SequenceReport> Reports { get; set; }

        public virtual List<ReportRoundingRules> RoundingRules { get; set; }
    }
}