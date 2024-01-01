namespace MTFClientServerCommon.DbEntities.DbReportViewer
{
    public class DbLineChartSeries : DbEntity
    {
        public string TableName { get; set; }
        public string RowName { get; set; }
        public string ColumnName { get; set; }
        public string Title { get; set; }
        public int? Color { get; set; }

        public int Index { get; set; }

        public int LineChartPanelId { get; set; }
        public DbLineChartPanel LineChartPanel { get; set; }
    }
}