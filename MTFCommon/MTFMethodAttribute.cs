using System;

namespace AutomotiveLighting.MTFCommon
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MTFMethodAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool SetupModeAvailable { get; set; }
        public string[] UsedDataNames { get; set; }
    }
}
