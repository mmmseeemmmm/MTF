using MTFClientServerCommon.Mathematics;
using MTFClientServerCommon.MTFValidationTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MTFApp.SequenceExecution;
using MTFApp.SequenceExecution.TableHandling;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFValidationTableEditor.xaml
    /// </summary>
    public partial class MTFValidationTableEditor : MTFEditorBase
    {
        private Visibility savingProgreessBarVisibility = Visibility.Hidden;
        private object selectedRow;
        private bool allowAutoScroll = true;
        private int savingProgreessBarValue;

        public MTFValidationTableEditor()
        {
            InitializeComponent();
            root.DataContext = this;
        }


        public double ColumnWidth => 150;


        public bool AllowAutoScroll
        {
            get => allowAutoScroll;
            set
            {
                allowAutoScroll = value;
                NotifyPropertyChanged();
            }
        }


        public MTFValidationTable TableData
        {
            get => (MTFValidationTable)GetValue(TableDataProperty);
            set => SetValue(TableDataProperty, value);
        }

        public static readonly DependencyProperty TableDataProperty =
            DependencyProperty.Register("TableData", typeof(MTFValidationTable), typeof(MTFValidationTableEditor),
            new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false });


        public object SelectedRow
        {
            get => selectedRow;
            set
            {
                var row = selectedRow as ExtendedRow;
                if (row != null)
                {
                    if (row.ActualValue is ActivityResultTerm && (row.ActualValue as ActivityResultTerm).Value==null)
                    {
                        row.ActualValue = new EmptyTerm();
                    }
                    row.RefreshActualValue();
                }
                if (value != selectedRow)
                {
                    selectedRow = value;
                    var tableTerm = this.DataContext as ValidationTableTerm;
                    if (tableTerm!=null)
                    {
                        foreach (var extendedRow in tableTerm.Rows)
                        {
                            extendedRow.IsSelected = extendedRow == selectedRow;
                        }
                    }
                }
                else
                {
                    selectedRow = null;
                }
                NotifyPropertyChanged();
            }
        }

        

        public ICommand AddRowCommand => new Command(AddRow);

        private void AddRow()
        {
            this.TableData.AddRow();
        }

        public ICommand AddColumnCommand => new Command(AddColumn);

        private void AddColumn()
        {
            this.TableData.AddColumn();
        }

        public ICommand RemoveColumnCommand => new Command(RemoveColumn);

        private void RemoveColumn(object param)
        {
            this.TableData.RemoveColumn((MTFValidationTableColumn)param);
        }

        public ICommand RemoveRowCommand => new Command(RemoveRow);

        private void RemoveRow(object param)
        {
            this.TableData.RemoveRow((MTFValidationTableRow)param);
        }

        public ICommand MoveUpRowCommand => new Command(MoveUpRow);
        public ICommand MoveDownRowCommand => new Command(MoveDownRow);

        private void MoveUpRow(object param)
        {
            TableData.MoveUp((MTFValidationTableRow)param);
        }

        private void MoveDownRow(object param)
        {
            TableData.MoveDown((MTFValidationTableRow)param);
        }

        public ICommand AddVariantRowCommand => new Command(AddVariantRow);

        private void AddVariantRow()
        {
            var row = Value as MTFValidationTableRow;
            if (row != null)
            {
                row.AddVariant();
            }
        }

        public ICommand RemoveVariantRowCommand => new Command(RemoveVariantRow);

        private void RemoveVariantRow(object param)
        {
            var variant = param as MTFValidationTableRowVariant;
            var row = Value as MTFValidationTableRow;
            if (row != null && variant!=null)
            {
                row.RemoveVariant(variant);
            }
        }

        public List<ValidationTableCondition> Conditions => FillConditios();


        private List<ValidationTableCondition> FillConditios()
        {
            var list = new List<ValidationTableCondition>
                       {
                           new ValidationTableCondition(ValidationTableConstants.ColumnMin, ValidationTableConstants.ColumnMinKey, new GreaterOrEqualTerm()),
                           new ValidationTableCondition(ValidationTableConstants.ColumnMax, ValidationTableConstants.ColumnMaxKey, new LessOrEqualTerm()),
                           new ValidationTableCondition(ValidationTableConstants.ColumnRequired, ValidationTableConstants.ColumnRequiredKey, new IsInListTerm()),
                           new ValidationTableCondition(ValidationTableConstants.ColumnProhibited, ValidationTableConstants.ColumnProhibitedKey, new NotIsInListTerm())
                       };
            return list;
        }

        private void ListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            var element = e.MouseDevice.DirectlyOver as FrameworkElement;
            if (element is Image || element is Border || element is TextBox)
            {
                return;
            }
            if (UIHelper.FindParent<ComboBox>(element) != null
                || UIHelper.FindParent<TextBox>(element) != null
                || UIHelper.FindParent<ComboBoxItem>(element) != null
                || UIHelper.FindParent<Button>(element)!=null)
            {
                return;
            }
            SelectedRow = UIHelper.GetObjectDataFromPoint(listBox, e.GetPosition(listBox));
        }

        private void ParamTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                var cell = textBox.Tag as MTFValidationTableCell;
                if (cell != null)
                {
                    cell.IsSet = true;
                }
            }
        }

        private void ParamTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var textBox = sender as TextBox;
                if (textBox != null)
                {
                    var cell = textBox.Tag as MTFValidationTableCell;
                    if (cell != null)
                    {
                        cell.IsSet = false;
                        textBox.Text = null;
                    }
                }
            }
        }

        private void GridSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var control = sender as GridSplitter;
            if (control != null && control.Tag is MTFValidationTableColumn)
            {
                var cell = control.Tag as MTFValidationTableColumn;
                if (e.HorizontalChange < 50 && cell.Width + e.HorizontalChange > MTFValidationTableColumn.MinWidth)
                {
                    cell.Width += e.HorizontalChange;
                }
            }
        }

        private void TableImagePreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var fe = sender as FrameworkElement;
            if (fe!=null)
            {
                var presenter = fe.Tag as SequenceExecutionPresenter;
                if (presenter!=null)
                {
                    var cell = fe.DataContext as MTFValidationTableCell;
                    if (cell!=null)
                    {
                        presenter.SetTableImageDetailCommand.Execute(cell.Value);
                    }
                }
            }
        }

        private void TableHeaderPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element != null)
            {
                var table = element.DataContext as ExecutionValidTable;
                if (table != null)
                {
                    table.IsCollapsed = !table.IsCollapsed;
                }
            }
            e.Handled = true;
        }

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UIHelper.RaiseScrollEvent(sender, e);
        }



        private void ParamTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.Tab))
            {
                var textBox = sender as TextBox;
                if (textBox != null)
                {
                    var cell = textBox.Tag as MTFValidationTableCell;
                    if (cell != null)
                    {
                        cell.IsSet = true;
                    }
                }
            }
        }

        private void TagListBox_SetItems(object sender, EventArgs e)
        {
            var tagListBox = sender as MTFApp.UIControls.TagListBoxControl.TagListBox;
            if (tagListBox != null)
            {
                var cell = tagListBox.Tag as MTFValidationTableCell;
                if (cell != null)
                {
                    cell.IsSet = tagListBox.Count > 0;
                }
            }
        }


        private void TimeViewListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange==0|| e.VerticalChange == 1 || e.VerticalChange==-1)
            {
                return;
            }
            var listBox = sender as ListBox;
            var scrollViewer = UIHelper.GetVisualDescendent<ScrollViewer>(listBox);
            if (scrollViewer != null)
            {
                AllowAutoScroll = scrollViewer.ScrollableHeight - scrollViewer.VerticalOffset <=2;
            }
        }

        private void ScrollViewer_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void RowListBoxItem_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox!=null)
            {
                listBox.SelectedItem = UIHelper.GetObjectDataFromPoint(listBox, e.GetPosition(listBox));
                
            }
        }

        private void SaveValidationTableRowButtonClick(object sender, RoutedEventArgs e)
        {
            RunProgressBarAsync();
            var button = sender as Button;
            var table = this.Value as ExecutionValidTable;
            if (button!=null && table!=null)
            {
                var row = button.CommandParameter as MTFValidationTableRow;
                if (row!=null)
                {
                    MTFClient.GetMTFClient().SetValidationTableRow(table.Id, row.OriginalRowForService ?? row);
                }
            }
        }

        private async void RunProgressBarAsync()
        {
            SavingProgreessBarValue = 0;
            SavingProgreessBarVisibility = Visibility.Visible;
            await Task.Run(() =>
                           {
                               for (int i = 1; i <= 5; i++)
                               {
                                   SavingProgreessBarValue = i;
                                   Task.Delay(15).Wait();
                               }
                           });
            SavingProgreessBarVisibility = Visibility.Hidden;
        }

        public Visibility SavingProgreessBarVisibility
        {
            get => savingProgreessBarVisibility;
            set
            {
                savingProgreessBarVisibility = value;
                NotifyPropertyChanged();
            }
        }

        public int SavingProgreessBarValue
        {
            get => savingProgreessBarValue;
            set
            {
                savingProgreessBarValue = value;
                NotifyPropertyChanged();
            }
        }
    }

    public class ValidationTableFixConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value is MTFValidationTable ? value : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

