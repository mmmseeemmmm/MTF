using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision
{
    public class VirtualCamera
    {
        public string identifier { get; set; }
        public string virtualCameraName { get; set; }
        public ushort[] virtualCameraParams { get; set; }
        public bool isSimulation { get; set; }
    }
}
