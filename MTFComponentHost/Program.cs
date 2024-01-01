using MTFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MTFComponentHost
{
    class Program
    {
        private static IMTFComponentExuctorCallback callbackHandler;
        private static IMTFComponentExecutor componentExecutor;
        private static string hostId;
        static void Main(string[] args)
        {
            string ip = Environment.GetCommandLineArgs().First(i => i.ToUpper().StartsWith("-SERVERADDRESS=")).Substring(15);
            string port = Environment.GetCommandLineArgs().First(i => i.ToUpper().StartsWith("-SERVERPORT=")).Substring(12);
            hostId = Environment.GetCommandLineArgs().First(i => i.ToUpper().StartsWith("-HOSTID=")).Substring(9);

            //System.Diagnostics.Process.GetCurrentProcess().Id

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port))
            {
                return;
            }

            var myBinding = new NetTcpBinding();
            myBinding.ReceiveTimeout = new TimeSpan(0, 20, 0);
            myBinding.SendTimeout = new TimeSpan(0, 20, 0);
            myBinding.MaxReceivedMessageSize = int.MaxValue;
            myBinding.MaxBufferSize = int.MaxValue;

            var myEndpoint = new EndpointAddress("net.tcp://" + ip + ":" + port + "/MTF/");
            var myChannelFactory = new DuplexChannelFactory<IMTFComponentExecutor>(callbackHandler, myBinding, myEndpoint);

            try
            {
                componentExecutor = myChannelFactory.CreateChannel();
            }
            catch
            {
                return;
            }

            //wait for exit signal
        }
    }
}
