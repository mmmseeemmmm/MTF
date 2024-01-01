using System;
using System.Collections.Generic;
using System.ServiceModel;
using MTFClientServerCommon.GoldSamplePersist;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFClientServerCommon
{
    [ServiceContract]
    public interface IMTFCoreServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnSequenceExecutionError(StatusMessage errorMessage);

        [OperationContract(IsOneWay = true)]
        void OnSequenceExecutionStateChanged(MTFSequenceExecutionState newState);

        [OperationContract(IsOneWay = true)]
        void OnSequenceActivityChanged(Guid[] executingActivityPath);

        [OperationContract(IsOneWay = true)]
        void OnActivityPercentProgress(ActivityPercentProgressChangedEventArgs e);

        [OperationContract(IsOneWay = true)]
        void OnActivityStringProgress(ActivityStringProgressChangedEventArgs e);

        [OperationContract(IsOneWay = true)]
        void OnActivityImageProgress(ActivityImageProgressChangedEventArgs e);

        [OperationContract(IsOneWay = true)]
        void OnNewActivityResult(MTFActivityResult result);

        [OperationContract(IsOneWay = true)]
        void OnTreeResults(MTFActivityResult[] result);

        [OperationContract(IsOneWay = true)]
        void OnTableViewResults(MTFValidationTableRowResult[] result);

        [OperationContract(IsOneWay = true)]
        void OnNewValidateRows(List<MTFValidationTableRowResult> rows, bool activityWillBeRepeated, Guid? dutId);

        [OperationContract(IsOneWay = true)]
        void RepeatSubSequence(Guid[] executingActivityPath);

        [OperationContract(IsOneWay = true)]
        void OnSequenceStatusMessage(string line1, string line2, string line3, StatusLinesFontSize fontSize, Guid? dutId);

        [OperationContract(IsOneWay = true)]
        void OnShowMessage(MessageInfo messageInfo);

        [OperationContract(IsOneWay = true)]
        void OnCloseMessage(List<Guid> executingActivityPath);

        [OperationContract(IsOneWay = true)]
        void OnSequenceFinished(MTFSequenceResult sequenceResult);

        [OperationContract(IsOneWay = true)]
        void OnClearValidationTables(bool clearAllTables, List<Guid> tablesForClearing, Guid? dutId);

        [OperationContract(IsOneWay = true)]
        void OnShowSetupVariantSelection(string activityName, Dictionary<string, IEnumerable<SequenceVariant>> dataVariants, Dictionary<string, string> extendetUsedDataNames);

        [OperationContract(IsOneWay = true)]
        void OnSequenceVariantChanged(SequenceVariantInfo sequenceVariantInfo, Guid? dutId);

        [OperationContract(IsOneWay = true)]
        void ServiceCommandsStateChanged(IEnumerable<Guid> allowedCommands, IEnumerable<Guid> checkedCommands);

        [OperationContract(IsOneWay = true)]
        void OnLoadGoldSamples(List<SequenceVariantInfo> rows);

        [OperationContract(IsOneWay = true)]
        void OnAllowSaveExecutedSequence(bool state);

        [OperationContract(IsOneWay = false)]
        void ClientUIReceiveData(byte[] data, ClientUIDataInfo info);

        [OperationContract(IsOneWay = true)]
        void OnDynamicLoadSequence(MTFSequence sequence, Dictionary<Guid, MTFExternalSequenceInfo> externalSequences);

        [OperationContract(IsOneWay = true)]
        void OnDynamicUnloadSequence(Guid sequenceId);

        [OperationContract(IsOneWay = true)]
        void OnDynamicExecuteSequence(Guid sequenceId, Guid subSequenceId, bool callSubSequence, Guid[] activityIdPath);

        [OperationContract(IsOneWay = true)]
        void OnSequenceExecutionOpenClientSetupControl(OpenClientSetupControlArgs args);

        [OperationContract(IsOneWay = true)]
        void OnSequenceExecutionUserCommandsStatusChanged(IEnumerable<UserCommandsState> commandsSettings);

        [OperationContract(IsOneWay = true)]
        void OnSequenceExecutionUserIndicatorValueChanged(Guid indicatorId, bool value);

        [OperationContract(IsOneWay = true)]
        void OnSequenceExecutionViewChanged(SequenceExecutionViewType view, Guid? graphicalViewId, Guid? dutId);
    }
}
