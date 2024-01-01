using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Services;
using MTFCore.Managers;
using MTFCore.Managers.Components;

namespace MTFCore.Services.Components
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class ComponentsService : ServiceWithCallback<IComponentsServiceCallback>, IComponentsService
    {
        private ComponentsManager componentsManager;
        public override void Init()
        {
            componentsManager = ManagersContainer.Get<ComponentsManager>();
        }

        public override IEnumerable<ServiceWcfEndpointConfiguration> EndpointConfigurations => new[]
        {
            new ServiceWcfEndpointConfiguration {Interface = typeof(IComponentsService), Address = "IComponentsService"},
        };

        public IEnumerable<MTFClassInfo> AvailableMonsterClasses() => componentsManager.AvailableMonsterClasses.ToArray();

        public IEnumerable<MTFClassCategory> MTFClassCategories()
        {
            RememberClient();
            return componentsManager.ClassCategories.ToArray();
        }

        public IEnumerable<ClientAssemblyInfo> GetClientAssemblies()
        {
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, BaseConstants.ClientControlAssemblyServerPath)))
            {
                return new ClientAssemblyInfo[0];
            }

            var fileList = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, BaseConstants.ClientControlAssemblyServerPath)).GetFiles("*.*", SearchOption.AllDirectories);

            return fileList.Select(i => new ClientAssemblyInfo
            {
                Name = i.Name,
                Path = Path.Combine(Environment.CurrentDirectory, BaseConstants.ClientControlAssemblyServerPath) != i.DirectoryName ? i.DirectoryName.Substring(Path.Combine(Environment.CurrentDirectory, BaseConstants.ClientControlAssemblyServerPath).Length + 1) : string.Empty,
                Size = i.Length
            });
        }

        public Stream DownloadClientAssembly(ClientAssemblyInfo assemblyInfo)
        {
            string fullFileName = Path.Combine(Environment.CurrentDirectory, BaseConstants.ClientControlAssemblyServerPath, assemblyInfo.Path, assemblyInfo.Name);

            if (!File.Exists(fullFileName))
            {
                return null;
            }

            return new FileStream(fullFileName, FileMode.Open, FileAccess.Read);
        }

        public List<MTFParameterDescriptor> GetParameterDescription(string helperClassName, string assemblyFullName)
        {
            var helperClassInstance = componentsManager.CreateHelperClassInstance(helperClassName, assemblyFullName);

            var paramDescriptors = helperClassInstance?.GetParameterDescriptors();

            return paramDescriptors;
        }

        public void ParameterDescriptionChanged(string helperClassName, ref List<MTFParameterDescriptor> currentParameterDescriptors, string assemblyFullName)
        {
            var helperClassInstance = componentsManager.CreateHelperClassInstance(helperClassName, assemblyFullName);

            helperClassInstance?.ParameterDescriptorChanged(ref currentParameterDescriptors);
        }

        public List<MTFClassInstanceConfiguration> ClassInstanceConfigurations(MTFClassInfo classInfo)
        {
            var instanceConfigs = Core.Persist.LoadDataList<MTFClassInstanceConfiguration>(Core.ClassInstanceConfigPersistPath(classInfo));

            //fix if configuration is stored in old MTF and there isn't information about assembly path,
            //this can be removed when data is in DB and there isn't data redundancy in sequence file
            foreach (var instanceConfig in instanceConfigs)
            {
                if (string.IsNullOrEmpty(instanceConfig.ClassInfo.RelativePath))
                {
                    var storedMonsterClass = componentsManager.AvailableMonsterClasses.FirstOrDefault(m => m.FullName == instanceConfig.ClassInfo.FullName);
                    instanceConfig.ClassInfo.RelativePath = storedMonsterClass?.RelativePath;
                }
            }

            return instanceConfigs;
        }

        public List<MTFClassInstanceConfiguration> ClassInstanceConfigurationsByNames(List<string> fileNames)
        {
            var outputList = new List<MTFClassInstanceConfiguration>();
            foreach (var name in fileNames)
            {
                outputList.AddRange(Core.Persist.LoadDataList<MTFClassInstanceConfiguration>(Path.Combine(BaseConstants.ClassInstanceConfigBasePath, name)));
            }
            return outputList;
        }

        public void SaveClassInstanceConfigurations(List<MTFClassInstanceConfiguration> classInstanceConfiguration, MTFClassInfo classInfo) =>
            Core.Persist.SaveData(classInstanceConfiguration, Core.ClassInstanceConfigPersistPath(classInfo));

        //only for editor
        public GenericClassInfo GetClassInfo(string fullName)
        {
            var genericClass = componentsManager.AvailableClasses.FirstOrDefault(c => c.FullName == fullName);
            if (genericClass == null && !string.IsNullOrEmpty(fullName))
            {
                if (fullName.StartsWith(typeof(List<>).FullName))//TODO dictionary
                {
                    string[] slitFullName = fullName.Split('[', ']', ',');
                    foreach (var item in slitFullName)
                    {
                        if (!item.Contains('=') && !string.IsNullOrEmpty(item))
                        {
                            var genericClassInList = componentsManager.AvailableClasses.FirstOrDefault(c => c.FullName == item);
                            if (genericClassInList != null)
                            {
                                return genericClassInList;
                            }
                        }
                    }
                }
                else if (fullName.EndsWith("[]"))
                {
                    var genericClassInArray = componentsManager.AvailableClasses.FirstOrDefault(c => c.FullName == fullName.Replace("[]", string.Empty));
                    return genericClassInArray;
                }
                else
                {
                    return null;
                }
            }
            return genericClass;
        }

        public string TestClassInstance(MTFClassInstanceConfiguration classInstanceConfiguration)
        {
            object instance = null;
            try
            {
                instance = componentsManager.CreateInstance(classInstanceConfiguration);
                return "OK";
            }
            catch (Exception ex)
            {
                return (ex.InnerException == null) ? ex.Message : ex.Message + "\n" + ex.InnerException.Message;
            }
            finally
            {
                if (instance is IDisposable)
                {
                    try
                    {
                        ((IDisposable)instance).Dispose();
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogException(ex);
                    }
                }
            }
        }

        public IList<MTFValueList> LoadValueLists(MTFClassInstanceConfiguration classInstanceConfiguration)
        {
            object instance = null;
            try
            {
                instance = componentsManager.CreateInstance(classInstanceConfiguration);
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }

            MTFComponentConfigContext componentConfigContext = new MTFComponentConfigContext
            {
                MessageBoxChoiseMethod = MessageBoxChoice,
                MessageBoxListBoxMethod = MessageBoxListBox,
                MessageBoxMultiChoiseMethod = MessageBoxMultiChoice,
            };
            //try find property of IMTFComponentConfigContext
            foreach (var property in instance.GetType().GetProperties().Where(p => p.PropertyType == typeof(IMTFComponentConfigContext) && p.CanWrite))
            {
                property.GetSetMethod().Invoke(instance, new object[] { componentConfigContext });
            }
            //try find filed of IMTFComponentConfigContext
            foreach (var field in instance.GetType().GetFields().Where(f => f.FieldType == typeof(IMTFComponentConfigContext)))
            {
                field.SetValue(instance, componentConfigContext);
            }

            //load lists
            var valueListGetterMethods = instance.GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(MTFValueListGetterMethodAttribute)).Any());
            List<MTFValueList> valueLists = new List<MTFValueList>();

            foreach (var m in valueListGetterMethods)
            {
                if (m.ReturnType != typeof(List<Tuple<string, object>>))
                {
                    throw new FaultException($"Method {m.Name} is taged as value list getter method. This method must have List<Tuple<string, object>> output type.");
                }
                try
                {
                    var values = m.Invoke(instance, null) as List<Tuple<string, object>>;
                    if (values != null)
                    {
                        valueLists.Add(new MTFValueList(m.Name, m.GetCustomAttribute<MTFValueListGetterMethodAttribute>().SubListSeparator, values));
                    }
                }
                catch (Exception e)
                {
                    if (instance is IDisposable)
                    {
                        ((IDisposable)instance).Dispose();
                    }
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Load value list crashed by executing method {m.Name} on object {classInstanceConfiguration.ClassInfo.FullName}.").AppendLine().Append("Method exception: ");
                    if (e.InnerException != null)
                    {
                        sb.Append(e.InnerException.Message);
                    }
                    else
                    {
                        sb.Append(e.Message);
                    }

                    throw new FaultException(sb.ToString());
                }
            }

            if (instance is IDisposable)
            {
                ((IDisposable)instance).Dispose();
            }
            return valueLists;
        }

        private static string MessageBoxChoice(string header, string text, IList<string> items)
        {
            var c = OperationContext.Current.GetCallbackChannel<IComponentsServiceCallback>();
            return c.OnComponentConfigQuestion(header, text, SequenceMessageType.Choice, SequenceMessageDisplayType.Buttons, items);
        }
        private static string MessageBoxListBox(string header, string text, IList<string> items)
        {
            var c = OperationContext.Current.GetCallbackChannel<IComponentsServiceCallback>();
            return c.OnComponentConfigQuestion(header, text, SequenceMessageType.Choice, SequenceMessageDisplayType.ComboBox, items);
        }
        private static string MessageBoxMultiChoice(string header, string text, IList<string> items)
        {
            var c = OperationContext.Current.GetCallbackChannel<IComponentsServiceCallback>();
            return c.OnComponentConfigQuestion(header, text, SequenceMessageType.Choice, SequenceMessageDisplayType.ToggleButtons, items);
        }

        public void RaiseOnMonsterClassLoaded() => CallbackInvoke(c => c.OnMonsterClassLoaded());
    }
}
