using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFVision.MtfVisionResults
{
    [MTFKnownClass]
    public class MtfVisionLine
    {
        public MtfVisionPoint StartPoint { get; set; }
        public MtfVisionPoint EndPoint { get; set; }
        public double Angle { get; set; }
    }
}
