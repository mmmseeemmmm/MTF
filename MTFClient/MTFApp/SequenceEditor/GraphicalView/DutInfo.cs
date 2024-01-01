using System;
using MTFApp.UIHelpers;

namespace MTFApp.SequenceEditor.GraphicalView
{
    public class DutInfo : NotifyPropertyBase
    {
        private bool isSelected;
        public Guid Id { get; set; }
        public string Name { get; set; }

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                NotifyPropertyChanged();
            }
        }
    }
}