using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MTFApp.SequenceEditor.Handlers;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.MTFTable;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFApp.SequenceEditor.Settings.SequenceVariant
{
    class SequenceVariantWizardPresenter : NotifyPropertyBase
    {
        private readonly MTFSequence sequence;
        private readonly Action handleClose;
        private SequenceVariantWizardModes wizardMode;
        private List<EmptyVariantHandler> variantConflicts;
        private bool nextCommandIsEnabled = true;
        private bool applied = false;
        private string applyTitle = "Apply";
        private readonly IEnumerable<EmptyVariantChoices> enumChoices;

        public SequenceVariantWizardPresenter(MTFSequence sequence, Action handleClose)
        {
            this.sequence = sequence;
            this.handleClose = handleClose;
            IsActiveChangeCommand = new Command(IsActiveChange);
            EditVariantCommand = new Command(EditVariant);
            RemoveVariantCommand = new Command(RemoveVariant);
            AddVariantCommand = new Command(AddVariant);
            AddVariantToExternalCommand = new Command(AddVariantToExternal);
            NextCommand = new Command(Next);
            BackCommand = new Command(Back, () => WizardMode == SequenceVariantWizardModes.Overview && !applied);
            ApplyCommand = new Command(Apply, () => WizardMode == SequenceVariantWizardModes.Overview);
            enumChoices = Enum.GetValues(typeof(EmptyVariantChoices)).Cast<EmptyVariantChoices>();

            Groups = new List<VariantGroupPackage>
                     {
                         new VariantGroupPackage("Version", sequence.Id, sequence.Name),
                         new VariantGroupPackage("Light distribution", sequence.Id, sequence.Name)
                     };
        }


        public List<VariantGroupPackage> Groups { get; }
        public ICommand IsActiveChangeCommand { get; }
        public ICommand EditVariantCommand { get; }
        public ICommand RemoveVariantCommand { get; }
        public ICommand AddVariantCommand { get; }
        public ICommand AddVariantToExternalCommand { get; }
        public Command NextCommand { get; }
        public Command BackCommand { get; }
        public Command ApplyCommand { get; }

        public SequenceVariantWizardModes WizardMode
        {
            get => wizardMode;
            set
            {
                wizardMode = value;
                NotifyPropertyChanged();
            }
        }

        public List<EmptyVariantHandler> VariantConflicts
        {
            get => variantConflicts;
            set
            {
                variantConflicts = value;
                NotifyPropertyChanged();
            }
        }

        public bool NextCommandIsEnabled
        {
            get { return nextCommandIsEnabled; }
            set
            {
                nextCommandIsEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public string ApplyTitle
        {
            get { return applyTitle; }
            set
            {
                applyTitle = value;
                NotifyPropertyChanged();
            }
        }

        public IEnumerable<EmptyVariantChoices> EnumChoices => enumChoices;

        private void IsActiveChange(object obj)
        {
            var g = (VariantGroupPackage)obj;

            var variantGroup = GetVariantGroup(g.Name);
            var externalVariantGroups = GetExternalVariantGroup(g.Name);
            g.GenerateValues(variantGroup, externalVariantGroups);
        }

        private void EditVariant(object obj)
        {
            ChangeMode((VariantValueSetting)obj, EditVariantMode.Edit);
        }

        private void RemoveVariant(object obj)
        {
            ChangeMode((VariantValueSetting)obj, EditVariantMode.Remove);
        }

        private void Next()
        {
            SwitchWizardMode(SequenceVariantWizardModes.Overview);
            foreach (var group in Groups)
            {
                group.PrepareVariants();
            }
        }

        private void Apply()
        {
            if (!applied)
            {
                applied = true;
                InvalidateCommands();
                NextCommandIsEnabled = false;
                ChangeVariants();
                UpdateVariantInSequences();
            }
            else
            {
                if (VariantConflicts != null && VariantConflicts.Count > 0)
                {
                    foreach (var variantConflict in VariantConflicts.Where(x => x?.Choice == EmptyVariantChoices.Remove))
                    {
                        Remove(variantConflict.ParentObject, variantConflict.ObjectToRemove);
                    }
                }

                handleClose();
            }
        }

        private void Remove(MTFDataTransferObject parentObject, object objectToRemove)
        {
            switch (parentObject)
            {
                case MTFSubSequenceActivity subSequence:
                    if (subSequence.Cases != null)
                    {
                        var cases = subSequence.Cases.ToList();
                        cases.Remove(objectToRemove as MTFCase);
                        subSequence.Cases = cases;
                    }


                    break;
                case MTFValidationTableRow validTableRow:
                    validTableRow.RowVariants?.Remove(objectToRemove as MTFValidationTableRowVariant);
                    break;
                case MTFTableRow tableRow:
                    tableRow.RowVariants?.Remove(objectToRemove as MTFTableRowVariant);
                    break;
                case MTFSequenceClassInfo classInfo:
                    if (classInfo.Data != null)
                    {
                        foreach (var d in classInfo.Data)
                        {
                            if (d.Value.Contains(objectToRemove))
                            {
                                d.Value.Remove(objectToRemove as ClassInfoData);
                                break;
                            }
                        }
                    }

                    break;
            }
        }

        private void UpdateVariantInSequences()
        {
            foreach (var groupPackage in Groups)
            {
                var currentSequence = GetSequenceById(groupPackage.SequenceId);
                ProcessUpdateVariantsInSequence(groupPackage.ValueSetting, currentSequence, groupPackage.Name);

                if (groupPackage.ChangeInExternal)
                {
                    foreach (var externalSequenceVariantGroup in groupPackage.ExternalSequenceVariantGroups)
                    {
                        var externalSequence = GetSequenceById(externalSequenceVariantGroup.SequenceId);
                        ProcessUpdateVariantsInSequence(externalSequenceVariantGroup.ValueSetting, externalSequence, groupPackage.Name);
                    }
                }
            }
        }

        private MTFSequence GetSequenceById(Guid id)
        {
            return sequence.Id == id
                ? sequence
                : sequence.ExternalSubSequences?.FirstOrDefault(x => x.ExternalSequence.Id == id)?.ExternalSequence;
        }

        private void ProcessUpdateVariantsInSequence(IEnumerable<VariantValueSetting> valueSettings, MTFSequence s, string groupName)
        {
            foreach (var valueSetting in valueSettings)
            {
                switch (valueSetting.EditMode)
                {
                    case EditVariantMode.Edit:
                        valueSetting.OriginalValue.Name = valueSetting.NewName;
                        break;
                    case EditVariantMode.Remove:
                        var values = s.VariantGroups?.FirstOrDefault(x => x.Name == groupName)?.Values;
                        values?.Remove(values.FirstOrDefault(x => x.Name == valueSetting.OriginalValue.Name));
                        break;
                    case EditVariantMode.Add:
                        s.VariantGroups?.FirstOrDefault(x => x.Name == groupName)?.Values
                            .Add(new SequenceVariantValue {Name = valueSetting.NewName});
                        break;
                }
            }
        }


        private List<SequenceDataPackage> PrepareData()
        {
            var data = new List<SequenceDataPackage> {new SequenceDataPackage {SequenceId = sequence.Id}};

            if (sequence.ExternalSubSequences != null)
            {
                foreach (var sequenceExternalSubSequence in sequence.ExternalSubSequences)
                {
                    data.Add(new SequenceDataPackage {SequenceId = sequenceExternalSubSequence.ExternalSequence.Id});
                }
            }


            foreach (var variantGroupPackage in Groups)
            {
                if (variantGroupPackage.IsActive)
                {
                    var dataPackage = data.FirstOrDefault(x => x.SequenceId == variantGroupPackage.SequenceId);
                    if (dataPackage != null)
                    {
                        if (dataPackage.Groups == null)
                        {
                            dataPackage.Groups = new List<GroupDataPackage>();
                        }

                        dataPackage.Groups.Add(new GroupDataPackage
                                               {Name = variantGroupPackage.Name, VariantSetting = variantGroupPackage.ValueSetting.ToList()});

                        if (variantGroupPackage.ChangeInExternal)
                        {
                            foreach (var externalSequenceVariantGroup in variantGroupPackage.ExternalSequenceVariantGroups)
                            {
                                var externalDataPackage = data.FirstOrDefault(x => x.SequenceId == externalSequenceVariantGroup.SequenceId);

                                if (externalDataPackage != null)
                                {
                                    if (externalDataPackage.Groups == null)
                                    {
                                        externalDataPackage.Groups = new List<GroupDataPackage>();
                                    }

                                    externalDataPackage.Groups.Add(new GroupDataPackage
                                                                   {
                                                                       Name = variantGroupPackage.Name,
                                                                       VariantSetting = externalSequenceVariantGroup.ValueSetting.ToList()
                                                                   });
                                }
                            }
                        }
                    }
                }
            }

            return data;
        }

        private void ChangeVariants()
        {
            var changer = new VariantChanger();
            var data = PrepareData();

            changer.UpdateVariants(sequence, data);

            var conflicts = changer.ParentsWithEmptyVariants;

            if (conflicts != null && conflicts.Count > 0)
            {
                ApplyTitle = "OK";
                VariantConflicts = conflicts;
            }

            else
            {
                MTFMessageBox.Show("Variant change", "Variants has been changed", MTFMessageBoxType.Info, MTFMessageBoxButtons.Ok);
                handleClose();
            }
        }


        private void SwitchWizardMode(SequenceVariantWizardModes mode)
        {
            WizardMode = mode;
            NextCommandIsEnabled = WizardMode == SequenceVariantWizardModes.Definition;
            InvalidateCommands();
        }

        private void Back(object obj)
        {
            SwitchWizardMode(SequenceVariantWizardModes.Definition);
        }

        private void InvalidateCommands()
        {
            BackCommand.RaiseCanExecuteChanged();
            NextCommand.RaiseCanExecuteChanged();
            ApplyCommand.RaiseCanExecuteChanged();
        }

        public void RemoveNewVariant(VariantValueSetting setting, object parent)
        {
            if (parent != null)
            {
                switch (parent)
                {
                    case VariantGroupPackage package:
                        package.ValueSetting.Remove(setting);
                        RemoveFromExternal(setting, package);
                        break;
                    case ExternalSequenceVariantGroup externalGroup:
                        externalGroup.ValueSetting.Remove(setting);
                        break;
                }
            }
        }

        private void RemoveFromExternal(VariantValueSetting setting, VariantGroupPackage package)
        {
            if (package.ExternalSequenceVariantGroups != null)
            {
                foreach (var externalSequenceVariantGroup in package.ExternalSequenceVariantGroups)
                {
                    var item = externalSequenceVariantGroup.ValueSetting.FirstOrDefault(x => x.Id == setting.Id);
                    if (item != null)
                    {
                        externalSequenceVariantGroup.ValueSetting.Remove(item);
                    }
                }
            }
        }

        private void AddVariant(object obj)
        {
            (obj as VariantGroupPackage)?.AddNewItem();
        }

        private void AddVariantToExternal(object obj)
        {
            (obj as ExternalSequenceVariantGroup)?.ValueSetting.Add(new VariantValueSetting { EditMode = EditVariantMode.Add });
        }

        private void ChangeMode(VariantValueSetting setting, EditVariantMode mode)
        {
            setting.EditMode = setting.EditMode == mode ? EditVariantMode.None : mode;
        }

        private SequenceVariantGroup GetVariantGroup(string groupName)
        {
            return sequence?.VariantGroups?.FirstOrDefault(x => x.Name == groupName);
        }

        private List<ExternalSequenceVariantGroup> GetExternalVariantGroup(string groupName)
        {
            return sequence?.ExternalSubSequences?.Where(e => e.IsEnabled && e.ExternalSequence != null).Select(
                e => new ExternalSequenceVariantGroup
                     {
                         SequenceId = e.ExternalSequence.Id,
                         SequenceName = e.ExternalSequence.Name,
                         Group = e.ExternalSequence?.VariantGroups?.FirstOrDefault(x => x.Name == groupName)
                     }).ToList();
        }

        public void SwitchUpdateExternal(bool value)
        {
            Groups?.ForEach(x => x.CanUpdateExternal = value);
        }
    }
}