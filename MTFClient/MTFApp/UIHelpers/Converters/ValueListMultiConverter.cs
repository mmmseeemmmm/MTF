using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;

namespace MTFApp.UIHelpers.Converters
{
    public class ValueListMultiConverter: IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values==null || values.Length<2)
            {
                return null;
            }

            var dto = values[0] as MTFDataTransferObject;
            var parameterValue = dto?.GetParent<MTFParameterValue>();
            var mainSequence = values[1] as MTFSequence;

            if (parameterValue == null || mainSequence == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(parameterValue.ValueListName))
            {
                var classInfoBase = MTFSequenceActivityHelper.GetParent(parameterValue);
                if (classInfoBase == null)
                {
                    return parameterValue.AllowedValues;
                }

                MTFSequenceClassInfo mainClassInfo = null;

                if (mainSequence.MTFSequenceClassInfos.Any(x=>x == classInfoBase.ClassInfo))
                {
                    mainClassInfo = classInfoBase.ClassInfo;
                }

                if (mainClassInfo == null || mainClassInfo.MTFClassInstanceConfiguration == null || mainClassInfo.MTFClassInstanceConfiguration.ValueLists == null)
                {
                    return parameterValue.AllowedValues;
                }

                var valueList = mainClassInfo.MTFClassInstanceConfiguration.ValueLists.FirstOrDefault(l => l.Name == parameterValue.ValueListName);
                if (valueList == null)
                {
                    return parameterValue.AllowedValues;
                }

                if (!string.IsNullOrEmpty(valueList.SubListSeparator) && parameterValue.ValueListLevel > 0)
                {
                    if (string.IsNullOrEmpty(parameterValue.ValueListParentName))
                    {
                        return null;
                    }

                    var listParent = classInfoBase.MTFParameters.FirstOrDefault(p => p.Name == parameterValue.ValueListParentName);
                    var listItem = valueList.Items.FirstOrDefault(i => toObjectArray(i.Value)[listParent.ValueListLevel].Equals(listParent.Value));
                    if (listItem == null)
                    {
                        return null;
                    }
                    var parentDisplayValue = listItem.DisplayName.Split(new[] { valueList.SubListSeparator }, StringSplitOptions.RemoveEmptyEntries);

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < parameterValue.ValueListLevel; i++)
                    {
                        sb.Append(parentDisplayValue[i]).Append(valueList.SubListSeparator);
                    }
                    string displayNamePrefix = sb.ToString();

                    List<MTFValueListItem> items = new List<MTFValueListItem>();
                    var filteredItems = valueList.Items.Where(i => i.DisplayName.StartsWith(displayNamePrefix));

                    foreach (var filteredItem in filteredItems)
                    {
                        var displayName = filteredItem.DisplayName.Split(new[] { valueList.SubListSeparator }, StringSplitOptions.None)[parameterValue.ValueListLevel];
                        if (items.All(i => i.DisplayName != displayName))
                        {
                            items.Add(new MTFValueListItem
                            {
                                DisplayName = displayName,
                                Value = toObjectArray(filteredItem.Value)[parameterValue.ValueListLevel]
                            });
                        }
                    }

                    return items;
                }

                if (!string.IsNullOrEmpty(valueList.SubListSeparator) && parameterValue.ValueListLevel == 0)
                {
                    //generate 1st level combobox
                    var filteredItems = valueList.Items
                        .Select(i => i.DisplayName.Split(new[] { valueList.SubListSeparator }, StringSplitOptions.RemoveEmptyEntries))
                        .Select(j => j[parameterValue.ValueListLevel])
                        .Distinct();
                    List<MTFValueListItem> items = new List<MTFValueListItem>();

                    foreach (var filteredItem in filteredItems)
                    {
                        var listValue = toObjectArray(valueList.Items.First(i => i.DisplayName.StartsWith(filteredItem + valueList.SubListSeparator)).Value);
                        object newListValue = null;
                        if (listValue != null)
                        {
                            newListValue = listValue[parameterValue.ValueListLevel];
                        }
                        items.Add(new MTFValueListItem
                        {
                            DisplayName = filteredItem,
                            Value = newListValue
                        });
                    }

                    return items;
                }

                return valueList.Items;
            }

            return parameterValue.AllowedValues;
        }

        private object[] toObjectArray(object obj)
        {
            var objArr = obj as object[];
            if (objArr != null)
            {
                return objArr;
            }

            var enumerable = obj as IEnumerable;
            return enumerable != null ? enumerable.Cast<object>().ToArray() : null;
        }

        private MTFSequenceClassInfo getClassInfo(MTFSequence sequence, Guid classInfoId)
        {
            if (sequence == null)
            {
                return null;
            }

            //main sequnce is reached
            if (sequence.ParentSequence == null)
            {
                return sequence.MTFSequenceClassInfos.FirstOrDefault(ci => ci.Id == classInfoId);
            }

            return sequence.ParentSequence.ComponetsMapping.ContainsKey(classInfoId) ? getClassInfo(sequence.ParentSequence, sequence.ParentSequence.ComponetsMapping[classInfoId]) : null;
        }

        private MTFSequence getMainSequnce(MTFSequence sequence)
        {
            if (sequence.ParentSequence == null)
            {
                return sequence;
            }

            return getMainSequnce(sequence.ParentSequence);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
