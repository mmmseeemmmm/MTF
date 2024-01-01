using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.CameraServer
{
    [Serializable]
    public class CameraConfigParams
    {
        public string VirtualCameraName { get; set; }
        public ushort[] VirtualCameraParams { get; set; }
        public ushort[] SimulatedVirtualCameraParams { get; set; }
        public string CameraIP { get; set; }
        public string CameraSN { get; set; }
        public bool IsSimulated { get; set; }
    }
   
}
