using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MTFApp.ReportViewer.ReportingWcf;
using MTFClientServerCommon.DbReporting.UiReportEntities;
using MTFCommon;

namespace MTFApp.ReportViewer
{
    /// <summary>
    /// Interaction logic for ReportDetailControl.xaml
    /// </summary>
    public partial class ReportDetailControl : UserControl
    {
        public ReportDetailControl()
        {
            InitializeComponent();
            ReportDetailRootGrid.DataContext = this;
        }
        
        public SequenceReportDetail ReportDetail
        {
            get { return (SequenceReportDetail)GetValue(ReportDetailProperty); }
            set { SetValue(ReportDetailProperty, value); }
        }

        public static readonly DependencyProperty ReportDetailProperty =
            DependencyProperty.Register("ReportDetail", typeof(SequenceReportDetail), typeof(ReportDetailControl), new PropertyMetadata(OnReportDetailChanged));

        private static void OnReportDetailChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ReportDetailControl;
            if (e.NewValue != e.OldValue)
            {
                control?.OnReportDetailChanged(e);
            }
        }

        private void OnReportDetailChanged(DependencyPropertyChangedEventArgs e)
        {
            PrepareGraphicalUIImages();
            PrepareErrorImages();
        }

        private void PrepareGraphicalUIImages()
        {
            GraphicalUIImages.Images.Clear();
            if (ReportDetail == null || string.IsNullOrEmpty(ReportDetail.GraphicalViews))
            {
                return;
            }

            var imageNames = ReportDetail.GraphicalViews.Split('|').ToList();

            Task.Run(() =>
            {
                imageNames.ForEach(i =>
                {
                    var img = ReportingClient.GetReportingClient().GetGraphicalUIImage(i).Result;
                    Application.Current.Dispatcher.Invoke(() => GraphicalUIImages.Images.Add(img));
                });
            });

        }


        private void PrepareErrorImages()
        {
            ErrorImages.Images.Clear();
            if (ReportDetail == null)
            {
                return;
            }
            var imageNames = ReportDetail.ValidationTables.SelectMany(t => t.Rows).Where(r => r.HasImage == true && r.Status == MTFValidationTableStatus.Nok).Select(r => r.ActualValue).ToList();
            Task.Run(() =>
            {
                imageNames.ForEach(i =>
                {
                    var img = ReportingClient.GetReportingClient().GetReportImage(i).Result;
                    Application.Current.Dispatcher.Invoke(() => ErrorImages.Images.Add(img));
                });
            });
        }
    }
}
