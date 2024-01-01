using System;
using System.Runtime.Serialization;

namespace MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport
{
    [Serializable]
    [KnownType(typeof(LineChartPanel))]
    [KnownType(typeof(ReportsOverviewPanel))]
    [KnownType(typeof(TextPanel))]
    public abstract class PanelBase : SummaryReportUiEntityBase
    {
        public int Index { get; set; }
    }
}