using AutomotiveLighting.MTFCommon;
using MTFVision.Common.VisionBasicObjects;

namespace MTFVision.HelperClasses
{
    [MTFKnownClass]
    public class MeasColorValuesResponse
    {
        public Point[] CIEcoordinates { get; set; }
        public bool[] results { get; set; }
        public string[] colorMeasSettings { get; set; }
        public MeasColorResults[] colorResult { get; set; }
    }
}
