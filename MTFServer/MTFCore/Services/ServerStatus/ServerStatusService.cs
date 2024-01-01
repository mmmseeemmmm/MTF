using System.Collections.Generic;
using MTFClientServerCommon.Services;

namespace MTFCore.Services.ServerStatus
{
    class ServerStatusService : ServiceBase, IServerStatusService
    {
        public override void Init()
        {
            
        }

        public override IEnumerable<ServiceWcfEndpointConfiguration> EndpointConfigurations => new[]
        {
            new ServiceWcfEndpointConfiguration {Interface = typeof(IServerStatusService), Address = "IServerStatusService"},
        };

        public string CommunicationTest() => "OK";
    }
}
