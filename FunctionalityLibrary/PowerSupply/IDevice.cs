using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTFCommon;

namespace PowerSupply
{
    public interface IDevice
    {
        //string Read();
        //void Write(string cmd);
        MTFPercentProgressEventHandler InitProgress { get; set; }
    }
}
