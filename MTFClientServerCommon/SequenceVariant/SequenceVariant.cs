using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Mathematics;

namespace MTFClientServerCommon
{
    [Serializable]
    public class SequenceVariant : MTFDataTransferObject, IComparable
    {
        public SequenceVariant()
        {
            VariantGroups = new List<SequenceVariantGroupValue>();
        }

        public SequenceVariant(IEnumerable<SequenceVariantGroup> variantGroups)
        {
            VariantGroups = variantGroups.Select(g => new SequenceVariantGroupValue
            {
                Name = g.Name,
            }).ToList();
        }

        public SequenceVariant(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public List<SequenceVariantGroupValue> VariantGroups
        {
            get { return GetProperty<List<SequenceVariantGroupValue>>(); }
            set { SetProperty(value); }
        }

        public IEnumerable<string> GetVariant(string groupName)
        {
            var group = getGroup(groupName);
            return group == null || group.Values == null ? null : group.Values.Select(x => x.Name);
        }

        public IEnumerable<string> Groups
        {
            get { return VariantGroups.Select(v => v.Name); }
        }

        private SequenceVariantGroupValue getGroup(string groupName)
        {
            return VariantGroups.FirstOrDefault(v => v.Name == groupName);
        }

        public void SetVariant(string groupName, int index, IEnumerable<SequenceVariantValue> values)
        {
            var group = getGroup(groupName);
            if (values == null)
            {
                if (group != null)
                {
                    VariantGroups.Remove(group);
                }
                return;
            }

            if (values.Any())
            {
                if (group == null)
                {
                    group = new SequenceVariantGroupValue { Name = groupName, Index = index };
                    InsertGroup(group);
                }
                group.Values = values.Select(x => x.Name).Select(name => new SequenceVariantValue { Name = name }).ToList();
            }
            else
            {
                if (group != null)
                {
                    VariantGroups.Remove(group);
                }
            }

        }

        public void SetVariant(string groupName, int index, Term term)
        {
            var group = getGroup(groupName);
            if (term == null || term is EmptyTerm)
            {
                if (group != null)
                {
                    VariantGroups.Remove(group);
                }
                return;
            }
            if (group == null)
            {
                group = new SequenceVariantGroupValue { Name = groupName, Index = index };
                InsertGroup(group);
                //VariantGroups.Add(group);
            }
            group.Values = null;
            group.Term = term;
        }

        private void InsertGroup(SequenceVariantGroupValue group)
        {
            bool add = true;
            if (VariantGroups.Count > 0)
            {
                int i = 0;
                while (i < VariantGroups.Count && add)
                {
                    if (VariantGroups[i].Index > group.Index)
                    {
                        add = false;
                        VariantGroups.Insert(i, group);
                    }
                    i++;
                }
            }
            if (add)
            {
                VariantGroups.Add(group);
            }
        }

        public bool IsEmpty
        {
            get { return VariantGroups == null || VariantGroups.Count == 0; }
        }

        public bool Match(SequenceVariant sequenceVariantToCompare)
        {
            return Match(sequenceVariantToCompare, null);
        }

        public bool Match(SequenceVariant sequenceVariantToCompare, IEnumerable<string> ignoreVariants)
        {
            if (sequenceVariantToCompare == null || sequenceVariantToCompare.VariantGroups == null || sequenceVariantToCompare.VariantGroups.Count == 0)
            {
                return false;
            }

            foreach (var group in VariantGroups)
            {
                if (ignoreVariants != null && ignoreVariants.Contains(group.Name))
                {
                    continue;
                }
                var variantGroup = sequenceVariantToCompare.VariantGroups.FirstOrDefault(g => g.Name == group.Name);
                if (variantGroup != null)
                {
                    if (variantGroup.Values != null && variantGroup.Values.Count != 0 && group.Values != null &&
                        group.Values.Count != 0)
                    {
                        //test if one of first sv is in second sv. If one is present, it means match, otherwise return 0
                        if (
                            !group.Values.Select(g => g.Name)
                                .Intersect(variantGroup.Values.Select(g1 => g1.Name))
                                .Any())
                        {
                            return false;
                        }
                    }
                }
            }
            //compute match by hits and size of both sv
            return true;
        }

        public bool MatchGoldSample(SequenceVariant goldSampleSelector, IList<SequenceVariantGroup> sequenceVariantGroups)
        {
            return isFull(sequenceVariantGroups) && Match(goldSampleSelector);
        }

        public SequenceVariant GetBestMatch(IEnumerable<SequenceVariant> sequenceVariants)
        {
            return GetBestMatch(sequenceVariants, null);
        }
        private SequenceVariant GetBestMatch(IEnumerable<SequenceVariant> sequenceVariants, IEnumerable<string> ignoreVariants)
        {
            return sequenceVariants.OrderByDescending(sv => sv == null || sv.Groups == null ? 0 : sv.Groups.Count()).FirstOrDefault(i => i != null && i.Match(this, ignoreVariants));
        }

        public SequenceVariant GetBestGoldSample(IEnumerable<SequenceVariant> goldSampleVariants, SequenceVariant goldSampleSelector)
        {
            return GetBestMatch(goldSampleVariants, goldSampleSelector.VariantGroups.Select(g => g.Name));
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            if (obj == this)
            {
                return true;
            }

            //compare object by values
            SequenceVariant sequenceVariantToCompare = (SequenceVariant)obj;
            if (Groups == null && sequenceVariantToCompare.Groups == null)
            {
                return true;
            }
            if (Groups == null || sequenceVariantToCompare.Groups == null)
            {
                return false;
            }
            if (VariantGroups.Count != sequenceVariantToCompare.VariantGroups.Count)
            {
                return false;
            }
            if (VariantGroups.Any(group => sequenceVariantToCompare.VariantGroups.All(g => g.Name != group.Name)))
            {
                return false;
            }

            foreach (var group in VariantGroups)
            {
                var groupToCompare = sequenceVariantToCompare.VariantGroups.First(g => g.Name == group.Name);
                if (group.Values.Any(val => groupToCompare.Values.All(v => v.Name != val.Name)))
                {
                    return false;
                }
            }

            return true;
        }

        public int CompareTo(object obj)
        {
            return obj == null ? 0 : ToString().CompareTo(obj.ToString());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var group in VariantGroups)
            {
                if (group.Values != null && group.Values.Any())
                {
                    sb.Append("[").Append(string.Join(";", group.Values.Select(g => g.Name))).Append("] ");
                }
            }

            return sb.ToString();
        }

        public SequenceVariantStringValues GetStringValues()
        {
            var output = new SequenceVariantStringValues();

            if (VariantGroups != null)
            {

                foreach (var group in VariantGroups)
                {
                    switch (group.Name)
                    {
                        case SequenceVariantConstants.VersionCategory:
                            output.Version = group.GetJoinedValues();
                            break;
                        case SequenceVariantConstants.LightDistributionCategory:
                            output.LightDistribution = group.GetJoinedValues();
                            break;
                        case SequenceVariantConstants.MountingSideCategory:
                            output.MountingSide = group.GetJoinedValues();
                            break;
                        case SequenceVariantConstants.GsDutCategory:
                            output.ProductionDut = group.GetJoinedValues();
                            break;
                    }
                }
            }

            return output;
        }

        private bool isFull(IList<SequenceVariantGroup> sequenceVariantGroups)
        {
            if (VariantGroups.Count != sequenceVariantGroups.Count)
            {
                return false;
            }

            return sequenceVariantGroups.All(variantGroup => VariantGroups.Any(g => g.Name == variantGroup.Name));
        }

    }
}
