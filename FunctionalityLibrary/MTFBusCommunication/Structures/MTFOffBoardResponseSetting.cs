using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class MTFOffBoardResponseSetting
    {
        private List<string> keys = new List<string>();

        public List<string> Keys
        {
            get { return keys; }
            set { keys = value; }
        }

        private List<string> ignoredValues = new List<string>();

        public List<string> IgnoredValues
        {
            get { return ignoredValues; }
            set { ignoredValues = value; }
        }
    }
}
