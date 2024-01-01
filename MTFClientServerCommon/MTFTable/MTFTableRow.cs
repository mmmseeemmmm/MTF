using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace MTFClientServerCommon.MTFTable
{
    [Serializable]
    public class MTFTableRow : MTFDataTransferObject
    {

        #region ctor

        public MTFTableRow()
        {

        }

        public MTFTableRow(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public MTFTableRow(int columnCount, IMTFDataTable table)
        {
            Items = new MTFObservableCollection<MTFTableCell>();
            for (int i = 0; i < columnCount; i++)
            {
                var column = table.Columns[i];
                var cell = new MTFTableCell(column);
                Items.Add(cell);
            }
        }

        #endregion

        #region Properties

        public ObservableCollection<MTFTableCell> Items
        {
            get { return GetProperty<ObservableCollection<MTFTableCell>>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<MTFTableRowVariant> RowVariants
        {
            get { return GetProperty<ObservableCollection<MTFTableRowVariant>>(); }
            set { SetProperty(value); }
        }

        public string Header
        {
            get
            {
                if (Items != null && Items.Count > 0)
                {
                    return Items[0].Value as string;
                }
                return string.Empty;
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

        #endregion


        #region public methods

        public void AddCell(MTFTableColumn column)
        {
            Items.Add(new MTFTableCell(column));
            if (RowVariants!=null)
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
            if (RowVariants!=null)
            {
                foreach (var rowVariant in RowVariants)
                {
                   rowVariant.RemoveColumn(cell);     
                }
            }
            Items.Remove(cell);
        }

        public object GetValue(string columnName, SequenceVariant sequenceVariant)
        {
            if (RowVariants == null)
            {
                var localcolumnn = Items.First(i => i.Column.Header == columnName);
                return localcolumnn != null ? localcolumnn.Value : null;
            }

            var variant = sequenceVariant.GetBestMatch(RowVariants.Select(x => x.SequenceVariant));
            MTFTableRowVariant rowVariant = null;
            if (variant!=null)
            {
                rowVariant = RowVariants.FirstOrDefault(x => Equals(x.SequenceVariant, variant));
            }
            var itemsToEvaluate = rowVariant == null ? Items : rowVariant.Items;
            var column = itemsToEvaluate.FirstOrDefault(x => x.Column.Header == columnName);
            return column != null ? column.Value : null;
        }

        public void AddVariant()
        {
            if (RowVariants == null)
            {
                RowVariants = new MTFObservableCollection<MTFTableRowVariant>();
            }
            var variant = new MTFTableRowVariant { Items = new MTFObservableCollection<MTFTableCell>() };
            foreach (var mtfTableCell in Items)
            {
                var newCell = new MTFTableCell(mtfTableCell.Column) { Value = mtfTableCell.Value };
                variant.Items.Add(newCell);
            }
            RowVariants.Add(variant);
            NotifyPropertyChanged("RowVariants");
        }

        public void RemoveVariant(MTFTableRowVariant variant)
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

        #endregion





    }
}
