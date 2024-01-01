using AutomotiveLighting.MTFCommon;


namespace MTFVision.FindPattern
{
    [MTFKnownClass]
    public class PatternResultDetails
    {
        public FindPatternResult Result { get; set; }
        public MTFImage[] Images { get; set; }
    }
    [MTFKnownClass]
    public class PatternResult
    {
        public bool TestPassed { get; set; }
        public PatternResultDetails PatternDetails { get; set; }
    }
}
