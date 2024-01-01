using System.Collections.Generic;

namespace MTFClientServerCommon
{
    public class ClientUIDataInfo
    {
        public string DataName { get; set; }
        public IEnumerable<SimpleClientControlInfo> ClientControlInfos { get; set; }
        public ClientDataDirection Direction { get; set; }
        public string DataType { get; set; }
    }

    public class SimpleClientControlInfo
    {
        public string TypeName { get; set; }
        public string AssemblyName { get; set; }        
    }

    public enum ClientDataDirection
    {
        ToClient,
        ToDriver
    }
}
