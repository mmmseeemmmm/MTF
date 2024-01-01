using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;
using MTFVision.HelperClasses;

namespace MTFVision
{
    [MTFKnownClass]
    public class ColorMeasurementResultDetails
    {
        public MTFImage[] Images { get; set; }
        public MeasColorValuesResponse Result { get; set; }
    }

    [MTFKnownClass]
    public class ColorMeasurementResult
    {
        public bool TestPassed { get; set; }
        public ColorMeasurementResultDetails ColorDetails { get; set; }
    }
}
