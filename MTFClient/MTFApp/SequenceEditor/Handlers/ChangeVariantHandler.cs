using System.Collections.Generic;
using System.Linq;
using MTFApp.SequenceEditor.Settings.SequenceVariant;
using MTFClientServerCommon;
using MTFClientServerCommon.MTFTable;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFApp.SequenceEditor.Handlers
{
    //public static class ChangeVariantHandler
    //{
    //    public static void ChangeVariant(MTFSequence sequence, IList<SequenceVariantGroup> sequenceVariantGroups,
    //        IEnumerable<SequenceVariantValue> newValues, bool applyForExternal)
    //    {
    //        sequence.ForEachActivity(x => ChangeVariantInActivities(x, sequenceVariantGroups, newValues));
    //        ChangeVariantOnVariables(sequence, sequenceVariantGroups, newValues);
    //        ChangeVariantInData(sequence, sequenceVariantGroups, newValues);

    //        if (applyForExternal && sequence.ExternalSubSequences != null)
    //        {
    //            foreach (var externalSubSequence in sequence.ExternalSubSequences)
    //            {
    //                ChangeVariant(externalSubSequence.ExternalSequence, sequenceVariantGroups, newValues, true);
    //            }
    //        }

    //        ReplaceVariantsInSequence(sequence, sequenceVariantGroups, newValues);
    //    }

    //    private static void ReplaceVariantsInSequence(MTFSequence sequence, IList<SequenceVariantGroup> sequenceVariantGroups,
    //        IEnumerable<SequenceVariantValue> newValues)
    //    {
    //        if (sequence.VariantGroups != null)
    //        {
    //            var originalVersions = sequenceVariantGroups.FirstOrDefault(x => x.Name == SequenceVariantHelper.VersionCategory);
    //            var currentSequenceVersions = sequence.VariantGroups.FirstOrDefault(x => x.Name == SequenceVariantHelper.VersionCategory);
    //            if (newValues != null && currentSequenceVersions != null && currentSequenceVersions.Values != null && originalVersions != null &&
    //                originalVersions.Values != null)
    //            {
    //                var variantsToRemoveTmp = originalVersions.Values.Where(x => newValues.All(n => n.Id != x.Id));
    //                var variantsToRemove = currentSequenceVersions.Values.Where(x => variantsToRemoveTmp.Any(r => r.Name == x.Name));
    //                var variantsToAdd = newValues.Where(x => originalVersions.Values.All(o => o.Id != x.Id)).ToList();

    //                foreach (var sequenceVariantValue in newValues)
    //                {
    //                    var originalValue = originalVersions.Values.FirstOrDefault(x => x.Id == sequenceVariantValue.Id);
    //                    if (originalValue != null && originalValue.Name != sequenceVariantValue.Name)
    //                    {
    //                        var externalVersion = currentSequenceVersions.Values.FirstOrDefault(x => x.Name == originalValue.Name);
    //                        if (externalVersion != null)
    //                        {
    //                            externalVersion.Name = sequenceVariantValue.Name;
    //                        }
    //                    }
    //                }

    //                foreach (var value in variantsToRemove.ToList())
    //                {
    //                    currentSequenceVersions.Values.Remove(value);
    //                }

    //                foreach (var value in variantsToAdd.ToList())
    //                {
    //                    currentSequenceVersions.Values.Add(new SequenceVariantValue {Name = value.Name});
    //                }
    //            }
    //        }
    //    }

    //    public static string CheckVariantsInExternal(MTFSequence sequence)
    //    {
    //        var wrongVariants = new List<Tuple<string, string>>();
    //        var mainSequenceVariantGroups = sequence.VariantGroups;
    //        if (mainSequenceVariantGroups != null && sequence.ExternalSubSequences != null)
    //        {
    //            foreach (var externalSequenceInfo in sequence.ExternalSubSequences)
    //            {
    //                if (externalSequenceInfo.ExternalSequence != null && externalSequenceInfo.ExternalSequence.VariantGroups != null)
    //                {
    //                    var extrnalVariantGroups = externalSequenceInfo.ExternalSequence.VariantGroups;
    //                    var mainVersionCategory = mainSequenceVariantGroups.FirstOrDefault(x => x.Name == SequenceVariantHelper.VersionCategory);
    //                    if (mainVersionCategory != null && mainVersionCategory.Values != null)
    //                    {
    //                        var externalVersionCategory =
    //                            extrnalVariantGroups.FirstOrDefault(x => x.Name == SequenceVariantHelper.VersionCategory);
    //                        if (externalVersionCategory != null && externalVersionCategory.Values != null)
    //                        {
    //                            foreach (var version in mainVersionCategory.Values)
    //                            {
    //                                if (externalVersionCategory.Values.All(x => x.Name != version.Name))
    //                                {
    //                                    wrongVariants.Add(new Tuple<string, string>(externalSequenceInfo.ExternalSequence.Name, version.Name));
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        if (wrongVariants.Count > 0)
    //        {
    //            var msg = new StringBuilder();
    //            msg.AppendLine("External sequence contains different variants. Following variants has not been found:").AppendLine();
    //            foreach (var wrongVariant in wrongVariants)
    //            {
    //                msg.Append("Sequence ").Append(wrongVariant.Item1).Append(": ").Append(wrongVariant.Item2).AppendLine(Environment.NewLine);
    //            }

    //            msg.AppendLine("Do you really want to continue to change variants?");
    //            return msg.ToString();
    //        }

    //        return null;
    //    }

    //    private static void ChangeVariantInActivities(MTFSequenceActivity activity, IList<SequenceVariantGroup> sequenceVariantGroups,
    //        IEnumerable<SequenceVariantValue> newValues)
    //    {
    //        var subSequence = activity as MTFSubSequenceActivity;
    //        if (subSequence != null)
    //        {
    //            if (subSequence.ExecutionType == ExecutionType.ExecuteByCase && subSequence.VariantSwitch && subSequence.Cases != null)
    //            {
    //                var casesToDelete = new List<MTFCase>();
    //                foreach (var mtfCase in subSequence.Cases)
    //                {
    //                    var caseVariant = mtfCase.Value as SequenceVariant;
    //                    if (caseVariant != null)
    //                    {
    //                        caseVariant.ChangeVariant(sequenceVariantGroups, newValues);
    //                        if (caseVariant.IsEmpty)
    //                        {
    //                            casesToDelete.Add(mtfCase);
    //                        }

    //                        mtfCase.Name = caseVariant.ToString();
    //                    }
    //                }

    //                if (casesToDelete.Count > 0)
    //                {
    //                    var cases = subSequence.Cases.ToList();
    //                    foreach (var caseToDelete in casesToDelete)
    //                    {
    //                        cases.Remove(caseToDelete);
    //                    }

    //                    subSequence.Cases = cases;
    //                }
    //            }

    //            return;
    //        }

    //        var sequenceHandlingActivity = activity as MTFSequenceHandlingActivity;
    //        if (sequenceHandlingActivity != null)
    //        {
    //            if (sequenceHandlingActivity.SequenceHandlingType == SequenceHandlingType.SetVariant &&
    //                sequenceHandlingActivity.SequenceVariant != null)
    //            {
    //                sequenceHandlingActivity.SequenceVariant.ChangeVariant(sequenceVariantGroups, newValues);
    //            }
    //        }
    //    }

    //    private static void ChangeVariantOnVariables(MTFSequence sequence, IList<SequenceVariantGroup> sequenceVariantGroups,
    //        IEnumerable<SequenceVariantValue> newValues)
    //    {
    //        var variables = sequence.MTFVariables;
    //        if (variables != null)
    //        {
    //            foreach (var mtfVariable in variables)
    //            {
    //                if (mtfVariable.HasTable)
    //                {
    //                    var mtfTable = (IMTFTable)mtfVariable.Value;
    //                    mtfTable.ChangeVariant(sequenceVariantGroups, newValues);
    //                }
    //            }
    //        }
    //    }

    //    private static void ChangeVariantInData(MTFSequence sequence, IList<SequenceVariantGroup> sequenceVariantGroups,
    //        IEnumerable<SequenceVariantValue> newValues)
    //    {
    //        var mtfSequenceClassInfos = sequence.MTFSequenceClassInfos;
    //        if (mtfSequenceClassInfos != null)
    //        {
    //            foreach (var classInfo in mtfSequenceClassInfos)
    //            {
    //                if (classInfo.Data != null)
    //                {
    //                    foreach (var dataValues in classInfo.Data.Values)
    //                    {
    //                        if (dataValues != null)
    //                        {
    //                            var dataToRemove = new List<ClassInfoData>();
    //                            foreach (var data in dataValues)
    //                            {
    //                                if (data.SequenceVariant != null)
    //                                {
    //                                    data.SequenceVariant.ChangeVariant(sequenceVariantGroups, newValues);
    //                                    if (data.SequenceVariant.IsEmpty)
    //                                    {
    //                                        dataToRemove.Add(data);
    //                                    }
    //                                }
    //                            }

    //                            if (dataToRemove.Count > 0)
    //                            {
    //                                foreach (var data in dataToRemove)
    //                                {
    //                                    dataValues.Remove(data);
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    public class VariantChanger
    {
        private readonly List<EmptyVariantHandler> parentsWithEmptyVariants = new List<EmptyVariantHandler>();

        public List<EmptyVariantHandler> ParentsWithEmptyVariants => parentsWithEmptyVariants;

        public void UpdateVariants(MTFSequence sequence, List<SequenceDataPackage> variantSetting)
        {
            var variantData = variantSetting.FirstOrDefault(x => x.SequenceId == sequence.Id);
            if (variantData?.Groups != null)
            {
                sequence.ForEachActivity(activity => UpdateVariantOnActivity(activity, variantData));
                UpdateVariantsOnVariables(variantData, sequence);
                UpdateVariantsOnData(variantData, sequence);

                if (sequence.ExternalSubSequences != null)
                {
                    foreach (var sequenceExternalSubSequence in sequence.ExternalSubSequences)
                    {
                        UpdateVariants(sequenceExternalSubSequence.ExternalSequence, variantSetting);
                    }
                }
            }
        }

        private void UpdateVariantsOnData(SequenceDataPackage variantData, MTFSequence sequence)
        {
            var mtfSequenceClassInfos = sequence.MTFSequenceClassInfos;
            if (mtfSequenceClassInfos != null)
            {
                foreach (var classInfo in mtfSequenceClassInfos)
                {
                    if (classInfo.Data != null)
                    {
                        foreach (var dataValues in classInfo.Data.Values)
                        {
                            if (dataValues != null)
                            {
                                foreach (var data in dataValues)
                                {
                                    ProcessVariant(data.SequenceVariant, variantData, new EmptyVariantHandler(classInfo, data));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateVariantsOnVariables(SequenceDataPackage variantData, MTFSequence sequence)
        {
            var variables = sequence.MTFVariables;
            if (variables != null)
            {
                foreach (var mtfVariable in variables)
                {
                    switch (mtfVariable.Value)
                    {
                        case MTFValidationTable validationTable:
                            ChangeValidationTable(validationTable, variantData);
                            break;
                        case MTFConstantTable constantTable:
                            ChangeConstantTable(constantTable, variantData);
                            break;
                    }
                }
            }
        }

        private void ChangeConstantTable(MTFConstantTable constantTable, SequenceDataPackage variantData)
        {
            if (constantTable.Rows != null)
            {
                foreach (var row in constantTable.Rows)
                {
                    if (row?.RowVariants != null)
                    {
                        foreach (var rowVariant in row.RowVariants)
                        {
                            if (rowVariant != null)
                            {
                                ProcessVariant(rowVariant.SequenceVariant, variantData, new EmptyVariantHandler(row, rowVariant));
                            }
                        }
                    }
                }
            }
        }

        private void ChangeValidationTable(MTFValidationTable validationTable, SequenceDataPackage variantData)
        {
            if (validationTable.Rows != null)
            {
                foreach (var row in validationTable.Rows)
                {
                    if (row?.RowVariants != null)
                    {
                        foreach (var rowVariant in row.RowVariants)
                        {
                            if (rowVariant != null)
                            {
                                ProcessVariant(rowVariant.SequenceVariant, variantData, new EmptyVariantHandler(row, rowVariant));
                            }
                        }
                    }
                }
            }
        }

        private void UpdateVariantOnActivity(MTFSequenceActivity activity, SequenceDataPackage variantData)
        {
            switch (activity)
            {
                case MTFSubSequenceActivity subSequence:
                    {
                        if (subSequence.ExecutionType == ExecutionType.ExecuteByCase && subSequence.VariantSwitch && subSequence.Cases != null)
                        {
                            foreach (var mtfCase in subSequence.Cases)
                            {
                                if (mtfCase.Value is SequenceVariant caseVariant)
                                {
                                    ProcessVariant(caseVariant, variantData, new EmptyVariantHandler(subSequence, mtfCase));
                                    mtfCase.Name = caseVariant.ToString();
                                }
                            }
                        }

                        break;
                    }

                case MTFSequenceHandlingActivity sequenceHandlingActivity:
                    {
                        if (sequenceHandlingActivity.SequenceHandlingType == SequenceHandlingType.SetVariant)
                        {
                            ProcessVariant(sequenceHandlingActivity.SequenceVariant, variantData, null);
                        }

                        break;
                    }
            }
        }

        private void ProcessVariant(SequenceVariant sequenceVariant, SequenceDataPackage variantData, EmptyVariantHandler emptyVariantHandler)
        {
            if (sequenceVariant == null)
            {
                return;
            }

            foreach (var group in variantData.Groups)
            {
                var variantGroup = sequenceVariant.VariantGroups.FirstOrDefault(x => x.Name == group.Name);


                if (variantGroup?.Values != null)
                {
                    foreach (var variantValueSetting in group.VariantSetting)
                    {
                        if (variantValueSetting.EditMode == EditVariantMode.Edit)
                        {
                            var currentValue = variantGroup.Values?.FirstOrDefault(x => x.Name == variantValueSetting.OriginalValue.Name);
                            if (currentValue != null)
                            {
                                currentValue.Name = variantValueSetting.NewName;
                            }
                        }
                        else if (variantValueSetting.EditMode == EditVariantMode.Remove)
                        {
                            var valToRemove = variantGroup.Values.FirstOrDefault(x => x.Name == variantValueSetting.OriginalValue.Name);
                            variantGroup.Values.Remove(valToRemove);
                            if (variantGroup.IsEmpty)
                            {
                                sequenceVariant.VariantGroups.Remove(variantGroup);
                            }
                        }
                    }
                }
            }

            if (sequenceVariant.IsEmpty && emptyVariantHandler != null)
            {
                parentsWithEmptyVariants.Add(emptyVariantHandler);
            }
        }
    }
}