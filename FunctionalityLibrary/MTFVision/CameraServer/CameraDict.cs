using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.CameraServer
{
    [Serializable]
    public class CameraDict
    {
        public Dictionary<string, CameraConfigParams> CameraConfigs { get; set; }
    }

}
