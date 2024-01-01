namespace MTFCommon.ClientControls
{
    public enum SequenceExecutionState
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
    }
}
