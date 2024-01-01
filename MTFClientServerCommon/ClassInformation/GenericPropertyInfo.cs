using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [Serializable]
    public class GenericPropertyInfo : MTFDataTransferObject
    {
        public GenericPropertyInfo()
            :base()
        {
        }

        public GenericPropertyInfo(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public GenericPropertyInfo(PropertyInfo propertyInfo)
        {
            Name = propertyInfo.Name;
            Type = propertyInfo.PropertyType.FullName;
            AssemblyQualifiedName = propertyInfo.PropertyType.AssemblyQualifiedName;
            CanRead = propertyInfo.CanRead;
            CanWrite = propertyInfo.CanWrite;

            foreach (var allowedPropertyValue in propertyInfo.GetCustomAttributes<MTFAllowedPropertyValueAttribute>())
            {
                if (AllowedValues == null)
                {
                    AllowedValues = new MTFObservableCollection<MTFAllowedValue>();
                }
                AllowedValues.Add(new MTFAllowedValue { DisplayName = allowedPropertyValue.PropetyValueDisplayText, Value = allowedPropertyValue.PropertyValue });
            }

            //order allowed parameters
            if (AllowedValues != null)
            {
                var paramClone = AllowedValues.Copy() as MTFObservableCollection<MTFAllowedValue>;
                AllowedValues = new MTFObservableCollection<MTFAllowedValue>();
                foreach (var p in paramClone.OrderBy(item => item.Value))
                {
                    AllowedValues.Add(p.Copy());
                }
            }

            var monsterMethodAttribute = propertyInfo.GetCustomAttribute<MTFPropertyAttribute>();
            if (monsterMethodAttribute != null)
            {
                DefautlValue = monsterMethodAttribute.DefaultValue;
                ValueListName = monsterMethodAttribute.ValueListName;
                ValueListLevel = monsterMethodAttribute.ValueListLevel;
                ValueListParentName = monsterMethodAttribute.ValueListParentName;
            }
        }

        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Type
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string AssemblyQualifiedName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool CanRead
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool CanWrite
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(").Append(Type).Append(")").Append(Name).Append(" ").Append(CanRead ? "R" : "").Append(CanWrite ? "W" : "");

            return sb.ToString();            
        }

        public MTFObservableCollection<MTFAllowedValue> AllowedValues
        {
            get { return GetProperty<MTFObservableCollection<MTFAllowedValue>>(); }
            set { SetProperty(value); }
        }

        public string ValueListName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public int ValueListLevel
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public string ValueListParentName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string DefautlValue
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public override string ObjectVersion
        {
            get
            {
                return "1.1.0";
            }
        }

        protected override void VersionConvert(string fromVersion)
        {
            if (fromVersion == "1.0.0")
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
