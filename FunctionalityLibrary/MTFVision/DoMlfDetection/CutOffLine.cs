using AutomotiveLighting.MTFCommon;
using MTFVision.Common.VisionBasicObjects;
using System;

namespace MTFVision.DoMlfDetection
{


        

    [MTFKnownClass]
    public class CutofflineResult
    {
        public ushort resultPoint { get; set; }
        public Point resultPointPosition { get; set; }
        public Points points { get; set; }
        public Lines lines { get; set; }
        public bool algorithmValid { get; set; }
    }

    [MTFKnownClass]
    public class Points
    {
        public Point startPoint { get; set; }
        public Point fallPoint { get; set; }
        public Point checkPoint { get; set; }
        public Point rawPoint { get; set; }
        public Point vOL { get; set; }
        public Point vOR { get; set; }
    }

    [MTFKnownClass]
    public class Lines
    {
        public LineWithAngle horizontalCOL { get; set; }
        public LineWithAngle verticalCOL { get; set; }
        public LineWithAngle upperHorizontalCOL { get; set; }
    }




}
