using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFVision
{
    [MTFKnownClass]
    public class CameraConfig
    {
        //[MTFAllowedPropertyValue("Cam001", "cam001")]
        //[MTFAllowedPropertyValue("Cam002", "cam002")]
        //[MTFAllowedPropertyValue("Cam003", "cam003")]
        //[MTFAllowedPropertyValue("Cam004", "cam004")]
        //[MTFAllowedPropertyValue("Cam005", "cam005")]
        //[MTFAllowedPropertyValue("Cam006", "cam006")]
        //[MTFAllowedPropertyValue("Cam007", "cam007")]
        //[MTFAllowedPropertyValue("Cam008", "cam008")]
        //[MTFAllowedPropertyValue("Cam009", "cam09")]
        public string VirtualCameraName { get; set; } //Elcom VCAM name
        public string IPAddress { get; set; }
        public bool IsSimulation { get; set; }
        public bool IsSimulationLogEnabled { get; set; }
        //public string VirtualCameraDisplayName { get; set; }
    }
}
