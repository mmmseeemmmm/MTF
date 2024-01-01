using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using LiveCharts;
using MTFApp.UIHelpers;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;
using MTFClientServerCommon.Helpers;

namespace MTFApp.ReportViewer.SummaryReports.PanelViews
{
    /// <summary>
    /// Interaction logic for ReportsOverviewView.xaml
    /// </summary>
    public partial class ReportsOverviewView : MTFUserControl, INotifyPropertyChanged
    {
        private string[] timeAxis;
        private IList<SummaryReportData> data;

        public ReportsOverviewView()
        {
            InitializeComponent();
            overViewRoot.DataContext = this;
        }

        #region ReportsOverviewPanel dependency property
        public static readonly DependencyProperty ReportsOverviewPanelProperty = DependencyProperty.Register(
            "ReportsOverviewPanel", typeof(ReportsOverviewPanel), typeof(ReportsOverviewView), new PropertyMetadata(default(ReportsOverviewPanel), ReportOverviewPanelChanged));

        private static void ReportOverviewPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var reportsOverviewView = d as ReportsOverviewView;
            reportsOverviewView?.InitGraph();
        }

        public ReportsOverviewPanel ReportsOverviewPanel
        {
            get => (ReportsOverviewPanel) GetValue(ReportsOverviewPanelProperty);
            set => SetValue(ReportsOverviewPanelProperty, value);
        }
        #endregion ReportsOverviewPanel dependency property

        #region ReportDataManager dependency property
        public static readonly DependencyProperty ReportDataManagerProperty = DependencyProperty.Register(
            "ReportDataManager", typeof(ReportDataManager), typeof(ReportsOverviewView), new PropertyMetadata(default(ReportDataManager), ReportDataManagerChanged));

        private static void ReportDataManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var reportsOverviewView = d as ReportsOverviewView;
            reportsOverviewView?.InitGraph();
        }

        public ReportDataManager ReportDataManager
        {
            get => (ReportDataManager) GetValue(ReportDataManagerProperty);
            set => SetValue(ReportDataManagerProperty, value);
        }
        #endregion ReportDataManager dependency property


        private async void InitGraph()
        {
            if (ReportDataManager == null || ReportsOverviewPanel == null)
            {
                return;
            }

            data = await ReportDataManager.GetReportsCountsPerTimeSlice(ReportsOverviewPanel.TimeQuantumInMinutes);
           
            TimeAxis = data?.Select(i => i.BeginInterval.ToString("g")).ToArray();
            OnPropertyChanged(nameof(OkValues));
            OnPropertyChanged(nameof(NokValues));
        }


        public string OkTitle => LanguageHelper.GetString("Execution_Status_Ok");
        public string NokTitle => LanguageHelper.GetString("Execution_Status_Nok");
        public ChartValues<int> OkValues => data == null ? null : new ChartValues<int>(data.Select(i => i.Ok));
        public ChartValues<int> NokValues => data == null ? null : new ChartValues<int>(data.Select(i => i.Nok));
        
        public string[] TimeAxis
        {
            get => timeAxis;
            set
            {
                timeAxis = value; 
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
