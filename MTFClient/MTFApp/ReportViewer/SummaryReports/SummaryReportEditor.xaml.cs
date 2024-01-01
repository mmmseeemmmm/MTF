using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using MTFApp.ReportViewer.ReportingWcf;
using MTFApp.UIHelpers;
using MTFClientServerCommon.DbReporting.UiReportEntities;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;

namespace MTFApp.ReportViewer.SummaryReports
{
    /// <summary>
    /// Interaction logic for SummaryReportEditor.xaml
    /// </summary>
    public partial class SummaryReportEditor : MTFUserControl, INotifyPropertyChanged
    {
        private Command refreshDataCommand;
        private Command removePanelCommand;
        private Command movePanelUpCommand;
        private Command movePanelDownCommand;
        private Command copyPanelCommand;
        private Command addTextPanelCommand;
        private Command addReportsOverviewPanelCommand;
        private Command addLineChartPanelCommand;
        private IEnumerable<string> tableNames;
        private IEnumerable<string> rowNames;

        public SummaryReportEditor()
        {
            InitializeComponent();
            summaryReportEditorRoot.DataContext = this;

            refreshDataCommand = new Command(RefreshData);
            removePanelCommand = new Command(RemovePanel);
            movePanelUpCommand = new Command(MovePanelUp);
            movePanelDownCommand = new Command(MovePanelDown);
            copyPanelCommand = new Command(CopyPanel);
            addTextPanelCommand = new Command(AddTextPanel);
            addReportsOverviewPanelCommand = new Command(AddReportsOverviewPanel);
            addLineChartPanelCommand = new Command(AddLineChartPanel);
        }

        #region SummaryReportSettings dependency property
        public static readonly DependencyProperty SummaryReportSettingsProperty = DependencyProperty.Register(
            "SummaryReportSettings", typeof(SummaryReportSettings), typeof(SummaryReportEditor), new PropertyMetadata(default(SummaryReportSettings)));

        public SummaryReportSettings SummaryReportSettings
        {
            get => (SummaryReportSettings) GetValue(SummaryReportSettingsProperty);
            set => SetValue(SummaryReportSettingsProperty, value);
        }
        #endregion SummaryReportSettings dependency property

        #region SequenceNames dependency property
        public static readonly DependencyProperty SequenceNamesProperty = DependencyProperty.Register(
            "SequenceNames", typeof(IEnumerable<string>), typeof(SummaryReportEditor), new PropertyMetadata(default(IEnumerable<string>)));

        public IEnumerable<string> SequenceNames
        {
            get => (IEnumerable<string>) GetValue(SequenceNamesProperty);
            set => SetValue(SequenceNamesProperty, value);
        }
        #endregion SequenceNames dependency property

        public Command RefreshDataCommand => refreshDataCommand;
        public Command RemovePanelCommand => removePanelCommand;
        public Command MovePanelUpCommand => movePanelUpCommand;
        public Command MovePanelDownCommand => movePanelDownCommand;
        public Command CopyPanelCommand => copyPanelCommand;
        public Command AddTextPanelCommand => addTextPanelCommand;
        public Command AddReportsOverviewPanelCommand => addReportsOverviewPanelCommand;
        public Command AddLineChartPanelCommand => addLineChartPanelCommand;

        public IEnumerable<ValidationTableInfo> ValidationTableInfos { get; private set; }

        private void RefreshData()
        {
            if (string.IsNullOrEmpty(SummaryReportSettings?.Filter?.SequenceName))
            {
                return;
            }

            var report = new SequenceReportPreviews(1, SummaryReportSettings.Filter).FirstOrDefault();
            if (report == null)
            {
                return;
            }

            ValidationTableInfos = ParseValidationTableData(ReportingClient.GetReportingClient().GetDetail(report.Id).Result);
            OnPropertyChanged(nameof(ValidationTableInfos));
        }

        private IEnumerable<ValidationTableInfo> ParseValidationTableData(SequenceReportDetail report) =>
            report.ValidationTables.Select(
                t => new ValidationTableInfo
                {
                    Name = t.Name,
                    Rows = t.Rows.Where(r => !r.HasImage == true).Select(r => new ValidationTableRowInfo
                    {
                        Name = r.Name,
                        Columns = new[]
                        {
                            new ValidationTableColumnInfo {Name = "ActualValue"},
                            new ValidationTableColumnInfo {Name = "MinValue"},
                            new ValidationTableColumnInfo {Name = "MaxValue"}
                        },
                    })
                });

        private void RemovePanel(object param)
        {
            if (param is PanelBase panel)
            {
                panel.IsDeleted = true;
            }
        }

        private void MovePanelUp(object param)
        {
            var panel = param as PanelBase;
            if (panel == null)
            {
                return;
            }

            var index = SummaryReportSettings.Panels.IndexOf(panel);
            if (index > 0)
            {
                SummaryReportSettings.Panels.Move(index, index - 1);
            }
        }

        private void MovePanelDown(object param)
        {
            var panel = param as PanelBase;
            if (panel == null)
            {
                return;
            }

            var index = SummaryReportSettings.Panels.IndexOf(panel);
            if (index < SummaryReportSettings.Panels.Count - 1)
            {
                SummaryReportSettings.Panels.Move(index, index + 1);
            }

        }

        private void CopyPanel(object param)
        {
            var panel = param as PanelBase;
            if (panel == null)
            {
                return;
            }

            SummaryReportSettings.Panels.Add(panel.Clone() as PanelBase);
        }

        private void AddTextPanel()
        {
            SummaryReportSettings.Panels.Add(new TextPanel{FontSize = 10});
        }

        private void AddReportsOverviewPanel()
        {
            SummaryReportSettings.Panels.Add(new ReportsOverviewPanel {TimeQuantumInMinutes = 60});
        }

        private void AddLineChartPanel()
        {
            SummaryReportSettings.Panels.Add(new LineChartPanel{Series = new ObservableCollection<LineChartSeriesSettings>()});
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
