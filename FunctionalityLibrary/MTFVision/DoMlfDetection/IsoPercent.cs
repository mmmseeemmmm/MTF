using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;
using MTFVision.DoMlfDetection;
using MTFVision.Common.VisionBasicObjects;

namespace MTFVision.MlfResult
{
    [MTFKnownClass]
    public class IsopercentDetectionResult
    {
        public AdjustmentPointPosition adjustmentPointPosition { get; set; }
        public HorizonalLine line { get; set; }
    }
    [MTFKnownClass]
    public class AdjustmentPointPosition
    {
        public float x { get; set; }
        public float y { get; set; }
    }
    [MTFKnownClass]
    public class HorizonalLine
    {
        public Line line { get; set; }
        public double angle { get; set; }
    }

}
