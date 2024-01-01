using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.DoMlfDetection
{
    public class IntensityMeasurementResult
    {
        public ushort result { get; set; }
        public PointResults[] pointsResults { get; set; }
        public MaximumIntensityPosition maximumPoint { get; set; }
    }

}
