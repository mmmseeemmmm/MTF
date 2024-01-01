using System;
using MTFApp.UIHelpers;

namespace MTFApp.SequenceEditor.GraphicalView
{
    public abstract class GraphicalViewTableItemBase : NotifyPropertyBase
    {
        private bool isUsed;

        public bool IsUsed
        {
            get => isUsed;
            set
            {
                isUsed = value;
                NotifyPropertyChanged();
            }
        }

        public abstract string Alias { get; }
        public abstract Guid TableId  { get; }
        public abstract Guid RowId  { get; }
    }
}