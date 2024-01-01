using System.ServiceModel;

namespace MTFClientServerCommon.Services
{
    [ServiceContract]
    public interface IServerStatusService
    {
        [OperationContract]
        string CommunicationTest();
    }
}
