using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.HelperClasses
{
    class ConfigureMLFDetectionData
    {
        public string virtualCameraName { get; set; }
        public string imgRefName { get; set; }
        public string detectionData { get; set; }       
        public ushort setupLevel { get; set; }
        public string goldenSampleName { get; set; }
    }
}
