using MTFClientServerCommon;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using MTFCore.Managers;
using MTFCore.Managers.Components;
using MTFCore.Services;

namespace MTFCore
{
    public class TcpSingletonMTFCore
    {
        private readonly List<ServiceHost> hosts = new List<ServiceHost>();

        public event OnMessageEventHandler OnMessage;
        public delegate void OnMessageEventHandler(string header, string message, int level);

        public event RequestServerStopHandler RequestServerStop;
        public delegate void RequestServerStopHandler();

        public event ServerSettingsChangedHandler ServerSettingsChanged;
        public delegate void ServerSettingsChangedHandler(ServerSettings settings);

        public TcpSingletonMTFCore() => InitMTFServerCore();

        public void Start(string address, string port)
        {
            Address = address;
            Port = port;

            foreach (var serviceConfiguration in ServiceContainer.GetAllServiceWcfConfigurations())
            {
                CreateHost(serviceConfiguration);
            }
        }

        public void Stop()
        {
            ServiceContainer.Get<Core>()?.StopSequence();

            foreach (var host in hosts)
            {
                if (host.State == CommunicationState.Opened)
                {
                    host.Close();
                }
            }

            hosts.Clear();
        }

        public void SaveServerSettings() => ServiceContainer.Get<Core>()?.SaveServerSettings();

        public string Address { get; private set; }

        public string Port { get; private set; }

        public ServerSettings ServerSettings
        {
            get => ServiceContainer.Get<Core>().ServerSettings;
            set => ServiceContainer.Get<Core>().ServerSettings = value;
        }

        private void CreateHost(ServiceWcfConfiguration serviceConfiguration)
        {
            List<Uri> baseAddresses = new List<Uri> { new Uri("net.tcp://" + Address + ":" + Port + "/MTF") };

            var host = new ServiceHost(serviceConfiguration.Instance, baseAddresses.ToArray());
            //Use object as singleton
            host.Description.Behaviors.Find<ServiceBehaviorAttribute>().InstanceContextMode = InstanceContextMode.Single;
            hosts.Add(host);

            var endpoint = new NetTcpBinding
            {
                ReceiveTimeout = TimeSpan.MaxValue,
                SendTimeout = TimeSpan.MaxValue,
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                Security = { Mode = SecurityMode.None }
            };

            foreach (var endpointConfiguration in serviceConfiguration.EndpointConfigurations)
            {
                host.AddServiceEndpoint(endpointConfiguration.Interface, endpoint, endpointConfiguration.Address);
            }

            host.Open();
        }
        
        private void InitMTFServerCore()
        {
            ManagersContainer.Init();
            ServiceContainer.Init();

            ServiceContainer.Get<Core>().OnMessage += mtfServerCore_OnMessage;
            ServiceContainer.Get<Core>().RequestServerStop += mtfServerCore_RequestServerStop;
            ServiceContainer.Get<Core>().ServerSettingsChanged += mtfServerCore_ServerSettingsChanged;
        }

        private void mtfServerCore_ServerSettingsChanged(ServerSettings settings) => ServerSettingsChanged?.Invoke(ServerSettings);

        void mtfServerCore_RequestServerStop() => RequestServerStop?.Invoke();

        void mtfServerCore_OnMessage(string header, string message, int level) => OnMessage?.Invoke(header, message, level);
    }
}
