using System.Windows;
using MTFApp.UIHelpers;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;

namespace MTFApp.ReportViewer.SummaryReports.PanelEditors
{
    /// <summary>
    /// Interaction logic for ReportsOverviewEditor.xaml
    /// </summary>
    public partial class ReportsOverviewEditor : MTFUserControl
    {
        public ReportsOverviewEditor()
        {
            InitializeComponent();
            overviewReportEditorRoot.DataContext = this;
        }

        public static readonly DependencyProperty ReportsOverviewPanelProperty = DependencyProperty.Register(
            "ReportsOverviewPanel", typeof(ReportsOverviewPanel), typeof(ReportsOverviewEditor), new PropertyMetadata(default(ReportsOverviewPanel)));

        public ReportsOverviewPanel ReportsOverviewPanel
        {
            get => (ReportsOverviewPanel) GetValue(ReportsOverviewPanelProperty);
            set => SetValue(ReportsOverviewPanelProperty, value);
        }
    }
}
