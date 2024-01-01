using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport
{
    public class LineChartPanel : PanelBase
    {
        private ObservableCollection<LineChartSeriesSettings> series;
        public string Title { get; set; }
        public LegendPosition LegendPosition { get; set; }

        public ObservableCollection<LineChartSeriesSettings> Series
        {
            get => series;
            set
            {
                series = value;
                NotifyPropertyChanged();
            }
        }

        public override object Clone()
        {
            return new LineChartPanel
            {
                Title = Title,
                LegendPosition = LegendPosition,
                Series = new ObservableCollection<LineChartSeriesSettings>(Series.Select(s=>s.Clone() as LineChartSeriesSettings)),
            };
        }
    }

    public enum LegendPosition
    {
        [Description("LineChartPanel_LegendPosition_None")]
        None,
        [Description("LineChartPanel_LegendPosition_Left")]
        Left,
        [Description("LineChartPanel_LegendPosition_Right")]
        Right,
        [Description("LineChartPanel_LegendPosition_Top")]
        Top,
        [Description("LineChartPanel_LegendPosition_Bottom")]
        Bottom
    }
}
