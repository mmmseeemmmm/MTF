using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTFVision.MtfVisionResults;

namespace MTFVision.DoMlfDetection
{
    public class MtfMlfResultResponse
    {
        public string MLFDetectionResult { get; set; }
        public string IntensityMeasurementResult { get; set; }
        public string gradientMeasurementResult { get; set; }
    }
}
