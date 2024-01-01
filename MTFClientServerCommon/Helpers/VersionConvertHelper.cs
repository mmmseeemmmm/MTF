using MTFClientServerCommon.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.MTFTable;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFClientServerCommon.Helpers
{
    public static class VersionConvertHelper
    {
        public static Type[] BaseTypes = new Type[]{ typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int) ,typeof(uint),
                typeof(long), typeof(ulong),typeof(float), typeof(double), typeof(decimal), typeof(string), typeof(bool)
              };

        public static void TransformValuesToConstantTerm(IList parameters)
        {
            if (parameters != null && parameters.Count > 0)
            {
                foreach (MTFParameterValue param in parameters)
                {
                    var type = Type.GetType(param.TypeName);
                    TransformValueFromObject(param, type);

                }
            }
        }

        private static void TransformValueFromObject(MTFParameterValue param, Type type)
        {
            if (BaseTypes.Contains(type) && !(param.Value is Term) && param.AllowedValues == null)
            {
                param.Value = CreateConstantTerm(type, param.Value);
            }
            else
            {
                TransformValueFromObject(param.Value);
            }
        }

        private static void TransformValueFromObject(GenericParameterValue genericParameterValue, Type type)
        {
            if (BaseTypes.Contains(type) && !(genericParameterValue.Value is Term) && genericParameterValue.AllowedValues == null)
            {
                genericParameterValue.Value = CreateConstantTerm(type, genericParameterValue.Value);
            }
            else
            {
                TransformValueFromObject(genericParameterValue);
            }
        }

        private static void TransformValueFromObject(object obj)
        {
            var z = obj.GetType();
            if (obj is IEnumerable && !(obj is string))
            {
                var t = obj.GetType();
                if (t != null && BaseTypes.Contains(t.GenericTypeArguments[0]))
                {
                    throw new NotImplementedException();
                }
                else
                {
                    foreach (var item in (IEnumerable)obj)
                    {
                        if (item is MTFParameterValue)
                        {
                            throw new NotImplementedException();
                        }
                        else if (item is GenericParameterValue)
                        {
                            var gpv = item as GenericParameterValue;
                            var type = gpv.Value.GetType();
                            TransformValueFromObject(item as GenericParameterValue, type);

                        }
                        else
                        {
                            TransformValueFromObject(item);
                        }
                    }
                }
            }
            else if (obj is GenericClassInstanceConfiguration)
            {
                var gcic = obj as GenericClassInstanceConfiguration;
                foreach (var item in gcic.PropertyValues)
                {
                    if (item is GenericParameterValue)
                    {
                        var gpv = item as GenericParameterValue;
                        if (gpv.Value != null)
                        {
                            var type = gpv.Value.GetType();
                            TransformValueFromObject(item as GenericParameterValue, type);
                        }
                    }
                }
            }
            else if (obj is GenericParameterValue)
            {
                if ((obj as GenericParameterValue).Value is ICollection)
                {
                    ICollection val = (obj as GenericParameterValue).Value as ICollection;
                    var newValue = new List<Term>();
                    foreach (var item in val)
                    {
                        if (BaseTypes.Contains(item.GetType()))
                        {
                            newValue.Add(CreateConstantTerm(item.GetType(), item));
                        }
                    }
                    (obj as GenericParameterValue).Value = newValue;
                }
            }
        }



        private static ConstantTerm CreateConstantTerm(Type type, object value)
        {
            return new ConstantTerm(type) { Value = value };
        }

        public static void FindAndReplaceGenericCollectionOfBaseType(ObservableCollection<MTFParameterValue> parameters)
        {
            foreach (var item in parameters)
            {
                if (item.Value != null)
                {
                    if (item.Value.GetType() == typeof(GenericClassInstanceConfiguration))
                    {
                        FindAndReplaceInGCIC(item.Value as GenericClassInstanceConfiguration);
                    }
                    else if (item.Value.GetType().IsGenericType)
                    {
                        item.Value = FindAndReplaceInGeneric(item.Value as IEnumerable);
                    }
                }
            }
        }

        private static object FindAndReplaceInGeneric(IEnumerable collection)
        {
            bool isBaseType = false;
            Type baseType = null;
            foreach (var genericItem in collection)
            {
                if (BaseTypes.Contains(genericItem.GetType()))
                {
                    isBaseType = true;
                    baseType = genericItem.GetType();
                    break;
                }
            }
            if (isBaseType)
            {
                var newList = new List<Term>();
                if (baseType != typeof(string))
                {
                    throw new NotImplementedException();
                }
                foreach (var item2 in collection)
                {
                    newList.Add(new ConstantTerm(baseType) { Value = item2 });
                }
                return newList;
            }
            else
            {
                foreach (var genericItem in collection)
                {
                    if (genericItem is GenericClassInstanceConfiguration)
                    {
                        FindAndReplaceInGCIC(genericItem as GenericClassInstanceConfiguration);
                    }
                    //else
                    //{
                    //    throw new NotImplementedException();
                    //}
                }
            }
            return collection;
        }



        private static void FindAndReplaceInGCIC(GenericClassInstanceConfiguration gcic)
        {
            foreach (var item in gcic.PropertyValues)
            {
                if (item.Value != null)
                {
                    var type = new TypeInfo(item.TypeName);
                    if (type.IsGenericType)
                    {
                        item.Value = FindAndReplaceInGeneric(item.Value as IEnumerable);
                    }
                    else if (type.IsUnknownType)
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }


        public static ObservableCollection<MTFParameterValue> ReplaceBusCommunicationDriverStructures(ObservableCollection<MTFParameterValue> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter != null && parameter.Value is GenericClassInstanceConfiguration)
                {
                    var newFullName = "MTFBusCommunication.Structures.MTFOffBoardConfig";
                    var classInfo = (parameter.Value as GenericClassInstanceConfiguration).ClassInfo;


                    if (classInfo != null)
                    {
                        if (classInfo.FullName == "ALBusComDriver.OffBoardConfig")
                        {
                            parameter.TypeName = newFullName;
                            classInfo.AssemblyName = "MTFBusCommunication.dll";
                            classInfo.FullName = newFullName;
                            classInfo.Name = "MTFOffBoardConfig";
                        }
                        else if (classInfo.FullName == "MTFBusCommunication.Structures.MTFOffBoardService")
                        {
                            foreach (var item in (parameter.Value as GenericClassInstanceConfiguration).PropertyValues)
                            {
                                if (item.Value != null)
                                {
                                    if (item.TypeName == "System.Collections.Generic.List`1[[ALBusComDriver.OffBoardRequestParameter, ALBusComDriver, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]")
                                    {
                                        item.TypeName = "System.Collections.Generic.List`1[[MTFBusCommunication.Structures.MTFOffBoardRequestParameter, MTFBusCommunication, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]";
                                        foreach (var item2 in item.Value as IList<GenericClassInstanceConfiguration>)
                                        {
                                            item2.ClassInfo.AssemblyName = "MTFBusCommunication.dll";
                                            item2.ClassInfo.FullName = "MTFBusCommunication.Structures.MTFOffBoardRequestParameter";
                                            item2.ClassInfo.Name = "MTFOffBoardRequestParameter";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return parameters;
        }

        public static void ReplaceSequenceHandlingActivities(MTFSequence sequence)
        {
            if (sequence.MTFSequenceActivities != null && sequence.MTFSequenceActivities.Count > 0)
            {
                ReplaceSequenceHandlingActivities(sequence.MTFSequenceActivities);
            }
            if (sequence.ActivitiesByCall != null && sequence.ActivitiesByCall.Count > 0)
            {
                ReplaceSequenceHandlingActivities(sequence.ActivitiesByCall);
            }
        }

        public static void FindAndReplaceNullActivities(MTFSequence sequence)
        {
            if (sequence.MTFSequenceActivities != null && sequence.MTFSequenceActivities.Count > 0)
            {
                ReplaceNullActivities(sequence.MTFSequenceActivities);
            }
            if (sequence.ActivitiesByCall != null && sequence.ActivitiesByCall.Count > 0)
            {
                ReplaceNullActivities(sequence.ActivitiesByCall);
            }
        }

        private static void ReplaceNullActivities(IList<MTFSequenceActivity> observableCollection)
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < observableCollection.Count; i++)
            {
                if (observableCollection[i] is MTFClientServerCommon.MTFSubSequenceActivity)
                {
                    ReplaceNullActivities((observableCollection[i] as MTFSubSequenceActivity).Activities);
                }
                if (observableCollection[i] == null)
                {
                    observableCollection.RemoveAt(i);
                    i--;
                }
            }
        }

        private static readonly Dictionary<int, string> originalTypes = new Dictionary<int, string>()
                {
                    {0,"CheckErrors"},//error handling
                    {1,"SaveReportAndCleanErrors"},//sequence Handling
                    {2,"StartDurationCounter"},//sequence Handling
                    {3,"CleanErrors"},//error handling
                    {4,"SetSequenceStatusMessage"},//sequence Handling
                    {5,"LogMessage"},//Logging
                    {6,"OpenLogFile"},//Logging
                    {7,"RaiseError"},//error handling
                };

        private static void ReplaceSequenceHandlingActivities(IList<MTFSequenceActivity> collection)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i] is MTFSubSequenceActivity)
                {
                    ReplaceSequenceHandlingActivities((collection[i] as MTFSubSequenceActivity).Activities);
                }
                else if (collection[i] is MTFSequenceHandlingActivity)
                {
                    var properties = collection[i].InternalProperties;
                    if (properties.ContainsKey("SequenceHandlingType"))
                    {
                        var value = properties["SequenceHandlingType"];
                        int key;
                        if (int.TryParse(value.ToString(), out key))
                        {
                            switch (key)
                            {
                                case 0:
                                case 3:
                                case 7:
                                    collection[i] = CreateErrorHandling(properties);
                                    break;
                                case 1:
                                case 2:
                                case 4:
                                    ((MTFSequenceHandlingActivity)collection[i]).InternalProperties["SequenceHandlingType"] =
                                        (SequenceHandlingType)Enum.Parse(typeof(SequenceHandlingType), originalTypes[key]);
                                    break;
                                case 5:
                                case 6:
                                    collection[i] = CreateLoggingActivity(properties);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        collection[i] = CreateErrorHandling(properties);
                    }
                }

            }
        }

        private static MTFSequenceActivity CreateLoggingActivity(Dictionary<string, object> properties)
        {
            return new MTFLoggingActivity()
            {
                ActivityName = GetProperty<string>(properties, "ActivityName"),
                Comment = GetProperty<string>(properties, "Comment"),
                LoggingType = GetProperty<LoggingType>(properties, "SequenceHandlingType"),
                ErrorOutput = GetProperty<MTFVariable>(properties, "ErrorOutput"),
                Id = GetProperty<Guid>(properties, "Id"),
                IsActive = GetProperty<bool>(properties, "IsActive"),
                MTFClassAlias = GetProperty<string>(properties, "MTFClassAlias"),
                MTFMethodDescription = GetProperty<string>(properties, "MTFMethodDescription"),
                MTFMethodDisplayName = GetProperty<string>(properties, "MTFMethodDisplayName"),
                MTFMethodName = GetProperty<string>(properties, "MTFMethodName"),
                MTFParameters = GetProperty<ObservableCollection<MTFParameterValue>>(properties, "MTFParameters"),
                NumberOfAttempts = GetProperty<uint>(properties, "NumberOfAttempts"),
                OnError = GetProperty<MTFErrorBehavior>(properties, "OnError"),
                LogMessage = GetProperty<MTFStringFormat>(properties, "LogMessage"),
                LogFileName = GetProperty<MTFStringFormat>(properties, "LogFileName"),
                Repeat = GetProperty<bool>(properties, "Repeat"),
                RepeatDelay = GetProperty<uint>(properties, "RepeatDelay"),
                ReturnType = GetProperty<string>(properties, "ReturnType"),
                RunOnce = GetProperty<bool>(properties, "RunOnce"),
                Term = GetProperty<Term>(properties, "Term"),
                Variable = GetProperty<MTFVariable>(properties, "Variable"),
                //WriteToLog = GetProperty<bool>(properties, "WriteToLog"),
                LogTimeStamp = GetProperty<bool>(properties, "LogTimeStamp"),
            };
        }

        private static T GetProperty<T>(Dictionary<string, object> properties, string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                if (properties.ContainsKey(propertyName))
                {
                    var property = properties[propertyName];
                    if (propertyName == "SequenceHandlingType")
                    {
                        return (T)Enum.Parse(typeof(T), originalTypes[(int)property]);
                    }
                    else
                    {
                        return property == null ? default(T) : (T)property;
                    }
                }
                else
                {
                    if (propertyName == "SequenceHandlingType")
                    {
                        return (T)Convert.ChangeType(ErrorHandlingType.CheckErrors, typeof(T));
                    }
                    else
                    {
                        return default(T);
                    }
                }
            }
            else
            {
                return default(T);
            }
        }



        private static MTFSequenceActivity CreateErrorHandling(Dictionary<string, object> properties)
        {
            return new MTFErrorHandlingActivity()
            {
                ActivityName = GetProperty<string>(properties, "ActivityName"),
                Comment = GetProperty<string>(properties, "Comment"),
                ErrorHandlingType = GetProperty<ErrorHandlingType>(properties, "SequenceHandlingType"),
                ErrorOutput = GetProperty<MTFVariable>(properties, "ErrorOutput"),
                Id = GetProperty<Guid>(properties, "Id"),
                IsActive = GetProperty<bool>(properties, "IsActive"),
                MTFClassAlias = GetProperty<string>(properties, "MTFClassAlias"),
                MTFMethodDescription = GetProperty<string>(properties, "MTFMethodDescription"),
                MTFMethodDisplayName = GetProperty<string>(properties, "MTFMethodDisplayName"),
                MTFMethodName = GetProperty<string>(properties, "MTFMethodName"),
                MTFParameters = GetProperty<ObservableCollection<MTFParameterValue>>(properties, "MTFParameters"),
                NumberOfAttempts = GetProperty<uint>(properties, "NumberOfAttempts"),
                OnError = GetProperty<MTFErrorBehavior>(properties, "OnError"),
                RaiseError = GetProperty<MTFStringFormat>(properties, "RaiseError"),
                Repeat = GetProperty<bool>(properties, "Repeat"),
                RepeatDelay = GetProperty<uint>(properties, "RepeatDelay"),
                ReturnType = GetProperty<string>(properties, "ReturnType"),
                RunOnce = GetProperty<bool>(properties, "RunOnce"),
                Term = GetProperty<Term>(properties, "Term"),
                Variable = GetProperty<MTFVariable>(properties, "Variable"),
                //WriteToLog = GetProperty<bool>(properties, "WriteToLog"),
            };
        }



        public static void ReplaceClassInfo(MTFSequence sequence)
        {
            if (sequence.MTFSequenceActivities != null && sequence.MTFSequenceActivities.Count > 0)
            {
                ReplaceClassInfo(sequence.MTFSequenceActivities, sequence.MTFSequenceClassInfos);
            }
            if (sequence.ActivitiesByCall != null && sequence.ActivitiesByCall.Count > 0)
            {
                ReplaceClassInfo(sequence.ActivitiesByCall, sequence.MTFSequenceClassInfos);
            }
        }

        private static void ReplaceClassInfo(IList<MTFSequenceActivity> activities,
            IList<MTFSequenceClassInfo> sequenceClassInfos)
        {
            foreach (var item in activities)
            {
                if (item is MTFSubSequenceActivity)
                {
                    ReplaceClassInfo((item as MTFSubSequenceActivity).Activities, sequenceClassInfos);
                }
                else if (item.ClassInfo == null && item.InternalProperties.ContainsKey("MTFClassAlias"))
                {
                    item.ClassInfo = sequenceClassInfos.FirstOrDefault(x => x.Alias == item.InternalProperties["MTFClassAlias"].ToString());
                }
            }
        }

        public static void ReplaceLoggingActivities(MTFSequence sequence)
        {
            if (sequence.MTFSequenceActivities != null && sequence.MTFSequenceActivities.Count > 0)
            {
                ReplaceLoggingActivities(sequence.MTFSequenceActivities);
            }
            if (sequence.ActivitiesByCall != null && sequence.ActivitiesByCall.Count > 0)
            {
                ReplaceLoggingActivities(sequence.ActivitiesByCall);
            }

        }

        private static void ReplaceLoggingActivities(IList<MTFSequenceActivity> collection)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                var activity = collection[i] as MTFSubSequenceActivity;
                if (activity != null)
                {
                    ReplaceLoggingActivities(activity.Activities);
                }
                else
                {
                    var logingActivity = collection[i] as MTFLoggingActivity;
                    if (logingActivity != null)
                    {
                        if (logingActivity.LoggingType == LoggingType.OpenLogFile)
                        {
                            collection.RemoveAt(i);
                            i--;
                        }
                        else if (logingActivity.LoggingType == LoggingType.LogMessage)
                        {
                            collection[i] = new MTFSequenceHandlingActivity
                            {
                                Id = logingActivity.Id,
                                ActivityName = logingActivity.ActivityName,
                                LogMessage = logingActivity.LogMessage,
                                MTFClassAlias = string.Empty,
                                IsActive = logingActivity.IsActive,
                                MTFMethodName = string.Empty,
                                MTFMethodDisplayName = string.Empty,
                                SetupModeSupport = false,
                                ReturnType = typeof(void).FullName,
                                SequenceHandlingType = SequenceHandlingType.LogMessage,
                            };
                        }
                    }
                }
            }
        }

        public static Dictionary<string, bool> ConvertExternalSubSequencesPath(object originalData)
        {
            var originalList = originalData as List<string>;
            return originalList != null ? originalList.ToDictionary(k => k, v => true) : null;
        }

        public static void ReloadGsColumn(MTFValidationTable.MTFValidationTable validationTable)
        {
            var badGs = validationTable.Columns.FirstOrDefault(x => x.Type == MTFTableColumnType.Value && x.Header == ValidationTableConstants.ColumnGs);
            if (badGs != null)
            {
                validationTable.Columns.Remove(badGs);
            }


            var goldSampleColumn = validationTable.Columns.FirstOrDefault(c => c.Type == MTFTableColumnType.GoldSample);

            if (goldSampleColumn != null)
            {
                var column = new MTFValidationTableColumn(MTFTableColumnType.GoldSample) { Header = ValidationTableConstants.ColumnGs };
                column.SetPercentage(validationTable.GoldSampleLimit);

                var index = validationTable.Columns.IndexOf(goldSampleColumn);
                validationTable.Columns.Remove(goldSampleColumn);
                validationTable.Columns.Add(column);

                ReloadCellsInRows(index, column, validationTable);
            }


        }

        private static void ReloadCellsInRows(int index, MTFValidationTableColumn column, MTFValidationTable.MTFValidationTable validationTable)
        {
            foreach (var row in validationTable.Rows)
            {
                RemoveBadCell(row);

                ReloadCell(index, column, row);

            }
        }

        private static void RemoveBadCell(MTFValidationTableRow row)
        {
            if (row.Items != null)
            {
                var c = row.Items.FirstOrDefault(x => x.Type == MTFTableColumnType.Value && x.Header == ValidationTableConstants.ColumnGs);
                if (c != null)
                {
                    row.Items.Remove(c);
                }
            }
            if (row.RowVariants != null)
            {
                foreach (var rowVariant in row.RowVariants)
                {
                    if (rowVariant.Items != null)
                    {
                        var c = rowVariant.Items.FirstOrDefault(x => x.Type == MTFTableColumnType.Value && x.Header == ValidationTableConstants.ColumnGs);
                        if (c != null)
                        {
                            rowVariant.Items.Remove(c);
                        }
                    }
                }
            }
        }

        private static void ReloadCell(int columnIndex, MTFValidationTableColumn column, MTFValidationTableRow row)
        {

            var cell = row.Items[columnIndex];
            if (row.RowVariants != null)
            {
                foreach (var rowVariant in row.RowVariants)
                {
                    if (rowVariant.Items != null)
                    {
                        rowVariant.Items.Remove(rowVariant.Items.FirstOrDefault(x => x.Type == MTFTableColumnType.GoldSample));
                    }
                }
            }
            row.Items.Remove(cell);

            row.Items.Add(new MTFValidationTableCell(column));
            if (row.RowVariants != null)
            {
                foreach (var rowVariant in row.RowVariants)
                {
                    if (rowVariant.Items != null)
                    {
                        rowVariant.Items.Add(new MTFValidationTableCell(column));
                    }
                }
            }

        }




        public static void ClearTerms(MTFValidationTable.MTFValidationTable validationTable)
        {
            if (validationTable.Columns != null)
            {
                foreach (var tableColumn in validationTable.Columns)
                {
                    if (tableColumn != null && tableColumn.ValidationTerm != null)
                    {
                        var blt = tableColumn.ValidationTerm as BinaryLogicalTerm;
                        if (blt != null)
                        {
                            blt.Value1 = null;
                            blt.Value2 = null;
                        }
                        var gst = tableColumn.ValidationTerm as GoldSampleTerm;
                        if (gst != null)
                        {
                            gst.ActualValue = null;
                            gst.GoldSampleValue = null;
                        }
                        var lt = tableColumn.ValidationTerm as ListTerm;
                        if (lt != null)
                        {
                            lt.Value1 = null;
                            lt.Value2 = null;
                        }
                    }
                }
            }
            if (validationTable.Rows != null)
            {
                foreach (var row in validationTable.Rows)
                {
                    if (row.Items != null)
                    {
                        var actualValue = row.Items.FirstOrDefault(x => x.Type == MTFTableColumnType.ActualValue);
                        if (actualValue != null)
                        {
                            actualValue.Value = null;
                        }
                    }
                }
            }
        }

        public static void ConvertValidationTableResultTerms(MTFSequence sequence)
        {
            sequence.ForEach<ValidationTableResultTerm>(x => AdjustTableResultTerm(x, sequence));
        }

        private static void AdjustTableResultTerm(ValidationTableResultTerm trt, MTFSequence sequence)
        {
            if (trt.InternalProperties.ContainsKey("SelectedRow"))
            {
                var selectedRow = trt.InternalProperties["SelectedRow"] as string;
                if (!string.IsNullOrEmpty(selectedRow))
                {
                    if (trt.InternalProperties.ContainsKey("ValidationTable"))
                    {
                        var table = trt.InternalProperties["ValidationTable"];
                        var io = table as MTFIdentityObject;
                        if (io != null && sequence.MTFVariables != null)
                        {
                            var variable =
                                sequence.MTFVariables.FirstOrDefault(
                                    x => x.HasValidationTable && ((MTFValidationTable.MTFValidationTable)x.Value).Id == io.Id);
                            if (variable != null)
                            {
                                var row =
                                    ((MTFValidationTable.MTFValidationTable)variable.Value).Rows.FirstOrDefault(
                                        x => x.Items.First().Value as string == selectedRow);
                                if (row != null)
                                {
                                    trt.SelectedRowId = row.Id;
                                }
                            }
                            else
                            {
                                variable = sequence.MTFVariables.FirstOrDefault(
                                    x => x.HasDataTable && ((MTFDataTransferObject)x.Value).Id == io.Id);
                                if (variable != null)
                                {
                                    var row =
                                        ((IMTFDataTable)variable.Value).Rows.FirstOrDefault(x => x.Items.First().Value as string == selectedRow);
                                    if (row != null)
                                    {
                                        trt.SelectedRowId = row.Id;
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

        public static void CreateHiddenColumn(MTFValidationTable.MTFValidationTable table)
        {
            if (table.Columns == null)
            {
                return;
            }
            var hiddenColumn = table.Columns.FirstOrDefault(x => x.Type == MTFTableColumnType.Hidden);
            if (hiddenColumn == null)
            {
                hiddenColumn = new MTFValidationTableColumn(MTFTableColumnType.Hidden)
                {
                    Header = MTFValidationTable.MTFValidationTable.HiddenHeader,
                    Width = 45
                };
                table.Columns.Insert(MTFValidationTable.MTFValidationTable.HiddenValuePosition, hiddenColumn);

                if (table.Rows != null)
                {
                    foreach (var row in table.Rows)
                    {
                        if (row.Items != null)
                        {
                            var identityObject = new MTFIdentityObject { IdentifierName = "Id", Id = hiddenColumn.Id, TypeName = typeof(MTFValidationTableColumn).FullName };
                            var cell = new MTFValidationTableCell(hiddenColumn);
                            cell.InternalProperties["Column"] = identityObject;
                            row.Items.Insert(MTFValidationTable.MTFValidationTable.HiddenValuePosition, cell);

                            if (row.RowVariants != null)
                            {
                                foreach (var rowVariant in row.RowVariants)
                                {
                                    if (rowVariant.Items != null)
                                    {
                                        var variantIdentity = new MTFIdentityObject { IdentifierName = "Id", Id = hiddenColumn.Id, TypeName = typeof(MTFValidationTableColumn).FullName };
                                        var variantCell = new MTFValidationTableCell(hiddenColumn);
                                        variantCell.InternalProperties["Column"] = variantIdentity;
                                        rowVariant.Items.Insert(MTFValidationTable.MTFValidationTable.HiddenValuePosition, variantCell);
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }

        public static void ConvertExternalActivities(MTFSequence sequence)
        {
            ConvertExternalActivities(sequence.MTFSequenceActivities);
            ConvertExternalActivities(sequence.ActivitiesByCall);
        }

        private static void ConvertExternalActivities(IList<MTFSequenceActivity> activities)
        {
            for (int i = 0; i < activities.Count; i++)
            {
                var externalActivity = activities[i] as MTFExternalActivity;
                if (externalActivity != null)
                {
                    activities[i] = new MTFExecuteActivity
                    {
                        //AvailableSubSequences = externalActivity.AvailableSubSequences,
                        //ExternalSequenceToCall = externalActivity.ExternalSequenceToCall,
                        ExternalCall = new ExternalCallInfo
                        {
                            ExternalSequenceToCall = externalActivity.ExternalSequenceToCall,
                            OriginalCallActivityName = externalActivity.InnerSubSequenceByCall,
                        },
                        //InnerSubSequenceByCall = externalActivity.InnerSubSequenceByCall,
                        IsActive = externalActivity.IsActive,
                        Type = ExecuteActyvityTypes.External,
                        Id = externalActivity.Id,
                        MTFMethodName = externalActivity.MTFMethodName,
                        MTFMethodDisplayName = externalActivity.MTFMethodDisplayName,
                        ReturnType = externalActivity.ReturnType,
                        Comment = externalActivity.Comment,
                        RunOnce = externalActivity.RunOnce,
                        SetupModeSupport = externalActivity.SetupModeSupport,
                        //WriteToLog = externalActivity.WriteToLog,
                        DynamicMethod = new MTFStringFormat(),
                        DynamicSequence = new MTFStringFormat(),
                        DynamicActivityType = DynamicActivityTypes.Load,
                        ActivityName = externalActivity.ActivityName,
                        Parent = externalActivity.Parent,
                    };
                }
                var executeActivity = activities[i] as MTFExecuteActivity;
                if (executeActivity != null)
                {
                    if (executeActivity.DynamicSequence == null)
                    {
                        executeActivity.DynamicSequence = new MTFStringFormat();
                    }
                    if (executeActivity.DynamicMethod == null)
                    {
                        executeActivity.DynamicMethod = new MTFStringFormat();
                    }
                }
                var subSequenceActivity = activities[i] as MTFSubSequenceActivity;
                if (subSequenceActivity != null)
                {
                    if (subSequenceActivity.ExecutionType == ExecutionType.ExecuteByCase)
                    {
                        if (subSequenceActivity.Cases != null && subSequenceActivity.Cases.Count > 0)
                        {
                            foreach (var mtfCase in subSequenceActivity.Cases)
                            {
                                ConvertExternalActivities(mtfCase.Activities);
                            }
                        }
                    }
                    else
                    {
                        ConvertExternalActivities(subSequenceActivity.Activities);
                    }
                }
            }
        }

        public static void ConvertGSSetting(MTFSequence sequence)
        {
            if (sequence.InternalProperties != null)
            {
                var props = sequence.InternalProperties;
                if (props.ContainsKey("GoldSampleSetting"))
                {
                    var value = props["GoldSampleSetting"] as GoldSampleSetting;
                    if (value == null)
                    {
                        sequence.GoldSampleSetting = new GoldSampleSetting();
                    }
                }
                else
                {
                    sequence.GoldSampleSetting = new GoldSampleSetting();
                }

                var gsSetting = sequence.GoldSampleSetting;

                gsSetting.UseGoldSample = true;

                if (props.ContainsKey("GoldSampleValidationMode"))
                {
                    gsSetting.GoldSampleValidationMode = (GoldSampleValidationMode)props["GoldSampleValidationMode"];
                }
                if (props.ContainsKey("GoldSampleCount"))
                {
                    gsSetting.GoldSampleCount = (int)props["GoldSampleCount"];
                }
                if (props.ContainsKey("GoldSampleMinutes"))
                {
                    gsSetting.GoldSampleMinutes = (int)props["GoldSampleMinutes"];
                }
                if (props.ContainsKey("UseGoldSample"))
                {
                    gsSetting.UseGoldSample = (bool)props["UseGoldSample"];
                }
                if (props.ContainsKey("GoldSampleAfterStartInMinutes"))
                {
                    gsSetting.GoldSampleAfterStartInMinutes = (int)props["GoldSampleAfterStartInMinutes"];
                }
                if (props.ContainsKey("GoldSampleAfterInactivityInMinutes"))
                {
                    gsSetting.GoldSampleAfterInactivityInMinutes = (int)props["GoldSampleAfterInactivityInMinutes"];
                }
                if (props.ContainsKey("GoldSampleAfterVariantChanged"))
                {
                    gsSetting.GoldSampleAfterVariantChanged = (bool)props["GoldSampleAfterVariantChanged"];
                }
                if (props.ContainsKey("VariantChangedAfterSamplesCount"))
                {
                    gsSetting.VariantChangedAfterSamplesCount = (int)props["VariantChangedAfterSamplesCount"];
                }
                if (props.ContainsKey("GoldSampleRequiredAfterShiftStartInMinutes"))
                {
                    gsSetting.GoldSampleRequiredAfterShiftStartInMinutes = (int)props["GoldSampleRequiredAfterShiftStartInMinutes"];
                }
                if (props.ContainsKey("GoldSampleShifts"))
                {
                    gsSetting.GoldSampleShifts = (MTFObservableCollection<GoldSampleShift>)props["GoldSampleShifts"];
                }
                if (props.ContainsKey("GoldSampleDataFile"))
                {
                    gsSetting.GoldSampleDataFile = (string)props["GoldSampleDataFile"];
                }
                if (props.ContainsKey("GoldSampleSelector"))
                {
                    gsSetting.GoldSampleSelector = (SequenceVariant)props["GoldSampleSelector"];
                }

            }
        }


        public static void AddNokGS(MTFSequence mtfSequence)
        {
            if (mtfSequence.VariantGroups != null)
            {
                var category = mtfSequence.VariantGroups.FirstOrDefault(x => x.Name == SequenceVariantConstants.GsDutCategory);
                if (category != null)
                {
                    category.Values = SequenceVariantHelper.GenerateGSValues();
                }
            }
            else
            {
                mtfSequence.VariantGroups = SequenceVariantHelper.GenerateSequenceVariants();
            }
        }

        public static void ConvertMessageButtons(MTFSequenceMessageActivity messageActivity, Dictionary<string, object> properties)
        {
            if (properties.ContainsKey("OkCancelButtons") && (bool)properties["OkCancelButtons"])
            {
                messageActivity.Buttons = MessageActivityButtons.OkCancel;
            }
            else if (properties.ContainsKey("YesNoButtons") && (bool)properties["YesNoButtons"])
            {
                messageActivity.Buttons = MessageActivityButtons.YesNo;
            }
            else if (properties.ContainsKey("OkButton") && (bool)properties["OkButton"])
            {
                messageActivity.Buttons = MessageActivityButtons.Ok;
            }
        }

        public static void ConvertMessageValues(MTFSequenceMessageActivity messageActivity, Dictionary<string, object> properties)
        {
            if (properties.ContainsKey("Values"))
            {
                var values = properties["Values"] as List<string>;
                if (values != null)
                {
                    messageActivity.Values = values.Select(x => new ConstantTerm { Value = x } as Term).ToList();
                }
            }
        }

        public static Version CreateVersion(MTFSequence sequence)
        {
            int major = 0;
            int minor = 0;
            int build = 0;
            int revision = 0;
            var prop = sequence.InternalProperties;
            if (prop != null)
            {
                if (prop.ContainsKey("VersionMajor"))
                {
                    major = (int)prop["VersionMajor"];
                }
                if (prop.ContainsKey("VersionMinor"))
                {
                    minor = (int)prop["VersionMinor"];
                }
                if (prop.ContainsKey("VersionBuild"))
                {
                    build = (int)prop["VersionBuild"];
                }
                if (prop.ContainsKey("VersionRevision"))
                {
                    revision = (int)prop["VersionRevision"];
                }
            }
            return new Version(major, minor, build, revision);
        }

        public static void ConvertCallActivities(IList<MTFSequence> sequences)
        {
            if (sequences == null)
            {
                return;
            }
            foreach (var sequence in sequences)
            {
                sequence?.ForEachActivity<MTFExecuteActivity>(x => UpdateExternalCall(x, sequences));
            }
        }

        private static void UpdateExternalCall(MTFExecuteActivity executeActivity, IList<MTFSequence> sequences)
        {
            if (executeActivity.Type == ExecuteActyvityTypes.External && executeActivity.InternalProperties != null)
            {
                if (executeActivity.ExternalCall == null)
                {
                    executeActivity.ExternalCall = new ExternalCallInfo();

                    if (executeActivity.InternalProperties.ContainsKey("ExternalSequenceToCall"))
                    {
                        var externalSequenceToCall = executeActivity.InternalProperties["ExternalSequenceToCall"];

                        if (externalSequenceToCall != null)
                        {
                            executeActivity.ExternalCall.ExternalSequenceToCall = externalSequenceToCall.ToString();

                            if (executeActivity.InternalProperties.ContainsKey("InnerSubSequenceByCall"))
                            {
                                var innerSubSequenceByCall = executeActivity.InternalProperties["InnerSubSequenceByCall"];

                                var sequence = sequences.FirstOrDefault(x => x.Name == executeActivity.ExternalCall.ExternalSequenceToCall);

                                if (sequence?.ActivitiesByCall != null && innerSubSequenceByCall != null)
                                {
                                    var subSequenceName = innerSubSequenceByCall.ToString();

                                    var subSequence = sequence.ActivitiesByCall.FirstOrDefault(x => x.ActivityName == subSequenceName);

                                    if (subSequence != null)
                                    {
                                        executeActivity.ExternalCall.InnerSubSequenceByCallId = subSequence.Id;
                                        executeActivity.ExternalCall.OriginalCallActivityName = subSequenceName;
                                    }
                                    else
                                    {
                                        if (subSequenceName == "-- Whole sequence --")
                                        {
                                            executeActivity.ExternalCall.InnerSubSequenceByCallId = ActivityNameConstants.CallWholeSequenceId;
                                            executeActivity.ExternalCall.OriginalCallActivityName = ActivityNameConstants.CallWholeSequenceKey;
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

        internal static void CleanOldProperties(MTFSequenceActivity activity)
        {
            var executeActivity = activity as MTFExecuteActivity;
            if (executeActivity != null)
            {
                if (executeActivity.InternalProperties.ContainsKey("ExternalCall"))
                {
                    var value = executeActivity.InternalProperties["ExternalCall"];
                    if (value != null)
                    {
                        if (executeActivity.InternalProperties.ContainsKey("ExternalSequenceToCall"))
                        {
                            executeActivity.InternalProperties.Remove("ExternalSequenceToCall");
                        }

                        if (executeActivity.InternalProperties.ContainsKey("InnerSubSequenceByCall"))
                        {
                            executeActivity.InternalProperties.Remove("InnerSubSequenceByCall");
                        }

                        if (executeActivity.InternalProperties.ContainsKey("AvailableSubSequences"))
                        {
                            executeActivity.InternalProperties.Remove("AvailableSubSequences");
                        }
                    }
                }
            }
        }

        public static void ConvertSwitchToView(MTFSequence sequence, IList<MTFSequenceActivity> listActivities)
        {
            if (listActivities.Count > 0)
            {
                foreach (var activity in listActivities)
                {
                    var found = ForEachActivity(sequence.MTFSequenceActivities, activity);
                    if (!found)
                    {
                        ForEachActivity(sequence.ActivitiesByCall, activity);
                    }
                }
            }
        }

        private static bool ForEachActivity(IList<MTFSequenceActivity> activities, MTFSequenceActivity selectActivity)
        {
            foreach (var activity in activities)
            {
                if (AddSwitchViewActivity(activities, selectActivity))
                {
                    return true;
                }

                if (activity is MTFSubSequenceActivity subSequenceActivity)
                {
                    if (subSequenceActivity.ExecutionType == ExecutionType.ExecuteByCase)
                    {
                        if (subSequenceActivity.Cases != null && subSequenceActivity.Cases.Count > 0)
                        {
                            foreach (var mtfCase in subSequenceActivity.Cases)
                            {
                                if (ForEachActivity(mtfCase.Activities, selectActivity))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ForEachActivity(subSequenceActivity.Activities, selectActivity))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool AddSwitchViewActivity(IList<MTFSequenceActivity> activities, MTFSequenceActivity selectActivity)
        {
            var index = activities.IndexOf(selectActivity);
            if (index > -1)
            {
                activities.Insert(index, createActivityForSwitchToView(selectActivity));
                return true;
            }

            return false;
        }


        private static MTFSequenceHandlingActivity createActivityForSwitchToView(MTFSequenceActivity activity)
        {
            var selectedView = SequenceExecutionViewType.TreeView;
            if (activity.InternalProperties.ContainsKey("SwitchToView"))
            {
                selectedView = (SequenceExecutionViewType)activity.InternalProperties["SwitchToView"];
            }
            return new MTFSequenceHandlingActivity
            {
                IsActive = true,
                SequenceHandlingType = SequenceHandlingType.SwitchView,
                SwitchGraphicalView =
                {
                    SwitchToView = selectedView
                }
            };
        }

        public static void RepairConstantTables(MTFObservableCollection<MTFVariable> mtfVariables)
        {
            if (mtfVariables != null)
            {
                foreach (var mtfVariable in mtfVariables)
                {
                    if (mtfVariable.Value is MTFConstantTable constantTable)
                    {
                        if (constantTable.Columns != null && constantTable.Rows != null)
                        {
                            foreach (var row in constantTable.Rows)
                            {
                                if (row.Items != null && row.Items.Count > 0)
                                {
                                    for (int i = 0; i < row.Items.Count; i++)
                                    {
                                        if (i < constantTable.Columns.Count)
                                        {
                                            CheckColumn(row.Items[i], constantTable.Columns[i]);
                                        }
                                    }

                                    if (row.RowVariants != null && row.RowVariants.Count > 0)
                                    {
                                        foreach (var rowVariant in row.RowVariants)
                                        {
                                            if (rowVariant.Items != null)
                                            {
                                                for (int i = 0; i < rowVariant.Items.Count; i++)
                                                {
                                                    if (i < constantTable.Columns.Count)
                                                    {
                                                        CheckColumn(rowVariant.Items[i], constantTable.Columns[i]);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void CheckColumn(MTFTableCell cell, MTFTableColumn constantTableColumn)
        {
            if (cell.InternalProperties.ContainsKey("Column"))
            {
                var column = cell.InternalProperties["Column"];
                if (column is MTFIdentityObject idObj)
                {
                    if (idObj.Id != constantTableColumn.Id)
                    {
                        cell.InternalProperties["Column"] = new MTFIdentityObject
                        {
                            Id = constantTableColumn.Id,
                            IdentifierName = idObj.IdentifierName,
                            TypeName = idObj.TypeName,
                        };
                    }
                }
            }
        }
    }
}
