namespace MTFClientServerCommon.DbEntities.DbReportViewer
{
    public interface IDbReportSummaryPanel
    {
        int Id { get; set; }
        int Index { get; set; }
        int SummaryReportId { get; set; }
        SummaryReport SummaryReport { get; set; }
    }
}