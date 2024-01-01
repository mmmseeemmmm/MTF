using AutomotiveLighting.MTFCommon;
using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFPropertyInfo : GenericPropertyInfo
    {
        public MTFPropertyInfo()
            :base()
        {
        }

        public MTFPropertyInfo(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public MTFPropertyInfo(PropertyInfo propertyInfo)
            : base(propertyInfo)
        {
            var monsterMethodAttribute = propertyInfo.GetCustomAttribute<MTFPropertyAttribute>();
            Description = monsterMethodAttribute.Description;
            DisplayName = string.IsNullOrEmpty(monsterMethodAttribute.Name) ? Name : monsterMethodAttribute.Name;
            ValueName = string.IsNullOrEmpty(monsterMethodAttribute.ValueName) ? Name : monsterMethodAttribute.ValueName;
        }

        public string DisplayName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Description
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ValueName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }
}
