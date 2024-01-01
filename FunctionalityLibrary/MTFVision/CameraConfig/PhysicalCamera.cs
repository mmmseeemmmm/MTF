using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFVision
{
    [MTFKnownClass]
    public class PhysicalCamera
    {
        public string Identifier { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
    }
}
