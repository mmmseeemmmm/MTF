using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.FindPattern
{
    public class FindPatternsCMD
    {
        public string imageRef { get; set; }
        public bool setupMode { get; set; }
        public string[] patternsSettings { get; set; }
        public ushort evaluationMethod { get; set; }
    }
}
