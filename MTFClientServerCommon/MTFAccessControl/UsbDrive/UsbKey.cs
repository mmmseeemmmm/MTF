using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbDrive
{
    public class UsbKey
    {
        internal UsbKey(string name)
        {
            this.DriveLetter = name;
            this.DeviceID = string.Empty;
            this.Volume = string.Empty;
        }

        public string DriveLetter { get; set; }
        public string Volume { get; set; }
        public string DeviceID { get; set; }
        public string HardwareID { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DriveLetter);
            sb.Append(" (");
            sb.Append(Volume);
            sb.Append(" )");
            return sb.ToString();
        }
    }

}
