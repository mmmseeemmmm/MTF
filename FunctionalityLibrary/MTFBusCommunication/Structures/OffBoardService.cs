using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class MTFOffBoardService
    {
        private List<MTFOffBoardRequestParameter> requestParameters = new List<MTFOffBoardRequestParameter>();
        private List<MTFOffBoardResponseSetting> responses = new List<MTFOffBoardResponseSetting>();


        public string ServiceName { get; set; }

        public List<MTFOffBoardRequestParameter> RequestParameters
        {
            get { return requestParameters; }
            set { requestParameters = value; }
        }

        public List<MTFOffBoardResponseSetting> Responses
        {
            get { return responses; }
            set { responses = value; }
        }
    }
}
