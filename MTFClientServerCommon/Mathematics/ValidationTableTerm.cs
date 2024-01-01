using MTFClientServerCommon.MTFValidationTable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.MTFTable;
using MTFCommon;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class ValidationTableTerm : Term, ITermErrorHandling
    {
        private bool registrRowEvent = false;

        public ValidationTableTerm()
        {
        }
        public ValidationTableTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            IdentityObjectReplaced += ValidationTableTerm_IdentityObjectReplaced;
        }

        void ValidationTableTerm_IdentityObjectReplaced(MTFDataTransferObject sender, MTFDataTransferObject newObject, string propertyName)
        {
            if (propertyName == nameof(ValidationTable))
            {
                if (newObject is MTFValidationTable.MTFValidationTable table && table.Rows != null)
                {
                    table.Rows.CollectionChanged += Rows_CollectionChanged;
                }
            }
        }

        void Rows_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Rows == null)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (var item in e.OldItems)
                        {
                            var rowToDelete = Rows.FirstOrDefault(x => x.Row == item as MTFValidationTableRow);
                            if (rowToDelete != null)
                            {
                                Rows.Remove(rowToDelete);
                            }
                        }

                        break;
                    }

                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (var item in e.NewItems)
                        {
                            Rows.Add(new ExtendedRow(item as MTFValidationTableRow));
                        }

                        break;
                    }

                case NotifyCollectionChangedAction.Move:
                    Rows.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
            }
        }

        [MTFPersistIdOnly]
        public MTFValidationTable.MTFValidationTable ValidationTable
        {
            get => GetProperty<MTFValidationTable.MTFValidationTable>();
            set
            {
                registrRowEvent = true;
                SetProperty(value);
            }
        }

        public void RegistrRowChangedEvent(MTFValidationTable.MTFValidationTable oldTable, MTFValidationTable.MTFValidationTable newTable)
        {
            if (registrRowEvent)
            {
                if (oldTable?.Rows != null)
                {
                    oldTable.Rows.CollectionChanged -= Rows_CollectionChanged;
                }
                if (newTable?.Rows != null)
                {
                    newTable.Rows.CollectionChanged -= Rows_CollectionChanged;
                    newTable.Rows.CollectionChanged += Rows_CollectionChanged;
                }
                registrRowEvent = false;
                CheckRows();
            }

        }

        protected override object CloneInternal(bool copyIdValue)
        {
            var cloneObject = base.CloneInternal(copyIdValue) as ValidationTableTerm;
            if (cloneObject != null)
            {
                cloneObject.IdentityObjectReplaced += cloneObject.ValidationTableTerm_IdentityObjectReplaced;
            }

            return cloneObject;
        }

        public SequenceVariant SequenceVariant { get; set; }

        public bool IsGoldSample { get; set; }


        private void CheckRows()
        {
            if (ValidationTable?.Rows != null)
            {
                int i = 0;
                bool onlyAdd = false;
                if (Rows == null)
                {
                    Rows = new ObservableCollection<ExtendedRow>();
                    onlyAdd = true;
                }
                var originalRows = Rows.ToList();
                foreach (var tableRow in ValidationTable.Rows)
                {
                    if (!onlyAdd && i < Rows.Count)
                    {
                        if (Rows[i].Row != tableRow)
                        {
                            var extendedRow = originalRows.FirstOrDefault(x => x.Row == tableRow);
                            if (extendedRow != null)
                            {
                                Rows[i] = extendedRow;
                            }
                            else
                            {
                                Rows[i] = new ExtendedRow(tableRow);
                            }
                        }
                    }
                    else
                    {
                        Rows.Add(new ExtendedRow(tableRow));
                    }

                    i++;
                }
                if (i < Rows.Count)
                {
                    var count = Rows.Count - i;
                    for (int j = 0; j < count; j++)
                    {
                        Rows.RemoveAt(Rows.Count - 1);
                    }
                }
                originalRows.Clear();
            }
        }



        public ObservableCollection<ExtendedRow> Rows
        {
            get => GetProperty<ObservableCollection<ExtendedRow>>();
            set => SetProperty(value);
        }

        public IEnumerable<MTFValidationTableRow> ValidatedRows
        {
            get
            {
                if (Rows != null)
                {
                    foreach (var item in Rows)
                    {
                        if (item.IsSet || (InjectedTable != null && InjectedTable.ValidateAllRows))
                        {
                            yield return item.Row;
                        }
                    }
                }
            }
        }

        public List<MTFValidationTableRowResult> GetValidatedRowsForDisplaying(DateTime timeStamp, Guid tableId, uint numberOfRepetition, Guid? dutId, out List<MTFValidationTableRow> rowsToLog)
        {
            var outputList = new List<MTFValidationTableRowResult>();
            rowsToLog = new List<MTFValidationTableRow>();

            bool firstRow = true;
            if (Rows != null)
            {
                foreach (var item in Rows)
                {
                    if (item.IsSet)
                    {
                        item.InjectedRow.NumberOfRepetition = numberOfRepetition;
                        rowsToLog.Add(item.InjectedRow);
                    }
                    if (item.IsHidden)
                    {
                        continue;
                    }
                    if (item.IsSet || InjectedTable.ValidateAllRows)
                    {
                        var newRowResult = new MTFValidationTableRowResult()
                        {
                            Row = item.InjectedRow,
                            TableName = InjectedTable.Name,
                            TableStatus = InjectedTable.Status,
                            TableId = tableId,
                            IsValidated = item.IsSet,
                            DutId = dutId,
                        };
                        if (firstRow)
                        {
                            newRowResult.HasTimeStamp = true;
                            newRowResult.TimeStamp = timeStamp;
                            firstRow = false;
                        }
                        outputList.Add(newRowResult);
                    }
                }
            }
            return outputList;
        }

        public string ActivityPath { get; set; }
        public string ErrorText => InjectedTable != null ? InjectedTable.ErrorText : string.Empty;

        public override object Evaluate()
        {
            bool result = true;

            if (Rows != null)
            {
                InjectedTable.HasAlreadyInLog = false;
                InjectedTable.SetValidationTime();
                foreach (var item in Rows)
                {
                    if (item.InjectedRow != null && item.IsSet)
                    {
                        var evalueatedActualValue = item.ActualValue.Evaluate();
                        item.InjectedRow.SetActualValue(evalueatedActualValue);
                        if (IsGoldSample)
                        {
                            item.InjectedRow.SetGoldSampleValue(evalueatedActualValue, SequenceVariant);
                        }
                        var currentRowResult = item.InjectedRow.IsEvaluated ? item.InjectedRow.Status == MTFValidationTableStatus.Ok :
                            item.InjectedRow.Evaluate(item.ActualValue, InjectedTable, SequenceVariant, IsGoldSample);
                        if (!item.IsHidden)
                        {
                            result &= currentRowResult;
                        }
                    }
                }


                if (InjectedTable.RowsCount == 0)
                {
                    InjectedTable.Status = MTFValidationTableStatus.NotFilled;
                }
                else
                {
                    if (InjectedTable.ValidateAllRows)
                    {
                        bool statusIsSet = false;
                        bool hasNok = false;
                        bool hasGsFail = false;
                        foreach (var row in InjectedTable.Rows)
                        {
                            if (row.IsHidden)
                            {
                                continue;
                            }
                            if (row.Status == MTFValidationTableStatus.NotFilled)
                            {
                                InjectedTable.Status = MTFValidationTableStatus.NotFilled;
                                statusIsSet = true;
                                break;
                            }
                            if (!hasNok)
                            {
                                if (row.Status == MTFValidationTableStatus.Nok)
                                {
                                    hasNok = true;
                                }
                                else if (!hasGsFail && row.Status == MTFValidationTableStatus.GSFail)
                                {
                                    hasGsFail = true;
                                }
                            }
                        }
                        if (!statusIsSet)
                        {
                            InjectedTable.Status = hasNok
                                ? MTFValidationTableStatus.Nok
                                : (hasGsFail ? MTFValidationTableStatus.GSFail : MTFValidationTableStatus.Ok);
                        }
                    }
                    else
                    {
                        bool hasGsFail = false;
                        bool statusIsSet = false;
                        foreach (var row in InjectedTable.Rows)
                        {
                            if (row.IsHidden)
                            {
                                continue;
                            }
                            if (row.Status == MTFValidationTableStatus.Nok)
                            {
                                InjectedTable.Status = MTFValidationTableStatus.Nok;
                                statusIsSet = true;
                                break;
                            }
                            if (!hasGsFail && row.Status == MTFValidationTableStatus.GSFail)
                            {
                                hasGsFail = true;
                            }
                        }
                        if (!statusIsSet)
                        {
                            InjectedTable.Status = hasGsFail ? MTFValidationTableStatus.GSFail : MTFValidationTableStatus.Ok;
                        }
                    }
                }
            }
            else
            {
                result = false;
            }
            if (InjectedTable.AllowNokGsForTable && SequenceVariant != null && SequenceVariant.Match(InjectedTable.NokVariantSelectorForTable))
            {
                InjectedTable.Status = InjectedTable.Status.InvertResult();
                //InjectedTable.InvertRowResults();
                result = !result;
            }
            return result;
        }


        public override Type ResultType => typeof(bool);

        public override string Symbol => "ValidationTable";

        public override TermGroups TermGroup =>
            TermGroups.None
            | TermGroups.LogicalTerm
            | TermGroups.NumberTerm
            | TermGroups.ObjectTerm
            | TermGroups.StringTerm;

        public override TermGroups ChildrenTermGroup => TermGroups.None;

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon => AutomotiveLighting.MTFCommon.MTFIcons.TermFillValidationTable;

        public override string Label => "Fill Validation Table";

        public override string ToString()
        {
            if (ValidationTable != null && !string.IsNullOrEmpty(ValidationTable.Name))
            {
                var wrongRows = ValidatedRows?.Where(r => r.Status == MTFValidationTableStatus.Nok).ToList();
                if (wrongRows != null && wrongRows.Any())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(ValidationTable.Name).Append(": ");
                    bool firstRow = true;
                    foreach (var wrongRow in wrongRows)
                    {
                        if (!firstRow)
                        {
                            sb.Append(", ");
                        }
                        sb.Append(wrongRow.Header).Append(" ").Append(wrongRow.ErrorText);
                        firstRow = false;
                    }
                    return sb.ToString();
                }

                return ValidationTable.Name;
            }
            return "Unset Validation Table";
        }

        public MTFValidationTable.MTFValidationTable InjectedTable { get; set; }
    }

    [Serializable]
    public class ExtendedRow : MTFDataTransferObject
    {
        [MTFPersistIdOnly]
        public MTFValidationTableRow Row
        {
            get => GetProperty<MTFValidationTableRow>();
            set => SetProperty(value);
        }

        public MTFValidationTableRow InjectedRow { get; set; }

        public bool IsSet => !(ActualValue is EmptyTerm || (ActualValue is ActivityResultTerm actualValue && actualValue.Value == null));

        public bool IsSelected { get; set; }

        public bool IsHidden => Row != null && Row.IsHidden;

        public Term ActualValue
        {
            get => GetProperty<Term>();
            set => SetProperty(value);
        }

        public ExtendedRow(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public ExtendedRow(MTFValidationTableRow row)
        {
            Row = row;
            ActualValue = new EmptyTerm(GetExpectedTypeName());
        }

        public ExtendedRow()
        {

        }

        public void RefreshActualValue()
        {
            NotifyPropertyChanged(nameof(ActualValue));
        }

        public string GetExpectedTypeName()
        {
            if (Row?.Items != null)
            {
                if (Row.Items.Any(x => x.Type == MTFTableColumnType.Value && x.IsSet && !x.Column.HasListTerm))
                {
                    return typeof(double).FullName;
                }
            }

            return typeof(string).FullName;
        }
    }
}
