using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;
using MTFVision.DoMlfDetection;
using MTFVision.MlfResult;

namespace MTFVision.MtfVisionResults
{
    [MTFKnownClass]
    public class MtfMlfDetectionResultsDetails
    {
        public MTFImage[] Images { get; set; }
        [MTFProperty(Description = "0=CutOffLine,1=IsoPercent,2=IsoLine")]
        public ushort DetectionType { get; set; }
        [MTFProperty(Description = "-1=Not evaluated,0=Not in area,1=In area,2=VisionError")]
        public ushort PointinArea { get; set; }

        public CutofflineResult CutOffLineResults { get; set; }
        public IsolineDetectionResult IsoLineResults { get; set; }
        public IsopercentDetectionResult IsoPercentResults { get; set; }
        public IntensityMeasurementResult IntensityMeasurementResult { get; set; }
        public GradientMeasurementResult GradientResults { get; set; }
    }
    [MTFKnownClass]
    public class MtfMlfDetectionResults
    {
        public MtfMlfDetectionResultsDetails VisionResultDetails { get; set; }
        public bool TestPassed { get; set; }

    }
}
