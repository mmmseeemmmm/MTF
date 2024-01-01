using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class MTFDaimlerCodingOffboardResponses : MTFOffBoardLogicalLinkParallelResponses
    {
        private List<byte> rawRequestForZenZefi;
        
        public List<byte> RawRequestForZenZefi
        {
            get { return rawRequestForZenZefi; }
            set { rawRequestForZenZefi = value; }
        }

        public byte[] ArrayRawRequestForZenZefi
        {
            get { return this.rawRequestForZenZefi.ToArray(); }
        }
    }
}
