using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision
{
    [Serializable]
    public class VirtualCameraConfigurations
    {
        public bool setupMode { get; set; }
        public VirtualCameraConfiguration[] virtualCameraConfigurations { get; set; }
    }
}
