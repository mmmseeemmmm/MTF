using System;
using System.Collections.Generic;
using System.Linq;
using MTFClientServerCommon;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFCore
{
    public class VariablesHandler
    {
        private Dictionary<MTFVariable, GuidContainer> allVariables;
        private readonly Dictionary<Guid, Dictionary<Guid, object>> internalVariableValues = new Dictionary<Guid, Dictionary<Guid, object>>();
        private readonly Dictionary<Guid, Guid> variablesMapping = new Dictionary<Guid, Guid>();
        private Guid DefaultDUTId = new Guid("80A9DF35-844F-45BA-9418-4590AB810586");
        private IEnumerable<DeviceUnderTestInfo> sequenceDuts;

        private Dictionary<Guid, object> VariablesForDUT(Guid? dutId) => internalVariableValues[dutId ?? DefaultDUTId];

        public void ClearAllVariables() => allVariables = new Dictionary<MTFVariable, GuidContainer>();
        public void ClearVariablesMapping() => variablesMapping.Clear();

        public Dictionary<MTFVariable, GuidContainer> AllVariables => allVariables;

        public bool ContainsVariable(Guid id) => ContainsVariable(id, null);
        public bool ContainsVariable(Guid id, Guid? dutId) => VariablesForDUT(dutId).ContainsKey(GetMainVariableId(id));
        public object GetVariableValue(Guid variableId)
        {
            return GetVariableValue(variableId, null);
        }

        public object GetVariableValue(Guid variableId, Guid? dutId)
        {
            if (ContainsVariable(variableId, dutId))
            {
                return VariablesForDUT(dutId)[GetMainVariableId(variableId)];
            }
            return VariablesForDUT(DefaultDUTId)[GetMainVariableId(variableId)];
        }

        public void SetVariableValue(Guid variableId, object value)
        {
            SetVariableValue(variableId, value, null);
        }
        public void SetVariableValue(Guid variableId, object value, Guid? dutId)
        {
            if (ContainsVariable(variableId, dutId))
            {
                VariablesForDUT(dutId)[GetMainVariableId(variableId)] = value;
            }
            else if (dutId != DefaultDUTId)
            {
                VariablesForDUT(DefaultDUTId)[GetMainVariableId(variableId)] = value;
            }
        }
        public void InitDuts(IEnumerable<DeviceUnderTestInfo> sequenceDeviceUnderTestInfos)
        {
            sequenceDuts = sequenceDeviceUnderTestInfos;
            internalVariableValues.Clear();
            if (sequenceDeviceUnderTestInfos?.FirstOrDefault() != null)
            {
                DefaultDUTId = sequenceDeviceUnderTestInfos.FirstOrDefault().Id;
                foreach (var dut in sequenceDeviceUnderTestInfos)
                {
                    internalVariableValues[dut.Id] = new Dictionary<Guid, object>();
                }
            }
            else
            {
                internalVariableValues[DefaultDUTId] = new Dictionary<Guid, object>();
            }
        }

        public void InitVariablesMapping(MTFSequence currentSequence, Guid parentSequenceGuid, DynamicSequenceHelper dynamic)
        {
            InitVariablesMapping(currentSequence, parentSequenceGuid, dynamic, true);
        }

        private void InitVariablesMapping(MTFSequence currentSequence, Guid parentSequenceGuid, DynamicSequenceHelper dynamic, bool doVariablesMapping)
        {
            if (currentSequence.MTFVariables != null)
            {
                foreach (var mtfVariable in currentSequence.MTFVariables)
                {
                    AdoptVariable(mtfVariable);
                    allVariables.Add(mtfVariable, new GuidContainer(currentSequence.Id, parentSequenceGuid));
                    dynamic?.AddVariable(currentSequence.Id, mtfVariable.Id);
                }
            }
            if (currentSequence.ExternalSubSequences != null)
            {
                foreach (var item in currentSequence.ExternalSubSequences)
                {
                    InitVariablesMapping(item.ExternalSequence, currentSequence.Id, dynamic, false);
                }
            }

            if (doVariablesMapping)
            { 
                CreateVariablesMapping(dynamic == null ? allVariables
                    : allVariables.Where(x => dynamic.GetAllSequenceVariables(currentSequence).Contains(x.Key.Id)).ToDictionary(x => x.Key, y => y.Value));
            }
        }

        private void AdoptVariable(MTFVariable mtfVariable)
        {
            if (mtfVariable.DependsOnDut)
            {
                if (sequenceDuts == null)
                {
                    VariablesForDUT(null).Add(mtfVariable.Id, CloneVariable(mtfVariable).Value);
                }
                else
                {
                    foreach (var dut in sequenceDuts)
                    {
                        VariablesForDUT(dut.Id).Add(mtfVariable.Id, CloneVariable(mtfVariable).Value);
                    }
                }
            }
            else
            {
                VariablesForDUT(null).Add(mtfVariable.Id, CloneVariable(mtfVariable).Value);
            }
        }

        public MTFVariable GetVariable(Guid id) => allVariables.Keys.FirstOrDefault(x => x != null && x.Id == GetMainVariableId(id));

        public MTFVariable GetVariable<T>(string name) => allVariables.Keys.FirstOrDefault(v => v.Name == name && v.TypeName == typeof(T).FullName);

        public MTFVariable GetValidationTableVariable(Guid tableId) => allVariables.Keys.FirstOrDefault(x => x.HasValidationTable && ((MTFValidationTable)VariablesForDUT(null)[x.Id]).Id == tableId);
        public IEnumerable<MTFValidationTable> AllValidationTables => allVariables.Keys.Where(x => x.HasValidationTable).Select(x => (MTFValidationTable)x.Value);

        public IEnumerable<MTFValidationTable> ValidationTables(Guid? dutId) =>
            VariablesForDUT(dutId)
                .Where(v => allVariables.Keys.Where(t => t.HasValidationTable).Select(t => t.Id).Any(id => id == v.Key))
                .Select(v => (MTFValidationTable) v.Value);

        public void RemoveVariable(Guid variableId)
        {
            variablesMapping.Remove(variableId);
            if (sequenceDuts != null)
            {
                foreach (var dut in sequenceDuts)
                {
                    if (VariablesForDUT(dut.Id).ContainsKey(variableId))
                    {
                        VariablesForDUT(dut.Id).Remove(variableId);
                    }
                }
            }
            else
            {
                VariablesForDUT(null).Remove(variableId);
            }

            var variable = allVariables.FirstOrDefault(x => x.Key.Id == variableId);
            if (variable.Key != null)
            {
                allVariables.Remove(variable.Key);
            }
        }

        public void SetValidationTableRow(Guid tableId, MTFValidationTableRow row)
        {
            var v = GetValidationTableVariable(tableId);
            if (v != null)
            {
                foreach (var dutId in internalVariableValues.Keys)
                {
                    if (ContainsVariable(v.Id, dutId))
                    {
                        var table = GetVariableValue(v.Id, dutId) as MTFValidationTable;
                        table?.UpdateRow(row);
                    }
                }
                ((MTFValidationTable)v.Value).UpdateRow(row);
            }
        }

        private Guid GetMainVariableId(Guid id)
        {
            Guid parentGuid = Guid.Empty;

            if (variablesMapping.ContainsKey(id))
            {
                parentGuid = variablesMapping[id];
            }

            return parentGuid == Guid.Empty ? id : parentGuid;
        }

        private void CreateVariablesMapping(Dictionary<MTFVariable, GuidContainer> variablesDictionary)
        {
            foreach (var item in variablesDictionary)
            {
                var variable = item.Key;
                var parentGuid = GetGlobalVariableId(item.Key, item.Value);
                variablesMapping.Add(variable.Id, parentGuid);
            }
        }

        private Guid GetGlobalVariableId(MTFVariable variable, GuidContainer guidPair)
        {
            var guid = Guid.Empty;
            if (guidPair.ParentSequenceId != Guid.Empty)
            {
                if (variable.IsGlobal)
                {
                    var parent = allVariables.FirstOrDefault(x => x.Value.CurrentSequenceId == guidPair.ParentSequenceId && x.Key.Name == variable.Name);
                    if (parent.Key != null && parent.Value != null)
                    {
                        if (!parent.Key.IsGlobal || parent.Value.ParentSequenceId == Guid.Empty)
                        {
                            guid = parent.Key.Id;
                        }
                        else
                        {
                            guid = GetGlobalVariableId(parent.Key, parent.Value);
                            //if variable isnt in parent sequence - use this like the root
                            if (guid == Guid.Empty)
                            {
                                guid = parent.Key.Id;
                            }
                        }
                    }
                }
            }
            return guid;
        }

        private MTFVariable CloneVariable(MTFVariable mtfVariable)
        {
            var cloneVariable = (MTFVariable)mtfVariable.Clone();
            cloneVariable.ReplaceIdentityObjectsNoCache(mtfVariable);
            fixIds(mtfVariable, cloneVariable);

            return cloneVariable;
        }

        private void fixIds(MTFDataTransferObject origin, MTFDataTransferObject target)
        {
            if (origin == null || target == null)
            {
                return;
            }

            target.Id = origin.Id;

            foreach (var propertyName in origin.InternalProperties.Keys)
            {
                if (origin.InternalProperties[propertyName] is MTFDataTransferObject)
                {
                    fixIds(origin.InternalProperties[propertyName] as MTFDataTransferObject, target.InternalProperties[propertyName] as MTFDataTransferObject);
                }
                else if (origin.InternalProperties[propertyName] is IEnumerable<MTFDataTransferObject>)
                {
                    var enumOrigin = ((IEnumerable<MTFDataTransferObject>)origin.InternalProperties[propertyName]).GetEnumerator();
                    var enumTarget = ((IEnumerable<MTFDataTransferObject>)target.InternalProperties[propertyName]).GetEnumerator();
                    while (enumOrigin.MoveNext() && enumTarget.MoveNext())
                    {
                        fixIds(enumOrigin.Current, enumTarget.Current);
                    }
                }
            }
        }
    }
}
