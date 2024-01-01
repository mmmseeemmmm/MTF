using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class MTFOffBoardErrorMemoryRequest
    {
        private List<MTFOffBoardRequestParameter> requestParameters = new List<MTFOffBoardRequestParameter>();

        public string ServiceName { get; set; }

        public List<MTFOffBoardRequestParameter> RequestParameters
        {
            get { return requestParameters; }
            set { requestParameters = value; }
        }

        public string KeyForErrorCode { get; set; }

        public string KeyForErrorText { get; set; }

        public List<string> IgnoredErrorCodes { get; set; }

    }
}
