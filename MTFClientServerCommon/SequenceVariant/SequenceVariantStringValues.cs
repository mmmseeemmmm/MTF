using System;

namespace MTFClientServerCommon
{
    public class SequenceVariantStringValues : ICloneable
    {
        public string Version { get; set; }
        public string LightDistribution { get; set; }
        public string MountingSide { get; set; }
        public string ProductionDut { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}