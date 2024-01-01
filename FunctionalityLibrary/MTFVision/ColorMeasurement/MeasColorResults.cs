using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.HelperClasses
{
    public class MeasColorResults
    {
        public string alias { get; set; }
        public ColorLimitSwitch colorLimit { get; set; } //switch if point is inside of area (false) or outside (true) to claim that test passed
        //public string colorLimitsROI { get; set; }
        public IntensityThreshold intensityTreshold { get; set; }
    }

    public class IntensityThreshold
    {
        public float lowerValue { get; set; }
        public float upperValue { get; set; }
    }

    public class ColorLimitSwitch
    {
        public bool negation { get; set; }
    }
}
