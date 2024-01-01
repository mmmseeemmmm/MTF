using MTFApp.UIHelpers;

namespace MTFApp.UIControls.MultiSelectComboBox
{
    public class ComboBoxItem : NotifyPropertyBase
    {
        private bool isSelected;
        private object displayValue;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                NotifyPropertyChanged();
            }
        }

        public object Value { get; set; }

        public object DisplayValue
        {
            get => displayValue;
            set
            {
                displayValue = value;
                NotifyPropertyChanged();
            }
        }

        public object OutputValue { get; set; }

        public override string ToString()
        {
            return Value != null ? Value.ToString() : string.Empty;
        }
    }
}