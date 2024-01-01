using System.Collections.Generic;
using MTFClientServerCommon.DbEntities.DbEnums;

namespace MTFClientServerCommon.DbEntities.DbReportViewer
{
    public class DbLineChartPanel : DbEntity, IDbReportSummaryPanel
    {
        public int Index { get; set; }
        public string Title { get; set; }
        public DbLegendPosition LegendPosition { get; set; }

        public List<DbLineChartSeries> Series { get; set; }

        public int SummaryReportId { get; set; }
        public SummaryReport SummaryReport { get; set; }
    }
}