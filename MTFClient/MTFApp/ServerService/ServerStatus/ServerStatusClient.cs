using MTFClientServerCommon.Services;

namespace MTFApp.ServerService.ServerStatus
{
    class ServerStatusClient : ServiceClientBase<IServerStatusService>
    {
        protected override string ServiceAddress => "IServerStatusService";

        public string CommunicationTest() => Service.CommunicationTest();
    }
}
