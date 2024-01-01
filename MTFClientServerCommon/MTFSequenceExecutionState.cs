using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    public enum MTFSequenceExecutionState
    {
        None,
        ExecutionPreparation,
        Executing,
        Stopping,
        Stopped,
        Pause,
        AborSubSequence,
        Aborted, //aborted by error in inicialization or by runtime
        Aborting,
        Finished, //sequence finished without errors
        DebugGoToTopPosition,
        DebugGoToNewPosition,
        CriticalAbort,// when the sequence is corrupted during saving
    }
}
