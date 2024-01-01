using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MTFApp.UIHelpers;
using MTFClientServerCommon.DbReporting;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;

namespace MTFApp.ReportViewer.SummaryReports
{
    /// <summary>
    /// Interaction logic for SummaryReportViewer.xaml
    /// </summary>
    public partial class SummaryReportViewer : MTFUserControl, INotifyPropertyChanged
    {
        private ReportFilter filter;
        private IEnumerable<string> filterSequenceNames;
        private Command refreshDataCommand;
        private ReportDataManager reportDataManager;

        public SummaryReportViewer()
        {
            refreshDataCommand = new Command(RefreshData);
            InitializeComponent();
            reportViewerRoot.DataContext = this;
        }

        #region SummaryReportSettings dependency property
        public static readonly DependencyProperty SummaryReportSettingsProperty = DependencyProperty.Register(
            "SummaryReportSettings", typeof(SummaryReportSettings), typeof(SummaryReportViewer), new PropertyMetadata(default(SummaryReportSettings), SummaryReportSettingsChanged));

        private static void SummaryReportSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var summaryReportViewer = d as SummaryReportViewer;
            summaryReportViewer?.SummaryReportSettingsChanged(e.NewValue as SummaryReportSettings);
        }


        public SummaryReportSettings SummaryReportSettings
        {
            get => (SummaryReportSettings)GetValue(SummaryReportSettingsProperty);
            set => SetValue(SummaryReportSettingsProperty, value);
        }
        #endregion SummaryReportSettings dependency property

        public ReportFilter Filter
        {
            get => filter;
            set
            {
                filter = value;
                FilterSequenceNames = new[] {Filter?.SequenceName};
                RefreshData();

                OnPropertyChanged();
            }
        }

        public IEnumerable<string> FilterSequenceNames
        {
            get => filterSequenceNames;
            set
            {
                filterSequenceNames = value; 
                OnPropertyChanged();
            }
        }

        public ReportDataManager ReportDataManager
        {
            get => reportDataManager;
            set
            {
                reportDataManager = value;
                OnPropertyChanged();
            }
        }

        public Command RefreshDataCommand => refreshDataCommand;

        private bool dontRefreshData = false;
        private void RefreshData()
        {
            if (dontRefreshData)
            {
                return;
            }

            ReportDataManager = new ReportDataManager(Filter);
        }

        private void SummaryReportSettingsChanged(SummaryReportSettings summaryReportSettings)
        {
            if (summaryReportSettings == null)
            {
                return;
            }

            dontRefreshData = summaryReportSettings.CanModifyFilterInView;
            Filter = summaryReportSettings.Filter?.Clone() as ReportFilter;
            dontRefreshData = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
