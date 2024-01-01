using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using AutomotiveLighting.MTFCommon;

namespace MTFClientServerCommon.Services
{
    [ServiceContract(CallbackContract = typeof(IComponentsServiceCallback))]
    public interface IComponentsService
    {
        [OperationContract]
        IEnumerable<MTFClassInfo> AvailableMonsterClasses();

        [OperationContract]
        IEnumerable<MTFClassCategory> MTFClassCategories();

        [OperationContract]
        IEnumerable<ClientAssemblyInfo> GetClientAssemblies();

        [OperationContract]
        Stream DownloadClientAssembly(ClientAssemblyInfo assemblyInfo);

        [OperationContract]
        List<MTFParameterDescriptor> GetParameterDescription(string helperClassName, string assemblyFullName);

        [OperationContract]
        void ParameterDescriptionChanged(string helperClassName, ref List<MTFParameterDescriptor> currentParameterDescriptors, string assemblyFullName);

        [OperationContract]
        List<MTFClassInstanceConfiguration> ClassInstanceConfigurations(MTFClassInfo classInfo);

        [OperationContract]
        List<MTFClassInstanceConfiguration> ClassInstanceConfigurationsByNames(List<string> fileNames);

        [OperationContract]
        void SaveClassInstanceConfigurations(List<MTFClassInstanceConfiguration> classInstanceConfiguration, MTFClassInfo classInfo);

        [OperationContract]
        GenericClassInfo GetClassInfo(string fullName);

        [OperationContract]
        string TestClassInstance(MTFClassInstanceConfiguration classInstanceConfiguration);

        [OperationContract]
        IList<MTFValueList> LoadValueLists(MTFClassInstanceConfiguration classInstanceConfiguration);
    }
}
