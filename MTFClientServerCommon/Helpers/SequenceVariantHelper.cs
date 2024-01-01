using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTFClientServerCommon.Constants;
using MTFCommon;

namespace MTFClientServerCommon.Helpers
{
    public static class SequenceVariantHelper
    {
        public static void SwitchSequence(MTFSequence sequence, SequenceVariant variant)
        {
            if (sequence == null)
            {
                return;
            }
            SwitchCases(sequence.MTFSequenceActivities, variant);
            SwitchCases(sequence.ActivitiesByCall, variant);
        }

        public static void SwitchCases(MTFSubSequenceActivity subSequence, SequenceVariant variant)
        {
            if (subSequence == null)
            {
                return;
            }
            if (subSequence.ExecutionType == ExecutionType.ExecuteByCase)
            {
                SwitchCaseSubSequence(subSequence, variant);
            }
            else
            {
                SwitchCases(subSequence.Activities, variant);
            }
        }

        private static void SwitchCases(IList<MTFSequenceActivity> activities, SequenceVariant variant)
        {
            if (activities == null)
            {
                return;
            }
            foreach (var activity in activities)
            {
                if (activity is MTFSubSequenceActivity subSequence)
                {
                    SwitchCaseSubSequence(subSequence, variant);
                }
            }
        }

        private static void SwitchCaseSubSequence(MTFSubSequenceActivity subSequence, SequenceVariant variant)
        {
            if (!subSequence.IsCollapsed)
            {
                if (subSequence.ExecutionType == ExecutionType.ExecuteByCase)
                {
                    if (subSequence.VariantSwitch)
                    {
                        subSequence.ActualCaseIndex = SelectVariantIndex(variant, subSequence.Cases);
                    }
                    if (subSequence.Cases != null && subSequence.ActualCaseIndex > -1 && subSequence.ActualCaseIndex < subSequence.Cases.Count)
                    {
                        var currentCase = subSequence.Cases[subSequence.ActualCaseIndex];
                        if (currentCase != null)
                        {
                            SwitchCases(currentCase.Activities, variant);
                        }
                    }
                }
                else
                {
                    SwitchCases(subSequence.Activities, variant);
                }
            }
        }

        private static int SelectVariantIndex(SequenceVariant variant, IList<MTFCase> cases)
        {
            if (variant != null && cases != null)
            {
                var bestVariant = variant.GetBestMatch(cases.Select(x => x.Value as SequenceVariant));
                int i = 0;
                if (bestVariant != null)
                {
                    foreach (var mtfCase in cases)
                    {
                        if (mtfCase.Value == bestVariant)
                        {
                            return i;
                        }
                        i++;
                    }
                }
                else
                {
                    foreach (var mtfCase in cases)
                    {
                        if (mtfCase.IsDefault)
                        {
                            return i;
                        }
                        i++;
                    }
                }
            }
            return -1;
        }

        public static MTFCase SelectCaseByVariant(SequenceVariant variant, IList<MTFCase> cases)
        {
            if (variant != null && cases != null)
            {
                var bestVariant = variant.GetBestMatch(cases.Select(x => x.Value as SequenceVariant));
                return bestVariant != null ? cases.FirstOrDefault(x => x.Value == bestVariant) : cases.FirstOrDefault(x => x.IsDefault);
            }
            return null;
        }

        //public static void ChangeVariant(this SequenceVariant variant, IList<SequenceVariantGroup> sequenceVariantGroups, IEnumerable<SequenceVariantValue> newValues)
        //{
        //    //Change only values from the first group
        //    if (variant == null || variant.Groups == null || newValues == null || sequenceVariantGroups == null)
        //    {
        //        return;
        //    }
        //    var itemVariantGroup = variant.VariantGroups.FirstOrDefault(x => x.Name == VersionCategory);
        //    var firstSequenceGroup = sequenceVariantGroups.FirstOrDefault();
        //    if (itemVariantGroup != null && firstSequenceGroup != null && firstSequenceGroup.Values != null)
        //    {
        //        var variantsToRemove = new List<SequenceVariantValue>();
        //        foreach (var variantValue in itemVariantGroup.Values)
        //        {
        //            if (variantValue.Name != null)
        //            {
        //                var originalValue = firstSequenceGroup.Values.FirstOrDefault(x => x.Name == variantValue.Name);
        //                if (originalValue != null)
        //                {
        //                    var newName = newValues.FirstOrDefault(x => x.Id == originalValue.Id);
        //                    if (newName != null)
        //                    {
        //                        variantValue.Name = newName.Name;
        //                    }
        //                    else
        //                    {
        //                        variantsToRemove.Add(variantValue);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                variantsToRemove.Add(variantValue);
        //            }
        //        }
        //        foreach (var sequenceVariantValue in variantsToRemove)
        //        {
        //            itemVariantGroup.Values.Remove(sequenceVariantValue);
        //        }
        //        if (itemVariantGroup.IsEmpty)
        //        {
        //            variant.VariantGroups.Remove(itemVariantGroup);
        //        }
        //    }
        //}

