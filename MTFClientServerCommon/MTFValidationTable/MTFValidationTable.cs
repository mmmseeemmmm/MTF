using MTFClientServerCommon.Mathematics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using MTFCommon;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using MTFClientServerCommon.Helpers;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.MTFTable;

namespace MTFClientServerCommon.MTFValidationTable
{
    [Serializable]
    public class MTFValidationTable : MTFDataTransferObject, IMTFTable
    {
        private bool hasValidated;
        public const string ActualValueHeader = "Actual";
        public const string NameHeader = "Name";
        public const string HiddenHeader = "Hidden";
        public const int IdentificationPosition = 0;
        public const int ActualValuePosition = 1;
        public const int HiddenValuePosition = 2;
        private MTFValidationTableStatus status;
        private bool hideAll;

        #region ctor
        public MTFValidationTable()
        {
            Rows = new ObservableCollection<MTFValidationTableRow>();
            Columns = new ObservableCollection<MTFValidationTableColumn>
                {
                    new MTFValidationTableColumn(MTFTableColumnType.Identification){ Header = NameHeader},
                    new MTFValidationTableColumn(MTFTableColumnType.ActualValue){ Header = ActualValueHeader},
                    new MTFValidationTableColumn(MTFTableColumnType.Hidden){Header = HiddenHeader, Width = 45},
                };
            ExecutionMode = MTFValidationTableExecutionMode.OnlySet;
            SaveToLog = true;
            CheckMultipleFilling = true;
        }



        public MTFValidationTable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Properties

