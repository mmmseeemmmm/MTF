using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.ComputerInfo
{
    public class CPUInfo
    {
        private string id;
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value.Replace("(TM)", "™").Replace("(tm)", "™").Replace("(R)", "®").Replace("(r)", "®").Replace("(C)", "©").Replace("(c)", "©").Replace("    ", " ").Replace("  ", " "); }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        private UInt16 architecture;
        public UInt16 Architecture
        {
            get { return architecture; }
            set { architecture = value; }
        }

        private uint cpuSpeedMHz;
        public uint CPUSpeedMHz
        {
            get { return cpuSpeedMHz; }
            set { cpuSpeedMHz = value; }
        }

        private uint cores;
        public uint Cores
        {
            get { return cores; }
            set { cores = value; }
        }

        private uint threads;
        public uint Threads
        {
            get { return threads; }
            set { threads = value; }
        }


    }
}
