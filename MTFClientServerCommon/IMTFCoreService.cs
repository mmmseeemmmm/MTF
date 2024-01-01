using MTFClientServerCommon.MTFValidationTable;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using MTFClientServerCommon.GraphicalView;
using MTFClientServerCommon.Import;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [ServiceContract(CallbackContract = typeof(IMTFCoreServiceCallback))]
    public interface IMTFCoreService
    {
        [OperationContract]
        List<MTFClassInfo> AvailableMonsterClasses();

        [OperationContract]
        List<MTFClassInstanceConfiguration> ClassInstanceConfigurations(MTFClassInfo classInfo);
        


        [OperationContract]
        void SaveSequence(MTFSequence sequence, string fileName, bool replaceGuids);

        [OperationContract]
        Stream OpenSequenceStream(string fileName);

        [OperationContract]
        Guid LastSequencePersistId(string fileName);

        [OperationContract]
        string CreateDirectory(string path, string newDirName, bool useDefaultMtfDirectory);

        [OperationContract]
        string RenameItem(string newName, string oldFullName, string root, bool useDefaultMtfDirectory);

        [OperationContract]
        void RemoveFile(string name, string root, bool useDefaultMtfDirectory);

        [OperationContract]
        void RemoveDirectory(string name, string root, bool useDefaultMtfDirectory);

        [OperationContract]
        List<MTFPersistDataInfo> GetSequencesInfo(string basePath, string path);

        [OperationContract]
        List<MTFPersistDataInfo> GetServerFileAndFolders(string path, bool getFiles);

        [OperationContract(IsOneWay = true)]
        void SetIsSetupMode(bool isSetupMode);

        [OperationContract(IsOneWay = true)]
        void AddSetupActivityPath(List<Guid> activityPath);

        [OperationContract(IsOneWay = true)]
        void RemoveSetupActivityPath(List<Guid> activityPath);

        [OperationContract(IsOneWay = true)]
        void SetIsDebugMode(bool isDebugMode);

        [OperationContract(IsOneWay = true)]
        void AddBreakPointActivityPath(List<Guid> activityPath);

        [OperationContract(IsOneWay = true)]
        void RemoveBreakPointActivityPath(List<Guid> activityPath);

        [OperationContract(IsOneWay = true)]
        void StartSequence(string sequenceName, bool isSetupMode, IEnumerable<List<Guid>> setupModeActivityPaths, bool isDebug, IEnumerable<List<Guid>> breakPointActivityPaths, bool isServiceMode, bool isTeachMode);

        [OperationContract]
        void StopSequence();

        [OperationContract]
        void PauseSequence();

        [OperationContract]
        string GetExecutingSequenceName();

        [OperationContract]
        Guid[] GetExecutingActivityPath();

        [OperationContract]
        MTFSequenceExecutionState GetSequenceExecutingState();

        [OperationContract]
        ServerSettings GetServerSettings();

        [OperationContract]
        string GetServerFullDirectoryPath(string relativePath);

        [OperationContract]
        void SaveServerSettings();

        [OperationContract]
        void UpdateServerSettings(ServerSettings settings);


        [OperationContract]
        string RequestStopServer();

        [OperationContract(IsOneWay = true)]
        void SendMessageBoxResult(MTFDialogResult dialogResult, List<Guid> executingActivityPath);

        [OperationContract]
        GenericClassInstanceConfiguration GetObjectFormLazyLoadCache(Guid id);

        [OperationContract]
        void ImportSequenceSetting(MTFImportSetting setting);

        [OperationContract]
        bool ImportSequence(Stream stream);

        
        [OperationContract]
        IList<MTFSequenceClassInfo> LoadSequenceClassInfo(string fileName);

        [OperationContract]
        bool ExistSequence(string filename);

        [OperationContract]
        void ChangeExecutionEventListener(SequenceExecutionEventType[] listenersToAdd, SequenceExecutionEventType[] listenersToRemove);

        [OperationContract(IsOneWay = true)]
        void ResultReuqest(ResultRequestTypes resultType);

        [OperationContract]
        [ServiceKnownType(typeof(MTFValidationTable.MTFValidationTable))]
        Dictionary<Guid, object> GetActualVariableValues(IEnumerable<Guid> variables);

        [OperationContract]
        void DebugStepOver();

        [OperationContract]
        void DebugStepInto();

        [OperationContract]
        void DebugStepOut();

        [OperationContract]
        void SetNewExecutionPointer(Guid[] executionPath);

        [OperationContract]
        void SetNewVariableValue(Guid variableId, object value);

        [OperationContract]
        void SetupVariantSelectionResult(IEnumerable<SetupVariantSelectionResult> variantSelectionResults);

        [OperationContract(IsOneWay = true)]
        void SetValidationTableRow(Guid tableId, MTFValidationTableRow row);

        [OperationContract(IsOneWay = true)]
        void SetIsServiceMode(bool isServiceMode);

        [OperationContract(IsOneWay = true)]
        void SetIsTeachingMode(bool isTeachingMode);

        [OperationContract]
        void ExecuteServiceCommand(Guid commandId);

        [OperationContract]
        void RemoveGoldSampleData(string fileName);

        [OperationContract]
        FileInfo GetGoldSampleDataFileInfo(string fileName);

        [OperationContract(IsOneWay = true)]
        void SaveExecutedSequence(string modifiedByUser);

        [OperationContract]
        void ClientUISendData(byte[] data, ClientUIDataInfo info);

        [OperationContract(IsOneWay = true)]
        void LogedUserChanged(string userName);

        [OperationContract]
        void SetupControlClosed(OpenClientSetupControlArgs args);

        [OperationContract]
        List<GraphicalViewImg> SaveGraphicalViewImages(List<GraphicalViewImg> images);

        [OperationContract]
        List<GraphicalViewImg> LoadGraphicalViewImages(bool loadAll, IEnumerable<string> fileNames);

        [OperationContract]
        List<string> GetGraphicalViewImageNames();

        [OperationContract(IsOneWay = true)]
        void ExecuteUserCommand(Guid commandId);

        [OperationContract]
        List<MTFActivityResult> GetMTFActivityResult();

        [OperationContract]
        DataMigrationInfo CheckPossibleDataMigration();

        [OperationContract]
        Task DoDataMigration(DataMigrationType dataMigrationType);
    }

    public enum SequenceExecutionEventType
    {
        Tree,
        Table
    }

    public enum DataMigrationType
    {
        DoNothing,
        Copy,
        Move,
    }
}
