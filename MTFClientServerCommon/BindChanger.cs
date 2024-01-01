using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace MTFClientServerCommon
{
    public class BindChanger : SerializationBinder
    {
        private Type type;
        public BindChanger(Type type)
        {
            this.type = type;
        }
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (type != null && (type.FullName == typeName
                || (Regex.Replace(type.FullName, @"Version=\d+.\d+.\d+.\d+,", string.Empty) == Regex.Replace(typeName, @"Version=\d+.\d+.\d+.\d+,", string.Empty))))
            {
                return type;
            }
            var defaultType = Type.GetType(typeName, AssemblyResolver, TypeResolver);
            if (defaultType != null)
            {
                return defaultType;
            }

            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName.Split(',')[0]);
            if (asm == null)
            {
                return null;
            }

            return asm.GetType(typeName);
        }

        private Assembly AssemblyResolver(AssemblyName name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == name.Name); ;
        }

        private Type TypeResolver(Assembly assembly, string typeName, bool b)
        {
            return assembly != null ? assembly.GetType(typeName, b) : Type.GetType(typeName, b);
        }
    }
}
