using MTFClientServerCommon.Mathematics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon.GoldSamplePersist;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.MTFTable;
using MTFCommon;

namespace MTFClientServerCommon.MTFValidationTable
{
    [Serializable]
    public class MTFValidationTableRow : MTFDataTransferObject, ITableStatus
    {
        #region ctor
        public MTFValidationTableRow(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public MTFValidationTableRow()
        {

        }

        public MTFValidationTableRow(int columnsCount, MTFValidationTable table)
        {
            Items = new ObservableCollection<MTFValidationTableCell>();
            for (int i = 0; i < columnsCount; i++)
            {
                var column = table.Columns[i];
                var cell = new MTFValidationTableCell(column);
                Items.Add(cell);
            }
        }
        #endregion

        #region Properties

        public ObservableCollection<MTFValidationTableCell> Items
        {
            get { return GetProperty<ObservableCollection<MTFValidationTableCell>>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<MTFValidationTableRowVariant> RowVariants
        {
            get { return GetProperty<ObservableCollection<MTFValidationTableRowVariant>>(); }
            set { SetProperty(value); }
        }

        public string Header
        {
            get { return Items != null && Items.Count > 0 ? Items[MTFValidationTable.IdentificationPosition].Value as string : string.Empty; }
        }

        public bool IsHidden
        {
            get
            {
                return Items != null && Items.Count > 0 && Items[MTFValidationTable.HiddenValuePosition].Value is bool &&
                       (bool)Items[MTFValidationTable.HiddenValuePosition].Value;
            }
        }

        public string VariantHeader
        {
            get
            {
                string postFix = "Row Variants";
                if (!string.IsNullOrEmpty(Header))
                {
                    return string.Format("{1} ({0})", Header, postFix);
                }
                return postFix;
            }
        }

        public MTFCommon.MTFValidationTableStatus Status
        {
            get { return GetProperty<MTFCommon.MTFValidationTableStatus>(); }
            set { SetProperty(value); }
        }


        public uint NumberOfRepetition
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public int EvaluatedVariant
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public double GoldSamplePercentage
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public bool IsActualValueImage
        {
            get { return GetActualValue() is MTFImage; }
        }

        public bool AllowNokGs
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public SequenceVariant NokVariantSelector
        {
            get { return GetProperty<SequenceVariant>(); }
            set { SetProperty(value); }
        }

        //Due to different structure of row in service tables
        #region Service hack
        public MTFValidationTableRow OriginalRowForService { get; set; }

        public SequenceVariant SequenceVariantForService { get; set; }
        #endregion


        public string ErrorText { get; set; }

        public bool IsEvaluated { get; set; }

        #endregion


        #region Public methods

        public void AddNewCell(MTFValidationTableColumn column)
        {
            Items.Add(new MTFValidationTableCell(column));
            if (RowVariants != null)
            {
                foreach (var rowVariant in RowVariants)
                {
                    rowVariant.AddColumn(column);
                }
            }
        }

        public void RemoveCell(int columnIndex)
        {
            var cell = Items[columnIndex];
            if (RowVariants != null)
            {
                foreach (var rowVariant in RowVariants)
                {
                    rowVariant.RemoveColumn(cell);
                }
            }
            Items.Remove(cell);
        }

        public bool Evaluate(Term actualValue, MTFValidationTable table, SequenceVariant sequenceVariant, bool isGoldSample)
        {
            MTFValidationTableStatus result = MTFValidationTableStatus.Ok;
            ErrorText = string.Empty;
            bool invertResult = false;

            bool evaluateGoldSample = table.Columns.Any(x => x.Type == MTFTableColumnType.Value && (x.ValidationTerm is BinaryLogicalTerm));
            int index = 0;
            var variant = GetVariantToEvaluate(sequenceVariant);
            var itemsToEvaluate = Items;
            if (variant != null)
            {
                itemsToEvaluate = variant.Items;
                foreach (var cell in Items)
                {
                    cell.Status = true;
                }
            }
            do
            {
                var condition = table.GetColumnCondition(index);
                if (condition != null)
                {
                    var itemResult = itemsToEvaluate[index].Evaluate(actualValue, condition, evaluateGoldSample, isGoldSample);
                    if (result == MTFValidationTableStatus.Ok)
                    {
                        result = itemResult;
                    }
                    else if (result == MTFValidationTableStatus.GSFail && itemResult == MTFValidationTableStatus.Nok)
                    {
                        result = itemResult;
                    }
                    if (itemResult == MTFValidationTableStatus.Nok)
                    {
                        if (!string.IsNullOrEmpty(ErrorText))
                        {
                            ErrorText += ", ";
                        }
                        ErrorText += condition.ToString();
                    }
                }

                index++;

            } while (index < itemsToEvaluate.Count);

            if (sequenceVariant != null && AllowNokGs)
            {
                Status.InvertResult();
                invertResult = sequenceVariant.Match(NokVariantSelector);
            }

            Status = invertResult ? result.InvertResult() : result;
            if (variant != null)
            {
                variant.Status = Status;
            }
            return Status == MTFValidationTableStatus.Ok || Status == MTFValidationTableStatus.GSFail;
        }


        private MTFValidationTableRowVariant GetVariantToEvaluate(SequenceVariant sequenceVariant)
        {
            MTFValidationTableRowVariant output = null;
            bool compare = true;
            EvaluatedVariant = -1;
            if (sequenceVariant != null && RowVariants != null)
            {
                var bestVariant = sequenceVariant.GetBestMatch(RowVariants.Select(x => x.SequenceVariant));
                if (bestVariant != null)
                {
                    int i = 0;
                    foreach (var rowVariant in RowVariants)
                    {
                        if (compare && Equals(rowVariant.SequenceVariant, bestVariant))
                        {
                            EvaluatedVariant = i;
                            output = rowVariant;
                            compare = false;
                        }
                        else
                        {
                            rowVariant.Status = MTFValidationTableStatus.NotFilled;
                            rowVariant.SetGSStatus(true);
                        }
                        i++;
                    }
                }
            }
            return output;
        }

        public void SetActualValue(object value)
        {
            if (!IsSerializable(value))
            {
                throw new Exception(string.Format("Seting of row {0} is not possible. Object of type {1} isn't serializable. Non serializable object isn't usable in validation table.", Header, value.GetType().FullName));
            }

            Items[MTFValidationTable.ActualValuePosition].Value = value;
        }

        private static bool IsSerializable(object obj)
        {
            if (obj == null || obj is ISerializable)
            {
                return true;
            }
            return Attribute.IsDefined(obj.GetType(), typeof(SerializableAttribute));
        }

        public void LoadGoldSampleValue(GoldSampleValue goldSampleValue)
        {
            if (goldSampleValue != null)
            {
                GoldSampleHelper.AssignGoldSampleValue(Items, goldSampleValue.DefaultValue);
                if (RowVariants != null && RowVariants.Count > 0 && goldSampleValue.VariantValues != null && goldSampleValue.VariantValues.Count > 0)
                {
                    foreach (var variantValue in goldSampleValue.VariantValues)
                    {
                        var variant = RowVariants.FirstOrDefault(x => x.Id == variantValue.Key);
                        if (variant != null)
                        {
                            GoldSampleHelper.AssignGoldSampleValue(variant.Items, variantValue.Value);
                        }
                    }
                }
            }
        }

        public GoldSampleValue GetGoldSampleValues()
        {
            var gs = new GoldSampleValue { DefaultValue = GoldSampleHelper.GetGoldSampleValue(Items) };
            if (RowVariants != null && RowVariants.Count > 0)
            {
                gs.VariantValues = new Dictionary<Guid, object>();
                foreach (var rowVariant in RowVariants)
                {
                    gs.VariantValues.Add(rowVariant.Id, GoldSampleHelper.GetGoldSampleValue(rowVariant.Items));
                }
            }
            return gs;
        }

        public void SetGoldSampleValue(object value, SequenceVariant sequenceVariant)
        {
            var currentItems = Items;
            if (RowVariants != null && RowVariants.Count > 0)
            {
                var variant = sequenceVariant.GetBestMatch(RowVariants.Select(x => x.SequenceVariant));
                if (variant != null)
                {
                    var rowVariant = RowVariants.FirstOrDefault(x => Equals(x.SequenceVariant, variant));
                    if (rowVariant != null)
                    {
                        currentItems = rowVariant.Items;
                    }
                }
            }
            GoldSampleHelper.AssignGoldSampleValue(currentItems, value);
        }


        public object GetValue(string selectedColumn, SequenceVariant sequenceVariant)
        {

            var variant = selectedColumn == MTFValidationTable.ActualValueHeader || selectedColumn == MTFValidationTable.NameHeader
                ? null
                : GetVariantToEvaluate(sequenceVariant);
            var itemsToEvaluate = variant == null ? Items : variant.Items;
            if (itemsToEvaluate != null && itemsToEvaluate.Count > 0)
            {
                MTFValidationTableCell column;
                if (selectedColumn.EndsWith("as string"))
                {
                    var columnName = selectedColumn.Replace(" as string", string.Empty);
                    column = itemsToEvaluate.FirstOrDefault(x => x.Column.Header == columnName);
                    if (column != null && column.Value is IEnumerable<string>)
                    {
                        return string.Join("; ", ((IEnumerable<string>)column.Value));
                    }
                }

                column = itemsToEvaluate.FirstOrDefault(x => x.Column.Header == selectedColumn);
                if (column != null)
                {
                    return column.Value;
                }
            }
            return null;
        }

        public object GetActualValue()
        {
            if (Items.Count > 1)
            {
                return Items[MTFValidationTable.ActualValuePosition].Value;
            }
            return null;
        }

        public MTFValidationTableCell GetActualValueCell()
        {
            return Items.Count > 1 ? Items[MTFValidationTable.ActualValuePosition] : null;
        }

        public void MarkChangedValue()
        {
            if (Items.Count > 1)
            {
                Items[MTFValidationTable.ActualValuePosition].IsChanged = true;
            }
        }

        public void UnmarkChangedValue()
        {
            if (Items.Count > 1)
            {
                Items[MTFValidationTable.ActualValuePosition].IsChanged = false;
            }
        }


        public void AddVariant()
        {
            if (RowVariants == null)
            {
                RowVariants = new MTFObservableCollection<MTFValidationTableRowVariant>();
            }
            var variant = new MTFValidationTableRowVariant { Items = new MTFObservableCollection<MTFValidationTableCell>() };
            foreach (var mtfValidationTableCell in Items)
            {
                var newCell = new MTFValidationTableCell(mtfValidationTableCell.Column)
                {
                    IsSet = mtfValidationTableCell.IsSet,
                    Value = mtfValidationTableCell.Value
                };
                variant.Items.Add(newCell);
            }
            RowVariants.Add(variant);
            NotifyPropertyChanged("RowVariants");
        }

        public void RemoveVariant(MTFValidationTableRowVariant variant)
        {
            if (RowVariants != null)
            {
                RowVariants.Remove(variant);
                if (RowVariants.Count == 0)
                {
                    RowVariants = null;
                }
            }
        }



        public void AssignEvaluatedVarianst()
        {
            if (EvaluatedVariant > -1 && RowVariants != null && EvaluatedVariant < RowVariants.Count)
            {
                var validatedItems = RowVariants[EvaluatedVariant].Items;
                for (int i = 0; i < validatedItems.Count; i++)
                {
                    if (validatedItems[i].Type == MTFTableColumnType.Value || validatedItems[i].Type == MTFTableColumnType.GoldSample)
                    {
                        Items[i].Value = validatedItems[i].Value;
                        Items[i].Status = validatedItems[i].Status;
                        Items[i].IsSet = validatedItems[i].IsSet;
                    }
                }
            }
        }

        //public void PrepareForDebug()
        //{
        //    if (EvaluatedVariant > -1 && RowVariants != null && EvaluatedVariant < RowVariants.Count)
        //    {
        //        var variant = RowVariants[EvaluatedVariant];
        //        var actualValueCell = variant.Items.FirstOrDefault(x => x.Type == MTFTableColumnType.ActualValue);
        //        if (actualValueCell != null)
        //        {
        //            actualValueCell.Value = GetActualValue();
        //            variant.IsExpandable = actualValueCell.Value is MTFImage;
        //        }
        //        SetActualValue(null);

        //    }
        //}

        public MTFValidationTableRow PrepareForDebug()
        {
            if (EvaluatedVariant > -1 && RowVariants != null && EvaluatedVariant < RowVariants.Count)
            {
                var variant = RowVariants[EvaluatedVariant];
                var r = new MTFValidationTableRow
                {
                    Id = Id,
                    Items = variant.Items,
                    Status = variant.Status,
                    NumberOfRepetition = NumberOfRepetition,
                    GoldSamplePercentage = GoldSamplePercentage,
                    RowVariants = new MTFObservableCollection<MTFValidationTableRowVariant>(),
                    SequenceVariantForService = variant.SequenceVariant,
                    OriginalRowForService = this,
                };
                var defaultVariant = new MTFValidationTableRowVariant
                {
                    Items = Items,
                    Status = Status,
                };
                r.RowVariants.Add(defaultVariant);
                foreach (var rowVariant in RowVariants)
                {
                    if (rowVariant != variant)
                    {
                        r.RowVariants.Add(rowVariant);
                    }
                }
                r.SetActualValue(GetActualValue());
                defaultVariant.SetActualValue(null);
                return r;
            }
            return this;
        }


        public void SetPercentage(double value)
        {
            GoldSamplePercentage = value;
        }

        public void SetHiddenStatus(bool value)
        {
            if (Items != null && Items.Count > MTFValidationTable.HiddenValuePosition)
            {
                Items[MTFValidationTable.HiddenValuePosition].Value = value;
            }
        }


        #endregion

    }
}
