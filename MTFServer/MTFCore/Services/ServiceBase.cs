using System.Collections.Generic;

namespace MTFCore.Services
{
    abstract class ServiceBase
    {
        public abstract void Init();
        public abstract IEnumerable<ServiceWcfEndpointConfiguration> EndpointConfigurations { get; }
    }
}
