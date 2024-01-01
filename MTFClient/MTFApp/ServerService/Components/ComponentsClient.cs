using System.Collections.Generic;
using System.IO;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon;
using MTFClientServerCommon.Services;

namespace MTFApp.ServerService.Components
{
    class ComponentsClient : ServiceClientBase<IComponentsService>
    {
        public delegate void MonsterClassLoadedEventHandler();
        public event MonsterClassLoadedEventHandler MonsterClassLoaded;
        
        public delegate string ComponentConfigOnQuestionEventHandler(string header, string text, SequenceMessageType messageType, SequenceMessageDisplayType displayType, IList<string> items);
        public event ComponentConfigOnQuestionEventHandler ComponentConfigOnQuestion;

        private readonly ComponentsServiceCallbackHandler callbackHandler = new ComponentsServiceCallbackHandler();

        public ComponentsClient()
        {
            //event registration
            callbackHandler.MonsterClassLoaded += OnMonsterClassLoaded;
            callbackHandler.ComponentConfigQuestion += OnComponentConfigQuestion;
        }

        protected override string ServiceAddress => "IComponentsService";
        protected override object CallbackHandler => callbackHandler;

        public IEnumerable<MTFClassInfo> AvailableMonsterClasses() => Service.AvailableMonsterClasses();
        public IEnumerable<MTFClassCategory> MTFClassCategories => Service.MTFClassCategories();
        public Stream DownloadClientAssembly(ClientAssemblyInfo assemblyInfo) => Service.DownloadClientAssembly(assemblyInfo);
        public IEnumerable<ClientAssemblyInfo> GetClientAssemblies() => Service.GetClientAssemblies();
        public void ParameterDescriptionChanged(string parameterHelperClassName, ref List<MTFParameterDescriptor> parameterHelperClone, string fullyQualifiedName) => 
            Service.ParameterDescriptionChanged(parameterHelperClassName, ref parameterHelperClone, fullyQualifiedName);
        public List<MTFParameterDescriptor> GetParameterDescription(string helperClassName, string assemblyFullName) => Service.GetParameterDescription(helperClassName, assemblyFullName);
        public List<MTFClassInstanceConfiguration> ClassInstanceConfigurations(MTFClassInfo classInfo) => Service.ClassInstanceConfigurations(classInfo);
        public List<MTFClassInstanceConfiguration> ClassInstanceConfigurationsByNames(List<string> fileNames) => Service.ClassInstanceConfigurationsByNames(fileNames);
        public void SaveClassInstanceConfiguration(List<MTFClassInstanceConfiguration> classInstanceConfiguration, MTFClassInfo classInfo) => Service.SaveClassInstanceConfigurations(classInstanceConfiguration, classInfo);
        public GenericClassInfo GetClassInfo(string fullName) => Service.GetClassInfo(fullName);
        public string TestClassInstance(MTFClassInstanceConfiguration classInstanceConfiguration) => Service.TestClassInstance(classInstanceConfiguration);
        public IList<MTFValueList> LoadValueLists(MTFClassInstanceConfiguration classInstanceConfiguration) => Service.LoadValueLists(classInstanceConfiguration);

        private void OnMonsterClassLoaded()
        {
            var handler = MonsterClassLoaded;
            handler?.Invoke();
        }
        private string OnComponentConfigQuestion(string header, string text, SequenceMessageType messageType, SequenceMessageDisplayType displayType, IList<string> items)
        {
            var handler = ComponentConfigOnQuestion;
            return handler?.Invoke(header, text, messageType, displayType, items);
        }
    }
}
