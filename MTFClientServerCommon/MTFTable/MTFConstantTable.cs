using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFClientServerCommon.MTFTable
{
    [Serializable]
    public class MTFConstantTable : MTFDataTransferObject, IMTFDataTable, IMTFTable
    {
        #region ctor

        public MTFConstantTable()
        {
            Rows = new MTFObservableCollection<MTFTableRow>();
            Columns = new MTFObservableCollection<MTFTableColumn>
                      {
                          new MTFTableColumn(true, false){Header = "Name"},
                          new MTFTableColumn(false, false){Header = "Value"}
                      };
        }

        public MTFConstantTable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion

        #region Properties

        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<MTFTableRow> Rows
        {
            get { return GetProperty<ObservableCollection<MTFTableRow>>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<MTFTableColumn> Columns
        {
            get { return GetProperty<ObservableCollection<MTFTableColumn>>(); }
            set { SetProperty(value); }
        }

        public IEnumerable<string> ColumnNames
        {
            get
            {
                return Columns.Select(col => col.Header).ToList();
            }
        }

        public bool SaveToLog
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public int RowsCount
        {
            get { return Rows.Count; }
        }

        public int ColumnsCount
        {
            get { return Columns.Count; }
        }

        #endregion

        #region public methods

        public void AddRow()
        {
            Rows.Add(new MTFTableRow(ColumnsCount, this));
        }

        public void RemoveRow(MTFTableRow row)
        {
            Rows.Remove(row);
        }

        public void AddColumn()
        {
            var column = new MTFTableColumn(false, true);
            Columns.Add(column);
            AddCellToExistingRows(column);
        }

        public void AddColumn(MTFTableColumn column)
        {
            Columns.Add(column);
            AddCellToExistingRows(column);
        }

        public void RemoveColumn(MTFTableColumn column)
        {
            if (column.CanRemove)
            {
                var index = Columns.IndexOf(column);
                Columns.Remove(column);
                RemoveCellFromRows(index);
            }
        }

        public void MoveUp(MTFTableRow row)
        {
            var index = Rows.IndexOf(row);
            if (index > 0)
            {
                Rows.Move(index, index - 1);
            }
        }

        public void MoveDown(MTFTableRow row)
        {
            var index = Rows.IndexOf(row);

            if (index != -1 && index < RowsCount - 1)
            {
                Rows.Move(index, index + 1);
            }
        }

        public object GetResult(ValidationTableResultType selectedResultType, Guid selectedRowId, string columnName, SequenceVariant sequenceVariant)
        {
            return GetRow(selectedRowId).GetValue(columnName, sequenceVariant);
        }

        //public void ChangeVariant(IList<SequenceVariantGroup> sequenceVariantGroups,IEnumerable<SequenceVariantValue> newValues)
        //{
        //    if (Rows != null)
        //    {
        //        foreach (var row in Rows)
        //        {
        //            if (row != null && row.RowVariants != null)
        //            {
        //                var rowVariantsTodelete = new List<MTFTableRowVariant>();
        //                foreach (var rowVariant in row.RowVariants)
        //                {
        //                    if (rowVariant != null && rowVariant.SequenceVariant != null)
        //                    {
        //                        rowVariant.SequenceVariant.ChangeVariant(sequenceVariantGroups, newValues);
        //                        if (rowVariant.SequenceVariant.IsEmpty)
        //                        {
        //                            rowVariantsTodelete.Add(rowVariant);
        //                        }
        //                    }
        //                }

        //                if (rowVariantsTodelete.Count > 0)
        //                {
        //                    foreach (var rowVariant in rowVariantsTodelete)
        //                    {
        //                        row.RowVariants.Remove(rowVariant);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        public bool ExistRow(Guid rowId)
        {
            return Rows != null && Rows.Any(x => x.Id == rowId);
        }

        public string GetRowNameById(Guid rowId)
        {
            if (Rows != null)
            {
                var row = Rows.FirstOrDefault(x => x.Id == rowId);
                if (row != null)
                {
                    return row.Header;
                }
            }
            return null;
        }

        public Guid GetRowIdByName(string name)
        {
            if (Rows != null)
            {
                var row = Rows.FirstOrDefault(x => x.Header == name);
                if (row != null)
                {
                    return row.Id;
                }
            }
            return Guid.Empty;
        }
        #endregion

        #region private methods

        private void AddCellToExistingRows(MTFTableColumn column)
        {
            foreach (var row in Rows)
            {
                row.AddCell(column);
            }
        }

        private void RemoveCellFromRows(int columnIndex)
        {
            foreach (var row in Rows)
            {
                row.RemoveCell(columnIndex);
            }
        }


        private MTFTableRow GetRow(Guid rowId)
        {
            if (rowId == Guid.Empty)
            {
                throw new Exception("Selected row is empty.");
            }
            if (Rows == null || Rows.Count < 1)
            {
                throw new Exception("Table has no rows.");
            }
            var row = Rows.FirstOrDefault(x => x.Id == rowId);
            if (row == null)
            {
                throw new Exception(string.Format("Row {0} was not found", rowId));
            }
            return row;
        }


        #endregion


        #region Override

        protected override object CloneInternal(bool copyIdValue)
        {
            var newTable =  base.CloneInternal(copyIdValue) as MTFConstantTable;

            if (newTable?.Rows != null && newTable.Columns != null)
            {
                foreach (var row in newTable.Rows)
                {
                    if (row.Items != null && row.Items.Count > 0)
                    {
                        for (int i = 0; i < row.Items.Count; i++)
                        {
                            row.Items[i].Column = newTable.Columns[i];
                        }

                        if (row.RowVariants != null && row.RowVariants.Count > 0)
                        {
                            foreach (var rowVariant in row.RowVariants)
                            {
                                rowVariant.AssignColumns(row.Items.Select(x => x.Column).ToList());
                            }
                        }
                    }
                }
            }

            return newTable;
        }

        #endregion








    }
}
