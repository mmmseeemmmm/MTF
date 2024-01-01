using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision
{
    public class SelectSpecificAreas
    {
        public string imgRefName { get; set; }
        public string coordSysName { get; set; }
        public string[] areas { get; set; }
        public string[] selectedAreasIDs { get; set; }
    }
}
