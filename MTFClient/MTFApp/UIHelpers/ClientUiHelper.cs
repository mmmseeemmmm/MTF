using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MTFClientServerCommon;
using MTFCommon.ClientControls;
using MTFClientServerCommon.MTFAccessControl;
using MTFCommon;
using MTFClientServerCommon.GoldSamplePersist;
using System.Runtime.Serialization.Formatters.Binary;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.SequenceLocalization;
using MessageType = MTFCommon.ClientControls.MessageType;

namespace MTFApp.UIHelpers
{
    static class ClientUiHelper
    {
        private static readonly IEnumerable<string> clientUiPaths = null;
        private static readonly Dictionary<string, MTFClientControlBase> clientUICache = new Dictionary<string, MTFClientControlBase>();

        static ClientUiHelper()
        {
            var clientUiDir = new DirectoryInfo(BaseConstants.ClientControlAssemblyClientCachePath);
            if (clientUiDir.Exists)
            {
                clientUiPaths = clientUiDir.GetFiles("*.dll", SearchOption.AllDirectories).Select(x => x.FullName);
            }
        }


        public static void InitCache(IEnumerable<ClientContolInfo> clientContolInfos)
        {
            foreach (var clientControlInfo in clientContolInfos)
            {
                LoadToCache(clientControlInfo.AssemblyName, clientControlInfo.TypeName);
            }
        }

        public static MTFClientControlBase GetControl(string assemblyName, string typeName)
        {
            string key = GetKey(assemblyName, typeName);
            if (clientUICache.ContainsKey(key))
            {
                return clientUICache[key];
            }

            return LoadToCache(assemblyName, typeName);
        }

        public static void Clean()
        {
            foreach (var control in clientUICache.Values)
            {
                RemoveFromCache(control);
            }
        }

        public static void RemoveFromCache(string assemblyName, string typeName)
        {
            RemoveFromCache(GetControl(assemblyName, typeName));
        }

        private static void RemoveFromCache(MTFClientControlBase control)
        {
            control.OnSendData -= InstanceOnOnSendData;
            string instanceName = null;
            try
            {
                instanceName = control.GetType().FullName;
                control.Dispose();
            }
            catch (Exception ex)
            {
                SystemLog.LogMessage(string.Format("Disposing of {0} crashed. See below.", instanceName));
                SystemLog.LogException(ex);
            }

            clientUICache.Remove(GetKey(control.AssemblyName, control.TypeName));
        }

        public static void ReceiveData(byte[] data, ClientUIDataInfo info)
        {
            BinaryFormatter formatter = new BinaryFormatter { Binder = new BindChanger(Type.GetType(info.DataType)) };
            object deserializedData;
            using (MemoryStream ms = new MemoryStream(data))
            {
                deserializedData = formatter.Deserialize(ms);
            }

            foreach (var cci in info.ClientControlInfos)
            {
                var key = GetKey(cci.AssemblyName, cci.TypeName);
                if (clientUICache.ContainsKey(key))
                {
                    clientUICache[key].OnReceiveData(deserializedData, info.DataName);
                }
            }
        }

        private static IEnumerable<MTFClientControl> clientControlsInCache
        {
            get
            {
                var clientControls = clientUICache.Values.OfType<MTFClientControl>();
                return clientControls;
            }
        }

        public static void OnLanguageChanged(string language)
        {
            foreach (var clientControl in clientUICache.Values)
            {
                clientControl.OnLanguageChanged(language);
            }
        }

        public static void OnActivityChanged(MTFSequenceActivity activity)
        {
            foreach (var clientControl in clientControlsInCache)
            {
                clientControl.OnActivityChanged(new MTFActivity
                {
                    Id = activity.Id,
                    ActivityName = activity.ActivityName,
                    ClassAlias = activity.ClassInfo != null ? activity.ClassInfo.Alias : string.Empty,
                    MethodName = activity.MTFMethodName
                });
            }
        }

        public static void OnNewActivityResult(MTFClientServerCommon.MTFActivityResult result)
        {
            foreach (var clientControl in clientControlsInCache)
            {
                clientControl.OnNewActivityResult(new AutomotiveLighting.MTFCommon.MTFActivityResult
                {
                    Activity = new MTFActivity
                    {
                        Id = result.ActivityId,
                        ActivityName = result.ActivityName,
                        ClassAlias = result.MTFClassAlias,
                        MethodName = result.MTFMethodName
                    },
                    ExceptionOccured = result.ExceptionOccured,
                    CheckOutpuValueFailed = result.CheckOutpuValueFailed,
                    ExceptionMessage = result.ExceptionMessage,
                    Status = convertStatus(result.Status),
                    ElapsedMs = result.ElapsedMs,
                    TimestampMs = result.TimestampMs,
                    NumberOfRepetition =  result.NumberOfRepetition,
                    ActivityIdPath = result.ActivityIdPath,
                });
            }            
        }

        private static MTFActivityResultStatus convertStatus(MTFExecutionActivityStatus status)
        {
            switch (status)
            {
                case MTFExecutionActivityStatus.Nok: return MTFActivityResultStatus.Nok;
                case MTFExecutionActivityStatus.None: return MTFActivityResultStatus.None;
                case MTFExecutionActivityStatus.Ok: return MTFActivityResultStatus.Ok;
                case MTFExecutionActivityStatus.Warning: return MTFActivityResultStatus.Warning;
            }

            throw new Exception("Enum MTFExecutionActivityStatus has value which is not specificated in enum MTFActivityResultStatus");
        }

