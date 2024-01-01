using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PLC.Helpers;

namespace PLC
{
    public interface IPLCCommunication
    {
        event OnDataEventHandler OnData;
        void Start(string IpPC, string PortPC, string IpPLC, string PortPLC, bool onlySending, string SendingPortPLC);
        void SendData(byte[] byteData);
        DateTime LastRxTimeStamp();
        void Close();
    }
}
