using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision
{
    public class AcquireImage
    {
        public string virtualCameraName { get; set; }
        public string imgRefName { get; set; }
        //public ushort imageState { get; set; } //0 post processed image, 1 raw data
    }
}
