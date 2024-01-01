using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace DataCollectionDriver
{
    [MTFKnownClass]
    public class EntryOfReport
    {
        public string Category { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
