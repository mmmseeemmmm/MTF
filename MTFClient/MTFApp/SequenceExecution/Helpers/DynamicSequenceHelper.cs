using System;
using System.Collections.Generic;
using System.Linq;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;

namespace MTFApp.SequenceExecution.Helpers
{
    public class DynamicSequenceHelper
    {
        //key: sequenceId, value: sequence
        private readonly Dictionary<Guid, MTFSequence> dynamicSequences = new Dictionary<Guid, MTFSequence>();
        //key: sequenceId, value: Variables guid list
        private readonly Dictionary<Guid, List<Guid>> dynamicVariables = new Dictionary<Guid, List<Guid>>();
        //key: sequenceId, value: (key: activityId, value: activityWrapper)
        private readonly Dictionary<Guid, Dictionary<MTFActivityVisualisationWrapper, List<MTFActivityVisualisationWrapper>>> dynamicSequenceActivities =
            new Dictionary<Guid, Dictionary<MTFActivityVisualisationWrapper, List<MTFActivityVisualisationWrapper>>>();

        private readonly List<MTFActivityVisualisationWrapper> dynamicActivities = new List<MTFActivityVisualisationWrapper>();
        private readonly Dictionary<List<Guid>, int> indexes = new Dictionary<List<Guid>, int>();

        public List<MTFActivityVisualisationWrapper> DynamicSource
        {
            get { return dynamicActivities; }
        }

        public void LoadSequence(MTFSequence dynamicSequence, Dictionary<Guid, MTFExternalSequenceInfo> externalSequences)
        {
            if (dynamicSequence != null)
            {
                dynamicSequence.FillExternalSequences(externalSequences);
                dynamicSequence.ReplaceIdentityObjectsInSequenceProject();
                dynamicSequences[dynamicSequence.Id] = dynamicSequence;
                if (dynamicSequence.MTFVariables != null)
                {
                    if (!dynamicVariables.ContainsKey(dynamicSequence.Id))
                    {
                        dynamicVariables[dynamicSequence.Id] = new List<Guid>();
                    }
                    dynamicVariables[dynamicSequence.Id].AddRange(dynamicSequence.MTFVariables.Select(x => x.Id));

                }
            }
        }

        public void UnLoad(Guid sequenceId, Action<int, int, MTFActivityVisualisationWrapper, List<MTFActivityVisualisationWrapper>> removeAction)
        {
            if (dynamicSequences.ContainsKey(sequenceId))
            {
                dynamicSequences.Remove(sequenceId);
            }
            if (dynamicSequenceActivities.ContainsKey(sequenceId))
            {
                foreach (var dyct in dynamicSequenceActivities[sequenceId])
                {
                    removeAction(-1, dyct.Key.Nesting, dyct.Key, dyct.Value);
                }
                dynamicSequenceActivities.Remove(sequenceId);
            }
        }

        
        public void CheckDynamicSequence(Guid sequenceId)
        {
            if (!dynamicSequenceActivities.ContainsKey(sequenceId))
            {
                dynamicSequenceActivities[sequenceId] = new Dictionary<MTFActivityVisualisationWrapper, List<MTFActivityVisualisationWrapper>>();
            }
        }

        public bool ContainsSequence(Guid sequenceId)
        {
            return dynamicSequences.ContainsKey(sequenceId);
        }

        public MTFSequence GetSequence(Guid sequenceId)
        {
            return dynamicSequences[sequenceId];
        }

        public int GetIndex(List<Guid> guidPath)
        {
            return indexes[guidPath];
        }


        public void AssignDynamicActivity(MTFActivityVisualisationWrapper activityToAdd, Guid sequenceId, MTFActivityVisualisationWrapper dynamicCallActivity)
        {
            if (!dynamicSequenceActivities[sequenceId].ContainsKey(dynamicCallActivity))
            {
                dynamicSequenceActivities[sequenceId][dynamicCallActivity] = new List<MTFActivityVisualisationWrapper>();
            }
            activityToAdd.IsDynamicActivity = true;
            dynamicSequenceActivities[sequenceId][dynamicCallActivity].Add(activityToAdd);
            indexes.Add(activityToAdd.GuidPath, dynamicActivities.Count);
            dynamicActivities.Add(activityToAdd);
        }

        public List<Guid> GetDynamicVariablesBySequence(Guid sequenceId)
        {
            return dynamicVariables.ContainsKey(sequenceId) ? dynamicVariables[sequenceId] : null;
        }

        public void Clear(Action<int, int, MTFActivityVisualisationWrapper, List<MTFActivityVisualisationWrapper>> removeAction)
        {
            dynamicSequences.Clear();
            dynamicVariables.Clear();
            foreach (var dyct in dynamicSequenceActivities.Keys)
            {
                foreach (var item in dynamicSequenceActivities[dyct])
                {
                    removeAction(-1, item.Key.Nesting, item.Key, item.Value);
                }
            }
            dynamicSequenceActivities.Clear();
            dynamicActivities.Clear();
            indexes.Clear();
        }

        public IEnumerable<MTFActivityVisualisationWrapper> GetDynamicActivities(MTFActivityVisualisationWrapper callAcitivity)
        {
            if (callAcitivity.IsDynamicRoot)
            {
                return dynamicSequenceActivities.Values.Select(dynamicCallPackage => dynamicCallPackage.FirstOrDefault(x => x.Key == callAcitivity).Value).FirstOrDefault();
            }
            if (callAcitivity.IsDynamicActivity)
            {
                var index = indexes[callAcitivity.GuidPath]+1;
                var output = new List<MTFActivityVisualisationWrapper>();
                while (index < dynamicActivities.Count && dynamicActivities[index].Nesting > callAcitivity.Nesting)
                {
                    output.Add(dynamicActivities[index]);
                    index++;
                }
                return output;
            }
            return null;
        }

        public MTFActivityVisualisationWrapper FindInDynamicWithIndexes(IList<Guid> guidArray)
        {
            var index = indexes.FirstOrDefault(x => GuidHelper.CompareGuidPath(x.Key, guidArray));
            if (index.Key != null)
            {
                return dynamicActivities[index.Value];
            }
            return null;
        }

        public bool CanInsert(MTFActivityVisualisationWrapper dynamicCallAcitivity)
        {
            return dynamicSequenceActivities.All(item => item.Value.All(subSequenceGroup => subSequenceGroup.Key != dynamicCallAcitivity));
        }

        public void ClearResults(MTFActivityVisualisationWrapper activity)
        {
            foreach (var item in dynamicSequenceActivities)
            {
                foreach (var subSequenceGroup in item.Value)
                {
                    if (subSequenceGroup.Key == activity)
                    {
                        foreach (var wrapper in subSequenceGroup.Value)
                        {
                            wrapper.Result = null;
                            wrapper.Status = MTFExecutionActivityStatus.None;
                        }
                    }
                }
            }
        }
    }

    
}
