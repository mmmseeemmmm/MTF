using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.Settings.SequenceVariant
{
    public class VariantGroupPackage : NotifyPropertyBase
    {
        private readonly string sequenceName;
        private ObservableCollection<VariantValueSetting> valueSetting = new ObservableCollection<VariantValueSetting>();
        private bool isActive;
        private bool changeInExternal;
        private List<ExternalSequenceVariantGroup> externalSequenceVariantGroups;

        public VariantGroupPackage(string name, Guid sequenceId, string sequenceName)
        {
            this.SequenceId = sequenceId;
            this.sequenceName = sequenceName;
            Id = Guid.NewGuid();
            Name = name;
        }

        public Guid Id { get; }
        public string Name { get; }

        public bool CanUpdateExternal { get; set; } = true;

        public bool ChangeInExternal
        {
            get => changeInExternal;
            set
            {
                changeInExternal = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                NotifyPropertyChanged();
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

        public List<ExternalSequenceVariantGroup> ExternalSequenceVariantGroups
        {
            get => externalSequenceVariantGroups;
            set
            {
                externalSequenceVariantGroups = value;
                NotifyPropertyChanged();
            }
        }

        public MainSequenceVariantResult Result { get; set; }

        public Guid SequenceId { get; }

        public void AddNewItem()
        {
            var newValue = new VariantValueSetting {EditMode = EditVariantMode.Add};
            newValue.PropertyChanged += Setting_PropertyChanged;
            ValueSetting.Add(newValue);
            AddToExternal(newValue);
        }


        public void GenerateValues(SequenceVariantGroup group, List<ExternalSequenceVariantGroup> externalVariantGroups)
        {
            ValueSetting.Clear();

            foreach (var sequenceVariantValue in group.Values)
            {
                var setting = new VariantValueSetting
                              {
                                  OriginalValue = sequenceVariantValue,
                              };
                setting.PropertyChanged += Setting_PropertyChanged;
                ValueSetting.Add(setting);
            }

            ExternalSequenceVariantGroups = externalVariantGroups;
        }

        private void Setting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var setting = (VariantValueSetting)sender;

            switch (e.PropertyName)
            {
                case nameof(VariantValueSetting.EditMode):
                case nameof(VariantValueSetting.NewName):
                    UpdateExternalMode(setting);
                    break;
            }
        }

        private void UpdateExternalMode(VariantValueSetting setting)
        {
            if (CanUpdateExternal && ExternalSequenceVariantGroups != null)
            {
                foreach (var externalSequenceVariantGroup in ExternalSequenceVariantGroups)
                {
                    VariantValueSetting group;

                    if (setting.EditMode == EditVariantMode.Add)
                    {
                        group = externalSequenceVariantGroup.ValueSetting.FirstOrDefault(x => x.Id == setting.Id);
                    }
                    else
                    {
                        group = externalSequenceVariantGroup.ValueSetting.FirstOrDefault(
                            x => x.OriginalValue?.Name == setting.OriginalValue.Name);
                    }

                    if (group != null)
                    {
                        group.EditMode = setting.EditMode;
                        group.NewName = setting.NewName;
                    }
                    else
                    {
                        externalSequenceVariantGroup.HasError = true;
                    }
                }
            }
        }

        private void AddToExternal(VariantValueSetting newSetting)
        {
            if (ExternalSequenceVariantGroups != null)
            {
                foreach (var externalSequenceVariantGroup in ExternalSequenceVariantGroups)
                {
                    externalSequenceVariantGroup.ValueSetting.Add(newSetting.GetCopy);
                }
            }
        }

        public void PrepareVariants()
        {
            Result = new MainSequenceVariantResult
                     {
                         GroupId = Id, GroupName = Name,
                         SequenceId = SequenceId,
                         SequenceName = sequenceName,
                         SequenceVariantValues = AssignValues(ValueSetting),
                         ExternalSequenceVariantResults = new List<ExternalSequenceVariantResult>()
                     };

            if (ChangeInExternal)
            {
                Result.ExternalSequenceVariantResults = new List<ExternalSequenceVariantResult>();
                foreach (var externalSequenceVariantGroup in ExternalSequenceVariantGroups)
                {
                    var externalResult = new ExternalSequenceVariantResult
                                         {
                                             SequenceId = externalSequenceVariantGroup.SequenceId,
                                             SequenceName = externalSequenceVariantGroup.SequenceName,
                                             SequenceVariantValues = AssignValues(externalSequenceVariantGroup.ValueSetting)
                                         };
                    Result.ExternalSequenceVariantResults.Add(externalResult);
                }
            }
        }

        private List<SequenceVariantValue> AssignValues(IEnumerable<VariantValueSetting> settings)
        {
            var output = new List<SequenceVariantValue>();
            foreach (var variantValueSetting in settings)
            {
                switch (variantValueSetting.EditMode)
                {
                    case EditVariantMode.None:
                        output.Add(variantValueSetting.OriginalValue.Clone() as SequenceVariantValue);
                        break;
                    case EditVariantMode.Add:
                        output.Add(new SequenceVariantValue {Name = variantValueSetting.NewName});
                        break;
                    case EditVariantMode.Edit:
                        output.Add(new SequenceVariantValue {Name = variantValueSetting.NewName, Id = variantValueSetting.OriginalValue.Id});
                        break;
                }
            }

            return output;
        }
    }
}