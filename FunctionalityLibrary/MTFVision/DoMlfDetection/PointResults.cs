using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.DoMlfDetection
{
    public class PointResults
    {
        public string pointName { get; set; }
        public double meanIntensity { get; set; }
        public ushort result { get; set; }

    }
}
