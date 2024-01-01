using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon;
using MTFClientServerCommon.ClassInformation;
using MTFClientServerCommon.Constants;

namespace MTFCore
{
    class AssemblyWalker : MarshalByRefObject
    {
        public AssemblyWalker()
        {
            SystemLog.LogDirectory = BaseConstants.ServerSystemLogsPath;
            AvailableClasses = new List<GenericClassInfo>();
            AvailableMonsterClasses = new List<MTFClassInfo>();
        }

        public List<MTFClassInfo> AvailableMonsterClasses { get; }
        public List<GenericClassInfo> AvailableClasses { get; }

        public void ProcessAssembly(string assemblyPath)
        {
            try
            {
                if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == Path.GetFileNameWithoutExtension(assemblyPath)))
                {
                    return;
                }
                ProcessAssembly(Assembly.LoadFrom(assemblyPath)); 
            }
            catch (BadImageFormatException)
            { }
            catch (Exception e)
            {
                    SystemLog.LogMessage($"Error during loading assembly {assemblyPath}. Inner exception: {e.Message}", true);
            }
        }

        public void ProcessAssembly(Assembly assembly)
        {
            ReadAllClassesFromAssembly(assembly);
            ReadMonsterClassesFromAssembly(assembly);
        }

        private void ReadAllClassesFromAssembly(Assembly asm)
        {
            foreach (var classType in asm.GetTypes())
            {
                if (classType.GetCustomAttributes<MTFKnownClassAttribute>(true).Any())
                {
                    AvailableClasses.Add(new GenericClassInfo(classType));
                }
            }
        }

        private void ReadMonsterClassesFromAssembly(Assembly asm)
        {
            var monsterTypes = asm.GetTypes().Where(item => item.GetCustomAttributes<MTFClassAttribute>(true)
                .Any()).ToArray();

            foreach (Type monsterType in monsterTypes)
            {
                var classInfo = new MTFClassInfo(monsterType);

                //Handle client controls - read needed client assemblies
                var clientControls = monsterType.GetCustomAttributes<MTFUseClientControlAttribute>(true);
                if (clientControls != null && clientControls.Any())
                {
                    classInfo.ClientControlInfos = new List<ClientContolInfo>(clientControls.Count());
                    foreach (var clientControl in clientControls)
                    {
                        classInfo.ClientControlInfos.Add(GetClientControlInfo(FindClientUIAssembly(clientControl.Assembly), clientControl.TypeName));
                    }
                }

                AvailableMonsterClasses.Add(classInfo);
            }
        }

        private static string FindClientUIAssembly(string assemblyName)
        {
            var fileList = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, BaseConstants.ClientControlAssemblyServerPath)).GetFiles(assemblyName, SearchOption.AllDirectories);
            if (fileList.Any())
            {
                return Path.Combine(fileList.First().DirectoryName, assemblyName);
            }

            throw new Exception($"Assembly {assemblyName} was not found.");
        }

        private static ClientContolInfo GetClientControlInfo(string assemblyName, string typeName)
        {
            var clientControlAttr = Assembly.LoadFile(assemblyName).GetType(typeName).GetCustomAttribute<MTFClientControlAttribute>();

            if (clientControlAttr != null)
            {
                return new ClientContolInfo
                {
                    Name = clientControlAttr.Name,
                    Description = clientControlAttr.Description,
                    Icon = clientControlAttr.Icon,
                    AssemblyName = Path.GetFileName(assemblyName),
                    TypeName = typeName,
                    Type = ClientControlType.ClientUI,
                };
            }

            var setupControlAttr = Assembly.LoadFile(assemblyName).GetType(typeName).GetCustomAttribute<MTFClientControlSetupAttribute>();
            if (setupControlAttr != null)
            {
                return new ClientContolInfo
                {
                    Name = setupControlAttr.Name,
                    Description = setupControlAttr.Description,
                    Icon = setupControlAttr.Icon,
                    AssemblyName = Path.GetFileName(assemblyName),
                    TypeName = typeName,
                    Type = ClientControlType.Setup,
                };
            }

            return null;
        }
    }
}
