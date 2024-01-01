using System.Collections.Generic;
using System.ServiceModel;

namespace MTFClientServerCommon.Services
{
    [ServiceContract]
    public interface IComponentsServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnMonsterClassLoaded();

        [OperationContract(IsOneWay = false)]
        string OnComponentConfigQuestion(string header, string text, SequenceMessageType messageType, SequenceMessageDisplayType displayType, IList<string> items);
    }
}
