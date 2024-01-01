using System;

namespace AutomotiveLighting.MTFCommon
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MTFUseClientControlAttribute : Attribute
    {
        public string Assembly { get; set; }
        public string TypeName { get; set; }
    }
}
