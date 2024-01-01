using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using AutomotiveLighting.MTFCommon;
using MTFApp.ReportViewer.ListOfReports;
using MTFApp.ReportViewer.ReportingWcf;
using MTFApp.ReportViewer.SummaryReports;
using MTFApp.UIHelpers;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;

namespace MTFApp.ReportViewer
{
    public class ReportViewerPresenter : PresenterBase, IMainCommands
    {
        private Command switchToListOfReportsCommand;
        private Command switchToSummaryReportsEditorCommand;
        private Command saveSummaryReportsCommand;

        private readonly ObservableCollection<Command> mainCommands = new ObservableCollection<Command>();
        private UserControl mainControl;
        private ListOfReportsControl listOfReportsControl;
        private SummaryReportsEditor summaryReportsEditorControl;
        private ReportingClient reportingClient;
        private readonly ObservableCollection<SummaryReportSettings> summaryReportSettings = new ObservableCollection<SummaryReportSettings>();


        public ReportViewerPresenter()
        {
            CreateCommands();

            LoadSummarySettingsAsync();

            SwitchToListOfReports();
        }

        private Task<ReportingClient> GetReportClient()
        {
            return Task.Run(() => ReportingClient.GetReportingClient());
        }

        private async void LoadSummarySettingsAsync()
        {
            reportingClient = await GetReportClient();

            foreach (var summaryReport in await reportingClient.GetSummaryReports())
            {
                summaryReportSettings.Add(summaryReport);
            }

            GenerateCommands();
        }

        public UserControl MainControl
        {
            get => mainControl;
            set
            {
                UnhookMainCommandsEvent();
                mainControl = value;
                HookMainCommandEvent();
                GenerateCommands();

                NotifyPropertyChanged();
            }
        }

        private void UnhookMainCommandsEvent()
        {
            if ((mainControl?.DataContext as IMainCommands)?.Commands() is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)((IMainCommands)mainControl.DataContext).Commands()).CollectionChanged -= InnerControlCommandChanged;
            }
        }

        private void HookMainCommandEvent()
        {
            if ((mainControl?.DataContext as IMainCommands)?.Commands() is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)((IMainCommands)mainControl.DataContext).Commands()).CollectionChanged += InnerControlCommandChanged;
            }
        }

        private void InnerControlCommandChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            GenerateCommands();
        }

        private void CreateCommands()
        {
            switchToListOfReportsCommand = new ToggleCommand(SwitchToListOfReports, () => true, MTFIcons.Test, nameof(IsListOfReportsActive)) { Name = "MainCommand_Reports" };
            switchToSummaryReportsEditorCommand = new ToggleCommand(SwitchToSummaryReportsEditor, () => true, MTFIcons.Graph, nameof(IsSummaryReportsEditorActive)) { Name = "MainCommand_Summary_Reports" };
            saveSummaryReportsCommand = new Command(SaveSummaryReports) { Icon = MTFIcons.SaveFile, Name = "MainCommand_Save" };
        }

        private void SwitchToListOfReports()
        {
            if (listOfReportsControl == null)
            {
                listOfReportsControl = new ListOfReportsControl();
            }

            MainControl = listOfReportsControl;
        }

        public bool IsListOfReportsActive => MainControl is ListOfReportsControl;

        private void SwitchToSummaryReportsEditor()
        {
            if (summaryReportsEditorControl == null)
            {
                summaryReportsEditorControl = new SummaryReportsEditor(summaryReportSettings);
            }

            MainControl = summaryReportsEditorControl;
        }

        public bool IsSummaryReportsEditorActive => MainControl is SummaryReportsEditor;

        public IEnumerable<Command> Commands() => mainCommands;

        private void SaveSummaryReports()
        {
            summaryReportsEditorControl?.Save();
            
            GenerateCommands();
        }

        private void ShowSummaryReport(SummaryReportSettings reportSettings)
        {
            MainControl = new SummaryReportViewer { SummaryReportSettings = reportSettings };
        }

        private void GenerateCommands()
        {
            mainCommands.Clear();
            mainCommands.Add(switchToListOfReportsCommand);
            GenerateSummaryReportViewsCommands();
            mainCommands.Add(switchToSummaryReportsEditorCommand);

            if (IsSummaryReportsEditorActive)
            {
                mainCommands.Add(saveSummaryReportsCommand);
            }

            if (MainControl?.DataContext is IMainCommands)
            {
                foreach (var command in ((IMainCommands)MainControl.DataContext).Commands())
                {
                    mainCommands.Add(command);
                }
            }

            NotifyPropertyChanged(nameof(IsListOfReportsActive));
        }

        private void GenerateSummaryReportViewsCommands()
        {
            foreach (var command in summaryReportSettings.Where(r => r.IsPinned).Select(r => new Command(() => ShowSummaryReport(r)) { Icon = MTFIcons.Graph, Name = r.Name }))
            {
                mainCommands.Add(command);
            }
        }
    }
}
