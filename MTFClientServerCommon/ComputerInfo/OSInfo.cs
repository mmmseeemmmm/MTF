using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.ComputerInfo
{
    public class OSInfo
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string version;
        public string Version
        {
            get { return version; }
            set { version = value; }
        }

        private string architecture;
        public string Architecture
        {
            get { return architecture; }
            set { architecture = value; }
        }

        private string serialNumber;
        public string SerialNumber
        {
            get { return serialNumber; }
            set { serialNumber = value; }
        }

        private string build;
        public string Build
        {
            get { return build; }
            set { build = value; }
        }
    }
}
