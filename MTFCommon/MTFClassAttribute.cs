using System;

namespace AutomotiveLighting.MTFCommon
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MTFClassAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public MTFIcons Icon { get; set; }
        public ThreadSafeLevel ThreadSafeLevel { get; set; }
        
    }
}
