using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace DataCollectionDriver
{
    [MTFKnownClass]
    public class CategoryOfReport
    {
        public string CategoryName { get; set; }
        public List<EntryOfReport> EntriesOfCategory { get; set; }
    }
}
