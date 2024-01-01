using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using MTFApp.ReportViewer.ReportingWcf;
using MTFApp.UIHelpers;
using MTFClientServerCommon.DbReporting;

namespace MTFApp.ReportViewer.Controls
{
    /// <summary>
    /// Interaction logic for ReportFilterControl.xaml
    /// </summary>
    public partial class ReportFilterControl : MTFUserControl, INotifyPropertyChanged
    {
        private string selectedSequence;
        private string cycleName;
        private bool last24Hours;
        private bool lastWeek;
        private DateTime startDateTimeFrom;
        private DateTime startDateTimeTo;
        private bool? reportStatus;

        private Command cleanFilterCommand;

        public ReportFilterControl()
        {
            startDateTimeFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            startDateTimeTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

            InitializeComponent();

            cleanFilterCommand = new Command(CreateDefaultFilter);
        }

        #region ReportFilter Dependency property
        public static readonly DependencyProperty ReportFilterProperty = DependencyProperty.Register(
            "ReportFilter", typeof(ReportFilter), typeof(ReportFilterControl), new PropertyMetadata(default(ReportFilter), ReportFilterChanged));

        private static void ReportFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newFilter = e.NewValue as ReportFilter;
            var reportFilterControl = d as ReportFilterControl;
            if (newFilter != null)
            {
                reportFilterControl?.CreateFilter(newFilter);
            }
            else
            {
                reportFilterControl?.CreateDefaultFilter();
            }
        }
       
        public ReportFilter ReportFilter
        {
            get => (ReportFilter) GetValue(ReportFilterProperty);
            set => SetValue(ReportFilterProperty, value);
        }
        #endregion ReportFilter Dependency property

        #region SequenceNames Dependency property
        public static readonly DependencyProperty SequenceNamesProperty = DependencyProperty.Register(
            "SequenceNames", typeof(IEnumerable<string>), typeof(ReportFilterControl), new PropertyMetadata(default(IEnumerable<string>)));

        public IEnumerable<string> SequenceNames
        {
            get => (IEnumerable<string>) GetValue(SequenceNamesProperty);
            set => SetValue(SequenceNamesProperty, value);
        }
        #endregion SequenceNames Dependency property

        #region ShowItemsCount Dependency property
        public static readonly DependencyProperty ShowItemsCountProperty = DependencyProperty.Register(
            "ShowItemsCount", typeof(bool), typeof(ReportFilterControl), new PropertyMetadata(false));

        public bool ShowItemsCount
        {
            get => (bool) GetValue(ShowItemsCountProperty);
            set => SetValue(ShowItemsCountProperty, value);
        }
        #endregion ShowItemsCount Dependency property

        #region RefreshCommand Dependency property
        public static readonly DependencyProperty RefreshCommandProperty = DependencyProperty.Register(
            "RefreshCommand", typeof(Command), typeof(ReportFilterControl), new PropertyMetadata(default(Command)));

        public Command RefreshCommand
        {
            get => (Command) GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }
        #endregion RefreshCommand Dependency property

        public Command CleanFilterCommand => cleanFilterCommand;

        public string SelectedSequence
        {
            get => selectedSequence;
            set
            {
                selectedSequence = value;
                UpdateFilter();
                NotifyPropertyChanged();
            }
        }

        public string CycleName
        {
            get => cycleName;
            set
            {
                cycleName = value;
                UpdateFilter();
                NotifyPropertyChanged();
            }
        }

        public bool Last24Hours
        {
            get => last24Hours;
            set
            {
                last24Hours = value;
                if (last24Hours)
                {
                    lastWeek = false;
                    NotifyPropertyChanged(nameof(LastWeek));
                }
                UpdateFilter();
                NotifyPropertyChanged();
            }
        }

        public bool LastWeek
        {
            get => lastWeek;
            set
            {
                lastWeek = value;
                if (lastWeek)
                {
                    last24Hours = false;
                    NotifyPropertyChanged(nameof(Last24Hours));
                }
                UpdateFilter();
                NotifyPropertyChanged();
            }
        }

        public DateTime StartDateTimeFrom
        {
            get => startDateTimeFrom;
            set
            {
                startDateTimeFrom = value;
                UpdateFilter();
                NotifyPropertyChanged();
            }
        }

        public DateTime StartDateTimeTo
        {
            get => startDateTimeTo;
            set
            {
                startDateTimeTo = value;
                UpdateFilter();
                NotifyPropertyChanged();
            }
        }

        public bool? ReportStatus
        {
            get => reportStatus;
            set
            {
                reportStatus = value;
                UpdateFilter();
                NotifyPropertyChanged();
            }
        }

        public int ItemsCount { get; private set; }

        private void CreateDefaultFilter()
        {
            UpdateFilterDisabled = true;
            SelectedSequence = SequenceNames?.FirstOrDefault();
            CycleName = string.Empty;
            StartDateTimeFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            StartDateTimeTo = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            Last24Hours = true;
            LastWeek = false;
            ReportStatus = null;
            UpdateFilterDisabled = false;

            UpdateFilter();
        }

        private void CreateFilter(ReportFilter filter)
        {
            UpdateFilterDisabled = true;
            SelectedSequence = filter.SequenceName;
            CycleName = filter.CycleName;
            StartDateTimeFrom = filter.StartTimeFrom ?? DateTime.Now;
            StartDateTimeTo = filter.StartTimeTo ?? DateTime.Now;
            Last24Hours = filter.Last24Hours;
            LastWeek = filter.LastWeek;
            ReportStatus = filter.ReportStatus;
            UpdateFilterDisabled = false;

            UpdateFilter();
        }

        private bool UpdateFilterDisabled = false;

        private async void UpdateFilter()
        {
            if (UpdateFilterDisabled || ReportFilter == null)
            {
                return;
            }

            ReportFilter.SequenceName = SelectedSequence;
            ReportFilter.CycleName = CycleName;
            ReportFilter.StartTimeFrom = (Last24Hours || LastWeek) ? (DateTime?) null : StartDateTimeFrom;
            ReportFilter.StartTimeTo = (Last24Hours || LastWeek) ? (DateTime?) null : StartDateTimeTo;
            ReportFilter.Last24Hours = Last24Hours;
            ReportFilter.LastWeek = LastWeek;
            ReportFilter.ReportStatus = ReportStatus;
            
            NotifyPropertyChanged(nameof(ItemsCount));

            if (!string.IsNullOrEmpty(ReportFilter.SequenceName))
            {
                ItemsCount = await ReportingClient.GetReportingClient().GetReportsCount(ReportFilter);
                NotifyPropertyChanged(nameof(ItemsCount));
            }

            RefreshCommand?.Execute(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
