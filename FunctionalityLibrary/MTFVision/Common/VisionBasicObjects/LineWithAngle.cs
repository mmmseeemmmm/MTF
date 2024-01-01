using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.Common.VisionBasicObjects
{
    [MTFKnownClass]
    public class LineWithAngle
    {
        public Line line { get; set; }
        public double angle { get; set; }
    }
}
