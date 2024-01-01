using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;
using Microsoft.Win32;
using MTFApp.ReportViewer.ReportingWcf;
using MTFApp.UIHelpers;
using MTFClientServerCommon.DbReporting;
using MTFClientServerCommon.DbReporting.UiReportEntities;

namespace MTFApp.ReportViewer.ListOfReports
{
    class ListOfReportsPresenter : PresenterBase, IMainCommands
    {
        private const int SequenceReportPreviewsPageSize = 100;

        private readonly ObservableCollection<Command> mainCommands = new ObservableCollection<Command>();
        private Command backCommand;
        private Command exportCommand;
        private Command refreshCommand;
        private ReportingClient reportingClient;
        private bool showDetail;

        public ListOfReportsPresenter()
        {
            CreateCommands();
            CreateConnectionAsync();
        }

        private async void CreateConnectionAsync()
        {
            reportingClient = await Task.Run(() => ReportingClient.GetReportingClient());

            SequenceNames = await reportingClient.GetSequenceNames();
            Filter = new ReportFilter { Last24Hours = true, SequenceName = SequenceNames?.FirstOrDefault()};
            NotifyPropertyChanged(nameof(SequenceNames));
        }


        private SequenceReportDetail selectedReport;
        private ReportFilter filter;

        private void ReloadReports()
        {
            if (string.IsNullOrEmpty(Filter?.SequenceName))
            {
                return;
            }
            Reports = new SequenceReportPreviews(SequenceReportPreviewsPageSize, Filter);
            NotifyPropertyChanged(nameof(Reports));
        }

        private async void RefreshData()
        {
            SequenceNames = await reportingClient.GetSequenceNames();
            ReloadReports();
        }

        public ReportFilter Filter
        {
            get => filter;
            set
            {
                filter = value; 
                NotifyPropertyChanged();
            }
        }

        private void CreateCommands()
        {
            backCommand = new Command(Back, () => true, MTFIcons.Back) { Name = "MainCommand_Back" };
            exportCommand = new Command(Export, () => SelectedReport != null, MTFIcons.Export) { Name = "MainCommand_Export" };
            refreshCommand = new Command(RefreshData);
        }

        public List<string> SequenceNames { get; private set; }


        public IEnumerable<SequenceReportPreview> Reports { get; private set; }

        public bool ShowDetail
        {
            get => showDetail;
            set
            {
                showDetail = value;
                if (showDetail)
                {

                    mainCommands.Add(exportCommand);
                    mainCommands.Add(backCommand);
                }
                else
                {
                    mainCommands.Clear();
                }
                NotifyPropertyChanged();
            }
        }

        public SequenceReportDetail SelectedReport
        {
            get => selectedReport;
            set
            {
                selectedReport = value;
                NotifyPropertyChanged();
                exportCommand.RaiseCanExecuteChanged();
            }
        }


        private void Export()
        {
            try
            {
                ExportToPdf();
            }
            catch (Exception e)
            {
                MTFMessageBox.Show(string.Empty, e.Message, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
            }
        }

        private void ExportToPdf()
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*",
                FileName = SelectedReport.SequenceName
            };
            if (!string.IsNullOrEmpty(SelectedReport.CycleName))
            {
                sfd.FileName += " - " + selectedReport.CycleName;
            }
            if (sfd.ShowDialog() == true)
            {
                var data = reportingClient.GetPdfReport(SelectedReport.Id);
                using (var fileStream = File.Open(sfd.FileName, FileMode.Create))
                {
                    data.CopyTo(fileStream);
                }
                data.Close();
                data.Dispose();
            }
        }

        private void Back()
        {
            ShowDetail = false;
            SelectedReport = null;
        }

        public async void OpenDetail(SequenceReportPreview report)
        {
            if (report != null)
            {
                ShowDetail = true;

                SelectedReport = await reportingClient.GetDetail(report.Id);
            }
        }

        public Command RefreshCommand => refreshCommand;
        public IEnumerable<Command> Commands() => mainCommands;

    }
}
