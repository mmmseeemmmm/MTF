using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.MtfVisionResults
{
    public enum DetectionTypes { CutOffLine = 0, IsoPercent = 1, IsoLine = 2 }

    public enum MLFIntensityMeasurementResult
    {
        NotMeasured = 0,
        NotPassed = 1,
        Passed = 2,
        Error = 3
    }

    public enum GradientDirection
    {
        vertical = 0,
        horizontal = 1,
    }
}
