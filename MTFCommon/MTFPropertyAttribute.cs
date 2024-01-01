using System;

namespace AutomotiveLighting.MTFCommon
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MTFPropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ValueName { get; set; }
        public string DefaultValue { get; set; }
        public string ValueListName { get; set; }
        public int ValueListLevel { get; set; }
        public string ValueListParentName { get; set; }
    }
}
