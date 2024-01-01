using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace MTFClientServerCommon.MTFTable
{
    [Serializable]
    public class MTFTableRowVariant : MTFDataTransferObject
    {
        public MTFTableRowVariant()
        {
            
        }

        public MTFTableRowVariant(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }

        public SequenceVariant SequenceVariant
        {
            get => GetProperty<SequenceVariant>();
            set => SetProperty(value);
        }

        public ObservableCollection<MTFTableCell> Items
        {
            get => GetProperty<ObservableCollection<MTFTableCell>>();
            set => SetProperty(value);
        }

        public void RemoveColumn(MTFTableCell cell)
        {
            Items?.Remove(Items.FirstOrDefault(c => c.Column == cell.Column));
        }

        public void AddColumn(MTFTableColumn column)
        {
            Items?.Add(new MTFTableCell(column));
        }

        public void AssignColumns(IList<MTFTableColumn> columns)
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
