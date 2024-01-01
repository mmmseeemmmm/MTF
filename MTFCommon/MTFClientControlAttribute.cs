using System;

namespace AutomotiveLighting.MTFCommon
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MTFClientControlAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public MTFIcons Icon { get; set; }
    }
}
