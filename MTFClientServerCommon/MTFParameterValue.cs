using System;
using System.Runtime.Serialization;
using System.Linq;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.Mathematics;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFParameterValue : MTFParameterInfo, IParameterValue
    {

        public MTFParameterValue()
            : base()
        {
        }

        public MTFParameterValue(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public MTFParameterValue(MTFParameterInfo parameterInfo)
        {
            Name = parameterInfo.Name;
            TypeName = parameterInfo.TypeName;
            Description = parameterInfo.Description;
            DisplayName = parameterInfo.DisplayName;
            AllowedValues = parameterInfo.AllowedValues;
            SetupMethodName = parameterInfo.SetupMethodName;
            DefautlValue = parameterInfo.DefautlValue;
            Value = TermHelper.CreateConstantTermByType(TypeName, MTFCommon.Helpers.TypeHelper.ConvertValue(DefautlValue, TypeName));
            ValueListName = parameterInfo.ValueListName;
            ValueListLevel = parameterInfo.ValueListLevel;
            ValueListParentName = parameterInfo.ValueListParentName;
            DataNameExtension = parameterInfo.DataNameExtension;
            IsOptional = parameterInfo.IsOptional;
            MinValue = parameterInfo.MinValue;
            MaxValue = parameterInfo.MaxValue;
        }

        public MTFParameterValue(MTFPropertyInfo propertyInfo)
        {
            Name = propertyInfo.Name;
            TypeName = propertyInfo.Type;
            ValueName = propertyInfo.ValueName;
            //Description = propertyInfo.Description;
            //DisplayName = propertyInfo.DisplayName;
            AllowedValues = propertyInfo.AllowedValues;
            DefautlValue = propertyInfo.DefautlValue;
            Value = TermHelper.CreateConstantTermByType(TypeName, MTFCommon.Helpers.TypeHelper.ConvertValue(DefautlValue, TypeName));
            ValueListName = propertyInfo.ValueListName;
            ValueListLevel = propertyInfo.ValueListLevel;
            ValueListParentName = propertyInfo.ValueListParentName;
        }


        public object Value
        {
            get => GetProperty<object>();
            set
            {
                SetProperty(value);
                NotifyPropertyChanged();//it make sense to have it here

                IsSimpleEditableValue = IsSimpleTermValue(value);

                var activity = Parent as MTFSequenceActivity;
                if (activity == null)
                {
                    return;
                }

                //refresh hack because of semrad's live list - because of filtracion of lists
                if (ShowValueList && activity.MTFParameters.Any(p => p.ValueListParentName == Name))
                {
                    var x = activity.MTFParameters;
                    activity.MTFParameters = null;
                    activity.MTFParameters = x;
                }
            }
        }

        

        public bool ShowValueList
        {
            get
            {
                if (AllowedValues!=null)
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(ValueListName))
                {
                    var classInfoBase = MTFSequenceActivityHelper.GetParent(this);
                    if (classInfoBase?.ClassInfo?.MTFClassInstanceConfiguration?.ValueLists!=null)
                    {
                        return classInfoBase.ClassInfo.MTFClassInstanceConfiguration.ValueLists.Any(l => l.Name == ValueListName);
                    }
                }

                return false;
            }
        }

        public bool IsValueListInCollection
        {
            get
            {
                var type = Type.GetType(TypeName);
                if (type != null)
                {
                    return type.IsGenericType || type.IsArray;
                }

                return false;
            }
        }

        public bool HasRange => !string.IsNullOrEmpty(MinValue) || !string.IsNullOrEmpty(MaxValue);

        public override string ToString()
        {
            return $"{base.ToString()} = {Value}";
        }

        public override string ObjectVersion => "1.1.0";

        protected override void VersionConvert(string fromVersion)
        {
            base.VersionConvert(fromVersion);
            if (fromVersion == "1.0.0")
            {
                if (TypeName == "MTFCommon.MTFImage")
                {
                    TypeName = "AutomotiveLighting.MTFCommon.MTFImage";
                }
            }
        }

        protected override bool GetIsSimpleEditableValue()
        {
            if (isSimpleEditableValue == null)
            {
                isSimpleEditableValue = IsSimpleTermValue(Value);
                return isSimpleEditableValue.Value;
            }

            return isSimpleEditableValue.Value;
        }
    }
}