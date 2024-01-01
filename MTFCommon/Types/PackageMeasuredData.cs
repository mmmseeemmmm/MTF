using System.Collections.Generic;

namespace AutomotiveLighting.MTFCommon.Types
{
    [MTFKnownClass]
    public class PackageMeasuredData
    {
        public MTFDateTime Time { get; set; }
        public List<MeasuredTypedData> Data { get; set; }
    }
}
