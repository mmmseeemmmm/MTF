namespace MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport
{
    public class ReportsOverviewPanel : PanelBase
    {
        public string Title { get; set; }
        public int TimeQuantumInMinutes { get; set; }
        public override object Clone()
        {
            return new ReportsOverviewPanel
            {
                Title = Title,
                TimeQuantumInMinutes = TimeQuantumInMinutes
            };
        }
    }
}
