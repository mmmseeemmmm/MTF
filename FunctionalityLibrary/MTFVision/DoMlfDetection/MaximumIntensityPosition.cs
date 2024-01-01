using MTFVision.Common.VisionBasicObjects;

namespace MTFVision.DoMlfDetection
{
    public class MaximumIntensityPosition
    {
        public Point maximumIntensityPosition { get; set; }
        public double maximumIntensity { get; set; }
        public ushort result { get; set; }
    }
}
