using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.HelperClasses
{
    [Serializable]
    public class GlobalRoiDictionary
    {
        public Dictionary<string, string> roiDict = new Dictionary<string, string>();
    }
}
