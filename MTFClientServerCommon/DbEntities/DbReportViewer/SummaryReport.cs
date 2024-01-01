using System;
using System.Collections.Generic;

namespace MTFClientServerCommon.DbEntities.DbReportViewer
{
    public class SummaryReport : DbEntity
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public int Index { get; set; }
        public bool IsPinned { get; set; }
        public bool CanModifyFilterInView { get; set; }
        public string SequenceName { get; set; }
        public string CycleName { get; set; }
        public DateTime? StartTimeFrom { get; set; }
        public DateTime? StartTimeTo { get; set; }
        public bool Last24Hours { get; set; }
        public bool LastWeek { get; set; }
        public bool? ReportStatus { get; set; }

        public List<DbLineChartPanel> LineChartPanels { get; set; }
        public List<DbReportsOverviewPanel> OverviewPanels { get; set; }
        public List<DbTextPanel> TextPanels { get; set; }
    }
}