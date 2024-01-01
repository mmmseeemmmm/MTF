using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MTFApp.ReportViewer.ReportingWcf;
using MTFApp.UIHelpers;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;
using MTFClientServerCommon.Helpers;

namespace MTFApp.ReportViewer.SummaryReports
{
    class SummaryReportsEditorPresenter : PresenterBase
    {
        private ObservableCollection<SummaryReportSettings> summaryReportSettings = new ObservableCollection<SummaryReportSettings>();
        private List<SummaryReportSettings> removedItems = new List<SummaryReportSettings>();
        private SummaryReportSettings selectedSummaryReportSettings = null;
        private bool isSummaryReportSettingsEdit = false;
        private ReportingClient reportingClient;
        private IEnumerable<string> sequenceNames;

        public SummaryReportsEditorPresenter()
        {
            InitConnection();

            AddNewSummaryReportSettingsCommand = new Command(AddNewSummaryReportSettings);
            DeleteSummaryReportSettingsCommand = new Command(DeleteSummaryReportSettings, () => SelectedSummaryReportSettings != null);
            MoveUpSummaryReportSettingsCommand = new Command(MoveUpSummaryReportSettings, () => SelectedSummaryReportSettings != null);
            MoveDownSummaryReportSettingsCommand = new Command(MoveDownSummaryReportSettings, () => SelectedSummaryReportSettings != null);
            CopySummaryReportSettingsCommand = new Command(CopySummaryReportSettings, () => SelectedSummaryReportSettings != null);
        }

        public Command AddNewSummaryReportSettingsCommand { get; }
        public Command DeleteSummaryReportSettingsCommand { get; }
        public Command MoveUpSummaryReportSettingsCommand { get; }
        public Command MoveDownSummaryReportSettingsCommand { get; }
        public Command CopySummaryReportSettingsCommand { get; }

        public ObservableCollection<SummaryReportSettings> SummaryReportSettings
        {
            get => summaryReportSettings;
            set
            {
                summaryReportSettings = value; 
                NotifyPropertyChanged();
            }
        }

        public SummaryReportSettings SelectedSummaryReportSettings
        {
            get => selectedSummaryReportSettings;
            set
            {
                selectedSummaryReportSettings = value;
                NotifyPropertyChanged();
                DeleteSummaryReportSettingsCommand.RaiseCanExecuteChanged();
                MoveUpSummaryReportSettingsCommand.RaiseCanExecuteChanged();
                MoveDownSummaryReportSettingsCommand.RaiseCanExecuteChanged();
                CopySummaryReportSettingsCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsSummaryReportSettingsEdit
        {
            get => isSummaryReportSettingsEdit;
            set
            {
                isSummaryReportSettingsEdit = value;
                NotifyPropertyChanged();
            }
        }

        public IEnumerable<string> SequenceNames
        {
            get => sequenceNames;
            set
            {
                sequenceNames = value; 
                NotifyPropertyChanged();
            }
        }

        private async void InitConnection()
        {
            reportingClient = await GetReportClient();

            SequenceNames = await reportingClient.GetSequenceNames();
        }

        private Task<ReportingClient> GetReportClient()
        {
            return Task.Run(() => ReportingClient.GetReportingClient());
        }
        
        private void AddNewSummaryReportSettings()
        {
            SummaryReportSettings.Add(new SummaryReportSettings
                {
                    Name = LanguageHelper.GetString("New_Summary_Report"),
                    Index = SummaryReportSettings.Count,
                });

            SelectedSummaryReportSettings = SummaryReportSettings.Last();
            IsSummaryReportSettingsEdit = true;
        }

        private void DeleteSummaryReportSettings()
        {
            if (SelectedSummaryReportSettings == null)
            {
                return;
            }

            removedItems.Add(SelectedSummaryReportSettings);
            SummaryReportSettings.Remove(SelectedSummaryReportSettings);
            ReindexSettings();

            SelectedSummaryReportSettings = SummaryReportSettings.LastOrDefault();
            IsSummaryReportSettingsEdit = false;
        }

        private void MoveUpSummaryReportSettings()
        {
            if (SelectedSummaryReportSettings == null)
            {
                return;
            }

            var index = SummaryReportSettings.IndexOf(SelectedSummaryReportSettings);
            if (index > 0)
            {
                SummaryReportSettings.Move(index, index - 1);
            }

            ReindexSettings();
        }

        private void MoveDownSummaryReportSettings()
        {
            if (SelectedSummaryReportSettings == null)
            {
                return;
            }

            var index = SummaryReportSettings.IndexOf(SelectedSummaryReportSettings);
            if (index < SummaryReportSettings.Count - 1)
            {
                SummaryReportSettings.Move(index, index + 1);
            }

            ReindexSettings();
        }

        private void CopySummaryReportSettings()
        {
            if (SelectedSummaryReportSettings == null)
            {
                return;
            }
            SummaryReportSettings.Add(SelectedSummaryReportSettings.Clone() as SummaryReportSettings);

            ReindexSettings();

            SelectedSummaryReportSettings = SummaryReportSettings.LastOrDefault();
        }

        private void ReindexSettings()
        {
            for (int i = 0; i < SummaryReportSettings.Count; i++)
            {
                SummaryReportSettings[i].Index = i;
            }
        }

        public async void Save()
        {
            var client = reportingClient ?? await GetReportClient();

            if (removedItems.Count > 0)
            {
                await client.RemoveSummarySettings(removedItems);
            }

            if (SummaryReportSettings.Count>0)
            {
                var selectionIndex = SummaryReportSettings.IndexOf(SelectedSummaryReportSettings);

                var result = await client.SaveSummarySettings(SummaryReportSettings.ToList()); 

                SummaryReportSettings = new ObservableCollection<SummaryReportSettings>(result);

                if (selectionIndex>=0 && selectionIndex<SummaryReportSettings.Count)
                {
                    SelectedSummaryReportSettings = SummaryReportSettings[selectionIndex];
                }
            }

            removedItems.Clear();
        }
    }
}
