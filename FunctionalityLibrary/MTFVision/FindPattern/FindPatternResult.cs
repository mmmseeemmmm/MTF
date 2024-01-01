using AutomotiveLighting.MTFCommon;
using MTFVision.Common.VisionBasicObjects;

namespace MTFVision.FindPattern
{
    [MTFKnownClass]
    public class FindPatternResult
    {
        public bool result { get; set; }
        public ushort evaluationMethod { get; set; }
        public string[] aliasses { get; set; }
        public string[] typesOfPatterns { get; set; }
        public bool[] patternsResults { get; set; }
        public ushort[] expectedMatches { get; set; }
        public ushort[] numberOfMatches { get; set; }
        public float[][] scores { get; set; }
        public Point[][] positions { get; set; }
        public string[] patternsSettings { get; set; }                               
    }
}
