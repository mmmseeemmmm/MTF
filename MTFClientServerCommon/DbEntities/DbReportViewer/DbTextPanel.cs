using MTFClientServerCommon.DbEntities.DbEnums;

namespace MTFClientServerCommon.DbEntities.DbReportViewer
{
    public class DbTextPanel : DbEntity, IDbReportSummaryPanel
    {
        public int Index { get; set; }
        public string Text { get; set; }
        public int FontSize { get; set; }
        public DbTextAlignment TextAlignment { get; set; }

        public int SummaryReportId { get; set; }
        public SummaryReport SummaryReport { get; set; }
    }
}