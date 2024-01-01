using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MTFClientServerCommon.MTFTable;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFDataTableEditor.xaml
    /// </summary>
    public partial class MTFDataTableEditor : MTFEditorBase
    {
        public MTFDataTableEditor()
        {
            //this.TableData = new MTFTable("Name");
            //NotifyPropertyChanged("Value");
            InitializeComponent();
            root.DataContext = this;


        }
        public IMTFDataTable TableData
        {
            get => (IMTFDataTable)GetValue(TableDataProperty);
            set => SetValue(TableDataProperty, value);
        }

        public static readonly DependencyProperty TableDataProperty =
            DependencyProperty.Register("TableData", typeof(IMTFDataTable), typeof(MTFDataTableEditor),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = false });

        
        private void ScrollViewer_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void GridSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (sender is GridSplitter control && control.Tag is MTFTableColumn cell)
            {
                if (e.HorizontalChange < 50 && cell.Width + e.HorizontalChange > MTFTableColumn.MinWidth)
                {
                    cell.Width += e.HorizontalChange;
                }
            }
        }

        private void RowListBoxItem_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                listBox.SelectedItem = UIHelper.GetObjectDataFromPoint(listBox, e.GetPosition(listBox));
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

        public ICommand MoveUpRowCommand => new Command(MoveUpRow);
        public ICommand MoveDownRowCommand => new Command(MoveDownRow);

        private void MoveUpRow(object param)
        {
            TableData.MoveUp((MTFTableRow)param);
        }

        private void MoveDownRow(object param)
        {
            TableData.MoveDown((MTFTableRow)param);
        }

        public ICommand RemoveColumnCommand => new Command(RemoveColumn);

        private void RemoveColumn(object param)
        {
            this.TableData.RemoveColumn((MTFTableColumn)param);
        }

        public ICommand RemoveRowCommand => new Command(RemoveRow);

        private void RemoveRow(object param)
        {
            this.TableData.RemoveRow((MTFTableRow)param);
        }

        public ICommand AddVariantRowCommand => new Command(AddVariantRow);

        private void AddVariantRow()
        {
            if (Value is MTFTableRow row)
            {
                row.AddVariant();
            }
        }

        public ICommand RemoveVariantRowCommand => new Command(RemoveVariantRow);

        private void RemoveVariantRow(object param)
        {
            var variant = param as MTFTableRowVariant;
            if (Value is MTFTableRow row && variant != null)
            {
                row.RemoveVariant(variant);
            }
        }

    }

    public class DataTableFixConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is IMTFDataTable ? value : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

namespace MTFApp.UIHelpers.Editors.TableEditor
{
    public class UniqueNameValidationRule : ValidationRule
    {
        public UniqueNameDPHelper UniqueNameDPHelper { get; set; }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string str = value as string;
            if (string.IsNullOrEmpty(str))
            {
                return new ValidationResult(false, "Pleas enter unique name.");
            }
            if (UniqueNameDPHelper.Table != null && UniqueNameDPHelper.Table.Rows != null)
            {
                if (UniqueNameDPHelper.Table.Rows.Any(x => x.Header == str))
                {
                    return new ValidationResult(false, "Pleas enter unique name.");
                }
            }
            return new ValidationResult(true, null);
        }
    }

    public class UniqueNameDPHelper : DependencyObject
    {
        public IMTFDataTable Table
        {
            get => (IMTFDataTable)GetValue(ValidationTableProperty);
            set => SetValue(ValidationTableProperty, value);
        }

        public static readonly DependencyProperty ValidationTableProperty =
            DependencyProperty.Register("Table", typeof(IMTFDataTable), typeof(UniqueNameDPHelper)
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
