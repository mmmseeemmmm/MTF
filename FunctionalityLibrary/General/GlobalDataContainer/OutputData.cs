using AutomotiveLighting.MTFCommon;

namespace AutomotiveLighting.MTF.GlobalDataContainer
{
    [MTFKnownClassAttribute]
    public class OutputData
    {
        [MTFProperty(Name = "ItemsCount")]
        public int ItemsCount { get; set; }
        [MTFProperty(Name = "Section")]
        public string[] Section { get; set; }
        [MTFProperty(Name = "Key")]
        public string[] Key { get; set; }
        [MTFProperty(Name = "Data")]
        public string[] Data { get; set; }
    }
}
