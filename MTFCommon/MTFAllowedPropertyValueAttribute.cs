using System;

namespace AutomotiveLighting.MTFCommon
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class MTFAllowedPropertyValueAttribute : Attribute
    {
        public MTFAllowedPropertyValueAttribute(string propetyValueDisplayText, object propertyValue)
        {
            PropetyValueDisplayText = propetyValueDisplayText;
            PropertyValue = propertyValue;
        }
        public string PropetyValueDisplayText { get; set; }
        public object PropertyValue { get; set; }
    }
}
