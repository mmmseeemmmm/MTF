using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class MTFOffBoardConfig
    {
        private string projectName = string.Empty;
        private bool enableByteTrace = false;
        private string byteTraceInterfaceName = string.Empty;
        private List<string> logicalLinks = new List<string>();


        public string ProjectName
        {
            get { return projectName; }
            set { projectName = value; }
        }
        private string vit = string.Empty;

        public string VIT
        {
            get { return vit; }
            set { vit = value; }
        }


        public bool EnableByteTrace
        {
            get { return enableByteTrace; }
            set { enableByteTrace = value; }
        }


        public string ByteTraceInterfaceName
        {
            get { return byteTraceInterfaceName; }
            set { byteTraceInterfaceName = value; }
        }


        public List<string> LogicalLinks
        {
            get { return logicalLinks; }
            set { logicalLinks = value; }
        }
    }
}
