using System;

namespace AutomotiveLighting.MTFCommon
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MTFClientControlSetupAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public MTFIcons Icon { get; set; }
    }
}
