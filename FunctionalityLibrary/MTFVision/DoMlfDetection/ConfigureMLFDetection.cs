using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.HelperClasses
{
    class ConfigureMLFDetection
    {
        public string virtualCameraName { get; set; }
        public string imgRefName { get; set; }
        //public int detectionType { get; set; }
        public string detectionData { get; set; }
        public string intensityPointsMeas { get; set; }
        public string gradientsMeas { get; set; }
        public string destinationArea { get; set; }
        public string inputCoordRef { get; set; }
        public string resultCoordRef { get; set; }               
        public ushort setupLevel { get; set; }
    }
}
