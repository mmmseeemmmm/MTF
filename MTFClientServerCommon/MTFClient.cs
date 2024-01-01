using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Linq;
using System.Runtime;
using MTFClientServerCommon.MTFValidationTable;
using MTFClientServerCommon.GoldSamplePersist;
using MTFClientServerCommon.GraphicalView;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.Import;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    public class MTFClient
    {
        private static MTFClient mtfClient;
        private List<MTFClassInfo> availableMonsterClasses;

        public static MTFClient GetMTFClient()
        {
            return mtfClient;
        }

        public static MTFClient GetMTFClient(string ip, string port)
        {
            try
            {
                mtfClient = new MTFClient(ip, port);
                mtfClient.Connect();
                return mtfClient;
            }
            catch (Exception e)
            {
                mtfClient = null;
                throw e;
            }
        }

        public event SequenceExecutionErrorEventHandler SequenceExecutionError;
        public delegate void SequenceExecutionErrorEventHandler(StatusMessage errorMessage);

        public event SequenceExecutionStateChangedEventHandler SequenceExecutionStateChanged;
        public delegate void SequenceExecutionStateChangedEventHandler(MTFSequenceExecutionState newState);

        public event SequenceExecutionActivityChangedEventHandler SequenceExecutionActivityChanged;
        public delegate void SequenceExecutionActivityChangedEventHandler(Guid[] executingActivityPath);

        public event SequenceExecutionActivityPercentProgressEventHandler SequenceExecutionActivityPercentProgress;
        public delegate void SequenceExecutionActivityPercentProgressEventHandler(ActivityPercentProgressChangedEventArgs e);

        public event SequenceExecutionActivityStringProgressEventHandler SequenceExecutionActivityStringProgress;
        public delegate void SequenceExecutionActivityStringProgressEventHandler(ActivityStringProgressChangedEventArgs e);

        public event SequenceExecutionActivityImageProgressEventHandler SequenceExecutionActivityImageProgress;
        public delegate void SequenceExecutionActivityImageProgressEventHandler(ActivityImageProgressChangedEventArgs e);

        public event SequenceExecutionNewActivityResultEventHandler SequenceExecutionNewActivityResult;
        public delegate void SequenceExecutionNewActivityResultEventHandler(MTFActivityResult result);

        public event SequenceExecutionTreeResultsEventHandler SequenceExecutionTreeResults;
        public delegate void SequenceExecutionTreeResultsEventHandler(MTFActivityResult[] results);

        public event SequenceExecutionTableViewResultsEventHandler SequenceExecutionTableViewResults;
        public delegate void SequenceExecutionTableViewResultsEventHandler(MTFValidationTableRowResult[] results);

        public event SequenceExecutionNewValidateRowsEventHandler SequenceExecutionNewValidateRows;
        public delegate void SequenceExecutionNewValidateRowsEventHandler(List<MTFValidationTableRowResult> rows, bool activityWillBeRepeated, Guid? dutId);

        public event SequenceExecutionRepeatSubSequenceEventHandler SequenceExecutionRepeatSubSequence;
        public delegate void SequenceExecutionRepeatSubSequenceEventHandler(Guid[] executingActivityPath);

        public event SequenceExecutionOnSequenceStatusMessageEventHandler SequenceExecutionOnSequenceStatusMessage;
        public delegate void SequenceExecutionOnSequenceStatusMessageEventHandler(string line1, string line2, string line3, StatusLinesFontSize fontSize, Guid? dutId);

        public event SequenceExecutionShowMessageEventHandler SequenceExecutionShowMessage;
        public delegate void SequenceExecutionShowMessageEventHandler(MessageInfo messageInfo);

        public event SequenceExecutionFinishedEventHandler SequenceExecutionFinished;
        public delegate void SequenceExecutionFinishedEventHandler(MTFSequenceResult sequenceResult);

        public event SequenceExecutionCloseMessageEventHandler SequenceExecutionCloseMessage;
        public delegate void SequenceExecutionCloseMessageEventHandler(List<Guid> executingActivityPath);

        public event SequenceExecutionOnClearValidationTablesEventHandler SequenceExecutionOnClearValidationTables;
        public delegate void SequenceExecutionOnClearValidationTablesEventHandler(bool clearAllTables, List<Guid> tablesForClearing, Guid? dutId);

        public event SequenceExecutionShowSetupVariantSelectionEventHandler SequenceExecutionShowSetupVariantSelection;
        public delegate void SequenceExecutionShowSetupVariantSelectionEventHandler(string activityName, Dictionary<string, IEnumerable<SequenceVariant>> dataVariants, Dictionary<string, string> extendetUsedDataNames);

        public event SequenceExecutionSequenceVariantChangedEventHandler SequenceExecutionSequenceVariantChanged;
        public delegate void SequenceExecutionSequenceVariantChangedEventHandler(SequenceVariantInfo sequenceVariantInfo, Guid? dutId);

        public event ServiceCommandsStateChangedEventHandler ServiceCommandsStateChanged;
        public delegate void ServiceCommandsStateChangedEventHandler(IEnumerable<Guid> allowedCommands, IEnumerable<Guid> checkedCommands);

        public event SequenceExecutionLoadGoldSamplesEventHandler SequenceExecutionLoadGoldSamples;
        public delegate void SequenceExecutionLoadGoldSamplesEventHandler(List<SequenceVariantInfo> goldSampleList);

        public event SequenceExecutionAllowSaveExecutedSequenceEventHandler SequenceExecutionAllowSaveExecutedSequence;
        public delegate void SequenceExecutionAllowSaveExecutedSequenceEventHandler(bool state);

        public event SequenceExecutionOnUIControlReceiveDataEventHandler SequenceExecutionOnUIControlReceiveData;
        public delegate void SequenceExecutionOnUIControlReceiveDataEventHandler(byte[] data, ClientUIDataInfo info);

        public event SequenceExecutionOnDynamicLoadSequenceEventHandler SequenceExecutionDynamicLoadSequence;
        public delegate void SequenceExecutionOnDynamicLoadSequenceEventHandler(MTFSequence sequence, Dictionary<Guid, MTFExternalSequenceInfo> externalSequence);

        public event SequenceExecutionOnDynamicUnloadSequenceEventHandler SequenceExecutionDynamicUnloadSequence;
        public delegate void SequenceExecutionOnDynamicUnloadSequenceEventHandler(Guid sequenceId);

        public event SequenceExecutionOnDynamicExecuteSequenceEventHandler SequenceExecutionDynamicExecuteSequence;
        public delegate void SequenceExecutionOnDynamicExecuteSequenceEventHandler(Guid sequenceId, Guid subSequenceId, bool callSubSequence, Guid[] activityIdPath);

        public event SequenceExecutionOpenClientSetupControlEventHandler SequenceExecutionOpenClientSetupControl;
        public delegate void SequenceExecutionOpenClientSetupControlEventHandler(OpenClientSetupControlArgs args);

        public event UserCommandsStatusChangedEventHandler OnUserCommandsStatusChanged;
        public delegate void UserCommandsStatusChangedEventHandler(IEnumerable<UserCommandsState> commandsSettings);

        public event OnUserIndicatorValueChangedEventHandler OnUserIndicatorValueChanged;
        public delegate void OnUserIndicatorValueChangedEventHandler(Guid indicatorId, bool value);

        public event OnViewChangedEventHandler OnViewChanged;
        public delegate void OnViewChangedEventHandler(SequenceExecutionViewType view, Guid? graphicalViewId, Guid? dutId);

        private string ip;
        private string port;
        private IMTFCoreService MTFCoreService;
        private MTFClientCallbackHandler callbackHandler;
        private MTFClient(string ip, string port)
        {
            this.ip = ip;
            this.port = port;
            MTFDataTransferObject.LazyLoadCallBack = GetObjectFromLazyLoadCache;
            callbackHandler = new MTFClientCallbackHandler();
            callbackHandler.SequenceExecutionError += callbackHandler_SequenceExecutionError;
            callbackHandler.SequenceExecutionStateChanged += callbackHandler_SequenceExecutionStateChanged;
            callbackHandler.SequenceActivityChanged += callbackHandler_SequenceActivityChanged;
            callbackHandler.SequenceExecutionActivityPercentProgress += callbackHandler_SequenceExecutionActivityPercentProgress;
            callbackHandler.SequenceExecutionActivityStringProgress += callbackHandler_SequenceExecutionActivityStringProgress;
            callbackHandler.SequenceExecutionActivityImageProgress += callbackHandler_SequenceExecutionActivityImageProgress;
            callbackHandler.SequenceExecutionNewActivityResult += callbackHandler_SequenceExecutionNewActivityResult;
            callbackHandler.SequenceExecutionNewTreeResults += callbackHandler_SequenceExecutionTreeResults;
            callbackHandler.SequenceExecutionNewTableViewResults += callbackHandler_SequenceExecutionTableViewResults;
            callbackHandler.SequenceExecutionNewValidateRows += callbackHandler_SequenceExecutionNewValidateRows;
            callbackHandler.SequenceExecutionRepeatSubSequence += callbackHandler_SequenceExecutionRepeatSubSequence;
            callbackHandler.SequenceStatusMessage += callbackHandler_SequenceStatusMessage;
            callbackHandler.SequenceShowMessage += callbackHandler_SequenceShowMessage;
            callbackHandler.SequenceExecutionFinished += callbackHandler_SequenceExecutionFinished;
            callbackHandler.SequenceCloseMessage += callbackHandler_SequenceCloseMessage;
            callbackHandler.ClearValidationTables += callbackHandler_ClearValidationTables;
            callbackHandler.ShowSetupVariantSelection += callbackHandler_ShowSetupVariantSelection;
            callbackHandler.SequenceVariantChanged += callbackHandler_SequenceVariantChanged;
            callbackHandler.OnServiceCommandsStateChanged += callbackHandler_ServiceCommandsStateChanged;
            callbackHandler.SequenceExecutionLoadGoldSamples += callbackHandler_SequenceExecutionLoadGoldSamples;
            callbackHandler.SequenceExecutionAllowSaveExecutedSequence += callbackHandler_SequenceExecutionAllowSaveExecutedSequence;
            callbackHandler.SequenceExecutionOnUIControlReceiveData += CallbackHandlerOnSequenceExecutionOnUiControlReceiveData;
            callbackHandler.SequenceExecutionDynamicLoadSequence += callbackHandler_SequenceExecutionDynamicLoadSequence;
            callbackHandler.SequenceExecutionDynamicUnloadSequence += callbackHandler_SequenceExecutionDynamicUnloadSequence;
            callbackHandler.SequenceExecutionDynamicExecuteSequence += callbackHandler_SequenceExecutionDynamicExecuteSequence;
            callbackHandler.SequenceExecutionOpenClientSetupControl += callbackHandlerOnSequenceExecutionOpenClientSetupControl;
            callbackHandler.UserCommandsStatusChanged += callbackHandlerOnUserCommandsStatusChanged; 
            callbackHandler.OnUserIndicatorValueChanged += CallbackHandlerOnUserIndicatorValueChanged;
            callbackHandler.OnViewChanged += CallbackHanlerViewChanged;
        }

        private void CallbackHanlerViewChanged(SequenceExecutionViewType view, Guid? graphicalViewId, Guid? dutId) => OnViewChanged?.Invoke(view, graphicalViewId, dutId);

        private void CallbackHandlerOnUserIndicatorValueChanged(Guid indicatorId, bool value) => OnUserIndicatorValueChanged?.Invoke(indicatorId, value);

        private void callbackHandlerOnUserCommandsStatusChanged(IEnumerable<UserCommandsState> commandsSettings) => OnUserCommandsStatusChanged?.Invoke(commandsSettings);

        private void callbackHandlerOnSequenceExecutionOpenClientSetupControl(OpenClientSetupControlArgs args) => SequenceExecutionOpenClientSetupControl?.Invoke(args);

        void callbackHandler_SequenceExecutionDynamicExecuteSequence(Guid sequenceId, Guid subSequenceId, bool callSubSequence, Guid[] activityIdPath) => SequenceExecutionDynamicExecuteSequence?.Invoke(sequenceId, subSequenceId, callSubSequence, activityIdPath);

        void callbackHandler_SequenceExecutionDynamicUnloadSequence(Guid sequenceId) => SequenceExecutionDynamicUnloadSequence?.Invoke(sequenceId);

        void callbackHandler_SequenceExecutionDynamicLoadSequence(MTFSequence sequence, Dictionary<Guid, MTFExternalSequenceInfo> externalSequences) => SequenceExecutionDynamicLoadSequence?.Invoke(sequence, externalSequences);

        private void CallbackHandlerOnSequenceExecutionOnUiControlReceiveData(byte[] data, ClientUIDataInfo info) => SequenceExecutionOnUIControlReceiveData?.Invoke(data, info);

        void callbackHandler_SequenceExecutionAllowSaveExecutedSequence(bool state) => SequenceExecutionAllowSaveExecutedSequence?.Invoke(state);

        void callbackHandler_SequenceExecutionLoadGoldSamples(List<SequenceVariantInfo> goldSampleList) => SequenceExecutionLoadGoldSamples?.Invoke(goldSampleList);

        private void callbackHandler_ShowSetupVariantSelection(string activityName, Dictionary<string, IEnumerable<SequenceVariant>> dataVariants, Dictionary<string, string> extendetUsedDataNames) => 
            SequenceExecutionShowSetupVariantSelection?.Invoke(activityName, dataVariants, extendetUsedDataNames);

        private void callbackHandler_ClearValidationTables(bool clearAllTables, List<Guid> tablesForClearing, Guid? dutId) => SequenceExecutionOnClearValidationTables?.Invoke(clearAllTables, tablesForClearing, dutId);

        private void callbackHandler_SequenceCloseMessage(List<Guid> executingActivityPath) => SequenceExecutionCloseMessage?.Invoke(executingActivityPath);

        private void callbackHandler_SequenceShowMessage(MessageInfo messageInfo) => SequenceExecutionShowMessage?.Invoke(messageInfo);

        private void callbackHandler_SequenceStatusMessage(string line1, string line2, string line3, StatusLinesFontSize fontSize, Guid? dutId) => SequenceExecutionOnSequenceStatusMessage?.Invoke(line1, line2, line3, fontSize, dutId);

        private void callbackHandler_SequenceExecutionRepeatSubSequence(Guid[] executingActivityPath) => SequenceExecutionRepeatSubSequence?.Invoke(executingActivityPath);

        private void callbackHandler_SequenceExecutionActivityImageProgress(ActivityImageProgressChangedEventArgs e) => SequenceExecutionActivityImageProgress?.Invoke(e);

        private void callbackHandler_SequenceExecutionStateChanged(MTFSequenceExecutionState newState) => SequenceExecutionStateChanged?.Invoke(newState);

        private void callbackHandler_SequenceExecutionActivityStringProgress(ActivityStringProgressChangedEventArgs e) => SequenceExecutionActivityStringProgress?.Invoke(e);

        private void callbackHandler_SequenceExecutionActivityPercentProgress(ActivityPercentProgressChangedEventArgs e) => SequenceExecutionActivityPercentProgress?.Invoke(e);

        private void callbackHandler_SequenceActivityChanged(Guid[] executingActivityPath) => SequenceExecutionActivityChanged?.Invoke(executingActivityPath);

        private void callbackHandler_SequenceVariantChanged(SequenceVariantInfo sequenceVariantInfo, Guid? dutId) => SequenceExecutionSequenceVariantChanged?.Invoke(sequenceVariantInfo, dutId);

        private void callbackHandler_ServiceCommandsStateChanged(IEnumerable<Guid> allowedCommands, IEnumerable<Guid> checkedCommands) => ServiceCommandsStateChanged?.Invoke(allowedCommands, checkedCommands);

        private void callbackHandler_SequenceExecutionError(StatusMessage errorMessage) => SequenceExecutionError?.Invoke(errorMessage);

        private void callbackHandler_SequenceExecutionNewActivityResult(MTFActivityResult result) => SequenceExecutionNewActivityResult?.Invoke(result);

        private void callbackHandler_SequenceExecutionTreeResults(MTFActivityResult[] results) => SequenceExecutionTreeResults?.Invoke(results);

        private void callbackHandler_SequenceExecutionTableViewResults(MTFValidationTableRowResult[] results) => SequenceExecutionTableViewResults?.Invoke(results);

        private void callbackHandler_SequenceExecutionNewValidateRows(List<MTFValidationTableRowResult> rows, bool activityWillBeRepeated, Guid? dutId) => SequenceExecutionNewValidateRows?.Invoke(rows, activityWillBeRepeated, dutId);

        private void callbackHandler_SequenceExecutionFinished(MTFSequenceResult sequenceResult) => SequenceExecutionFinished?.Invoke(sequenceResult);



        public void Connect()
        {
            var myBinding = new NetTcpBinding
                            {
                                ReceiveTimeout = new TimeSpan(0, 20, 0),
                                SendTimeout = new TimeSpan(0, 20, 0),
                                MaxReceivedMessageSize = int.MaxValue,
                                MaxBufferSize = int.MaxValue,
                                Security = { Mode = SecurityMode.None }
                            };

            var myEndpoint = new EndpointAddress("net.tcp://" + ip + ":" + port + "/MTF/");
            var myChannelFactory = new DuplexChannelFactory<IMTFCoreService>(callbackHandler, myBinding, myEndpoint);

            try
            {
                MTFCoreService = myChannelFactory.CreateChannel();
            }
            catch (Exception e)
            {
                throw new Exception(
                    string.Format("{0}{1}{1}{2}", LanguageHelper.GetString("MTF_Connection_CantConnect"), Environment.NewLine, e.Message),
                    e);
            }
            IsConnected = true;
        }

        public MTFDataTransferObject GetObjectFromLazyLoadCache(Guid id) => CallWithReconnect(() => MTFCoreService.GetObjectFormLazyLoadCache(id));

        public bool IsConnected { get;private set; }

        private List<MTFClassInfo> AvailableMonsterClasses
        {
            get
            {
                availableMonsterClasses = CallWithReconnect(() => MTFCoreService.AvailableMonsterClasses());
                return availableMonsterClasses;
            }
        }

        private List<MTFClassInstanceConfiguration> ClassInstanceConfigurations(MTFClassInfo classInfo)
        {
            List<MTFClassInstanceConfiguration> dataList = CallWithReconnect(() => MTFCoreService.ClassInstanceConfigurations(classInfo));
            dataList.ForEach(item => item.IsModified = false);

            return dataList;
        }

        //private Dictionary<string, MTFSequence> sequenceCache = new Dictionary<string, MTFSequence>();
        public MTFSequence LoadSequence(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                //get sequence stream
                var sequenceStream = CallWithReconnect(() => MTFCoreService.OpenSequenceStream(fileName));

                BinaryFormatter formatter = new BinaryFormatter();
                MTFSequence data;
                try
                {
                    data = formatter.Deserialize(sequenceStream) as MTFSequence;
                }
                catch (Exception)
                {
                    return null;
                }
                finally
                {
                    sequenceStream.Close();
                    sequenceStream.Dispose();
                }

                data.ReplaceIdentityObjects();
                UpdateSequenceClassInfos(data);
                data.IsNew = false;
                data.IsModified = false;
                return data;
            }
            return null;
        }

        public void SaveSequence(MTFSequence sequence, string fileName, bool replaceGuids, string modifiedByUser)
        {
            sequence.LastPersistId = Guid.NewGuid();
            sequence.IncreaseRevision();
            sequence.AddChange(modifiedByUser);

            //hack - delete all null activities
            deleteNullActivities(sequence);
            //hack - big sequence - opened for editing and executing -> memory problem
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            CallWithReconnect(() => MTFCoreService.SaveSequence(sequence, fileName, replaceGuids));
        }

        private static void deleteNullActivities(MTFSequence sequence)
        {
            ForEachActivity(sequence.MTFSequenceActivities);
            ForEachActivity(sequence.ActivitiesByCall);
        }

        private static void ForEachActivity(IList<MTFSequenceActivity> activities)
        {
            for (int i = 0; i < activities.Count; i++)
            {
                if (activities[i] == null)
                {
                    activities.RemoveAt(i);
                }
            }
            foreach (var activity in activities)
            {
                var subSequenceActivity = activity as MTFSubSequenceActivity;
                if (subSequenceActivity != null)
                {
                    if (subSequenceActivity.ExecutionType == ExecutionType.ExecuteByCase)
                    {
                        if (subSequenceActivity.Cases != null && subSequenceActivity.Cases.Count > 0)
                        {
                            foreach (var mtfCase in subSequenceActivity.Cases)
                            {
                                ForEachActivity(mtfCase.Activities);
                            }
                        }
                    }
                    else
                    {
                        ForEachActivity(subSequenceActivity.Activities);
                    }
                }
            }
        }


        public void SetIsSetupMode(bool isSetupMode) => CallWithReconnect(() => MTFCoreService.SetIsSetupMode(isSetupMode));

        public void AddSetupActivityPath(List<Guid> activityPath) => CallWithReconnect(() => MTFCoreService.AddSetupActivityPath(activityPath));

        public void RemoveSetupActivityPath(List<Guid> activityPath) => CallWithReconnect(() => MTFCoreService.RemoveSetupActivityPath(activityPath));

        public void SetIsDebugMode(bool isDebugMode) => CallWithReconnect(() => MTFCoreService.SetIsDebugMode(isDebugMode));

        public void AddBreakPointActivityPath(List<Guid> activityPath) => CallWithReconnect(() => MTFCoreService.AddBreakPointActivityPath(activityPath));

        public void RemoveBreakPointActivityPath(List<Guid> activityPath) => CallWithReconnect(() => MTFCoreService.RemoveBreakPointActivityPath(activityPath));

        public void StartSequence(string sequenceName, bool isSetupMode, IEnumerable<List<Guid>> setupModeActivityPaths, bool isDebug, IEnumerable<List<Guid>> breakPointActivityPaths, bool isServiceMode, bool isTeachMode) => 
            CallWithReconnect(() => MTFCoreService.StartSequence(sequenceName, isSetupMode, setupModeActivityPaths, isDebug, breakPointActivityPaths, isServiceMode, isTeachMode));

        public void StopSequence() => CallWithReconnect(MTFCoreService.StopSequence);

        public void PauseSequence() => CallWithReconnect(MTFCoreService.PauseSequence);

        public string GetExecutingSequenceName() => CallWithReconnect(() => MTFCoreService.GetExecutingSequenceName());

        public Guid[] GetExecutingActivityPath() => CallWithReconnect(() => MTFCoreService.GetExecutingActivityPath());

        public MTFSequenceExecutionState GetSequenceExecutingState() => CallWithReconnect(() => MTFCoreService.GetSequenceExecutingState());

        public string CreateDirectory(string path, string newDirName, bool useDefaultMtfDirectory) => CallWithReconnect(() => MTFCoreService.CreateDirectory(path, newDirName, useDefaultMtfDirectory));

        public List<MTFPersistDataInfo> GetSequencesInfo(string basePath, string path) => CallWithReconnect(() => MTFCoreService.GetSequencesInfo(basePath, path));

        public List<MTFPersistDataInfo> GetServerFileAndFolders(string path, bool getFiles) => CallWithReconnect(() => MTFCoreService.GetServerFileAndFolders(path, getFiles));

        public void RenameItem(string newName, string oldFullname, string root, bool useDefaultMtfDirectory)
        {
            string output = CallWithReconnect(() => MTFCoreService.RenameItem(newName, oldFullname, root, useDefaultMtfDirectory));
            if (!string.IsNullOrEmpty(output))
            {
                throw new Exception(output);
            }
        }

        public void RemoveFile(string name, string root, bool useDefaultMtfDirectory) => CallWithReconnect(() => MTFCoreService.RemoveFile(name, root, useDefaultMtfDirectory));

        public void RemoveDirectory(string name, string root, bool useDefaultMtfDirectory) => CallWithReconnect(() => MTFCoreService.RemoveDirectory(name, root, useDefaultMtfDirectory));

        public ServerSettings GetServerSettings()
        {
            var settings = CallWithReconnect(() => MTFCoreService.GetServerSettings());
            return settings ?? new ServerSettings();
        }

        public string GetServerFullDirectoryPath(string relativePath) => CallWithReconnect(() => MTFCoreService.GetServerFullDirectoryPath(relativePath));

        public void SaveServerSettings() => CallWithReconnect(() => MTFCoreService.SaveServerSettings());

        public void UpdateServerSettings(ServerSettings settings) => CallWithReconnect(() => MTFCoreService.UpdateServerSettings(settings));

        public string RequestStopServer() => CallWithReconnect(() => MTFCoreService.RequestStopServer());

        public void SendMessageBoxResult(MTFDialogResult dialogResult, List<Guid> executingActivityPath) => CallWithReconnect(() => MTFCoreService.SendMessageBoxResult(dialogResult, executingActivityPath));

        public void ImportSequenceSetting(MTFImportSetting setting) => CallWithReconnect(() => MTFCoreService.ImportSequenceSetting(setting));

        public bool ImportSequence(Stream stream) => CallWithReconnect(() => MTFCoreService.ImportSequence(stream));

        public IList<MTFSequenceClassInfo> LoadSequenceClassInfo(string fileName)
        {
            var collection = CallWithReconnect(() => MTFCoreService.LoadSequenceClassInfo(fileName));
            return collection == null ? null : new MTFObservableCollection<MTFSequenceClassInfo>(collection);
        }

        public bool ExistSequence(string fileName) => CallWithReconnect(() => MTFCoreService.ExistSequence(fileName));

        public void ChangeExecutionEventListener(SequenceExecutionEventType[] listenersToAdd, SequenceExecutionEventType[] listenersToRemove) => CallWithReconnect(() => MTFCoreService.ChangeExecutionEventListener(listenersToAdd, listenersToRemove));

        public void ResultReuqest(ResultRequestTypes requestType) => MTFCoreService.ResultReuqest(requestType);

        public Dictionary<Guid, object> GetActualVariableValues(IEnumerable<Guid> variables) => CallWithReconnect(() => MTFCoreService.GetActualVariableValues(variables));
        public void DebugStepOver() => CallWithReconnect(MTFCoreService.DebugStepOver);

        public void DebugStepInto() => CallWithReconnect(MTFCoreService.DebugStepInto);

        public void DebugStepOut() => CallWithReconnect(() => MTFCoreService.DebugStepOut());

        public void SetNewExecutionPointer(Guid[] executionPath) => CallWithReconnect(() => MTFCoreService.SetNewExecutionPointer(executionPath));

        public void SetNewVariableValue(Guid variableId, object value) => CallWithReconnect(() => MTFCoreService.SetNewVariableValue(variableId, value));

        public void SetupVariantSelectionResult(IEnumerable<SetupVariantSelectionResult> variantSelectionResults) => CallWithReconnect(() => MTFCoreService.SetupVariantSelectionResult(variantSelectionResults));

        public void SetValidationTableRow(Guid tableId, MTFValidationTableRow row) => CallWithReconnect(() => MTFCoreService.SetValidationTableRow(tableId, row));

        public void SetIsServiceMode(bool isServiceMode) => CallWithReconnect(() => MTFCoreService.SetIsServiceMode(isServiceMode));

        public void SetIsTeachingMode(bool isTeachingMode) => CallWithReconnect(() => MTFCoreService.SetIsTeachingMode(isTeachingMode));

        public void ExecuteServiceCommand(Guid commandId) => CallWithReconnect(() => MTFCoreService.ExecuteServiceCommand(commandId));

        public void ExecuteUserCommand(Guid commandId) => CallWithReconnect(() => MTFCoreService.ExecuteUserCommand(commandId));

        public void RemoveGoldSampleData(string fileName) => CallWithReconnect(() => MTFCoreService.RemoveGoldSampleData(fileName));

        public FileInfo GetGoldSampleDataFileInfo(string fileName) => CallWithReconnect(() => MTFCoreService.GetGoldSampleDataFileInfo(fileName));

        public void SaveExecutedSequence(string modifiedByUser) => CallWithReconnect(() => MTFCoreService.SaveExecutedSequence(modifiedByUser));

        public void ClientUISendData(byte[] data, ClientUIDataInfo info) => CallWithReconnect(() => MTFCoreService.ClientUISendData(data, info));

        public void LogedUserChanged(string userName) => CallWithReconnect(() => MTFCoreService.LogedUserChanged(userName));

        public void SetupControlClosed(OpenClientSetupControlArgs args) => CallWithReconnect(() => MTFCoreService.SetupControlClosed(args));

        public List<GraphicalViewImg> SaveGraphicalViewImages(List<GraphicalViewImg> images) => CallWithReconnect(() => MTFCoreService.SaveGraphicalViewImages(images));

        public List<GraphicalViewImg> LoadAllGraphicalViewImages() => CallWithReconnect(() => MTFCoreService.LoadGraphicalViewImages(true, null));

        public List<GraphicalViewImg> LoadGraphicalViewImages(IEnumerable<string> fileNames) => CallWithReconnect(() => MTFCoreService.LoadGraphicalViewImages(false, fileNames));

        public List<string> GetGraphicalViewImageNames() => CallWithReconnect(() => MTFCoreService.GetGraphicalViewImageNames());

        public List<MTFActivityResult> GetMTFActivityResult() => CallWithReconnect(() => MTFCoreService.GetMTFActivityResult());

        public DataMigrationInfo CheckPossibleDataMigration() => CallWithReconnect(MTFCoreService.CheckPossibleDataMigration);

        public Task DoDataMigration(DataMigrationType dataMigrationType) => CallWithReconnect(() => MTFCoreService.DoDataMigration(dataMigrationType));

        private T CallWithReconnect<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (FaultException)
            {
                throw;
            }
            catch (CommunicationException ex)
            {
                SystemLog.LogMessage(ex.Message);
                Connect();
                return func();
            }
        }

        private void CallWithReconnect(Action func)
        {
            try
            {
                func();
            }
            catch (FaultException)
            {
                throw;
            }
            catch (CommunicationException ex)
            {
                SystemLog.LogMessage(ex.Message);
                Connect();
                func();
            }
        }

        private void UpdateSequenceClassInfos(MTFSequence sequence)
        {
            var monsterClasses = availableMonsterClasses ?? AvailableMonsterClasses;
            if (sequence.MTFSequenceClassInfos != null && monsterClasses != null)
            {
                foreach (var classInfo in sequence.MTFSequenceClassInfos)
                {
                    var cl = monsterClasses.FirstOrDefault(c => c.FullName == classInfo.MTFClass.FullName);
                    if (cl != null)
                    {
                        classInfo.MTFClass = (MTFClassInfo)cl.Copy();
                        if (classInfo.MTFClassInstanceConfiguration != null)
                        {
                            classInfo.MTFClassInstanceConfiguration = ClassInstanceConfigurations(cl).
                                FirstOrDefault(cic => cic.Id == classInfo.MTFClassInstanceConfiguration.Id || cic.Name == classInfo.MTFClassInstanceConfiguration.Name);

                            UpdateActivitiesByClassInfo(sequence, cl);
                        }
                    }
                }
            }
            sequence.IsModified = false;
        }

        private void UpdateActivitiesByClassInfo(MTFSequence sequence, MTFClassInfo classInfo)
        {
            foreach (var activity in sequence.MTFSequenceActivities)
            {
                UpdateActivitiesByClassInfo(activity, classInfo);
            }

            foreach (var activity in sequence.ActivitiesByCall)
            {
                UpdateActivitiesByClassInfo(activity, classInfo);
            }
        }

        private void UpdateActivitiesByClassInfo(MTFSequenceActivity activity, MTFClassInfo classInfo)
        {
            if (activity == null)
            {
                return;
            }

            if (activity is MTFSubSequenceActivity)
            {
                foreach (var act in ((MTFSubSequenceActivity)activity).Activities)
                {
                    UpdateActivitiesByClassInfo(act, classInfo);
                }
                return;
            }

            if (activity.ClassInfo != null && activity.ClassInfo.MTFClass != null && activity.ClassInfo.MTFClass.FullName != classInfo.FullName)
            {
                return;
            }
            var method = classInfo.Methods.FirstOrDefault(m => m.Name == activity.MTFMethodName);
            if (method != null)
            {
                activity.SetupModeSupport = method.SetupModeSupport;
                activity.MTFMethodDescription = method.Description;
                foreach (var param in activity.MTFParameters)
                {
                    var methodParam = method.Parameters.FirstOrDefault(p => p.Name == param.Name);
                    if (methodParam != null)
                    {
                        param.SetupMethodName = methodParam.SetupMethodName;
                        param.Description = methodParam.Description;
                    }
                }
            }
        }
    }
}
