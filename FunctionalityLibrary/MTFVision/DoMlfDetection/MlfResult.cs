using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTFVision.DoMlfDetection;

namespace MTFVision.MtfVisionResults
{
    public class MtfMlfResult
    {
        public ushort detectionPointInArea { get; set; }
        public ushort resultType { get; set; }
        public string resultData { get; set; }
    }    
}
