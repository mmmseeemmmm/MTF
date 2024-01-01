using System;

namespace AutomotiveLighting.MTFCommon
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MTFValueListGetterMethodAttribute : Attribute
    {
        public string SubListSeparator { get; set; }
    }
}
