using System;
using System.IO;
using System.Reflection;

namespace MTFCommon
{
    public class MTFDriverHandler : MarshalByRefObject
    {
        public MTFDriverHandler()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        private object instance;
        public void CreateInstance(string assemblyName, string className, object[] parameters)
        {
            var asm = Assembly.Load(assemblyName);
            instance = asm.CreateInstance(className, false, BindingFlags.CreateInstance, null, parameters, null, null);
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.Load(File.ReadAllBytes(args.Name));
        }
    }
}
