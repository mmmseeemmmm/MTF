using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MTFApp.ReportViewer.ReportingWcf;
using MTFClientServerCommon.DbReporting.UiReportEntities;

namespace MTFApp.ReportViewer
{
    /// <summary>
    /// Interaction logic for ValidationTableReportControl.xaml
    /// </summary>
    public partial class ValidationTableReportControl : UserControl
    {
        public ValidationTableReportControl()
        {
            InitializeComponent();
        }

        #region Sequences dependency property
        public static readonly DependencyProperty ValidationTableProperty = DependencyProperty.Register("ValidationTable", typeof(SequenceReportValidationTableDetail), typeof(ValidationTableReportControl));

        public SequenceReportValidationTableDetail ValidationTable
        {
            get => (SequenceReportValidationTableDetail)GetValue(ValidationTableProperty);
            set => SetValue(ValidationTableProperty, value);
        }

        #endregion Sequences dependency property


        public IList<RoundingRuleUi> RoundingRules
        {
            get => (IList<RoundingRuleUi>)GetValue(RoundingRulesProperty);
            set => SetValue(RoundingRulesProperty, value);
        }

        public static readonly DependencyProperty RoundingRulesProperty =
            DependencyProperty.Register("RoundingRules", typeof(IList<RoundingRuleUi>), typeof(ValidationTableReportControl), new PropertyMetadata(null));


        private async void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as Grid;
            var validationTableRow = grid?.DataContext as SequenceReportValidationTableRowDetail;
            if (validationTableRow == null || validationTableRow.HasImage == false)
                return;

            Window w = new Window
            {
                Width = Application.Current.MainWindow.Width,
                Height = Application.Current.MainWindow.Height,
                Owner = Application.Current.MainWindow,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.Black,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
            };

            w.MouseLeftButtonUp += (o, args) => w.Close();

            w.Show();

            var imgData = await ReportingClient.GetReportingClient().GetReportImage(validationTableRow.ActualValue);
            w.Content = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Source = imgData,
            };
        }
    }
}
