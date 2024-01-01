using System;

namespace AutomotiveLighting.MTFCommon
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class MTFConstructorAttribute : Attribute
    {
        public string Description { get; set; }
        public string ParameterHelperClassName { get; set; }
    }
}
