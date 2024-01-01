using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFVision.DoMlfDetection
{
    [MTFKnownClass]
    public class MlfValidationTableConfig
    {
        public string TableName { get; set; }
        public string Function { get; set; }
        public ComputeRatio[] ComputeRatios { get; set; }

    }
    [MTFKnownClass]
    public class ComputeRatio
    {
        public string Description { get; set; }
        public ushort NumeratorIndex { get; set; }
        public ushort DenominatorIndex { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public bool UseMaximusAsDenominator { get; set; }
    }
}
