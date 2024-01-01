using MTFClientServerCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MTFClientServerCommon.Mathematics;
using System.Collections;
using MTFApp.UIHelpers;

namespace MTFApp.MergeSequences
{
    public static class MergeHelper
    {
        public static void MergeVariableOrComponent<T>(IList<T> property1, IEnumerable<T> property2,
            ObservableCollection<MergeSetting> setting)
            where T : MTFDataTransferObject
        {
            if (property2 != null)
            {
                foreach (var item in property2)
                {
                    var currentSetting = setting.FirstOrDefault(x => x.MergedComponent == item);
                    if (currentSetting != null && !currentSetting.Replace)
                    {
                        item.Id = Guid.NewGuid();
                        property1.Add(item);
                        item.AdjustName(property1);
                    }
                }
            }
        }

        public static void MergeActivities<T>(IList<T> property1, IEnumerable<T> property2,
            MergeSharedData sharedData)
            where T : MTFSequenceActivity
        {
            if (property2 != null)
            {
                foreach (var item in property2)
                {
                    item.Id = Guid.NewGuid();
                    EditProperty(item, sharedData);
                    property1.Add(item);
                    item.AdjustName(property1);
                }
            }
        }

        private static void EditProperty(MTFDataTransferObject item, MergeSharedData sharedData)
        {
            if (item == null)
            {
                return;
            }
            foreach (var prop in item.GetType().GetProperties())
            {
                if (prop.CanWrite && prop.CanRead)
                {
                    var value = prop.GetValue(item);
                    if (value != null)
                    {
                        if (value is MTFVariable)
                        {
                            var currentSetting = sharedData.MergeVariablesSetting.FirstOrDefault(x => x.MergedComponent == value);
                            if (currentSetting != null && currentSetting.Replace)
                            {
                                prop.SetValue(item, currentSetting.OriginalComponent);
                            }
                        }
                        else if (value is Term)
                        {
                            if (!(value is EmptyTerm))
                            {
                                var action = new Action<VariableTerm>((tern) =>
                                {
                                    var currentSetting = sharedData.MergeVariablesSetting.FirstOrDefault(x => x.MergedComponent == tern.MTFVariable);
                                    if (currentSetting != null && currentSetting.Replace)
                                    {
                                        tern.MTFVariable = currentSetting.OriginalComponent as MTFVariable;
                                    }
                                });
                                (value as Term).ForEachTerm<VariableTerm>(action);
                            }
                        }
                        else if (value is MTFSequenceClassInfo)
                        {
                            var currentSetting = sharedData.MergeComponentsSetting.FirstOrDefault(x => x.MergedComponent == value);
                            if (currentSetting != null && currentSetting.Replace)
                            {
                                prop.SetValue(item, currentSetting.OriginalComponent);
                            }
                        }
                        else if (value is ICollection)
                        {
                            foreach (var param in value as ICollection)
                            {
                                if (param is MTFDataTransferObject)
                                {
                                    EditProperty(param as MTFDataTransferObject, sharedData);
                                }
                            }
                        }
                        else if (value is MTFStringFormat)
                        {
                            EditProperty(value as MTFStringFormat, sharedData);
                        }
                    }
                }

            }
        }
    }
}
