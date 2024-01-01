using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTFApp.Managers;
using MTFApp.ServerService.Components;
using MTFApp.ServerService.ServerStatus;

namespace MTFApp.ServerService
{
    static class ServiceClientsContainer
    {
        private static readonly List<IServiceClientBase> serviceClients = new List<IServiceClientBase>();

        static ServiceClientsContainer()
        {
            serviceClients = new List<IServiceClientBase>
            {
                new ComponentsClient(),
                new ServerStatusClient(),
            };
        }

        public static void ConnectAll()
        {
            List<Task> connectTasks = new List<Task>();
            foreach (var serviceClient in serviceClients)
            {
                connectTasks.Add(Task.Run(() =>
                {
                    serviceClient.Connect();
                }));
            }
            Task.WaitAll(connectTasks.ToArray());

            try
            {
                Get<ServerStatusClient>().CommunicationTest();
            }
            catch (Exception)
            {
                try
                {
                    List<Task> disconnectTasks = new List<Task>();
                    foreach (var serviceClient in serviceClients)
                    {
                        disconnectTasks.Add(Task.Run(() =>
                        {
                            serviceClient.Disconnect();
                        }));
                    }
                    Task.WaitAll(disconnectTasks.ToArray());
                }
                catch { }

                throw;
            }

            if (AllIsConnected)
            {
                ManagersContainer.Init();
            }
        }

        public static bool AllIsConnected => serviceClients.All(s => s.IsConnected);

        public static T Get<T>() where T : class => serviceClients.FirstOrDefault(s => s.GetType() == typeof(T)) as T;
    }
}
