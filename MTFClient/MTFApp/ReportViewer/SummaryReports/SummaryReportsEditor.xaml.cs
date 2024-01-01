using System.Collections.ObjectModel;
using MTFApp.UIHelpers;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;

namespace MTFApp.ReportViewer.SummaryReports
{
    /// <summary>
    /// Interaction logic for SummaryReportsEditor.xaml
    /// </summary>
    public partial class SummaryReportsEditor : MTFUserControl
    {
        public SummaryReportsEditor(ObservableCollection<SummaryReportSettings> summaryReportSettings)
        {
            InitializeComponent();
            DataContext = new SummaryReportsEditorPresenter
            {
                SummaryReportSettings = summaryReportSettings
            };
        }

        public void Save()
        {
            ((SummaryReportsEditorPresenter)DataContext).Save();
        }
    }
}
