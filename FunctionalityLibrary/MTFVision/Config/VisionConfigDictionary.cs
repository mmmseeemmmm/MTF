using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.Config
{
    [Serializable]
    public class VisionConfigDictionary<T>
    {
        public VisionConfigDictionary()
        {
            Dict = new Dictionary<string, T>();
        }
        public Dictionary<string, T> Dict { get; set; }
    }
}