        public string Name
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
            }
        }

        public ObservableCollection<MTFValidationTableRow> Rows
        {
            get { return GetProperty<ObservableCollection<MTFValidationTableRow>>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<MTFValidationTableColumn> Columns
        {
            get { return GetProperty<ObservableCollection<MTFValidationTableColumn>>(); }
            set { SetProperty(value); }
        }

        public IEnumerable<string> ColumnNames
        {
            get
            {
                List<string> l = new List<string>();
                foreach (var col in Columns)
                {
                    l.Add(col.Header);
                    if (col.ValidationTerm != null && col.ValidationTerm.GetType() == typeof(IsInListTerm))
                    {
                        l.Add(col.Header + " as string");
                    }
                }

                return l;
            }
        }

        public int ColumnsCount
        {
            get { return Columns.Count; }
        }

        public int RowsCount
        {
            get { return Rows.Count; }
        }

        public MTFValidationTableStatus Status
        {
            get { return status; }
            set { status = value; }
        }

        public bool ValidateAllRows
        {
            get { return ExecutionMode == MTFValidationTableExecutionMode.AllRows; }
        }

        public MTFValidationTableExecutionMode ExecutionMode
        {
            get { return GetProperty<MTFValidationTableExecutionMode>(); }
            set { SetProperty(value); }
        }

        public bool SaveToLog
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool CheckMultipleFilling
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool HideAll
        {
            get { return hideAll; }
            set
            {
                hideAll = value;
                ChangeRowVisibility(value);
            }
        }

        public bool UseGoldSample
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                CheckGoldSampleColumn(value);
            }
        }

        public bool AllowNokGsForTable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public SequenceVariant NokVariantSelectorForTable
        {
            get { return GetProperty<SequenceVariant>(); }
            set { SetProperty(value); }
        }

        public double GoldSampleLimit
        {
            get { return GetProperty<double>(); }
            set
            {
                SetProperty(value);
                var goldSampleColumn = Columns.FirstOrDefault(c => c.Type == MTFTableColumnType.GoldSample);
                if (goldSampleColumn != null)
                {
                    goldSampleColumn.SetPercentage(value);
                }
                if (Rows != null)
                {
                    foreach (var row in Rows)
                    {
                        row.SetPercentage(value);
                    }
                }
            }
        }

        public bool IsClear
        {
            get
            {
                if (Rows == null || Rows.Count == 0)
                {
                    return true;
                }
                return Rows.All(x => x.Status == MTFValidationTableStatus.NotFilled);
            }
        }

        public bool HasAlreadyInLog { get; set; }

        public DateTime TimeOfFirstValidation
        {
            get { return GetProperty<DateTime>(); }
            private set { SetProperty(value); }
        }


        public string ErrorText
        {
            get
            {
                if (Rows != null)
                {
                    var sb = new StringBuilder();
                    foreach (var tableRow in Rows)
                    {
                        if (!string.IsNullOrEmpty(tableRow.ErrorText))
                        {
                            if (sb.Length > 0)
                            {
                                sb.AppendLine();
                            }
                            sb.Append(tableRow.ErrorText);
                        }
                    }
                    return sb.ToString();
                }
                return null;
            }
        }

        public string DisplayName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }


        #endregion

        #region public methods

        public void SetValidationTime()
        {
            if (!hasValidated)
            {
                TimeOfFirstValidation = DateTime.Now;
                hasValidated = true;
            }
        }

        public void ResetValidationTime()
        {
            hasValidated = false;
            TimeOfFirstValidation = DateTime.MinValue;
        }

        public void AddRow()
        {
            Rows.Add(new MTFValidationTableRow(ColumnsCount, this));
            NotifyPropertyChanged("RowsCount");
        }

        public void AddRow(string name)
        {
            var row = new MTFValidationTableRow(ColumnsCount, this);
            row.Items[IdentificationPosition].Value = name;
            Rows.Add(row);
            NotifyPropertyChanged("RowsCount");
            if (RowsCount != 0)
            {
                AllowNokGsForTable = false;
            }
        }

        public void AddColumn()
        {
            var column = new MTFValidationTableColumn(MTFTableColumnType.Value) { Header = "Min" };
            Columns.Add(column);
            AddCellToExistingRows(column);
        }

        public void AddColumn(MTFValidationTableColumn column)
        {
            Columns.Add(column);
            AddCellToExistingRows(column);
        }

        public void RemoveColumn(MTFValidationTableColumn column)
        {
            if (column.CanRemove)
            {
                var index = Columns.IndexOf(column);
                Columns.Remove(column);
                RemoveCellFromRows(index);
            }
        }

        public void RemoveRow(MTFValidationTableRow row)
        {
            Rows.Remove(row);
            NotifyPropertyChanged("RowsCount");
        }

        public Term GetColumnCondition(int index)
        {
            Term t = null;
            if (index < Columns.Count && index >= 0)
            {
                t = Columns[index].ValidationTerm;
            }
            return t;
        }


        public object GetResult(ValidationTableResultType selectedResultType, Guid selectedRowId, string selectedColumn, SequenceVariant sequenceVariant)
        {
            switch (selectedResultType)
            {
                case ValidationTableResultType.TableResult:
                    return Status;
                case ValidationTableResultType.LineResult:
                    return CheckRowsForResult(selectedRowId).Status;
                case ValidationTableResultType.CellResult:
                    if (string.IsNullOrEmpty(selectedColumn))
                    {
                        throw new Exception("Selected column is empty.");
                    }
                    return CheckRowsForResult(selectedRowId).GetValue(selectedColumn, sequenceVariant);
                default:
                    throw new ArgumentOutOfRangeException("selectedResultType", selectedResultType, null);
            }
        }

        public void LogValidationTable(XmlSerializer serializer, Stream writer, string xmlHeader, string actualDir)
        {
            var data = new MTFValidationTablesLog { Tables = new List<MTFValidationTableLog>() };
            data.Tables.Add(CreateDataForLogging(actualDir));
            try
            {
                if (writer.CanWrite)
                {
                    writer.SetLength(0);
                    using (var xmlWriter = System.Xml.XmlWriter.Create(writer))
                    {
                        xmlWriter.WriteProcessingInstruction("xml-stylesheet", xmlHeader);
                        serializer.Serialize(xmlWriter, data);
                    }
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                SystemLog.LogException(ex);
            }
        }

        public MTFValidationTableLog CreateDataForLogging(Func<MTFImage, bool, string, string> logActionCallback, IList<RoundingRule> roundingRules, bool logHiddenRows, string actualDir)
        {
            HasAlreadyInLog = true;
            return new MTFValidationTableLog
            {
                Name = Name,
                ValidationMode = ExecutionMode.Description(),
                Status = Status,
                Rows = GetRowsToLog(Rows, roundingRules, logHiddenRows, actualDir, logActionCallback).ToList(),
                TimeOfValidation = TimeOfFirstValidation,
            };
        }

        public void UpdateRow(MTFValidationTableRow row)
        {
            if (row != null)
            {
                var originalRow = Rows.FirstOrDefault(x => x.Id == row.Id);
                if (originalRow != null)
                {
                    ReplaceItems(originalRow.Items, row.Items);
                    if (originalRow.RowVariants != null && row.RowVariants != null && originalRow.RowVariants.Count == row.RowVariants.Count)
                    {
                        int i = 0;
                        foreach (var rowVariant in originalRow.RowVariants)
                        {
                            //rowVariant.SequenceVariant = row.RowVariants[i].SequenceVariant;
                            ReplaceItems(rowVariant.Items, row.RowVariants[i].Items);
                            i++;
                        }

                    }
                }
            }
        }

        public MTFValidationTableRow CreateTableRow(string rowName)
        {
            var row = new MTFValidationTableRow { Items = new MTFObservableCollection<MTFValidationTableCell>() };
            foreach (var col in Columns)
            {
                row.Items.Add(new MTFValidationTableCell { Column = col, Header = col.Header, Type = col.Type });
            }
            row.Items[IdentificationPosition].Value = rowName;
            Rows.Add(row);

            return row;
        }

        //public void ChangeVariant(IList<SequenceVariantGroup> sequenceVariantGroups, IEnumerable<SequenceVariantValue> newValues)
        //{
        //    if (Rows != null)
        //    {
        //        foreach (var row in Rows)
        //        {
        //            if (row != null && row.RowVariants != null)
        //            {
        //                var rowVariantsTodelete = new List<MTFValidationTableRowVariant>();
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

        public void Reset()
        {
            resetRows();
            Status = MTFValidationTableStatus.NotFilled;
        }

        private void resetRows()
        {
            foreach (var row in Rows)
            {
                var actualValueItem = row.Items.FirstOrDefault(i => i.Type == MTFTableColumnType.ActualValue);
                actualValueItem.IsSet = false;
                actualValueItem.Value = null;
                row.NumberOfRepetition = 0;
                row.Status = MTFCommon.MTFValidationTableStatus.NotFilled;
                row.ErrorText = string.Empty;
                foreach (var cell in row.Items)
                {
                    cell.Status = false;
                }
            }
        }

        public void MoveUp(MTFValidationTableRow row)
        {
            var index = Rows.IndexOf(row);
            if (index > 0)
            {
                Rows.Move(index, index - 1);
            }
        }

        public void MoveDown(MTFValidationTableRow row)
        {
            var index = Rows.IndexOf(row);

            if (index != -1 && index < RowsCount - 1)
            {
                Rows.Move(index, index + 1);
            }
        }

        #endregion

        #region private methods

        private void AddCellToExistingRows(MTFValidationTableColumn column)
        {
            foreach (var row in Rows)
            {
                row.AddNewCell(column);
            }
        }

        private void RemoveCellFromRows(int columnIndex)
        {
            foreach (var row in Rows)
            {
                row.RemoveCell(columnIndex);
            }
        }

        private void ReplaceItems(IList<MTFValidationTableCell> oldData, IList<MTFValidationTableCell> newData)
        {
            if (oldData.Count == newData.Count)
            {
                for (int i = 0; i < oldData.Count; i++)
                {
                    if (oldData[i].IsActualParam)
                    {
                        oldData[i].Value = newData[i].Value;
                    }
                }
            }
        }

        private void CheckGoldSampleColumn(bool addGoldSampleColumn)
        {
            var goldSampleColumn = Columns.FirstOrDefault(c => c.Type == MTFTableColumnType.GoldSample);
            if (addGoldSampleColumn)
            {
                if (goldSampleColumn == null)
                {
                    var column = new MTFValidationTableColumn(MTFTableColumnType.GoldSample) { Header = ValidationTableConstants.ColumnGs };
                    column.SetPercentage(GoldSampleLimit);
                    Columns.Add(column);
                    AddCellToExistingRows(column);
                }
            }
            else
            {
                if (goldSampleColumn != null)
                {
                    var index = Columns.IndexOf(goldSampleColumn);
                    Columns.Remove(goldSampleColumn);
                    RemoveCellFromRows(index);
                }
            }
        }

        private MTFValidationTableRow CheckRowsForResult(string selectedRow)
        {
            if (string.IsNullOrEmpty(selectedRow))
            {
                throw new Exception("Selected row is empty.");
            }
            if (Rows == null || Rows.Count < 1)
            {
                throw new Exception("Table has no rows.");
            }
            var row = Rows.FirstOrDefault(x => x.Items != null && x.Items.First().Value as string == selectedRow);
            if (row == null)
            {
                throw new Exception(string.Format("Row {0} was not found", selectedRow));
            }
            return row;
        }

        private MTFValidationTableRow CheckRowsForResult(Guid selectedRowId)
        {
            if (selectedRowId == Guid.Empty)
            {
                throw new Exception("Selected row is empty.");
            }
            if (Rows == null || Rows.Count < 1)
            {
                throw new Exception("Table has no rows.");
            }
            var row = Rows.FirstOrDefault(x => x.Id == selectedRowId);
            if (row == null)
            {
                throw new Exception(string.Format("Row {0} was not found", selectedRowId));
            }
            return row;
        }

        private MTFValidationTableLog CreateDataForLogging(string actualDir)
        {
            return new MTFValidationTableLog
            {
                Name = Name,
                ValidationMode = ExecutionMode.Description(),
                Status = Status,
                Rows = GetRowsToLog(Rows, null, false, actualDir).ToList(),
            };
        }

        private IEnumerable<MTFValidationTableRowLog> GetRowsToLog(ObservableCollection<MTFValidationTableRow> rows,
            IList<RoundingRule> roundingRules, bool logHiddenRows, string actualDir, Func<MTFImage, bool, string, string> logActionCallback = null)
        {
            var collection = new List<MTFValidationTableRowLog>();
            try
            {
                foreach (var row in rows)
                {
                    if ((logHiddenRows || !row.IsHidden) && (ValidateAllRows || row.Status != MTFValidationTableStatus.NotFilled))
                    {
                        var actualValue = row.GetActualValue();
                        var roundedValue = MTFSequenceHelper.RoundActualValue(actualValue, roundingRules);
                        var roundedValueToLog = roundedValue == null ? null : roundedValue.ToString();
                        var actualValueToLog = actualValue == null ? null : actualValue.ToString();
                        var image = row.GetActualValue() as MTFImage;
                        if (image != null && logActionCallback != null)
                        {
                            actualValueToLog = logActionCallback(image, row.Status == MTFValidationTableStatus.Nok, actualDir);
                        }
                        collection.Add(new MTFValidationTableRowLog
                        {
                            Name = row.Header,
                            ActualValue = actualValueToLog,
                            RoundedValue = roundedValueToLog == actualValueToLog ? null : roundedValueToLog,
                            NumberOfRepetition = row.NumberOfRepetition,
                            Status = row.Status,
                            Conditions = GetRowConditionsToLog(row, roundingRules).ToList(),
                            HasImage = image != null,
                        });
                    }
                }
            }
            catch (InvalidOperationException)
            {
                return collection;
            }
            return collection;
        }

        private IEnumerable<MTFValidationTableRowCondition> GetRowConditionsToLog(MTFValidationTableRow row, IList<RoundingRule> roundingRules)
        {
            if (row.RowVariants != null && row.RowVariants.Count > 0 && row.EvaluatedVariant > -1)
            {
                return GetRowConditionsToLog(row.RowVariants[row.EvaluatedVariant].Items, row.GoldSamplePercentage, roundingRules);
            }
            return GetRowConditionsToLog(row.Items, row.GoldSamplePercentage, roundingRules);
        }

        private IEnumerable<MTFValidationTableRowCondition> GetRowConditionsToLog(ObservableCollection<MTFValidationTableCell> items, double goldSamplePercentage, IList<RoundingRule> roundingRules)
        {
            foreach (var item in items)
            {
                if (item.Type == MTFTableColumnType.Value || item.Type == MTFTableColumnType.GoldSample)
                {
                    var condition = new MTFValidationTableRowCondition
                    {
                        Name = item.Type == MTFTableColumnType.GoldSample ? string.Format("{0} (+/-{1}%)", item.Header, goldSamplePercentage) : item.Header,
                        Value = item.Value == null ? null : item.GetValueAsString(),
                        Status = item.Status,
                        IsSet = item.IsSet
                    };
                    if (item.Type == MTFTableColumnType.GoldSample)
                    {
                        var roundedValue = MTFSequenceHelper.RoundActualValue(item.Value, roundingRules);
                        var roundedValueString = roundedValue == null ? null : roundedValue.ToString();
                        condition.RoundedValue = roundedValueString == condition.Value ? null : roundedValueString;
                    }
                    yield return condition;
                }
            }
        }

        private void ChangeRowVisibility(bool value)
        {
            if (Rows != null)
            {
                foreach (var row in Rows)
                {
                    row.SetHiddenStatus(value);
                }
            }
        }

        #endregion

        #region Override

        public override string ObjectVersion
        {
            get { return "1.0.4"; }
        }

        protected override void VersionConvert(string fromVersion)
        {
            base.VersionConvert(fromVersion);
            if (fromVersion == "1.0.0")
            {
                CheckMultipleFilling = true;
                fromVersion = "1.0.1";
            }
            if (fromVersion == "1.0.1")
            {
                if (UseGoldSample)
                {
                    VersionConvertHelper.ReloadGsColumn(this);
                }
                fromVersion = "1.0.2";
            }
            if (fromVersion == "1.0.2")
            {
                VersionConvertHelper.ClearTerms(this);
                fromVersion = "1.0.3";
            }
            if (fromVersion == "1.0.3")
            {
                VersionConvertHelper.CreateHiddenColumn(this);
                fromVersion = "1.0.4";
            }
        }

        protected override object CloneInternal(bool copyIdValue)
        {
            var newTable = base.CloneInternal(copyIdValue) as MTFValidationTable;
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

        public override string ToString()
        {
            return Name;
        }

        #endregion

        
    }
}
