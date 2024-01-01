using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace AERAHeadlightServerInterface
{
    [MTFClass(Name = "AERA Headlight Server", Icon = MTFIcons.Vision, Description = "Interface for managing Headlight Server")]
    [MTFClassCategory("ALCZ EEV2")]
    public class HeadlightServerInterface : IDisposable
    {
        //public IMTFSequenceRuntimeContext runtimeContext;
        private string ipAdr;
        private int port;
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        [MTFConstructor(Description = "AERA Headlight Server")]
        [MTFAdditionalParameterInfo(Description = "", DisplayName = "Net name", DefaultValue = "127.0.0.1", ParameterName = "name")]
        [MTFAdditionalParameterInfo(Description = "", DisplayName = "Port", DefaultValue = "4005", ParameterName = "ethPort")]
        public HeadlightServerInterface(string name, int ethPort)
        {
            this.ipAdr = name;
            this.port = ethPort;

            s.Connect(IPAddress.Parse(ipAdr), port);
        }

        [MTFMethod]
        public string Capture()
        {
            byte[] ack = new byte[32];
            s.Send(Encoding.ASCII.GetBytes("capture\r\n"));
            s.Receive(ack);
            return Encoding.UTF8.GetString(ack);
        }

        [MTFMethod]
        public string getValues()
        {
            byte[] data = new byte[48];
            byte[] ack = new byte[16];
            string result;
            s.Send(Encoding.ASCII.GetBytes("getvalues lwr,afs,corr_Max\r\n"));
            s.Receive(data);
            s.Receive(ack);
            result = Encoding.UTF8.GetString(data);
            return result;
        }

        public void Dispose()
        {
            s.Close();
            s.Dispose();
        }
    }
}
