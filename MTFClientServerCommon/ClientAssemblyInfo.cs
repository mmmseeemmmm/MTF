using System;

namespace MTFClientServerCommon
{
    [Serializable]
    public class ClientAssemblyInfo
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string Path { get; set; }
    }
}
