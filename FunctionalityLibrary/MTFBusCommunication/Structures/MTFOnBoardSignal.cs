using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class MTFOnBoardSignal
    {
        private string value = string.Empty;
        private string unit = string.Empty;
        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public string Unit
        {
            get { return unit; }
            set { unit = value; }
        }
    }
}
