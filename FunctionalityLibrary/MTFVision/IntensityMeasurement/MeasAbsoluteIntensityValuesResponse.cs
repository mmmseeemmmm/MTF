using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFVision.HelperClasses
{
    [MTFKnownClass]
    public class MeasAbsoluteIntensityValuesResponse
    {
        public double[] MeasuredValues { get; set; }
        public bool[] results { get; set; }
        public MeasAbsoluteIntensityValuesResults[] intensityMeasSettings { get; set; }
    }
}
