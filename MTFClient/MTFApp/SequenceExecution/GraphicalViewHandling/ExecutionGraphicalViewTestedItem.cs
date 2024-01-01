using System;
using System.Collections.Specialized;
using System.Windows;
using MTFApp.SequenceExecution.TableHandling;
using MTFApp.UIHelpers;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFApp.SequenceExecution.GraphicalViewHandling
{
    public class ExecutionGraphicalViewTestedItem : NotifyPropertyBase
    {
        private ExecutionValidTable validationTable;
        private Point position;
        private string alias;
        private ITableStatus executionItem;
        private Guid rowId;

        public ExecutionValidTable ValidationTable
        {
            get => validationTable;
            set => validationTable = value;
        }

        public Point Position
        {
            get => position;
            set => position = value;
        }

        public string Alias
        {
            get => alias;
            set => alias = value;
        }

        public ITableStatus ExecutionItem
        {
            get => executionItem;
            set
            {
                executionItem = value; 
                NotifyPropertyChanged();
            }
        }

        public void AssignRow(Guid validationTableRowId)
        {
            rowId = validationTableRowId;
            if (ValidationTable != null)
            {
                ValidationTable.Rows.CollectionChanged += RowsCollectionChanged;
            }
        }

        private void RowsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ExecutionItem = null;
            }
            else if (e.NewItems?.Count > 0)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is MTFValidationTableRow row && row.Id == rowId)
                    {
                        ExecutionItem = row;
                        break;
                    }
                }
            }
        }
    }
}