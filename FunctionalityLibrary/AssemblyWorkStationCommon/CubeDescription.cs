using System;
using System.Collections.Generic;

namespace AssemblyWorkStationCommon
{
    [Serializable]
    public class CubeDescription
    {
        public int Spdl { get; set; }
        public List<string> InpuntNames { get; set; }
        public List<string> OutputNames { get; set; }
    }
}
