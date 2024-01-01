using System;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.Settings.SequenceVariant
{
    public class VariantValueSetting : NotifyPropertyBase
    {
        private EditVariantMode editMode;
        private string newName;

        public VariantValueSetting()
        {
            Id = Guid.NewGuid();
        }

        public SequenceVariantValue OriginalValue { get; set; }
        public Guid Id { get; set; }

        public string NewName
        {
            get => newName;
            set
            {
                newName = value;
                NotifyPropertyChanged();
            }
        }

        public EditVariantMode EditMode
        {
            get => editMode;
            set
            {
                editMode = value;
                NotifyPropertyChanged();
            }
        }

        public VariantValueSetting GetCopy => new VariantValueSetting
                                              {
                                                  Id = this.Id,
                                                  editMode = this.EditMode,
                                                  newName = this.NewName,
                                                  OriginalValue = this.OriginalValue
                                              };
    }
}