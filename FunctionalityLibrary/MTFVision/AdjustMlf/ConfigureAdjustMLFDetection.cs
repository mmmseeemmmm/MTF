using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTFVision.AdjustMlfConfig;

namespace MTFVision.HelperClasses
{
    class ConfigureAdjustMLFDetection
    {
        public string virtualCameraName { get; set; }
        public string imgRefName { get; set; }
        public string detectionData { get; set; }
        public string destinationArea { get; set; }
        public string screwsParams { get; set; }
        public string inputCoordRef { get; set; }
        public string resultCoordRef { get; set; }
        public ushort setupLevel { get; set; }
        //public ushort timeout { get; set; }
    }
}
