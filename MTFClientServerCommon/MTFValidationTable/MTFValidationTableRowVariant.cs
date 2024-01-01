using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon.MTFTable;

namespace MTFClientServerCommon.MTFValidationTable
{
    [Serializable]
    public class MTFValidationTableRowVariant : MTFDataTransferObject
    {
        private bool isCollapsed;

        public MTFValidationTableRowVariant(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            isCollapsed = true;
        }

        public MTFValidationTableRowVariant()
        {
            isCollapsed = true;
        }
        public SequenceVariant SequenceVariant
        {
            get { return GetProperty<SequenceVariant>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<MTFValidationTableCell> Items
        {
            get { return GetProperty<ObservableCollection<MTFValidationTableCell>>(); }
            set { SetProperty(value); }
        }

        public MTFCommon.MTFValidationTableStatus Status
        {
            get { return GetProperty<MTFCommon.MTFValidationTableStatus>(); }
            set { SetProperty(value); }
        }

        public void RemoveColumn(MTFValidationTableCell cell)
        {
            if (Items != null)
            {
                Items.Remove(Items.FirstOrDefault(c => c.Column == cell.Column));
            }
        }

        public void AddColumn(MTFValidationTableColumn colum)
        {
            if (Items != null)
            {
                Items.Add(new MTFValidationTableCell(colum));
            }
        }

        public void SetGSStatus(bool status)
        {
            if (Items != null && Items.Count > 0)
            {
                var gsColumn = Items.Last();
                if (gsColumn.Type == MTFTableColumnType.GoldSample)
                {
                    gsColumn.Status = status;
                }
            }
        }

        public bool IsActualValueImage
        {
            get { return GetActualValue() is MTFImage; }
        }

        public object GetActualValue()
        {
            if (Items.Count > 1)
            {
                return Items[MTFValidationTable.ActualValuePosition].Value;
            }
            return null;
        }

        public void SetActualValue(object value)
        {
            if (Items != null && Items.Count > 0)
            {
                var actualValueItem = Items.FirstOrDefault(x => x.Type == MTFTableColumnType.ActualValue);
                if (actualValueItem != null)
                {
                    actualValueItem.Value = value;
                }
            }
        }

        public void AssignColumns(IList<MTFValidationTableColumn> columns)
        {
            if (Items != null)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (i < columns.Count)
                    {
                        Items[i].Column = columns[i];
                    }
                }
            }
        }
    }
}
