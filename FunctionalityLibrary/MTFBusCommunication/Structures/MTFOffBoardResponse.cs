using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ALBusComDriver;
using AutomotiveLighting.MTFCommon;


namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class MTFOffBoardResponse
    {
        private List<MTFOffBoardServiceResult> results = new List<MTFOffBoardServiceResult>();

        public List<MTFOffBoardServiceResult> Results
        {
            get { return results; }
            set { results = value; }
        }
        public ExecutionState ExecState { get; set; }
        public Byte[] RawRequest { get; set; }
        public Byte[] RawResponse { get; set; }
    }
}
