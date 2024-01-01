using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTFCore.Services.Components;
using MTFCore.Services.ServerStatus;

namespace MTFCore.Services
{
    static class ServiceContainer
    {
        private static readonly List<ServiceBase> services;
        static ServiceContainer()
        {
            services = new List<ServiceBase>
            {
                new Core(),
                new ComponentsService(),
                new ServerStatusService(),
            };
        }

        public static void Init()
        {
            List<Task> initTasks = new List<Task>();
            foreach (var service in services)
            {
                initTasks.Add(Task.Run(() => service.Init()));
            }

            Task.WaitAll(initTasks.ToArray());
        }

        public static IEnumerable<ServiceWcfConfiguration> GetAllServiceWcfConfigurations()
        {
            return services.Select(s => new ServiceWcfConfiguration{Instance = s, EndpointConfigurations = s.EndpointConfigurations} );
        }

        public static T Get<T>() where T : class => services.FirstOrDefault(s => s.GetType() == typeof(T)) as T;
    }
}
