using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MTFClientServerCommon.Import
{
    class ImportSerializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (typeName.StartsWith("System.Collections.Generic.List`1[[MTFClientServerCommon.SequenceMetaData, MTFClientServerCommon"))
            {
                return typeof(List<SequenceMetaData>);
            }
            if (typeName == "MTFClientServerCommon.SequenceMetaData")
            {
                return typeof(SequenceMetaData);
            }

            var type = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
            return type;
        }
    }
}