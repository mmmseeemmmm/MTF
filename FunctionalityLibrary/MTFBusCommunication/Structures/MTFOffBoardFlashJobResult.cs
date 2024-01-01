using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class MTFOffBoardFlashJobResult: MTFOffBoardFlashJobSetting
    {
        private string executionState = string.Empty;

        public string ExecutionState
        {
            get { return executionState; }
            set { executionState = value; }
        }
        private string jobResult = string.Empty;

        public string JobResult
        {
            get { return jobResult; }
            set { jobResult = value; }
        }
    }
}
