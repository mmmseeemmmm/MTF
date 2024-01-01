using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTFVision.AdjustMlfConfig;

namespace MTFVision.HelperClasses
{
    public class AdjustMLFDetection:DoMLFDetection
    {
        public string screwsParams { get; set; }
        public ushort timeout { get; set; }
    }
}
