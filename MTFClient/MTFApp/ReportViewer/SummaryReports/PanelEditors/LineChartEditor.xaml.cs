using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using MTFApp.UIHelpers;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;
using MTFClientServerCommon.Helpers;

namespace MTFApp.ReportViewer.SummaryReports.PanelEditors
{
    /// <summary>
    /// Interaction logic for LineChartEditor.xaml
    /// </summary>
    public partial class LineChartEditor : MTFUserControl
    {
        private Command addLineSeriesSettingsCommand;
        private Command moveLineSeriesSettingsUpCommand;
        private Command moveLineSeriesSettingsDownCommand;
        private Command copyLineSeriesSettingsCommand;
        private Command deleteLineSeriesSettingsCommand;

        public LineChartEditor()
        {
            addLineSeriesSettingsCommand = new Command(AddLineSeriesSettings);
            moveLineSeriesSettingsUpCommand = new Command(MoveLineSeriesSettingsUp);
            moveLineSeriesSettingsDownCommand = new Command(MoveLineSeriesSettingsDown);
            copyLineSeriesSettingsCommand = new Command(CopyLineSeriesSettings);
            deleteLineSeriesSettingsCommand = new Command(DeleteLineSeriesSettings);
            InitializeComponent();
            lineChartEditorRoot.DataContext = this;
        }

        #region  LineChartPanel dependency property

        public static readonly DependencyProperty LineChartPanelProperty = DependencyProperty.Register(
            "LineChartPanel", typeof(LineChartPanel), typeof(LineChartEditor), new PropertyMetadata(default(LineChartPanel)));

        public LineChartPanel LineChartPanel
        {
            get => (LineChartPanel) GetValue(LineChartPanelProperty);
            set => SetValue(LineChartPanelProperty, value);
        }

        public IEnumerable<EnumValueDescription> LegendPositions => EnumHelper.GetAllValuesAndDescriptions<LegendPosition>();
        public IEnumerable<EnumValueDescription> ChartColors => EnumHelper.GetAllValuesAndDescriptions<ChartColors>();

        #endregion LineChartPanel dependency property

        #region ValidationTableInfos dependency property

        public static readonly DependencyProperty ValidationTableInfosProperty = DependencyProperty.Register(
            "ValidationTableInfos", typeof(IEnumerable<ValidationTableInfo>), typeof(LineChartEditor), new PropertyMetadata(default(IEnumerable<ValidationTableInfo>)));

        public IEnumerable<ValidationTableInfo> ValidationTableInfos
        {
            get => (IEnumerable<ValidationTableInfo>) GetValue(ValidationTableInfosProperty);
            set => SetValue(ValidationTableInfosProperty, value);
        }

        #endregion ValidationTableInfos dependency property

        public Command AddLineSeriesSettingsCommand => addLineSeriesSettingsCommand;
        public Command MoveLineSeriesSettingsUpCommand => moveLineSeriesSettingsUpCommand;
        public Command MoveLineSeriesSettingsDownCommand => moveLineSeriesSettingsDownCommand;
        public Command CopyLineSeriesSettingsCommand => copyLineSeriesSettingsCommand;
        public Command DeleteLineSeriesSettingsCommand => deleteLineSeriesSettingsCommand;

        private void AddLineSeriesSettings()
        {
            LineChartPanel.Series.Add(new LineChartSeriesSettings());
        }

        private void MoveLineSeriesSettingsUp(object param)
        {
            var seriesSettings = param as LineChartSeriesSettings;
            if (seriesSettings == null)
            {
                return;
            }

            var index = LineChartPanel.Series.IndexOf(seriesSettings);
            if (index > 0)
            {
                LineChartPanel.Series.Move(index, index - 1);
            }
        }

        private void MoveLineSeriesSettingsDown(object param)
        {
            var seriesSettings = param as LineChartSeriesSettings;
            if (seriesSettings == null)
            {
                return;
            }

            var index = LineChartPanel.Series.IndexOf(seriesSettings);
            if (index < LineChartPanel.Series.Count - 1)
            {
                LineChartPanel.Series.Move(index, index + 1);
            }
        }

        private void CopyLineSeriesSettings(object param)
        {
            var seriesSettings = param as LineChartSeriesSettings;
            if (seriesSettings == null)
            {
                return;
            }

            LineChartPanel.Series.Add(seriesSettings.Clone() as LineChartSeriesSettings);
        }

        private void DeleteLineSeriesSettings(object param)
        {
            if (param is LineChartSeriesSettings seriesSettings)
            {
                seriesSettings.IsDeleted = true;
            }
        }

        private void CmbTable_OnTargetUpdated(object sender)
        {
            var cmb = sender as ComboBox;
            if (cmb == null)
            {
                return;
            }

            foreach (ValidationTableInfo item in cmb.Items)
            {
                if (item.Name == cmb.Text)
                {
                    cmb.SelectedItem = item;
                    return;
                }
            }
        }

        private void CmbRow_OnTargetUpdated(object sender)
        {
            var cmb = sender as ComboBox;
            if (cmb == null)
            {
                return;
            }

            foreach (ValidationTableRowInfo item in cmb.Items)
            {
                if (item.Name == cmb.Text)
                {
                    cmb.SelectedItem = item;
                    return;
                }
            }
        }

        private void Binding_OnTargetUpdated(object sender)
        {
            var cmb = sender as ComboBox;
            if (cmb == null)
            {
                return;
            }

            foreach (ValidationTableColumnInfo item in cmb.Items)
            {
                if (item.Name == cmb.Text)
                {
                    cmb.SelectedItem = item;
                    return;
                }
            }
        }
    }


    public class DontDeleteSelectedItemAfterItemsSourceChangedComboBox : ComboBox
    {
        public delegate void OnItemsChangedEventHandler(object sender);

        public event OnItemsChangedEventHandler OnItemsChangedEvent;

        private bool ignore = false;
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (!ignore)
            {
                base.OnSelectionChanged(e);
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            ignore = true;
            try
            {
                base.OnItemsChanged(e);
                OnItemsChangedEvent?.Invoke(this);
            }
            finally
            {
                ignore = false;
            }
        }
    }
}
