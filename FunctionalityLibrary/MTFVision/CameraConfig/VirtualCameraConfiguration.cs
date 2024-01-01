using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision
{
    [Serializable]
    public class VirtualCameraConfiguration
    {
        public string virtualCameraName { get; set; }
        public ushort[] virtualCameraParams { get; set; }
    }
}
