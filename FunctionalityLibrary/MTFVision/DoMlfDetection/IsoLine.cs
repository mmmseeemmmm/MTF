using AutomotiveLighting.MTFCommon;
using MTFVision.Common.VisionBasicObjects;

namespace MTFVision.MtfVisionResults
{
    [MTFKnownClass]
    public class IsolineDetectionResult
    {
        public ushort adjustmentPoint { get; set; }
        public Point adjustmentPointPosition { get; set; }
    }
}
