using System;
using System.Collections.ObjectModel;
using System.Linq;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.Settings.SequenceVariant
{
    public class ExternalSequenceVariantGroup : NotifyPropertyBase
    {
        private ObservableCollection<VariantValueSetting> valueSetting = new ObservableCollection<VariantValueSetting>();
        private SequenceVariantGroup group;
        private bool hasError;
        public Guid SequenceId { get; set; }
        public string SequenceName { get; set; }

        public bool HasError
        {
            get => hasError;
            set
            {
                hasError = value;
                NotifyPropertyChanged();
            }
        }

        public SequenceVariantGroup Group
        {
            get => group;
            set
            {
                group = value;
                ValueSetting =
                    new ObservableCollection<VariantValueSetting>(value.Values.Select(x => new VariantValueSetting {OriginalValue = x}));
            }
        }

        public ObservableCollection<VariantValueSetting> ValueSetting
        {
            get => valueSetting;
            set
            {
                valueSetting = value;
                NotifyPropertyChanged();
            }
        }
    }
}