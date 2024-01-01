using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;

namespace MTFCore
{
    public class DynamicSequenceHelper
    {
        private readonly List<MTFSequence> dynamicSequences = new List<MTFSequence>();
        //Key: SequenceId, Value: List of variablesId
        private readonly Dictionary<Guid, List<Guid>> dynamicSequenceVariables = new Dictionary<Guid, List<Guid>>();
        //Key: SequenceId, Value: List of componentId
        private readonly Dictionary<Guid, List<Guid>> dynamicComponents = new Dictionary<Guid, List<Guid>>();



        public void AddComponent(Guid sequenceId, Guid componentId)
        {
            if (!dynamicComponents.ContainsKey(sequenceId))
            {
                dynamicComponents[sequenceId] = new List<Guid>();
            }
            dynamicComponents[sequenceId].Add(componentId);
        }

        public void AddVariable(Guid sequenceId, Guid variableId)
        {
            if (!dynamicSequenceVariables.ContainsKey(sequenceId))
            {
                dynamicSequenceVariables[sequenceId] = new List<Guid>();
            }
            dynamicSequenceVariables[sequenceId].Add(variableId);
        }

        public MTFSequence GetSequenceByName(string name, string mainSequenceFullPath)
        {
            var fullPath = GetSequenceFullPath(name, mainSequenceFullPath);
            return dynamicSequences.FirstOrDefault(x => x.FullPath == fullPath);
        }

        public string GetSequenceFullPath(string name, string mainSequenceFullPath)
        {
            var basePath = Path.GetDirectoryName(mainSequenceFullPath);
            return basePath != null
                ? string.Format("{0}{1}", Path.Combine(BaseConstants.SequenceBasePath, basePath, name), BaseConstants.SequenceExtension)
                : string.Empty;
        }


        public bool ExistSequence(string sequenceFullPath)
        {
            return dynamicSequences.Any(x => x.FullPath == sequenceFullPath);
        }

        public void AddSequence(MTFSequence sequence)
        {
            dynamicSequences.Add(sequence);
        }

        public IEnumerable<Guid> GetAllSequenceVariables(MTFSequence sequence)
        {
            var allSequenceIds = sequence.GetAllSequenceIds();
            foreach (var variableIdList in dynamicSequenceVariables.Where(x => allSequenceIds.Contains(x.Key)).Select(x => x.Value))
            {
                foreach (var guid in variableIdList)
                {
                    yield return guid;
                }
            }
        }

        public void ReleaseSequence(MTFSequence dynamicSequence, VariablesHandler variablesHandler, Dictionary<Guid, Guid> componentsMapping)
        {
            dynamicSequences.Remove(dynamicSequence);
            if (dynamicComponents.ContainsKey(dynamicSequence.Id))
            {
                var allComponentsId = dynamicComponents[dynamicSequence.Id];
                foreach (var componentId in allComponentsId)
                {
                    componentsMapping.Remove(componentId);
                }

                if (dynamicSequenceVariables.ContainsKey(dynamicSequence.Id))
                {
                    var dynamicSequenceVariable = dynamicSequenceVariables[dynamicSequence.Id];
                    foreach (var variableId in dynamicSequenceVariable)
                    {
                        variablesHandler.RemoveVariable(variableId);
                    }
                }
            }

        }
    }
}