        public static void OnErrorMessage(StatusMessage message)
        {
            foreach (var clientControl in clientControlsInCache)
            {
                clientControl.OnErrorMessage(message.TimeStamp, SequenceLocalizationHelper.TranslateActivityPath(message.ActivityNames),
                    message.Text, (MessageType)message.Type);
            }            
        }
        
        public static void OnExecutionStateChanged(MTFSequenceExecutionState state)
        {
            foreach (var clientControl in clientControlsInCache)
            {
                clientControl.OnExecutionStateChanged((SequenceExecutionState)state);
            }            
        }

        public static void OnStatusMessage(string line1, string line2, string line3, StatusLinesFontSize fontSize)
        {
            foreach (var clientControl in clientControlsInCache)
            {
                clientControl.OnStatusMessage(line1, line2, line3, fontSize.Line1, fontSize.Line2, fontSize.Line3);
            }            
        }

        public static void OnVariantChanged(SequenceVariantInfo sequenceVariantInfo)
        {
            MTFSequenceVariant variant = new MTFSequenceVariant { VariantGroups = new List<MTFSequenceVariantGroup>() };
            foreach (var variantGroup in sequenceVariantInfo.SequenceVariant.VariantGroups)
            {
                variant.VariantGroups.Add(new MTFSequenceVariantGroup {Name = variantGroup.Name, Values = variantGroup.Values.Select(v => new MTFSequenceVariantValue{Name = v.Name}).ToList()});
            }

            foreach (var clientControl in clientControlsInCache)
            {
                clientControl.OnVariantChanged(variant);
            } 
        }

        public static void OnAccessKeyChanged(AccessKey accessKey)
        {
            foreach (var clientControl in clientUICache.Values)
            {
                clientControl.OnAccessKeyChanged(ConvertAccessKey(accessKey));
            } 
        }

        private static MTFAccessKey ConvertAccessKey(AccessKey accessKey)
        {
            if (accessKey == null)
            {
                return null;
            }

            MTFAccessKey key = null;
            if (accessKey.IsValid)
            {
                key = new MTFAccessKey
                {
                    UserFirstName = accessKey.KeyOwnerFirstName,
                    UserLastName = accessKey.KeyOwnerLastName,
                    Type = accessKey.AccessKeyType,
                    Expiration = accessKey.Expiration,
                };
            }

            return key;
        }

        private static string GetKey(string assemblyName, string typeName)
        {
            return string.Format("{0}:{1}", assemblyName, typeName);
        }

        private static MTFClientControlBase LoadToCache(string assemblyName, string typeName)
        {
            var fullPath = clientUiPaths?.FirstOrDefault(x => x.EndsWith(assemblyName));
            if (fullPath == null)
            {
                throw new Exception("Assembly with client UI not found.");
            }

            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
                var instance = Activator.CreateInstanceFrom(fullPath, typeName).Unwrap() as MTFClientControlBase;
                LoadAssembliesFromFolder(Path.GetDirectoryName(fullPath));
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
                    
                if (instance != null)
                {
                    instance.OnSendData += InstanceOnOnSendData;
                    instance.HasUserRole = HasUserRole;
                    instance.AssemblyName = assemblyName;
                    instance.TypeName = typeName;
                    clientUICache[GetKey(assemblyName, typeName)] = instance;
                    instance.OnAccessKeyChanged(ConvertAccessKey(EnvironmentHelper.CurrentAccessKey));
                    return instance;
                }
            }
            catch (Exception ex)
            {
                SystemLog.LogMessage(string.Format("Creating of {0} crashed. See below.", typeName));
                SystemLog.LogException(ex);

                throw;
            }   
         
            return null;
        }

        private static bool HasUserRole(string roleName)
        {
            return EnvironmentHelper.HasAccessKeyRole(roleName);
        }

        private static void LoadAssembliesFromFolder(string folder)
        {
            foreach (var asmFile in Directory.GetFiles(folder, "*.dll"))
            {
                try
                {
                    Assembly.LoadFile(asmFile);
                }
                catch
                {
                }
            }
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.RequestingAssembly == null)
            {
                return null;
            }

            string assemblyFullName = Path.Combine(Path.GetDirectoryName(args.RequestingAssembly.Location), args.Name.Split(',')[0] + ".dll");
            if (File.Exists(assemblyFullName))
            {
                return Assembly.LoadFile(assemblyFullName);
            }
           
            return null;
        }

        private static void InstanceOnOnSendData(object sender, object data, string dataName)
        {
            var senderType = sender.GetType();

            byte[] rawData;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, data);

                rawData = ms.ToArray();
            }

            MTFClient.GetMTFClient().ClientUISendData(rawData, new ClientUIDataInfo
            {
                DataName = dataName,
                DataType = data.GetType().AssemblyQualifiedName,
                Direction = ClientDataDirection.ToDriver,
                ClientControlInfos = new[] {new SimpleClientControlInfo{AssemblyName = Path.GetFileName(senderType.Assembly.Location), TypeName = senderType.FullName}}
            });
        }
    }
}
