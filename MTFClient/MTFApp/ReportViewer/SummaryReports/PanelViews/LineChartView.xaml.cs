using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using MTFApp.ReportViewer.Converters;
using MTFApp.UIHelpers;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;

namespace MTFApp.ReportViewer.SummaryReports.PanelViews
{
    /// <summary>
    /// Interaction logic for LineChartView.xaml
    /// </summary>
    public partial class LineChartView : MTFUserControl, INotifyPropertyChanged
    {
        private ChartColorToColorConverter colorConverter = new ChartColorToColorConverter();
        public LineChartView()
        {
            SeriesCollection = new SeriesCollection();
            InitializeComponent();
            lineChartViewRoot.DataContext = this;
        }

        #region LineChartPanel dependency property

        public static readonly DependencyProperty LineChartPanelProperty = DependencyProperty.Register(
            "LineChartPanel", typeof(LineChartPanel), typeof(LineChartView), new PropertyMetadata(default(LineChartPanel), LineChartPanelChanged));

        private static void LineChartPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var reportsOverviewView = d as LineChartView;
            reportsOverviewView?.InitGraph();
        }

        public LineChartPanel LineChartPanel
        {
            get => (LineChartPanel)GetValue(LineChartPanelProperty);
            set => SetValue(LineChartPanelProperty, value);
        }

        #endregion LineChartPanel dependency property

        #region ReportDataManager dependency property

        public static readonly DependencyProperty ReportDataManagerProperty = DependencyProperty.Register(
            "ReportDataManager", typeof(ReportDataManager), typeof(LineChartView), new PropertyMetadata(default(ReportDataManager), ReportDataManagerChanged));

        private static void ReportDataManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var reportsOverviewView = d as LineChartView;
            reportsOverviewView?.InitGraph();
        }

        public ReportDataManager ReportDataManager
        {
            get => (ReportDataManager) GetValue(ReportDataManagerProperty);
            set => SetValue(ReportDataManagerProperty, value);
        }

        #endregion ReportDataManager dependency property

        public SeriesCollection SeriesCollection { get; set; }

        public LegendLocation LegendLocation { get; set; }

        private void InitGraph()
        {
            if (LineChartPanel != null)
            {
                LegendLocation = LegendLocationConverter(LineChartPanel.LegendPosition);
            }

            if (ReportDataManager == null || LineChartPanel == null)
            {
                return;
            }
            
            SeriesCollection.Clear();

            if (LineChartPanel.Series!=null)
            {
                foreach (var seriesSettings in LineChartPanel.Series)
                {
                    AddLineSeries(seriesSettings.TableName, seriesSettings.RowName, seriesSettings.ColumnName, seriesSettings.Title, seriesSettings.Color);
                } 
            }
        }

        private async void AddLineSeries(string tableName, string rowName, string columnName, string title, ChartColors? color)
        {
            var data = await ReportDataManager.GetTableSummaryData(tableName, rowName, columnName);

            SeriesCollection.Add(
                new LineSeries
                {
                    Values = data == null ? new ChartValues<double>() : new ChartValues<double>(data),
                    PointGeometry = DefaultGeometries.None,
                    Title = string.IsNullOrEmpty(title) ? $"{tableName}.{rowName}.{columnName}" : title,
                    LineSmoothness = 0,
                    Stroke = color == null ? null : colorConverter.ConvertToBrush((ChartColors) color),
                    Fill = Brushes.Transparent,
                });
        }

        private LegendLocation LegendLocationConverter(LegendPosition legendPosition)
        {
            switch (legendPosition)
            {
                case LegendPosition.Top: return LegendLocation.Top;
                case LegendPosition.Bottom: return LegendLocation.Bottom;
                case LegendPosition.Left: return LegendLocation.Left;
                case LegendPosition.Right: return LegendLocation.Right;
                default: return LegendLocation.None;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