        //public static void ChangeVariantGroup(this SequenceVariantGroupValue itemVariantGroup,
        //    IList<SequenceVariantGroup> sequenceVariantGroups, IList<SequenceVariantValue> newValues, SequenceVariant variant)
        //{
        //    var variantsToRemove = new List<SequenceVariantValue>();
        //    var sequenceGroup = sequenceVariantGroups.FirstOrDefault(x=>x.Name == itemVariantGroup.Name);
        //    foreach (var variantValue in itemVariantGroup.Values)
        //    {
        //        if (variantValue.Name != null)
        //        {
        //            var originalValue = sequenceGroup.Values.FirstOrDefault(x => x.Name == variantValue.Name);
        //            if (originalValue != null)
        //            {
        //                var newName = newValues.FirstOrDefault(x => x.Id == originalValue.Id);
        //                if (newName != null)
        //                {
        //                    variantValue.Name = newName.Name;
        //                }
        //                else
        //                {
        //                    variantsToRemove.Add(variantValue);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            variantsToRemove.Add(variantValue);
        //        }
        //    }
        //    foreach (var sequenceVariantValue in variantsToRemove)
        //    {
        //        itemVariantGroup.Values.Remove(sequenceVariantValue);
        //    }
        //    if (itemVariantGroup.IsEmpty)
        //    {
        //        variant.VariantGroups.Remove(itemVariantGroup);
        //    }
        //}


        public static MTFValidationTableStatus InvertResult(this MTFValidationTableStatus status)
        {
            switch (status)
            {
                case MTFValidationTableStatus.NotFilled:
                    return MTFValidationTableStatus.NotFilled;
                case MTFValidationTableStatus.Ok:
                    return MTFValidationTableStatus.Nok;
                case MTFValidationTableStatus.Nok:
                    return MTFValidationTableStatus.Ok;
                case MTFValidationTableStatus.GSFail:
                    return MTFValidationTableStatus.GSFail;
                default:
                    return MTFValidationTableStatus.NotFilled;
            }
        }

        public static List<SequenceVariantGroup> GenerateSequenceVariants()
        {
            return new List<SequenceVariantGroup>
                   {
                       new SequenceVariantGroup
                       {
                           Name = SequenceVariantConstants.VersionCategory,
                           Values = new List<SequenceVariantValue>
                                    {
                                        new SequenceVariantValue
                                        {
                                            Name = "Base",
                                        },
                                        new SequenceVariantValue
                                        {
                                            Name = "Medium",
                                        },
                                        new SequenceVariantValue
                                        {
                                            Name = "Premium",
                                        },
                                    }
                       },
                       new SequenceVariantGroup
                       {
                           Name = SequenceVariantConstants.LightDistributionCategory,
                           Values = new List<SequenceVariantValue>
                                    {
                                        new SequenceVariantValue
                                        {
                                            Name = "ECE LV",
                                        },
                                        new SequenceVariantValue
                                        {
                                            Name = "ECE RV",
                                        },
                                        new SequenceVariantValue
                                        {
                                            Name = "SAE",
                                        },
                                    }
                       },
                       new SequenceVariantGroup
                       {
                           Name = SequenceVariantConstants.MountingSideCategory,
                           Values = new List<SequenceVariantValue>
                                    {
                                        new SequenceVariantValue
                                        {
                                            Name = "Left",
                                        },
                                        new SequenceVariantValue
                                        {
                                            Name = "Right",
                                        },
                                    }
                       },
                       new SequenceVariantGroup
                       {
                           Name = SequenceVariantConstants.GsDutCategory,
                           Values = GenerateGSValues()
                       },
                   };
        }

        public static List<SequenceVariantValue> GenerateGSValues()
        {
            var values = new List<SequenceVariantValue>
                         {
                             new SequenceVariantValue
                             {
                                 Name = SequenceVariantConstants.GoldSample,
                             },
                             new SequenceVariantValue
                             {
                                 Name = SequenceVariantConstants.ProductionDut,
                             },
                         };
            for (int i = 1; i <= 5; i++)
            {
                values.Add(new SequenceVariantValue
                           {
                               Name = $"{SequenceVariantConstants.NokGoldSample} {i}",
                           });
            }
            return values;
        }

        public static string CombineVariant(string version, string lightDistribution, string mountingSide, string gsDut)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(version))
            {
                sb.Append($"[{version}] ");
            }

            if (!string.IsNullOrEmpty(lightDistribution))
            {
                sb.Append($"[{lightDistribution}] ");
            }

            if (!string.IsNullOrEmpty(mountingSide))
            {
                sb.Append($"[{mountingSide}] ");
            }

            if (!string.IsNullOrEmpty(gsDut))
            {
                sb.Append($"[{gsDut}]");
            }

            return sb.ToString();
        }
    }
}