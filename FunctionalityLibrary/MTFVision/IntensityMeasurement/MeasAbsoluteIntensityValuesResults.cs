using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.HelperClasses
{
    public class MeasAbsoluteIntensityValuesResults
    {
        public string alias { get; set; }
        public string areaUID { get; set; }
        public byte valueType { get; set; }
        public Limits intensityLimit { get; set; }
        public Limits intensityThreshold { get; set; }
    }
}
