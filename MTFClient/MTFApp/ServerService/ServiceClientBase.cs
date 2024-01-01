using System;
using System.ServiceModel;
using MTFClientServerCommon.Helpers;

namespace MTFApp.ServerService
{
    abstract class ServiceClientBase<TServiceInterface> : IServiceClientBase
    {
        private ChannelFactory<TServiceInterface> channelFactory;
        public bool IsConnected { get; private set; }

        protected TServiceInterface Service { get; private set; }

        protected abstract string ServiceAddress { get; }

        protected virtual object CallbackHandler => null;

        public void Connect()
        {
            if (IsConnected)
            {
                return;
            }

            var myBinding = new NetTcpBinding
            {
                ReceiveTimeout = new TimeSpan(0, 20, 0),
                SendTimeout = new TimeSpan(0, 20, 0),
                MaxReceivedMessageSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                Security = { Mode = SecurityMode.None }
            };

            var myEndpoint = new EndpointAddress($"net.tcp://{SettingsClass.SelectedConnection.Host}:{SettingsClass.SelectedConnection.Port}/MTF/{ServiceAddress}");
            channelFactory = CallbackHandler == null
                ? new ChannelFactory<TServiceInterface>(myBinding, myEndpoint)
                : new DuplexChannelFactory<TServiceInterface>(CallbackHandler, myBinding, myEndpoint);

            try
            {
                Service = channelFactory.CreateChannel();
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("{0}{1}{1}{2}", LanguageHelper.GetString("MTF_Connection_CantConnect"), Environment.NewLine, e.Message), e);
            }
            IsConnected = true;
        }

        public void Disconnect()
        {
            IsConnected = false;
            try
            {
                if (Service != null && ((IClientChannel) Service).State == CommunicationState.Opened)
                {
                    ((IClientChannel) Service).Close();
                }
            }
            catch { }

            try
            {
                if (channelFactory != null && channelFactory.State == CommunicationState.Opened)
                {
                    channelFactory.Close();
                }
            }
            catch { }

            Service = default(TServiceInterface);
            channelFactory = null;
        }
    }
}
