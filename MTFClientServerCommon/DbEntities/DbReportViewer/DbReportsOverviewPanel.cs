namespace MTFClientServerCommon.DbEntities.DbReportViewer
{
    public class DbReportsOverviewPanel : DbEntity, IDbReportSummaryPanel
    {
        public int Index { get; set; }
        public string Title { get; set; }
        public int TimeQuantumInMinutes { get; set; }

        public int SummaryReportId { get; set; }
        public SummaryReport SummaryReport { get; set; }
    }
}