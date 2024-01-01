using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision
{
    public class AcquireImageLog
    {
        public string imgRefName { get; set; }
        public string absolutePath { get; set; }
        public int imageFormat { get; set; }
        public bool overlay { get; set; }
        
    }
}