namespace MTFApp.UIHelpers.Editors.ValidationTableEditor
{
    public class UniqueNameValidationRule : ValidationRule
    {
        public UniqueNameDPHelper UniqueNameDPHelper { get; set; }





        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                return new ValidationResult(false, LanguageHelper.GetString("ValidationTable_EnterUniquenName"));
            }
            if (UniqueNameDPHelper.ValidationTable != null && UniqueNameDPHelper.ValidationTable.Rows != null)
            {
                if (UniqueNameDPHelper.ValidationTable.Rows.Any(x => x.Header == str))
                {
                    return new ValidationResult(false, LanguageHelper.GetString("ValidationTable_EnterUniquenName"));
                }
            }
            return new ValidationResult(true, null);
        }
    }

    public class UniqueNameDPHelper : DependencyObject
    {
        public MTFValidationTable ValidationTable
        {
            get => (MTFValidationTable)GetValue(ValidationTableProperty);
            set => SetValue(ValidationTableProperty, value);
        }

        public static readonly DependencyProperty ValidationTableProperty =
            DependencyProperty.Register("ValidationTable", typeof(MTFValidationTable), typeof(UniqueNameDPHelper)
            , new FrameworkPropertyMetadata() { BindsTwoWayByDefault = false });
    }

    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }



        public object Data
        {
            get => (object)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new PropertyMetadata(null));


    }

}




