using System;

namespace MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport
{
    public class LineChartSeriesSettings : SummaryReportUiEntityBase
    {
        public string TableName { get; set; }
        public string RowName { get; set; }
        public string ColumnName { get; set; }
        public string Title { get; set; }
        public ChartColors? Color { get; set; }
        public int Index { get; set; }

        public override object Clone()
        {
            return new LineChartSeriesSettings
                   {
                       TableName = TableName,
                       RowName = RowName,
                       ColumnName = ColumnName,
                       Title = Title,
                       Color = Color,
                       Index = Index,
                   };
        }
    }
}