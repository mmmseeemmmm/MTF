using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class MTFOffBoardLogicalLinkParallelServices
    {
        private string logicalLink;
        private List<MTFOffBoardService> offBoardServices;

        public string LogicalLink
        {
            get { return logicalLink; }
            set { logicalLink = value; }
        }

        public List<MTFOffBoardService> OffBoardServices
        {
            get { return offBoardServices; }
            set { offBoardServices = value; }
        }
    }
}
