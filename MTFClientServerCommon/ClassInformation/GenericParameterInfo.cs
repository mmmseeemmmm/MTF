using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using MTFClientServerCommon.Mathematics;

namespace MTFClientServerCommon
{
    [Serializable]
    public class GenericParameterInfo : MTFDataTransferObject
    {
        protected bool? isSimpleEditableValue;

        public GenericParameterInfo()
            : base()
        {
        }

        public GenericParameterInfo(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public GenericParameterInfo(ParameterInfo parameterInfo)
        {
            Name = parameterInfo.Name;
            DisplayName = Name;
            TypeName = parameterInfo.ParameterType.FullName;
        }

        public string Name
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string DisplayName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string TypeName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public MTFObservableCollection<MTFAllowedValue> AllowedValues
        {
            get => GetProperty<MTFObservableCollection<MTFAllowedValue>>();
            set => SetProperty(value);
        }

        public string ValueListName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public int ValueListLevel
        {
            get => GetProperty<int>();
            set => SetProperty(value);
        }

        public string ValueListParentName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string DataNameExtension
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }


        public bool HasAllowedValues => AllowedValues?.Count > 0;

        public override string ToString()
        {
            return string.Format("({0}){1}",TypeName, Name);
        }

        public override string ObjectVersion => "1.1.0";

        public bool IsSimpleEditableValue
        {
            get => GetIsSimpleEditableValue();
            set
            {
                var originValue = isSimpleEditableValue;
                isSimpleEditableValue = value;
                if (originValue != isSimpleEditableValue)
                {
                    NotifyPropertyChanged();
                }
            }
        }

        protected virtual bool GetIsSimpleEditableValue()
        {
            return isSimpleEditableValue == true;
            
        }

        protected bool IsSimpleTermValue(object value)
        {
            return !(value is Term) || value is ConstantTerm || value is EmptyTerm || value is ActivityResultTerm || value is TermWrapper || value is ActivityTargetTerm;
        }

        protected override void VersionConvert(string fromVersion)
        {
            if (fromVersion == "1.0.0" || fromVersion == "1.0.1")
            {
                if (this.InternalProperties != null && this.InternalProperties.ContainsKey("AllowedValues") && this.InternalProperties["AllowedValues"] as IEnumerable<MTFAllowedValue> != null)
                {
                    var allowedValues = this.InternalProperties["AllowedValues"] as IEnumerable<MTFAllowedValue>;
                    this.AllowedValues = new MTFObservableCollection<MTFAllowedValue>(allowedValues);
                }
            }
            base.VersionConvert(fromVersion);
        }
    }
}
