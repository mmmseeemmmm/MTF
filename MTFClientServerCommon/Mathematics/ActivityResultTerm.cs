using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MTFClientServerCommon.SequenceLocalization;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class ActivityResultTerm : Term
    {
        public ActivityResultTerm()
            : base()
        {
        }

        public ActivityResultTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        [MTFPersistIdOnly]
        public MTFSequenceActivity Value
        {
            get { return GetProperty<MTFSequenceActivity>(); }
            set
            {
                if (value != null && Value != null && value.ReturnType != Value.ReturnType)
                {
                    PropertyPath = null;
                }
                SetProperty(value, false);
                NotifyPropertyChanged("ResultType");
            }
        }

        public object ActivityResult
        {
            get;
            set;
        }


        public IList<GenericPropertyInfo> PropertyPath
        {
            get { return GetProperty<IList<GenericPropertyInfo>>(); }
            set { SetProperty(value); }
        }

        protected override void VersionConvert(string fromVersion)
        {
            base.VersionConvert(fromVersion);

            if (fromVersion == "1.0.0")
            {
                if (PropertyPath != null)
                {
                    List<GenericPropertyInfo> newPropertyPath = new List<GenericPropertyInfo>();
                    foreach (var propertyInfo in PropertyPath)
                    {
                        if (propertyInfo is GenericPropertyIndexer)
                        {
                            GenericPropertyInfo newPropertyInfo = new GenericPropertyInfo
                            {
                                AllowedValues = propertyInfo.AllowedValues,
                                CanRead = propertyInfo.CanRead,
                                CanWrite = propertyInfo.CanWrite,
                                Name = propertyInfo.Name,
                                Type = propertyInfo.Type
                            };

                            GenericPropertyIndexer newPropertyIndexer = new GenericPropertyIndexer
                            {
                                AllowedValues = propertyInfo.AllowedValues,
                                CanRead = propertyInfo.CanRead,
                                CanWrite = propertyInfo.CanWrite,
                                Name = propertyInfo.Name,
                                Type = propertyInfo.Type,
                                Index = ((GenericPropertyIndexer)propertyInfo).Index
                            };
                            newPropertyPath.Add(newPropertyInfo);
                            newPropertyPath.Add(newPropertyIndexer);
                        }
                        else
                        {
                            newPropertyPath.Add(propertyInfo);
                        }
                    }

                    PropertyPath = newPropertyPath;
                }
            }

        }

        public override object Evaluate()
        {
            object result = ActivityResult;
            if (PropertyPath != null)
            {
                foreach (var property in PropertyPath)
                {
                    if (property is GenericPropertyIndexer)
                    {
                        System.Reflection.MethodInfo getItem = result.GetType().GetMethod("GetValue", new Type[] { typeof(Int64) });
                        if (getItem == null)
                        {
                            getItem = result.GetType().GetMethod("get_Item");
                        }
                        object index = ((GenericPropertyIndexer)property).Index;
                        if (index is Term)
                        {
                            index = (index as Term).Evaluate();
                        }

                        try
                        {
                            result = getItem.Invoke(result, new object[] { index });
                        }
                        catch (Exception ex)
                        {
                            if (ex.InnerException != null)
                            {
                                throw ex.InnerException;
                            }

                            throw;
                        }
                    }
                    else
                    {
                        result = result.GetType().GetProperty(property.Name).GetMethod.Invoke(result, null);
                    }
                }
            }

            return result;
        }


        public override Type ResultType
        {
            get
            {
                if (PropertyPath != null && PropertyPath.Count > 0)
                {
                    string lastType = PropertyPath.Last().Type;
                    if (string.IsNullOrEmpty(lastType))
                    {
                        lastType = PropertyPath[PropertyPath.Count - 2].Type;
                    }
                    var type = Type.GetType(lastType);
                    if (type != null)
                    {
                        if (type.IsArray)
                        {
                            return type.BaseType;
                        }
                        if (type.IsGenericType)
                        {
                            return type.GenericTypeArguments[0];
                        }
                    }
                    return type;
                }
                if (Value != null && Value.ReturnType != null)
                {
                    return Type.GetType(Value.ReturnType);
                }

                return null;
            }
        }

        public override string ToString()
        {
            return Value == null ? "Null" : SequenceLocalizationHelper.ActualDictionary.GetValue(Value.ActivityName);

            //if (Value != null)
            //{
            //    StringBuilder sb = new StringBuilder();
            //    const string separator = ", ";

            //    if (Value.MTFMethodDisplayName == null)
            //    {
            //        return "Null";
            //    }
            //    sb.Append(Value.ActivityName).Append('.').Append(Value.MTFMethodDisplayName).Append("(");
            //    if (Value.MTFParameters == null)
            //    {
            //        sb.Append(")");
            //        appendPropertyPath(sb);

            //        return sb.ToString();
            //    }

            //    foreach (var param in Value.MTFParameters)
            //    {
            //        if (param.Value is GenericClassInstanceConfiguration)
            //        {
            //            sb.Append(param.Name).Append("=").Append(((GenericClassInstanceConfiguration)param.Value).ClassInfo.FullName);
            //        }
            //        else
            //        {
            //            sb.Append(param.Name).Append("=").Append(param.Value);
            //        }
            //        sb.Append(separator);
            //    }
            //    if (EndsWith(sb, separator))
            //    {
            //        sb.Remove(sb.Length - separator.Length, separator.Length);
            //    }
            //    sb.Append(")");
            //    appendPropertyPath(sb);

            //    return sb.ToString();
            //}

            //return "Null";
        }

        private void appendPropertyPath(StringBuilder sb)
        {
            if (PropertyPath == null)
            {
                return;
            }

            foreach (var propertyInfo in PropertyPath)
            {
                if (propertyInfo is GenericPropertyIndexer)
                {
                    sb.Append(".").Append(propertyInfo.Name).Append("[").Append((propertyInfo as GenericPropertyIndexer).Index).Append("]");
                }
                else
                {
                    sb.Append(".").Append(propertyInfo.Name);
                }
            }
        }

        private bool EndsWith(StringBuilder sb, string test)
        {
            if (sb.Length < test.Length)
            {
                return false;
            }
            string end = sb.ToString(sb.Length - test.Length, test.Length);
            return end.Equals(test);
        }

        public override string ToStringAsSubterm()
        {
            return ToString();
        }

        public override string Symbol
        {
            get { return "ActivityResult"; }
        }

        public override TermGroups TermGroup
        {
            get
            {
                return TermGroups.None
                    | TermGroups.LogicalTerm
                    | TermGroups.NumberTerm
                    | TermGroups.ObjectTerm
                    | TermGroups.StringTerm;
            }
        }

        public override TermGroups ChildrenTermGroup
        {
            get { return TermGroups.None; }
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.TermAct; }
        }

        public override string Label
        {
            get { return "Activity result"; }
        }
    }
}
