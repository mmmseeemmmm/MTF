using AutomotiveLighting.MTFCommon;

namespace MTFZenZefi.Structures
{
    [MTFKnownClass]
    public class CodingSignature
    {
        public byte[] Signature { get; set; }
        public byte[] SerialNumber { get; set; }
    }
}
