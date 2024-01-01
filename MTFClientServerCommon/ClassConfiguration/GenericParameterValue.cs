using System;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class GenericParameterValue : GenericParameterInfo, IParameterValue
    {
        public GenericParameterValue()
            : base()
        {
        }

        public GenericParameterValue(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public GenericParameterValue(GenericParameterInfo parameterInfo)
        {
            Name = parameterInfo.Name;
            TypeName = parameterInfo.TypeName;
            DisplayName = parameterInfo.DisplayName;
            AllowedValues = parameterInfo.AllowedValues;
        }

        public GenericParameterValue(GenericPropertyInfo propertyInfo)
        {
            Name = propertyInfo.Name;
            TypeName = propertyInfo.Type;
            AllowedValues = propertyInfo.AllowedValues;
            ValueListName = propertyInfo.ValueListName;
            ValueListLevel = propertyInfo.ValueListLevel;
            ValueListParentName = propertyInfo.ValueListParentName;
            
            if (!string.IsNullOrEmpty(propertyInfo.DefautlValue))
            {
                Value = MTFCommon.Helpers.TypeHelper.ConvertValue(propertyInfo.DefautlValue, TypeName);
            }
        }

        public object Value
        {
            get => GetProperty<object>();
            set
            {
                SetProperty(value);
                NotifyPropertyChanged();//it make sense to have it here

                IsSimpleEditableValue = IsSimpleTermValue(value);
            }
        }

        public override string ToString()
        {
            return $"{base.ToString()} = {Value}";
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
