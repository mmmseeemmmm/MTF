using System;
using System.Collections.Generic;
using System.ServiceModel;
using MTFClientServerCommon.GoldSamplePersist;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFClientServerCommon
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    class MTFClientCallbackHandler : IMTFCoreServiceCallback
    {
        public event SequenceExecutionErrorEventHandler SequenceExecutionError;
        public delegate void SequenceExecutionErrorEventHandler(StatusMessage errorMessage);

        public event SequenceExecutionStateChangedEventHandler SequenceExecutionStateChanged;
        public delegate void SequenceExecutionStateChangedEventHandler(MTFSequenceExecutionState newState);

        public event SequenceActivityChangedEventHandler SequenceActivityChanged;
        public delegate void SequenceActivityChangedEventHandler(Guid[] executingActivityPath);

        public event SequenceExecutionActivityPercentProgressEventHandler SequenceExecutionActivityPercentProgress;
        public delegate void SequenceExecutionActivityPercentProgressEventHandler(ActivityPercentProgressChangedEventArgs e);

        public event SequenceExecutionActivityStringProgressEventHandler SequenceExecutionActivityStringProgress;
        public delegate void SequenceExecutionActivityStringProgressEventHandler(ActivityStringProgressChangedEventArgs e);

        public event SequenceExecutionActivityImageProgressEventHandler SequenceExecutionActivityImageProgress;
        public delegate void SequenceExecutionActivityImageProgressEventHandler(ActivityImageProgressChangedEventArgs e);

        public event SequenceExecutionNewActivityResultEventHandler SequenceExecutionNewActivityResult;
        public delegate void SequenceExecutionNewActivityResultEventHandler(MTFActivityResult result);

        public event SequenceExecutionNewTreeResultsEventHandler SequenceExecutionNewTreeResults;
        public delegate void SequenceExecutionNewTreeResultsEventHandler(MTFActivityResult[] results);

        public event SequenceExecutionNewTableViewResultsEventHandler SequenceExecutionNewTableViewResults;
        public delegate void SequenceExecutionNewTableViewResultsEventHandler(MTFValidationTableRowResult[] results);

        public event SequenceExecutionNewValidateRowsEventHandler SequenceExecutionNewValidateRows;
        public delegate void SequenceExecutionNewValidateRowsEventHandler(List<MTFValidationTableRowResult> rows, bool activityWillBeRepeated, Guid? dutId);

        public event SequenceExecutionRepeatSubSequenceEventHandler SequenceExecutionRepeatSubSequence;
        public delegate void SequenceExecutionRepeatSubSequenceEventHandler(Guid[] executingActivityPath);

        public event OnSequenceStatusMessageEventHandler SequenceStatusMessage;
        public delegate void OnSequenceStatusMessageEventHandler(string line1, string line2, string line3, StatusLinesFontSize fontSize, Guid? dutId);

        public event SequenceShowMessageEventHandler SequenceShowMessage;
        public delegate void SequenceShowMessageEventHandler(MessageInfo messageInfo);

        public event SequenceExecutionFinishedEventHandler SequenceExecutionFinished;
        public delegate void SequenceExecutionFinishedEventHandler(MTFSequenceResult sequenceResult);

        public event SequenceCloseMessageEventHandler SequenceCloseMessage;
        public delegate void SequenceCloseMessageEventHandler(List<Guid> executingActivityPath);

        public event OnClearValidationTablesEventHandler ClearValidationTables;
        public delegate void OnClearValidationTablesEventHandler(bool clearAllTables, List<Guid> tablesForClearing, Guid? dutId);

        public event OnShowSetupVariantSelectionEventHandler ShowSetupVariantSelection;
        public delegate void OnShowSetupVariantSelectionEventHandler(string activityName, Dictionary<string, IEnumerable<SequenceVariant>> dataVariants, Dictionary<string, string> extendetUsedDataNames);

        public event SequenceVariantChangedEventHandler SequenceVariantChanged;
        public delegate void SequenceVariantChangedEventHandler(SequenceVariantInfo sequenceVariantInfo, Guid? dutId);

        public event ServiceCommandsStateChangedEventHandler OnServiceCommandsStateChanged;
        public delegate void ServiceCommandsStateChangedEventHandler(IEnumerable<Guid> allowedCommands, IEnumerable<Guid> checkedCommands);

        public event SequenceExecutionLoadGoldSamplesEventHandler SequenceExecutionLoadGoldSamples;
        public delegate void SequenceExecutionLoadGoldSamplesEventHandler(List<SequenceVariantInfo> goldSampleList);

        public event SequenceExecutionAllowSaveExecutedSequenceEventHandler SequenceExecutionAllowSaveExecutedSequence;
        public delegate void SequenceExecutionAllowSaveExecutedSequenceEventHandler(bool state);

        public event SequenceExecutionOnUIControlReceiveDataEventHandler SequenceExecutionOnUIControlReceiveData;
        public delegate void SequenceExecutionOnUIControlReceiveDataEventHandler(byte[] data, ClientUIDataInfo info);

        public event SequenceExecutionDynamicLoadSequenceEventHandler SequenceExecutionDynamicLoadSequence;
        public delegate void SequenceExecutionDynamicLoadSequenceEventHandler(MTFSequence sequence, Dictionary<Guid, MTFExternalSequenceInfo> externalSequences);

        public event SequenceExecutionDynamicUnloadSequenceEventHandler SequenceExecutionDynamicUnloadSequence;
        public delegate void SequenceExecutionDynamicUnloadSequenceEventHandler(Guid sequenceId);

        public event SequenceExecutionDynamicExecuteSequenceEventHandler SequenceExecutionDynamicExecuteSequence;
        public delegate void SequenceExecutionDynamicExecuteSequenceEventHandler(Guid sequenceId, Guid subSequenceId, bool callSubSequence, Guid[] activityIdPath);

        public event SequenceExecutionOpenClientSetupControlEventHandler SequenceExecutionOpenClientSetupControl;
        public delegate void SequenceExecutionOpenClientSetupControlEventHandler(OpenClientSetupControlArgs args);

        public event UserCommandsStatusChangedEventHandler UserCommandsStatusChanged;
        public delegate void UserCommandsStatusChangedEventHandler(IEnumerable<UserCommandsState> commandsSettings);

        public event OnUserIndicatorValueChangedEventHandler OnUserIndicatorValueChanged;
        public delegate void OnUserIndicatorValueChangedEventHandler(Guid indicatorId, bool value);

        public event OnViewChangedEventHandler OnViewChanged;
        public delegate void OnViewChangedEventHandler(SequenceExecutionViewType view, Guid? graphicalViewId, Guid? dutId);

        public void OnSequenceExecutionError(StatusMessage errorMessage)
        {
            //handling server events on client side
            SequenceExecutionError?.Invoke(errorMessage);
        }

        public void OnSequenceExecutionStateChanged(MTFSequenceExecutionState newState)
        {
            SequenceExecutionStateChanged?.Invoke(newState);
        }

        public void OnSequenceActivityChanged(Guid[] executingActivityPath)
        {
            SequenceActivityChanged?.Invoke(executingActivityPath);
        }

        public void OnActivityPercentProgress(ActivityPercentProgressChangedEventArgs e)
        {
            SequenceExecutionActivityPercentProgress?.Invoke(e);
        }

        public void OnActivityStringProgress(ActivityStringProgressChangedEventArgs e)
        {
            SequenceExecutionActivityStringProgress?.Invoke(e);
        }

        public void OnActivityImageProgress(ActivityImageProgressChangedEventArgs e)
        {
            SequenceExecutionActivityImageProgress?.Invoke(e);
        }

        public void OnNewActivityResult(MTFActivityResult result)
        {
            SequenceExecutionNewActivityResult?.Invoke(result);
        }

        public void OnTreeResults(MTFActivityResult[] results)
        {
            SequenceExecutionNewTreeResults?.Invoke(results);
        }

        public void OnTableViewResults(MTFValidationTableRowResult[] results)
        {
            SequenceExecutionNewTableViewResults?.Invoke(results);
        }

        public void OnNewValidateRows(List<MTFValidationTable.MTFValidationTableRowResult> rows, bool activityWillBeRepeated, Guid? dutId)
        {
            SequenceExecutionNewValidateRows?.Invoke(rows, activityWillBeRepeated, dutId);
        }

        public void RepeatSubSequence(Guid[] executingActivityPath)
        {
            SequenceExecutionRepeatSubSequence?.Invoke(executingActivityPath);
        }

        public void OnSequenceStatusMessage(string line1, string line2, string line3, StatusLinesFontSize fontSize, Guid? dutId)
        {
            SequenceStatusMessage?.Invoke(line1, line2, line3, fontSize, dutId);
        }

        public void OnClearValidationTables(bool clearAllTables, List<Guid> tablesForClearing, Guid? dutId)
        {
            ClearValidationTables?.Invoke(clearAllTables, tablesForClearing, dutId);
        }

        public void OnShowMessage(MessageInfo messageInfo)
        {
            SequenceShowMessage?.Invoke(messageInfo);
        }

        public void OnCloseMessage(List<Guid> executingActivityPath)
        {
            SequenceCloseMessage?.Invoke(executingActivityPath);
        }

        public void OnSequenceFinished(MTFSequenceResult sequenceResult)
        {
            SequenceExecutionFinished?.Invoke(sequenceResult);
        }

        public void OnShowSetupVariantSelection(string activityName, Dictionary<string, IEnumerable<SequenceVariant>> dataVariants, Dictionary<string, string> extendetUsedDataNames)
        {
            ShowSetupVariantSelection?.Invoke(activityName, dataVariants, extendetUsedDataNames);
        }

        public void OnSequenceVariantChanged(SequenceVariantInfo sequenceVariantInfo, Guid? dutId)
        {
            SequenceVariantChanged?.Invoke(sequenceVariantInfo, dutId);
        }

        public void ServiceCommandsStateChanged(IEnumerable<Guid> allowedCommands, IEnumerable<Guid> checkedCommands)
        {
            OnServiceCommandsStateChanged?.Invoke(allowedCommands, checkedCommands);
        }

        public void OnLoadGoldSamples(List<SequenceVariantInfo> goldSampleList)
        {
            SequenceExecutionLoadGoldSamples?.Invoke(goldSampleList);
        }

        public void OnAllowSaveExecutedSequence(bool state)
        {
            SequenceExecutionAllowSaveExecutedSequence?.Invoke(state);
        }

        public void ClientUIReceiveData(byte[] data, ClientUIDataInfo info)
        {
            SequenceExecutionOnUIControlReceiveData?.Invoke(data, info);
        }

        public void OnDynamicLoadSequence(MTFSequence sequence, Dictionary<Guid, MTFExternalSequenceInfo> externalSequences)
        {
            SequenceExecutionDynamicLoadSequence?.Invoke(sequence, externalSequences);
        }

        public void OnDynamicUnloadSequence(Guid sequenceId)
        {
            SequenceExecutionDynamicUnloadSequence?.Invoke(sequenceId);
        }

        public void OnDynamicExecuteSequence(Guid sequenceId, Guid subSequenceId, bool callSubSequence, Guid[] activityIdPath)
        {
            SequenceExecutionDynamicExecuteSequence?.Invoke(sequenceId, subSequenceId, callSubSequence, activityIdPath);
        }

        public void OnSequenceExecutionOpenClientSetupControl(OpenClientSetupControlArgs args)
        {
            SequenceExecutionOpenClientSetupControl?.Invoke(args);
        }

        public void OnSequenceExecutionUserCommandsStatusChanged(IEnumerable<UserCommandsState> commandsSettings)
        {
            UserCommandsStatusChanged?.Invoke(commandsSettings);
        }

        public void OnSequenceExecutionUserIndicatorValueChanged(Guid indicatorId, bool value)
        {
            OnUserIndicatorValueChanged?.Invoke(indicatorId, value);
        }

        public void OnSequenceExecutionViewChanged(SequenceExecutionViewType view, Guid? graphicalViewId, Guid? dutId)
        {
            OnViewChanged?.Invoke(view, graphicalViewId, dutId);
        }
    }
}
