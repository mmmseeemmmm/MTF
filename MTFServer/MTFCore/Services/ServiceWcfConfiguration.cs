using System.Collections.Generic;

namespace MTFCore.Services
{
    class ServiceWcfConfiguration
    {
        public object Instance;
        public IEnumerable<ServiceWcfEndpointConfiguration> EndpointConfigurations;
    }
}
