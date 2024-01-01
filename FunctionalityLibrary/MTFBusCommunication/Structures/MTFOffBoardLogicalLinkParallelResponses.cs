using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class MTFOffBoardLogicalLinkParallelResponses
    {
        private string logicalLink;
        private List<MTFOffBoardResponse> offBoardResponses;

        public string LogicalLink
        {
            get { return logicalLink; }
            set { logicalLink = value; }
        }

        public List<MTFOffBoardResponse> OffBoardResponses
        {
            get { return offBoardResponses; }
            set { offBoardResponses = value; }
        }
    }
}
