using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.Common.VisionBasicObjects
{
    [MTFKnownClass]
    public class Line
    {
        public Point startPoint { get; set; }
        public Point endPoint { get; set; }
    }
}
