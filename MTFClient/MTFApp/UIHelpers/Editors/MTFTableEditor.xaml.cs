using AutomotiveLighting.MTFCommon;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Editors
{
    /// <summary>
    /// Interaction logic for MTFTableEditor.xaml
    /// </summary>
    public partial class MTFTableEditor : MTFEditorBase
    {
        public MTFTableEditor()
        {
            InitializeComponent();

            root.DataContext = this;
        }

        protected override void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(source, e);

            if (e.Property != ValueProperty)
            {
                return;
            }

            MTFDataTable dataTable = e.NewValue as MTFDataTable;
            if (dataTable == null)
            {
                return;
            }

            prepaireTable(dataTable);
        }

        private DataTable generatedDataTable;
        private void prepaireTable(MTFDataTable dataTable)
        {
            mainDataGrid.CanUserSortColumns = dataTable.CanSort;
            mainDataGrid.CanUserReorderColumns = dataTable.CanReorderColumns;
            mainDataGrid.CanUserResizeColumns = dataTable.CanResizeColumns;
            mainDataGrid.CanUserAddRows = dataTable.CanAddRow;

            prepaireHeaderVisibility(dataTable.HeaderVisibility);
            prepaireTableColumns(dataTable.Columns);
            mainDataGrid.AutoGeneratingColumn += mainDataGrid_AutoGeneratingColumn;

            fillTableData(dataTable.TableData);

            mainDataGrid.ItemsSource = generatedDataTable.DefaultView;

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (dataTable.Columns[i].ReadOnly)
                {
                    generatedDataTable.Columns[i].ReadOnly = true;
                }
            }
        }

        void mainDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            MTFDataTable dataTable = this.Value as MTFDataTable;
            if (dataTable == null)
            {
                return;
            }
            var dataTableColumn = dataTable.Columns.First(c => c.Name == e.PropertyName);
            if (dataTableColumn == null)
            {
                return;
            }

            if (dataTableColumn.DataType == ColumnDataType.ListBox)
            {
                e.Column = new DataGridComboBoxColumn
                {
                    Header = e.PropertyName,
                    ItemsSource = dataTableColumn.ListBoxItems,
                    SelectedValueBinding = new Binding(e.PropertyName) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged }
                    //SelectedValueBinding = new Binding(e.PropertyName)
                };
            }
            else if (dataTableColumn.DataType == ColumnDataType.Checkbox)
            {
                ((DataGridCheckBoxColumn)e.Column).Binding = new Binding(e.PropertyName) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            }
            else if (dataTableColumn.DataType == ColumnDataType.Text)
            {
                ((DataGridTextColumn)e.Column).Binding = new Binding(e.PropertyName) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            }

            e.Column.IsReadOnly = dataTableColumn.ReadOnly;
        }

        private void prepaireHeaderVisibility(HeaderVisibility headerVisibility)
        {
            if (headerVisibility == HeaderVisibility.All)
            {
                mainDataGrid.HeadersVisibility = DataGridHeadersVisibility.All;
            }
            else if (headerVisibility == HeaderVisibility.Column)
            {
                mainDataGrid.HeadersVisibility = DataGridHeadersVisibility.Column;
            }
            else if (headerVisibility == HeaderVisibility.Row)
            {
                mainDataGrid.HeadersVisibility = DataGridHeadersVisibility.Row;
            }
            else if (headerVisibility == HeaderVisibility.No)
            {
                mainDataGrid.HeadersVisibility = DataGridHeadersVisibility.None;
            }
        }

        private void prepaireTableColumns(List<ColumnDescription> columns)
        {
            generatedDataTable = new DataTable();
            foreach (var columnDescription in columns)
            {
                DataColumn col = new DataColumn(columnDescription.Name);
                if (columnDescription.DataType == ColumnDataType.Checkbox)
                {
                    col.DataType = typeof(bool);
                }
                generatedDataTable.Columns.Add(col);
            }
        }

        private void fillTableData(List<List<object>> tableData)
        {
            foreach (var line in tableData)
            {
                var row = generatedDataTable.NewRow();
                generatedDataTable.Rows.Add(row);
                int i = 0;
                foreach (var item in line)
                {
                    row[i] = item;
                    i++;
                }
            }
        }

        private void mainDataGrid_LostFocus(object sender, RoutedEventArgs e)
        {

            var mtfDatatable = Value as MTFDataTable;
            if (mtfDatatable == null)
            {
                return;
            }
            int r = 0;
            foreach (DataRow row in generatedDataTable.Rows)
            {
                int c = 0;
                foreach (object val in row.ItemArray)
                {
                    mtfDatatable.TableData[r][c] = val;
                    c++;
                }
                r++;
            }
        }
    }
}
