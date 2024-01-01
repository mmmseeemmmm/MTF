using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeScanner
{
    [MTFKnownClass]
    public class IndexesPair
    {
        public ushort StartIndex { get; set; }
        public ushort Length { get; set; }
    }
}
