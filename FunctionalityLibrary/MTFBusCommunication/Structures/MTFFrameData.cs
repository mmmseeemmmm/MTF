using System.Collections.Generic;
using AutomotiveLighting.MTFCommon;

namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class MTFFrameData
    {
        public uint Id { get; set; }
        public List<string> Data { get; set; }
    }
}
