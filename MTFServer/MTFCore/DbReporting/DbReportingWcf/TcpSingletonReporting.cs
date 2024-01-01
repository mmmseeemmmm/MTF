using System;
using System.Collections.Generic;
using System.ServiceModel;
using MTFClientServerCommon.DbReporting;

namespace MTFCore.DbReporting.DbReportingWcf
{
    public class TcpSingletonReporting
    {
        private readonly DbReportingCore reportingCore;
        private static TcpSingletonReporting instance;
        private ServiceHost host;

        private TcpSingletonReporting()
        {
            reportingCore = new DbReportingCore();
        }

        public static TcpSingletonReporting Instance => instance ?? (instance = new TcpSingletonReporting());

        public void Start(string address, string port)
        {
            var baseAddresses = new List<Uri>{ new Uri("net.tcp://" + address + ":" + port + "/MTFReporting") };
            var endpointInterfaces = new List<Type> { typeof(IDbReportingService) };

            host = new ServiceHost(reportingCore, baseAddresses.ToArray());
            //Use object as singleton
            host.Description.Behaviors.Find<ServiceBehaviorAttribute>().InstanceContextMode = InstanceContextMode.Single;

            var endpoint = new NetTcpBinding
                           {
                               ReceiveTimeout = TimeSpan.MaxValue,
                               SendTimeout = TimeSpan.MaxValue,
                               MaxBufferSize = int.MaxValue,
                               MaxReceivedMessageSize = int.MaxValue,
                               Security = {Mode = SecurityMode.None}
                           };



            foreach (var endpointInterface in endpointInterfaces)
            {
                host.AddServiceEndpoint(endpointInterface, endpoint, "");
            }

            host.Open();
        }

        public void Stop()
        {
            reportingCore?.Dispose();
            host?.Close();
            instance = null;
        }
    }
}
