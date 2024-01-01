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
    public class AbsoluteMeasurementResultDetails
    {
        public MTFImage[] Images { get; set; }
        public MeasAbsoluteIntensityValuesResponse Result { get; set; }
    }

    [MTFKnownClass]
    public class AbsoluteMeasurementResult
    {
        public bool TestPassed { get; set; }
        public AbsoluteMeasurementResultDetails IntensityDetails { get; set; }
    }

}
