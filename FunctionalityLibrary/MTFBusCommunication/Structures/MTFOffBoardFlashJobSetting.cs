using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class MTFOffBoardFlashJobSetting
    {
        private string logicalLink = string.Empty;

        public string LogicalLink
        {
            get { return logicalLink; }
            set { logicalLink = value; }
        }
        private string flashJobName = string.Empty;

        public string FlashJobName
        {
            get { return flashJobName; }
            set { flashJobName = value; }
        }
        private string sessionName = string.Empty;

        public string SessionName
        {
            get { return sessionName; }
            set { sessionName = value; }
        }
    }
}
