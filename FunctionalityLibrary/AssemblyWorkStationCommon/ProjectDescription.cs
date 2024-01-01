using System;
using System.Collections.Generic;

namespace AssemblyWorkStationCommon
{
    [Serializable]
    public class ProjectDescription
    {
        public string ProjectName { get; set; }
        public List<CubeDescription> CubeDescriptions { get; set; }
        public List<string> Icons { get; set; }
        public List<string> AvailableVariants { get; set; }
    }
}
