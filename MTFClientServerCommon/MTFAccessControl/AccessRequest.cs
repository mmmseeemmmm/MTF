using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.MTFAccessControl.UsbDrive
{
    [Serializable]
    public class AccessRequest
    {
        public string UsbID { get; set; }        
        public string SWVersion { get; set; }
        public string UsbHwID { get; set; }

        public AccessRequest(string UsbID, string SWVersion, string UsbHwId)
        {
            this.UsbID = UsbID;
            this.SWVersion = SWVersion;
            this.UsbHwID = UsbHwId;
        }
    }
}
