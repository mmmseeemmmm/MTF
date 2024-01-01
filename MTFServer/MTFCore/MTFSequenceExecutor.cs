using MTFClientServerCommon;
using MTFClientServerCommon.Mathematics;
using AutomotiveLighting.MTFCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using MTFClientServerCommon.GoldSamplePersist;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.MTFTable;
using MTFClientServerCommon.MTFValidationTable;
using MTFCommon;
using MTFCommon.ClientControls;
using MTFActivityResult = MTFClientServerCommon.MTFActivityResult;
using System.Drawing;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.SequenceLocalization;
using MTFCore.DbReporting;
using MTFCore.Managers;
using MTFCore.Managers.Components;

namespace MTFCore
{
    class MTFSequenceExecutor
    {
        private readonly MTFSequence sequence;
        private readonly Thread sequenceThread;
        private List<Guid> currentActivityPath = new List<Guid>();
        private MTFSequenceExecutionState executionState = MTFSequenceExecutionState.None;
        private readonly object lastActivityResultLock = new object();
        private List<Guid> executedActivities;
        private readonly object executedActivitesLock = new object();
        private DateTime sequenceHandlingResultStopwatch;
        private MTFSequenceResult sequenceResult;
        // ReSharper disable once RedundantDefaultMemberInitializer
        //private bool sequenceModifiedBySetupMode = false;
        private readonly Dictionary<Guid, ClassInstance> instances = new Dictionary<Guid, ClassInstance>();
        private readonly List<MTFSequenceClassInfo> staticTypes = new List<MTFSequenceClassInfo>();
        private readonly Dictionary<string, IList<IReceiveClientControlData>> receiveClientControlDataInstances = new Dictionary<string, IList<IReceiveClientControlData>>();
        //Key - guid of component from subsequence; Value - guid of component from mainSequence
        private readonly Dictionary<Guid, Guid> componentsMapping = new Dictionary<Guid, Guid>();
        //Key - guid of each variable; Value - Guid of variable from parent sequence with the same name as variable in key
        // ReSharper disable once RedundantDefaultMemberInitializer
        private bool createInstanceBeforeFirstStart = false;
        private Guid componentIdForCreation = Guid.Empty;
        private readonly Func<MTFClassInfo, List<MTFClassInstanceConfiguration>> classInstanceConfigurationsCallBack;
        private Guid[] debugNewExecutionPath;
        private readonly ServerSettings serverSettings;
        private readonly GoldSamplePersist goldSampleData;
        private bool goingToServiceMode;
        private bool goingToTeachMode;
        private IList<Guid> allowedCommands = new List<Guid>();
        private readonly Dictionary<Guid, bool> toggleCommands = new Dictionary<Guid, bool>();
        private MultipleTableRowFillCheckHelper multipleTableRowFillCheck = new MultipleTableRowFillCheckHelper();
        private readonly DynamicSequenceHelper dynamic = new DynamicSequenceHelper();
        private readonly DateTime sequenceStart;
        private bool checkGoldSampleAfterStart;
        private bool requireGoldSampleAfterInactivity = false;
        private bool asyncExecution;
        private MTFSequenceActivity lastErrorActivity;
        //private readonly object exceptionLock = new object();
        //private readonly List<MTFActivityError> exceptions = new List<MTFActivityError>();
        private List<Thread> listActivitiesOnBackground = new List<Thread>();
        private ScopeData scopeForService;
        private ScopeData rootScopeData;
        private Dictionary<string, int> dictionaryOfSynchronization = new Dictionary<string, int>();
        private VariablesHandler variablesHandler = new VariablesHandler();
        private ActivityErrorsHandler activityErrorsHandler = new ActivityErrorsHandler();
        private MTFSequenceActivity actualExecuteCatch;
        private ReportManager reportManager;
        private readonly ComponentsManager componentsManager;

        public event OnErrorEventHandler OnError;
        public delegate void OnErrorEventHandler(StatusMessage errorMessage);

        public event OnFinishEventHandler OnFinish;
        public delegate void OnFinishEventHandler(MTFSequenceResult sequenceResult);

        public event OnActivityChangedEventHandler OnActivityChanged;
        public delegate void OnActivityChangedEventHandler(Guid[] executingActivityPath);

        public event OnStateChangedEventHandler OnStateChanged;
        public delegate void OnStateChangedEventHandler(MTFSequenceExecutionState newState);

        public event OnActivityPercentProgressChangedEventHandler OnActivityPercentProgressChanged;
        public delegate void OnActivityPercentProgressChangedEventHandler(ActivityPercentProgressChangedEventArgs e);

        public event OnActivityStringProgressChangedEventHandler OnActivityStringProgressChanged;
        public delegate void OnActivityStringProgressChangedEventHandler(ActivityStringProgressChangedEventArgs e);

        public event OnActivityImageProgressChangedEventHandler OnActivityImageProgressChanged;
        public delegate void OnActivityImageProgressChangedEventHandler(ActivityImageProgressChangedEventArgs e);

        public event OnNewActivityResultEventHandler OnNewActivityResult;
        public delegate void OnNewActivityResultEventHandler(MTFActivityResult result);

        public event OnNewValidatiteRowsEventHandler OnNewValidatiteRows;
        public delegate void OnNewValidatiteRowsEventHandler(List<MTFValidationTableRowResult> rows, bool activityWillBeRepeated, Guid? dutId);

        public event OnRepeatSubSequenceEventHandler OnRepeatSubSequence;
        public delegate void OnRepeatSubSequenceEventHandler(Guid[] executingActivityPath);

        public event OnSequenceStatusMessageEventHandler OnSequenceStatusMessage;
        public delegate void OnSequenceStatusMessageEventHandler(string line1, string line2, string line3, StatusLinesFontSize fontSize, Guid? dutId);

        public event OnShowMessageEventHandler OnShowMessage;
        public delegate void OnShowMessageEventHandler(MessageInfo messageInfo);

        public event OnClearValidationTablesEventHandler OnClearValidationTables;
        public delegate void OnClearValidationTablesEventHandler(bool clearAllTables, List<Guid> tablesForClearing, Guid? dutId);

        public event OnShowSetupVariantSelectionEventHandler OnShowSetupVariantSelection;
        public delegate void OnShowSetupVariantSelectionEventHandler(string activityName, Dictionary<string, IEnumerable<SequenceVariant>> dataVariants, Dictionary<string, string> extendetUsedDataNames);

        public event OnSequenceVariantChangedEventHandler OnSequenceVariantChanged;
        public delegate void OnSequenceVariantChangedEventHandler(SequenceVariantInfo sequenceVariantInfo, Guid? dutId);

        public event OnSerivceExecutionCommandsStateChangedEventHandler OnSerivceExecutionCommandsStateChanged;
        public delegate void OnSerivceExecutionCommandsStateChangedEventHandler(IEnumerable<Guid> allowedCommands, IEnumerable<Guid> checkedCommands);

        public event OnLoadGoldSamplesEventHandler OnLoadGoldSamples;
        public delegate void OnLoadGoldSamplesEventHandler(List<SequenceVariantInfo> goldSampleList);

        public event OnAllowSaveExecutedSequenceEventHandler OnAllowSaveExecutedSequence;
        public delegate void OnAllowSaveExecutedSequenceEventHandler(bool state);

        public event OnUIControlSendDataEventHandler OnUIControlSendData;
        public delegate void OnUIControlSendDataEventHandler(byte[] data, ClientUIDataInfo info);

        public event OnDynamicLoadSequenceEventHandler OnDynamicLoadSequence;
        public delegate void OnDynamicLoadSequenceEventHandler(MTFSequence sequence, Dictionary<Guid, MTFExternalSequenceInfo> externalSequences);

        public event OnDynamicUnLoadSequenceEventHandler OnDynamicUnloadSequence;
        public delegate void OnDynamicUnLoadSequenceEventHandler(Guid sequenceId);

        public event OnDynamicExecuteSequenceEventHandler OnDynamicExecuteSequence;
        public delegate void OnDynamicExecuteSequenceEventHandler(Guid sequenceId, Guid subSequenceId, bool callSubSequence, Guid[] activityIdPath);

        public event OnCloseMessageEventHandler OnCloseMessage;
        public delegate void OnCloseMessageEventHandler(Guid acitivityId);

        public event OnOpenClientSetupControlEventHandler OnOpenClientSetupControl;
        public delegate void OnOpenClientSetupControlEventHandler(OpenClientSetupControlArgs args);

        public event OnUserCommandsStatusChangedEventHandler OnUserCommandsStatusChanged;
        public delegate void OnUserCommandsStatusChangedEventHandler(IEnumerable<UserCommandsState> commandsSettings);

        public event OnUserIndicatorValueChangedEventHandler OnUserIndicatorValueChanged;
        public delegate void OnUserIndicatorValueChangedEventHandler(Guid indicatorId, bool value);

        public event OnViewChangedEventHandler OnViewChanged;
        public delegate void OnViewChangedEventHandler(SequenceExecutionViewType view, Guid? graphicalViewId, Guid? dutId);

        private MTFSequenceRuntimeContext sequenceRuntimeContext;

        public Action<IEnumerable<MTFSequence>, string> SaveSequneceCallback;


        public MTFSequenceExecutor(MTFSequence sequence, Func<MTFClassInfo, List<MTFClassInstanceConfiguration>> callBack, ServerSettings serverSettings)
        {
            sequenceStart = DateTime.Now;
            componentsManager = ManagersContainer.Get<ComponentsManager>();
            //loading is not finished - types which should be unknown are also loaded - test sequence bugfix.
            //MTFInvoker lin 65 var parameterType = Type.GetType(value.TypeName); must return null on unknown type
            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            classInstanceConfigurationsCallBack = callBack;
            this.serverSettings = serverSettings;
            sequenceRuntimeContext = new MTFSequenceRuntimeContext(sequence.Name)
            {
                RaiseExceptionMethod = handleCrititcalAsynchrounousException,
                LogMessageMethod = handleLogMessage,
                TextNotificationMethod = handleTextNotification,
                ImageNotificationMethod = handleImageNotification,
                ProgressNotificationMethod = handleProgressNotification,
                ProgressNotificationIndeterminateMethod = HandleProgressNotificationIndeterminate,
                LoadDataMethod = loadData,
                SaveDataMethod = saveData,
                GetTargetDataMethod = getTargetData,
                AddToValidationTableMethod = AddToValidationTable,
                GetFromValidationTableMethod = GetFromValidationTable,
                GetFromConstantTableMethod = GetFromConstantTable,
                GetValidationTableStatusMethod = (tableName, scope) => getTable(tableName, scope).Status,
                GetValidationTableRowStatusMethod = (tableName, rowName, scope) => getTableRow(getTable(tableName, scope), rowName).Status,
                GetValidationTableErrorTextMethod = (tableName, scope) => getTable(tableName, scope).ErrorText,
                GetValidationTableRowErrorTextMethod = (tableName, rowName, scope) => getTableRow(getTable(tableName, scope), rowName).ErrorText,
                GetVariantForSaveDataMethod = VariantForSaveData,
                GetVariantForLoadDataMethod = VariantForLoadData,
                GetValidationTableRowsMethod = GetValidationTableRows,
                AddRangeToValidationTableMethod = AddRangeToValidationTable,
                IsGoldSampleMethod = IsGoldSample,
                GoldSampleRequiredMethod =  GoldSampleRequired,
                GoldSampleInvalidateMethod = GoldSampleInvalidate,
                SetSequenceVariantMethod = SetSequenceVariantRuntimeContext,
                SendToClientControlMethod = SendToClientControl,
                SendToClientSetupControlMethod = SendToClientSetupControl,
                OpenClientSetupContorolMethod = OpenClientSetupControl,
                RoundValueMethod = RoundData,
            };

            this.sequence = sequence;
            PrepareCommands(sequence.ServiceCommands);
            checkGoldSampleAfterStart = sequence.GoldSampleSetting.AllowGoldSampleAfterStart;
            goldSampleData = GoldSampleHelper.LoadGSData(sequence.GoldSampleSetting);
            sequenceThread = new Thread(ExecuteInThread);
        }

        private double RoundData(double value)
        {
            return Sequence != null ? MTFSequenceHelper.RoundValue(value, Sequence.RoundingRules) : value;
        }

        private void PrepareCommands(MTFObservableCollection<MTFServiceCommand> commands)
        {
            if (commands != null)
            {
                foreach (var command in commands)
                {
                    allowedCommands.Add(command.Id);
                    if (command.Type == MTFServiceCommandType.ToggleButton)
                    {
                        toggleCommands.Add(command.Id, false);
                    }
                }
            }
        }


        ~MTFSequenceExecutor()
        {
            CleanLazyLoadCache();
        }

        public void CleanLazyLoadCache()
        {
            lock (lastActivityResultLock)
            {
                foreach (var id in idsOfLazyLoadObjects.Keys)
                {
                    idsOfLazyLoadObjects[id].ForEach(MTFDataTransferObject.RemoveFromLazyLoadCache);
                }
            }
        }

        private SequenceVariant getSaveDataVariant(string dataName)
        {
            if (variantSelectionResults == null)
            {
                return null;
            }

            var result = variantSelectionResults.FirstOrDefault(v => v.DataName == dataName);
            return result == null ? null : result.SaveVariant;
        }

        private SequenceVariant getLoadDataVariant(string dataName, ScopeData scope)
        {
            if (sequenceRuntimeContext.IsSetupMode)
            {
                if (variantSelectionResults == null)
                {
                    return null;
                }

                var result = variantSelectionResults.FirstOrDefault(v => v.DataName == dataName);
                return result == null ? null : result.UseVariant;
            }

            return scope.SequenceVariant;
        }

        private string VariantForSaveData(string dataName)
        {
            var variant = getSaveDataVariant(dataName);
            return variant != null ? variant.ToString() : string.Empty;
        }

        private string VariantForLoadData(string dataName, ScopeData scope)
        {
            var variant = getLoadDataVariant(dataName, scope);
            return variant != null ? variant.ToString() : string.Empty;
        }

        private void saveData(string dataName, object data)
        {
            //if (!sequenceRuntimeContext.IsSetupMode)
            //{
            //    throw new Exception("Save data is not possible, sequence isn't in setup mode");
            //}

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, data);

            var activity = getActivityByThread();
            if (activity == null || activity.ClassInfo == null)
            {
                return;
            }

            var classInfo = activity.ClassInfo;
            //if activity is form external sequence, get maped class info
            if (componentsMapping.ContainsKey(classInfo.Id))
            {
                classInfo = sequence.MTFSequenceClassInfos.FirstOrDefault(ci => ci.Id == componentsMapping[classInfo.Id]);
            }

            if (classInfo != null)
            {
                saveDataToClassInfo(classInfo, dataName, ms);
            }

            ms.Dispose();

            RaiseAllowSaveExecutedSequence(true);
        }

        private void saveDataToClassInfo(MTFSequenceClassInfo classInfo, string dataName, MemoryStream ms)
        {
            if (classInfo.Data == null)
            {
                classInfo.Data = new Dictionary<string, IList<ClassInfoData>>();
            }

            var variant = getSaveDataVariant(dataName);
            if (classInfo.Data.ContainsKey(dataName))
            {
                ClassInfoData oldData = variant == null || variant.IsEmpty ? classInfo.Data[dataName].FirstOrDefault(d => d.SequenceVariant == null) :
                    classInfo.Data[dataName].FirstOrDefault(d => d.SequenceVariant != null && d.SequenceVariant.Equals(variant));
                if (oldData == null)
                {
                    oldData = new ClassInfoData { SequenceVariant = variant };
                    classInfo.Data[dataName].Add(oldData);
                }

                oldData.Data = ms.ToArray();
                oldData.LastModifiedTime = DateTime.Now;
            }
            else
            {
                classInfo.Data[dataName] = new List<ClassInfoData>
                                           {
                                               new ClassInfoData {SequenceVariant = variant, Data = ms.ToArray(), LastModifiedTime = DateTime.Now}
                                           };

                RaiseAllowSaveExecutedSequence(true);
            }
        }

        private object loadData(string dataName, Type type, ScopeData scope)
        {
            return loadData(dataName, type, getLoadDataVariant(dataName, scope));
        }

        private object loadData(string dataName, Type type, SequenceVariant variant)
        {
            var activity = getActivityByThread();
            var classInfo = activity.ClassInfo;
            //if activity is form external sequence, get maped class info
            if (componentsMapping.ContainsKey(classInfo.Id))
            {
                classInfo = sequence.MTFSequenceClassInfos.FirstOrDefault(ci => ci.Id == componentsMapping[classInfo.Id]);
            }

            if (classInfo == null || classInfo.Data == null || !classInfo.Data.ContainsKey(dataName))
            {
                return null;
            }

            BinaryFormatter formatter = new BinaryFormatter { Binder = new BindChanger(type) };

            SequenceVariant bestStoredVariant = variant == null ? null : variant.GetBestMatch(classInfo.Data[dataName].Select(d => d.SequenceVariant));

            ClassInfoData storedData = bestStoredVariant == null ? classInfo.Data[dataName].FirstOrDefault(d => d.SequenceVariant == null) :
                classInfo.Data[dataName].FirstOrDefault(d => d.SequenceVariant != null && d.SequenceVariant.Equals(bestStoredVariant));

            if (storedData == null)
            {
                return null;
            }
            using (MemoryStream ms = new MemoryStream(storedData.Data))
            {
                return formatter.Deserialize(ms);
            }
        }

        private object getTargetData(string dataName, Type type)
        {
            return loadData(dataName, type, getSaveDataVariant(dataName));
        }

        private void handleCrititcalAsynchrounousException(object sender, Exception e, ExceptionLevel exceptionLevel)
        {
            reportManager.AddError(ErrorTypes.SequenceError, "-", e.Message, null);
            StatusMessage.MessageType messageType = StatusMessage.MessageType.Error;
            switch (exceptionLevel)
            {
                case ExceptionLevel.JustInfo:
                    messageType = StatusMessage.MessageType.Info;
                    break;
                case ExceptionLevel.Warning:
                    messageType = StatusMessage.MessageType.Warning;
                    break;
            }
            SystemLog.LogException(e);
            RaiseOnError(DateTime.Now, e.Message, messageType, null, null);

            if (exceptionLevel == ExceptionLevel.CriticalAsynchronousException)
            {
                Abort();
            }
        }

        private void handleLogMessage(string message, LogLevel logLevel)
        {
            StringBuilder sb = new StringBuilder();

            bool first = true;
            foreach (var activityId in getActivityPathByThread())
            {
                var act = sequence.GetActivity(activityId);
                if (act != null)
                {
                    if (!first)
                    {
                        sb.Append(" -> ");
                    }
                    sb.Append(act.ActivityName);
                    first = false;
                }
            }
            SystemLog.LogMessage(message, sb.ToString());
        }

        private List<Guid> getActivityPathByThread()
        {
            return getActivityPathByThread(true);
        }

        private List<Guid> getActivityPathByThread(bool throwException)
        {
            if (!sequenceRuntimeContext.ThreadIdToScopeData.ContainsKey(Thread.CurrentThread.ManagedThreadId))
            {
                if (throwException)
                {
                    throw new Exception("This kind of call must be executed on MTF thread. Please check driver implementation!");
                }

                return null;
            }

            return sequenceRuntimeContext.ThreadIdToScopeData[Thread.CurrentThread.ManagedThreadId].ExecutingActivityPath;
        }

        private MTFSequenceActivity getActivityByThread()
        {
            return sequence.GetActivity(getActivityPathByThread().Last());
        }

        private MTFActivityResult getActivityResultByThread()
        {
            if (!threadIdToActivityResult.ContainsKey(Thread.CurrentThread.ManagedThreadId))
            {
                throw new Exception("This kind of call must be executed on MTF thread. Please check driver implementation!");
            }

            return threadIdToActivityResult[Thread.CurrentThread.ManagedThreadId];
        }

        public MTFSequence Sequence
        {
            get { return sequence; }
        }

        public void SetIsSetupMode(bool isSetupMode)
        {
            sequenceRuntimeContext.IsSetupModeActive = isSetupMode;
        }
        public void AddSetupActivityPath(List<Guid> activityPath)
        {
            lock (setupModeActivityPathsLock)
            {
                sequenceRuntimeContext.SetupModeActivityPaths.Add(activityPath);
            }
        }
        public void RemoveSetupActivityPath(List<Guid> activityPath)
        {
            lock (setupModeActivityPathsLock)
            {
                var path = string.Join(";", activityPath);
                var pathToRemove = sequenceRuntimeContext.SetupModeActivityPaths.FirstOrDefault(i => string.Join(";", i).Equals(path));
                if (pathToRemove != null)
                {
                    sequenceRuntimeContext.SetupModeActivityPaths.Remove(pathToRemove);
                }
            }
        }
        public void SetIsDebugMode(bool isDebugMode)
        {
            sequenceRuntimeContext.IsDebugModeActive = isDebugMode;
        }
        public void AddBreakPointActivityPath(List<Guid> activityPath)
        {
            lock (breakPointActivityPathsLock)
            {
                var path = string.Join(";", activityPath);
                if (!sequenceRuntimeContext.BreakPointActivityPaths.Any(i => string.Join(";", i).Equals(path)))
                {
                    sequenceRuntimeContext.BreakPointActivityPaths.Add(activityPath);
                }
            }
        }
        public void RemoveBreakPointActivityPath(List<Guid> activityPath)
        {
            lock (breakPointActivityPathsLock)
            {
                var path = string.Join(";", activityPath);
                var pathToRemove = sequenceRuntimeContext.BreakPointActivityPaths.FirstOrDefault(i => string.Join(";", i).Equals(path));
                if (pathToRemove != null)
                {
                    sequenceRuntimeContext.BreakPointActivityPaths.Remove(pathToRemove);
                }
            }
        }

        private readonly object setupModeActivityPathsLock = new object();
        private readonly object breakPointActivityPathsLock = new object();
        private Dictionary<string, string> loggedUsers;
        public void Start(bool isSetupMode, IEnumerable<List<Guid>> setupModeActivityPaths, bool isDebug, IEnumerable<List<Guid>> breakPointActivityPaths, bool isServiceMode, bool isTeachMode, Dictionary<string, string> loggedUsers)
        {
            this.loggedUsers = loggedUsers;
            //prepaire runtimme context
            lock (setupModeActivityPathsLock)
            {
                sequenceRuntimeContext.SetupModeActivityPaths = new List<List<Guid>>(setupModeActivityPaths);
                sequenceRuntimeContext.IsSetupModeActive = isSetupMode;
            }
            lock (breakPointActivityPathsLock)
            {
                sequenceRuntimeContext.BreakPointActivityPaths = new List<List<Guid>>(breakPointActivityPaths);
                sequenceRuntimeContext.IsDebugModeActive = isDebug;
            }

            goingToServiceMode = isServiceMode;
            goingToTeachMode = isTeachMode;
            sequenceRuntimeContext.IsServiceModeActive = isServiceMode;
            sequenceRuntimeContext.IsTeachModeActive = isTeachMode;

            if (executionState == MTFSequenceExecutionState.Pause)
            {
                ChangeExecutionState(MTFSequenceExecutionState.Executing);
                return;
            }

            if (executionState == MTFSequenceExecutionState.None || executionState == MTFSequenceExecutionState.Stopped)
            {
                ChangeExecutionState(MTFSequenceExecutionState.Executing);
                executedActivities = new List<Guid>();
                variablesHandler.ClearAllVariables();
                sequenceResult = new MTFSequenceResult();
                sequenceResult.ExecutionStart = DateTime.Now;
                sequenceResult.SequenceName = sequence.Name;
                sequenceResult.SequenceId = sequence.Id;
                resetSequenceHandlingStopwatch();

                sequenceThread.Start();
            }
        }

        public void LogedUserChanged(string clientId, string userName)
        {
            loggedUsers[clientId] = userName;
        }
        
        public void Stop()
        {
            if (executionState == MTFSequenceExecutionState.Executing || executionState == MTFSequenceExecutionState.Pause)
            {
                ChangeExecutionState(MTFSequenceExecutionState.Stopping);
            }

            HandleICanStop();
        }

        public void Pause()
        {
            if (executionState == MTFSequenceExecutionState.Executing)
            {
                ChangeExecutionState(MTFSequenceExecutionState.Pause);
            }
        }

        public void Abort()
        {
            if (executionState == MTFSequenceExecutionState.Executing || executionState == MTFSequenceExecutionState.Pause)
            {
                ChangeExecutionState(MTFSequenceExecutionState.Aborting);
            }

            HandleICanStop();
        }

        private void HandleICanStop()
        {
            foreach (var instance in instances.Values.Where(i => i.Instance is ICanStop).ToList())
            {
                try
                {
                    ((ICanStop)instance.Instance).Stop = true;
                }
                catch (Exception ex)
                {
                    SystemLog.LogException(ex);
                    Task.Run(() => RaiseOnError(DateTime.Now, ex.Message, StatusMessage.MessageType.Error, null, null));
                }
            }
        }


        public MTFSequenceExecutionState State
        {
            get { return executionState; }
        }

        private bool messageBoxResultReceived;
        private MTFDialogResult messageBoxResult;
        public void SendMessageBoxResult(MTFDialogResult dialogResult, List<Guid> executingActivityPath)
        {
            messageBoxResultReceived = true;
            messageBoxResult = dialogResult;
        }

        public Dictionary<Guid, object> GetActualVariableValues(IEnumerable<Guid> variables)
        {
            Dictionary<Guid, object> tmp = new Dictionary<Guid, object>();
            foreach (var variableId in variables)
            {
                tmp[variableId] = variablesHandler.GetVariableValue(variableId);
            }

            return tmp;
        }

        public void DebugStepOver()
        {
            breakOnActivityLevel = currentActivityPath.Count;
            ChangeExecutionState(MTFSequenceExecutionState.Executing);
        }


        public void DebugStepInto()
        {
            breakOnNextActivity = true;
            ChangeExecutionState(MTFSequenceExecutionState.Executing);
        }

        public void DebugStepOut()
        {
            breakOnActivityLevel = currentActivityPath.Count - 1;
            ChangeExecutionState(MTFSequenceExecutionState.Executing);
        }

        public void SetNewExecutionPointer(Guid[] executionPath)
        {
            debugNewExecutionPath = executionPath;
            ChangeExecutionState(MTFSequenceExecutionState.DebugGoToTopPosition);
        }

        public void SetNewVariableValue(Guid variableId, object value) => variablesHandler.SetVariableValue(variableId, value);

        public void SetSetupVariantSelection(IEnumerable<SetupVariantSelectionResult> variantSelectionResults)
        {
            this.variantSelectionResults = variantSelectionResults;
            setupVariantCanceled = this.variantSelectionResults == null;
            setupVariantRequested = true;
        }

        public void ClientUIDataSend(byte[] data, ClientUIDataInfo info)
        {
            BinaryFormatter formatter = new BinaryFormatter { Binder = new BindChanger(Type.GetType(info.DataType)) };
            object deserializedData;
            using (MemoryStream ms = new MemoryStream(data))
            {
                deserializedData = formatter.Deserialize(ms);
            }

            foreach (var cci in info.ClientControlInfos)
            {
                var key = string.Format("{0}.{1}", cci.AssemblyName, cci.TypeName);
                if (receiveClientControlDataInstances.ContainsKey(key))
                {
                    foreach (var instance in receiveClientControlDataInstances[key])
                    {
                        instance.ReceiveData(deserializedData, info.DataName);
                    }
                }
            }
        }

        private void InitComponentsMapping(MTFSequence currentSequence)
        {
            if (currentSequence.ComponetsMapping != null)
            {
                try
                {
                    foreach (var item in currentSequence.ComponetsMapping)
                    {
                        componentsMapping.Add(item.Key, item.Value);
                    }
                }
                catch (Exception)
                {
                    throw new Exception("Sequence " + currentSequence.Name + " contains bad components mapping. Please check this sequences.");
                }
            }
            InitComponentsMappingForExternal(currentSequence);
        }

        private void InitComponentsMappingForExternal(MTFSequence currentSequence)
        {
            if (currentSequence.ExternalSubSequences != null)
            {
                foreach (var item in currentSequence.ExternalSubSequences)
                {
                    InitComponentsMapping(item.ExternalSequence);
                }
            }
        }

        private void CreateDynamicComponentMapping(MTFSequence dynamicSequence)
        {
            if (dynamicSequence.MTFSequenceClassInfos != null)
            {
                foreach (var classInfo in dynamicSequence.MTFSequenceClassInfos)
                {
                    var mainSequenceClassInfo = Sequence.MTFSequenceClassInfos.FirstOrDefault(x => x.Alias == classInfo.Alias);
                    if (mainSequenceClassInfo != null)
                    {
                        if (!componentsMapping.ContainsKey(classInfo.Id))
                        {
                            componentsMapping.Add(classInfo.Id, mainSequenceClassInfo.Id);
                            dynamic.AddComponent(dynamicSequence.Id, classInfo.Id);
                        }
                    }
                    else
                    {
                        throw new Exception(
                            $"Component {classInfo.Alias} in dynamic loaded sequence {dynamicSequence.Name} has no required component in Main sequence. Please check your sequnce. The component name has to be the same.");
                    }
                }
            }
            InitComponentsMapping(dynamicSequence);
        }

        private void InitLogging(MTFSequence currentSequence)
        {
            reportManager = new ReportManager(sequence.Name, currentSequence.RoundingRules, serverSettings);
            reportManager.InitializeReporting(currentSequence.DeviceUnderTestInfos?.Select(x=>x.Id));
        }

        private void ChangeExecutionStateInternal(MTFSequenceExecutionState newState)
        {
            //Console.WriteLine("ChangeExecutionStateInternal {0} -> {1}", executionState, newState);
            executionState = newState;
        }

        private readonly object eventHandlingLock = new object();
        private void ChangeExecutionState(MTFSequenceExecutionState newState)
        {
            //executionState = newState;
            lock (eventHandlingLock)
            {
                ChangeExecutionStateInternal(newState);
                OnStateChanged?.Invoke(newState);
            }
        }

        private void RaiseActivityResult(MTFActivityResult result)
        {
            lock (eventHandlingLock)
            {
                OnNewActivityResult?.Invoke(result);
            }
        }

        private void RaiseActivityPercentProgressChanged(ActivityPercentProgressChangedEventArgs e)
        {
            lock (eventHandlingLock)
            {
                OnActivityPercentProgressChanged?.Invoke(e);
            }
        }

        private void RaiseActivityStringProgressChanged(ActivityStringProgressChangedEventArgs e)
        {
            lock (eventHandlingLock)
            {
                OnActivityStringProgressChanged?.Invoke(e);
            }
        }

        private void RaiseActivityImageProgressChanged(ActivityImageProgressChangedEventArgs e)
        {
            lock (eventHandlingLock)
            {
                OnActivityImageProgressChanged?.Invoke(e);
            }
        }

        private void RaiseSerivceExecutionCommandsStateChanged(IEnumerable<Guid> allowedCommands, IEnumerable<Guid> checkedCommands)
        {
            lock (eventHandlingLock)
            {
                OnSerivceExecutionCommandsStateChanged?.Invoke(allowedCommands, checkedCommands);
            }
        }

        private void RaiseSequenceStatusMessage(string line1, string line2, string line3, StatusLinesFontSize fontSize, Guid? dutId)
        {
            lock (eventHandlingLock)
            {
                OnSequenceStatusMessage?.Invoke(line1, line2, line3, fontSize, dutId);
            }
        }

        private void RaiseAllowSaveExecutedSequence(bool state)
        {
            lock (eventHandlingLock)
            {
                OnAllowSaveExecutedSequence?.Invoke(state);
            }
        }

        private void LoadGoldSampleValuesToTables()
        {
            if (goldSampleData.TableValues != null && goldSampleData.TableValues.Count > 0 &&  variablesHandler.AllVariables != null)
            {
                foreach (var tableValue in goldSampleData.TableValues)
                {
                    var variable = variablesHandler.AllVariables.Keys.FirstOrDefault(x => x.HasValidationTable
                        && ((MTFValidationTable)variablesHandler.GetVariableValue(x.Id)).Id == tableValue.Key 
                        && ((MTFValidationTable)variablesHandler.GetVariableValue(x.Id)).UseGoldSample);

                    if (variable != null && tableValue.Value != null)
                    {
                        if (variable.DependsOnDut)
                        {
                            foreach (var dut in sequence.DeviceUnderTestInfos)
                            {
                                ApplyGoldenSampleOnTable((MTFValidationTable)variablesHandler.GetVariableValue(variable.Id, dut.Id), tableValue);
                            }
                        }
                        else
                        {
                            ApplyGoldenSampleOnTable((MTFValidationTable)variablesHandler.GetVariableValue(variable.Id), tableValue);
                        }
                    }
                }
            }
        }

        private static void ApplyGoldenSampleOnTable(MTFValidationTable table, KeyValuePair<Guid, Dictionary<Guid, GoldSampleValue>> tableValue)
        {
            if (table.Rows != null)
            {
                foreach (var rowValues in tableValue.Value)
                {
                    var row = table.Rows.FirstOrDefault(r => r.Id == rowValues.Key);
                    row?.LoadGoldSampleValue(rowValues.Value);
                }
            }
        }

        private void ExecuteInThread()
        {
            try
            {
                componentsMapping.Clear();
                variablesHandler.InitDuts(sequence.DeviceUnderTestInfos);
                activityErrorsHandler.InitDuts(sequence.DeviceUnderTestInfos);
                variablesHandler.ClearVariablesMapping();
                InitComponentsMapping(sequence);
                variablesHandler.InitVariablesMapping(sequence, Guid.Empty, null);
                InitLogging(sequence);
                //create instances 
                instances.Clear();
                staticTypes.Clear();
                foreach (var classInfo in sequence.MTFSequenceClassInfos)
                {
                    if (classInfo.ConstructionType == MTFClassConstructionType.OnSequenceStart)
                    {
                        CreateClassInstance(classInfo);
                    }
                }
                if (goldSampleData.GoldSampleList != null && OnLoadGoldSamples != null)
                {
                    lock (eventHandlingLock)
                    {
                        OnLoadGoldSamples(goldSampleData.GoldSampleList);
                    }
                }
                LoadGoldSampleValuesToTables();

                //execute all activities
                ExecuteAllActivities(new ScopeData());

            }
            catch (Exception e)
            {
                var now = DateTime.Now;
                reportManager.AddError(ErrorTypes.SequenceError, "-", e.Message, null);
                var exString = StringHelper.GetExceptionStringMessage(e, true);
                activityErrorsHandler.AddError(null, CreateActivityError(now, null, exString));
                SystemLog.LogException(e);
                RaiseOnError(now, exString, StatusMessage.MessageType.Error, null, null);
                StopExecution(MTFSequenceExecutionState.Aborted);
            }
        }

        private void ExecuteAllActivities(ScopeData scope)
        {
            rootScopeData = scope;
            currentActivityPath = scope.ExecutingActivityPath;

            scope.ExecutingActivityPath.Add(sequence.Id);

            foreach (var activity in sequence.MTFSequenceActivities)
            {
                if (executionState == MTFSequenceExecutionState.Stopping)
                {
                    StopExecution(MTFSequenceExecutionState.Stopped);
                    return;
                }

                if (executionState == MTFSequenceExecutionState.Aborting || executionState == MTFSequenceExecutionState.AborSubSequence)
                {
                    StopExecution(MTFSequenceExecutionState.Aborted);
                    return;
                }

                if (executionState == MTFSequenceExecutionState.Executing || executionState == MTFSequenceExecutionState.Pause)
                {
                    ExecuteActivity(activity, scope);
                }
                else if (executionState == MTFSequenceExecutionState.DebugGoToNewPosition)
                {
                    scope.ExecutingActivityPath.Add(activity.Id);
                    bool pathStartsWithNewPath = ActivityPathStartWith(scope.ExecutingActivityPath, debugNewExecutionPath);
                    scope.ExecutingActivityPath.Remove(activity.Id);

                    if (pathStartsWithNewPath)
                    {
                        //if execution activity path is same as new debug path, switch to pause and execute activity
                        if (scope.ExecutingActivityPath.Count + 1 == debugNewExecutionPath.Length)
                        {
                            ChangeExecutionState(MTFSequenceExecutionState.Pause);
                        }
                        ExecuteActivity(activity, scope);
                    }
                }
            }
            if (executionState == MTFSequenceExecutionState.Aborting || executionState == MTFSequenceExecutionState.AborSubSequence)
            {
                StopExecution(MTFSequenceExecutionState.Aborted);
            }
            else if (executionState == MTFSequenceExecutionState.DebugGoToTopPosition)
            {
                ChangeExecutionState(MTFSequenceExecutionState.DebugGoToNewPosition);
                scope.ExecutingActivityPath.Clear();
                ExecuteAllActivities(scope);
            }
            else if (executionState == MTFSequenceExecutionState.Stopping)
            {
                StopExecution(MTFSequenceExecutionState.Stopped);
            }
            else
            {
                StopExecution(MTFSequenceExecutionState.Finished);
            }
        }

        private bool ActivityPathStartWith(IEnumerable<Guid> path1, IEnumerable<Guid> path2)
        {
            var enumerator1 = path1.GetEnumerator();
            var enumerator2 = path2.GetEnumerator();

            while (enumerator1.MoveNext() && enumerator2.MoveNext())
            {
                if (enumerator1.Current != enumerator2.Current)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsSequencePass(Guid? dutId, bool includeValidationTables)
        {
            var status = true;
            if (includeValidationTables)
            {
                status = variablesHandler.ValidationTables(dutId).All(v => v.Status != MTFValidationTableStatus.Nok);
            }
            return !activityErrorsHandler.HasError(dutId) && status;
        }

        private void SetSequenceStatusForAllDuts(double duration, StatusLinesFontSize fontSize)
        {
            if (sequence.DeviceUnderTestInfos == null || sequence.DeviceUnderTestInfos.Count == 0)
            {
                SetSequenceStatus(duration, fontSize, null, true);
                return;
            }

            foreach (var dut in sequence.DeviceUnderTestInfos)
            {
                SetSequenceStatus(duration, fontSize, dut.Id, true);
            }
        }

        private void SetSequenceStatus(double duration, StatusLinesFontSize fontSize, Guid? dutId, bool includeValidationTables)
        {
            var status = IsSequencePass(dutId, includeValidationTables);
            var sequenceStatus = status ? BaseConstants.ExecutionStatusOk : BaseConstants.ExecutionStatusNok;

            reportManager.SetSequenceStatus(sequenceStatus, dutId);
            
            string line2 = $"{LanguageHelper.GetString("Execution_Status_Duration")} {duration / 1000:F}";
            string line3 = string.Empty;

            RaiseSequenceStatusMessage(sequenceStatus, line2, line3, fontSize,dutId);
        }

        private void StopExecution(MTFSequenceExecutionState state)
        {
            lock (threadLock)
            {
                threadIdToActivityResult[Thread.CurrentThread.ManagedThreadId] = null;
            }

            foreach (Thread t in listActivitiesOnBackground)
            {
                t.Join();
            }

            goingToServiceMode = false;
            goingToTeachMode = false;
            if (sequenceRuntimeContext != null)
            {
                sequenceRuntimeContext.IsServiceModeActive = false;
                sequenceRuntimeContext.IsTeachModeActive = false;
            }

            foreach (var key in instances.Keys)
            {
                var instance = instances[key].Instance as IDisposable;

                if (instance != null)
                {
                    string instanceName = key.ToString();

                    try
                    {
                        instanceName = instance.GetType().FullName;
                        InjectRuntimeContext(instance, null);
                        instance.Dispose();
                    }
                    catch (Exception e)
                    {
                        SystemLog.LogMessage($"Disposing of {instanceName} crashed. See below.");
                        SystemLog.LogException(e);
                    }
                }
            }

            foreach (var staticType in staticTypes)
            {
                InjectRuntimeContextToStaticDrivers(staticType, null);
            }

            SetSequenceStatusForAllDuts(sequenceHandlingStopwatchMs(), new StatusLinesFontSize());

            reportManager.CreateLastReport();

            reportManager.Dispose();

            //sequenceResult.SequenceChangedByExecution = sequenceModifiedBySetupMode;

            //SaveSequenceIfModifiedBySetupMode();

            instances.Clear();
            staticTypes.Clear();

            lock (threadLock)
            {
                threadIdToActivityResult.Remove(Thread.CurrentThread.ManagedThreadId);
            }

            sequenceResult.ExecutionStop = DateTime.Now;
            ChangeExecutionState(state);

            executedActivities.Clear();
            cleanMemory();
            SaveGoldSampleList();

            lock (eventHandlingLock)
            {
                OnFinish?.Invoke(sequenceResult);
            }

            sequenceRuntimeContext?.Dispose();
            sequenceRuntimeContext = null;
        }

        public void SaveSequence(string modifiedByUser)
        {
            Task.Run(() =>
            {
                RaiseAllowSaveExecutedSequence(false);
            });

            sequenceResult.SequenceChangedByExecution = true;

            List<MTFSequence> sequencesToSave = new List<MTFSequence>();
            sequencesToSave.Add(this.sequence);
            if (this.sequence.ExternalSubSequences != null)
            {
                sequencesToSave.AddRange(sequence.ExternalSubSequences.Select(externalInfo => externalInfo.ExternalSequence));
            }

            this.SaveSequneceCallback(sequencesToSave, modifiedByUser);
        }

        private void cleanMemory()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        }

        private void CreateClassInstance(MTFSequenceClassInfo sequenceClassInfo)
        {
            if (!sequenceClassInfo.IsEnabled)
            {
                return;
            }

            if (sequenceClassInfo.MTFClass.IsStatic)
            {
                var mtfSequenceClassInfo = sequence.MTFSequenceClassInfos.FirstOrDefault(ci => ci.Alias == sequenceClassInfo.Alias);
                if (mtfSequenceClassInfo != null)
                {
                    staticTypes.Add(mtfSequenceClassInfo);
                    InjectRuntimeContextToStaticDrivers(mtfSequenceClassInfo, sequenceRuntimeContext);
                }

                return;
            }

            if (sequenceClassInfo.MTFClassInstanceConfiguration == null)
            {
                throw new Exception(sequenceClassInfo.Alias + " isn't mapped to functional library configuration.");
            }
            var classInstanceConfiurations = classInstanceConfigurationsCallBack(sequenceClassInfo.MTFClassInstanceConfiguration.ClassInfo);
            var currentConfiguration = classInstanceConfiurations.
                FirstOrDefault(cfg => cfg.Id == sequenceClassInfo.MTFClassInstanceConfiguration.Id);
            if (currentConfiguration == null)
            {
                throw new Exception(sequenceClassInfo.Alias + " isn't mapped to functional library configuration.");
            }

            object instance = null;
            if (instances.Values.Any(i => i.ClassInstanceConfigurationId == currentConfiguration.Id))
            {
                instance = instances.Values.First(i => i.ClassInstanceConfigurationId == currentConfiguration.Id).Instance;
            }

            if (instance == null)
            {
                instance = componentsManager.CreateInstance(currentConfiguration);

                InjectRuntimeContext(instance, sequenceRuntimeContext);
            }

            instances[sequenceClassInfo.Id] = new ClassInstance
            {
                Instance = instance,
                ClassInstanceConfigurationId = currentConfiguration.Id,
                ClassInfo = sequenceClassInfo.MTFClassInstanceConfiguration.ClassInfo,
                IsEnabled = sequenceClassInfo.IsEnabled
            };

            if (currentConfiguration.ClassInfo.ClientControlInfos != null && instance is IReceiveClientControlData)
            {
                foreach (var clientControlInfo in currentConfiguration.ClassInfo.ClientControlInfos)
                {
                    var key = string.Format("{0}.{1}", clientControlInfo.AssemblyName, clientControlInfo.TypeName);
                    if (!receiveClientControlDataInstances.ContainsKey(key))
                    {
                        receiveClientControlDataInstances[key] = new List<IReceiveClientControlData>();
                    }
                    receiveClientControlDataInstances[key].Add(instance as IReceiveClientControlData);
                }
            }
        }

        private void InjectRuntimeContextToStaticDrivers(MTFSequenceClassInfo classInfo, IMTFSequenceRuntimeContext runtimeContext)
        {
            Type mtfClass;
            try
            {
                mtfClass = componentsManager.GetType(classInfo.MTFClass);
            }
            catch (Exception)
            {
                throw new Exception($"Static class {classInfo.MTFClass.FullName} from assembly {classInfo.MTFClass.AssemblyName} has not been found. Check if driver is installed properly.");
            }

            if (mtfClass != null)
            {
                foreach (
                    var property in
                        mtfClass.GetProperties().Where(p => p.PropertyType == typeof(IMTFSequenceRuntimeContext) && p.CanWrite))
                {
                    try
                    {
                        property.GetSetMethod().Invoke(mtfClass, new object[] { runtimeContext });
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogException(ex);
                    }
                }

                foreach (var field in mtfClass.GetFields().Where(f => f.FieldType == typeof(IMTFSequenceRuntimeContext)))
                {
                    field.SetValue(mtfClass, runtimeContext);
                }
            }
        }

        private void InjectRuntimeContext(object instance, IMTFSequenceRuntimeContext runtimeContext)
        {
            //try find property of IMTFSequenceRuntimeContext
            foreach (
                var property in instance.GetType().GetProperties().Where(p => p.PropertyType == typeof(IMTFSequenceRuntimeContext) && p.CanWrite)
                )
            {
                property.GetSetMethod().Invoke(instance, new object[] { runtimeContext });
            }

            //try find filed of IMTFSequenceRuntimeContext
            foreach (var field in instance.GetType().GetFields().Where(f => f.FieldType == typeof(IMTFSequenceRuntimeContext)))
            {
                field.SetValue(instance, runtimeContext);
            }
        }

        private readonly Dictionary<int, MTFActivityResult> threadIdToActivityResult = new Dictionary<int, MTFActivityResult>();
        private bool breakOnNextActivity;
        private int breakOnActivityLevel;
        private List<Guid> currentBreakPointPath;
        private readonly object threadLock = new object();

        private MTFActivityResult ExecuteActivity(MTFSequenceActivity activity, ScopeData scope)
        {
            var timeStamp = DateTime.Now;
            if (sequenceRuntimeContext.IsDebugModeActive && activity.IsActive && executionState != MTFSequenceExecutionState.DebugGoToNewPosition && executionState != MTFSequenceExecutionState.DebugGoToTopPosition)
            {
                string activityExecutingPath = string.Join(".", scope.ExecutingActivityPath) + '.' + activity.Id;
                if (breakOnNextActivity || breakOnActivityLevel >= scope.ExecutingActivityPath.Count ||
                    sequenceRuntimeContext.BreakPointActivityPaths.Any(b => string.Join(".", b).Equals(activityExecutingPath)))
                {
                    breakOnNextActivity = false;
                    breakOnActivityLevel = 0;
                    currentBreakPointPath = new List<Guid>(scope.ExecutingActivityPath);
                    currentBreakPointPath.Add(activity.Id);
                    raiseOnActivityChanged(currentBreakPointPath.ToArray(), activity, timeStamp, scope);

                    ChangeExecutionState(MTFSequenceExecutionState.Pause);
                }
            }

            if (executionState == MTFSequenceExecutionState.DebugGoToNewPosition)
            {
                scope.ExecutingActivityPath.Add(activity.Id);
                bool pathStartsWithNewPath = ActivityPathStartWith(scope.ExecutingActivityPath, debugNewExecutionPath);
                scope.ExecutingActivityPath.Remove(activity.Id);

                if (pathStartsWithNewPath)
                {
                    //if execution activity path is same as new debug path, switch to pause and execute activity
                    if (scope.ExecutingActivityPath.Count + 1 == debugNewExecutionPath.Length)
                    {
                        ChangeExecutionState(MTFSequenceExecutionState.Pause);
                    }
                }
                else
                {
                    return null;
                }
            }

            if (executionState == MTFSequenceExecutionState.Pause)
            {
                var activityPath = new List<Guid>(scope.ExecutingActivityPath);
                activityPath.Add(activity.Id);
                raiseOnActivityChanged(activityPath.ToArray(), activity, timeStamp, scope);
                while (executionState == MTFSequenceExecutionState.Pause)
                {
                    Thread.Sleep(500);
                }
            }
            currentBreakPointPath = null;

            if (executionState == MTFSequenceExecutionState.Stopping ||
                executionState == MTFSequenceExecutionState.AborSubSequence ||
                executionState == MTFSequenceExecutionState.Aborting ||
                executionState == MTFSequenceExecutionState.DebugGoToTopPosition)
            {
                return null;
            }

            if (!activity.IsActive)
            {
                return null;
            }

            lock (executedActivitesLock)
            {
                if (activity.RunOnce && executedActivities.Contains(activity.Id))
                {
                    return null;
                }
            }

            MTFSubSequenceActivity subActivity = activity as MTFSubSequenceActivity;
            if (subActivity != null)
            {
                scope.ExecutingActivityPath.Add(activity.Id);
                DeviceUnderTestInfo lastDUTInfo = null;
                if (subActivity.SwitchDUT)
                {
                    lastDUTInfo = scope.DeviceUnderTestInfo;
                    scope.DeviceUnderTestInfo = subActivity.SwitchToDeviceUnderTestInfo;
                }

                raiseOnActivityChanged(scope.ExecutingActivityPath.ToArray(), activity, timeStamp, scope);

                MTFActivityResult subSeqResult = new MTFActivityResult(subActivity);
                var result = executeSubActivity(subActivity, scope, subSeqResult);
                if (subActivity.SwitchDUT)
                {
                    scope.DeviceUnderTestInfo = lastDUTInfo;
                }
                scope.ExecutingActivityPath.Remove(activity.Id);
                return result;
            }


            MTFExecuteActivity executeActivity = activity as MTFExecuteActivity;
            if (executeActivity != null)
            {
                MTFActivityResult actResult = new MTFActivityResult(executeActivity);
                DateTime timestamp = DateTime.Now;
                actResult.TimestampMs = sequenceHandlingStopwatchMs();
                scope.ExecutingActivityPath.Add(activity.Id);
                raiseOnActivityChanged(scope.ExecutingActivityPath.ToArray(), activity, timeStamp, scope);
                var result = ExecuteMTFExecuteActivity(executeActivity, scope);
                actResult.ActivityIdPath = scope.ExecutingActivityPath.ToArray();
                
                if (executionState != MTFSequenceExecutionState.DebugGoToNewPosition &&
                    executionState != MTFSequenceExecutionState.DebugGoToTopPosition && result != null)
                {
                    actResult.Status = result.Status;
                    actResult.ExceptionMessage = result.ExceptionMessage;
                    actResult.CheckOutpuValueFailed = result.CheckOutpuValueFailed;
                    actResult.ExceptionOccured = result.ExceptionOccured;
                }
                actResult.ElapsedMs = (DateTime.Now - timestamp).TotalMilliseconds;
                raiseOnNewActivityresult(actResult, scope);
                scope.ExecutingActivityPath.Remove(activity.Id);
                return actResult;
            }

            MTFSequenceHandlingActivity sequenceHandlingActivity = activity as MTFSequenceHandlingActivity;
            if (sequenceHandlingActivity != null)
            {
                return ExecuteSequenceHandlingActivity(sequenceHandlingActivity, scope);
            }

            MTFErrorHandlingActivity errorHandlingActivity = activity as MTFErrorHandlingActivity;
            if (errorHandlingActivity != null)
            {
                return ExecuteErrorHandlingActivity(errorHandlingActivity, scope);
            }

            MTFLoggingActivity loggingActivity = activity as MTFLoggingActivity;
            if (loggingActivity != null)
            {
                return ExecuteLoggingActivity(loggingActivity, scope);
            }

            MTFSequenceMessageActivity sequenceMessageActivity = activity as MTFSequenceMessageActivity;
            if (sequenceMessageActivity != null)
            {
                return ExecuteSequenceMessageActivity(sequenceMessageActivity, scope);
            }

            MTFVariableActivity variableActivity = activity as MTFVariableActivity;
            if (variableActivity != null)
            {
                return ExecuteVariableActivity(variableActivity, scope);
            }

            var fillValidationTableActivity = activity as MTFFillValidationTableActivity;
            if (fillValidationTableActivity != null)
            {
                return ExecuteFillValidationTableActivity(fillValidationTableActivity, scope);
            }



            ClassInstance currentClassInstance = null;
            createInstanceBeforeFirstStart = false;
            componentIdForCreation = Guid.Empty;
            if (instances.ContainsKey(activity.ClassInfo.Id))
            {
                currentClassInstance = instances[activity.ClassInfo.Id];
            }
            else
            {
                if (componentsMapping.ContainsKey(activity.ClassInfo.Id))
                {
                    var mainSequenceComponentId = GetMainSequenceComponentIdFromMapping(activity.ClassInfo.Id);
                    if (instances.ContainsKey(mainSequenceComponentId))
                    {
                        currentClassInstance = instances[mainSequenceComponentId];
                    }
                    else
                    {
                        createInstanceBeforeFirstStart = true;
                        componentIdForCreation = mainSequenceComponentId;
                    }
                }
                else if (!activity.ClassInfo.MTFClass.IsAbstract)
                {
                    createInstanceBeforeFirstStart = true;
                    componentIdForCreation = activity.ClassInfo.Id;
                }
            }

            if (createInstanceBeforeFirstStart)
            {
                var classInfo = sequence.MTFSequenceClassInfos.FirstOrDefault(cI => cI.Id == componentIdForCreation);
                if (classInfo != null)
                {
                    if (classInfo.IsEnabled)
                    {
                        CreateClassInstance(classInfo);
                        currentClassInstance = instances[classInfo.Id];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    throw new Exception("Missing mapping for component. Please check your sequence.");
                }
            }

            if (currentClassInstance != null && !currentClassInstance.IsEnabled)
            {
                return null;
            }
            var currentInstance = currentClassInstance != null ? currentClassInstance.Instance : null;

            scope.ExecutingActivityPath.Add(activity.Id);
            raiseOnActivityChanged(scope.ExecutingActivityPath.ToArray(), activity, timeStamp, scope);

            lock (threadLock)
            {
                sequenceRuntimeContext.ThreadIdToScopeData[Thread.CurrentThread.ManagedThreadId] = scope;
                sequenceRuntimeContext.ThreadIdToActivity[Thread.CurrentThread.ManagedThreadId] = activity;
            }

            MTFActivityResult activityResult = new MTFActivityResult(activity);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            activityResult.TimestampMs = sequenceHandlingStopwatchMs();

            object[] parameters = evaluateParameters(activity.MTFParameters, activity, activityResult, scope);
            //if setup mode is active for current method call, replace parameters by params from helper
            bool doSetupMode;
            lock (setupModeActivityPathsLock)
            {
                doSetupMode = sequenceRuntimeContext.IsServiceExecutionAllowed || sequenceRuntimeContext.IsSetupModeActive && sequenceRuntimeContext.SetupModeActivityPaths.Any(path => activityPathEqual(path, scope.ExecutingActivityPath));
            }
            if (doSetupMode)
            {
                setupActivityParameters(activity, parameters, currentInstance, scope);
                if (setupVariantCanceled)
                {
                    scope.ExecutingActivityPath.Remove(activity.Id);
                    return null;
                }
            }

            activityResult.ActivityIdPath = scope.ExecutingActivityPath.ToArray();
            activityResult.Status = MTFExecutionActivityStatus.Ok;

            MethodInfo method = getMethod(currentInstance, activity, ref activityResult, scope);
            if (method == null)
            {
                Abort();
                return null;
            }

            //save parameter values to result
            activityResult.MTFParameters = new ObservableCollection<MTFParameterValue>();
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    try
                    {
                        var param = parameters[i];
                        if (param != null && componentsManager.AvailableClasses.Any(c => c.FullName == param.GetType().FullName))
                        {
                            param = new GenericClassInstanceConfiguration(param);
                        }
                        activityResult.MTFParameters.Add(evaluateParameterValue(activity.MTFParameters[i], param, activity, activityResult, scope));
                    }
                    catch (Exception e)
                    {
                        var ex = new Exception(string.Format("Exception occured during saving input parameter ({0}){1} current value.", activity.MTFParameters[i].TypeName, activity.MTFParameters[i].Name), e);
                        SystemLog.LogException(ex);
                        throw ex;
                    }
                }
            }
            else
            {
                activityResult.MTFParameters = activity.MTFParameters;
            }

            lock (threadLock)
            {
                threadIdToActivityResult[Thread.CurrentThread.ManagedThreadId] = activityResult;
            }

            //clean lazy load cache
            cleanLazyLoad(scope.ExecutingActivityPahtAsString);

            executeMethod(activity, currentInstance, parameters, method, activityResult, scope);

            try
            {
                if (!string.IsNullOrEmpty(activityResult.ExceptionMessage))
                {
                    if (activity.ErrorOutput != null)
                    {
                        var variable = variablesHandler.GetVariable(activity.ErrorOutput.Id);
                        if (variable != null && variable.TypeName == typeof(string).FullName)
                        {
                            string msg = string.Format("{0}{1}{2}", variablesHandler.GetVariableValue(activity.ErrorOutput.Id, scope.DeviceUnderTestInfo?.Id), activityResult.ExceptionMessage, Environment.NewLine);
                            variablesHandler.SetVariableValue(activity.ErrorOutput.Id, msg, scope.DeviceUnderTestInfo?.Id);
                            var result = new MTFVariableActivityResult
                            {
                                VariableId = variable.Id,
                                Value = msg,
                                ActivityIdPath = scope.ExecutingActivityPath.ToArray()
                            };
                            RaiseActivityResult(result);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var ex = new Exception(string.Format("Exception occured by checking activity {0} exception message", activityResult.ActivityName), e);
                SystemLog.LogException(ex);
                throw ex;
            }

            stopwatch.Stop();
            lock (threadLock)
            {
                sequenceRuntimeContext.ThreadIdToScopeData.Remove(Thread.CurrentThread.ManagedThreadId);
                threadIdToActivityResult.Remove(Thread.CurrentThread.ManagedThreadId);
                sequenceRuntimeContext.ThreadIdToActivity.Remove(Thread.CurrentThread.ManagedThreadId);
            }

            activityResult.ElapsedMs = stopwatch.Elapsed.TotalMilliseconds;

            //lazy load input parameters
            if (activityResult.MTFParameters != null)
            {
                foreach (var param in activityResult.MTFParameters)
                {
                    if (param != null && param.Value is GenericClassInstanceConfiguration)
                    {
                        addToLazyLoad((MTFDataTransferObject)param.Value, scope.ExecutingActivityPahtAsString);
                    }
                }
            }

            raiseOnNewActivityresult(activityResult, scope);

            lock (executedActivitesLock)
            {
                if (activity.RunOnce && !executedActivities.Contains(activity.Id))
                {
                    executedActivities.Add(activity.Id);
                }
            }

            scope.ExecutingActivityPath.Remove(activity.Id);

            return activityResult;
        }

        private void addToLazyLoad(MTFDataTransferObject obj, string activityPath)
        {
            lock (lastActivityResultLock)
            {
                obj.IsLazyLoad = true;
                if (!idsOfLazyLoadObjects.ContainsKey(activityPath))
                {
                    idsOfLazyLoadObjects[activityPath] = new List<Guid>();
                }
                idsOfLazyLoadObjects[activityPath].Add(obj.Id);
            }
        }

        private void cleanLazyLoad(string activityPath)
        {
            //clean lazy load cache
            lock (lastActivityResultLock)
            {
                if (idsOfLazyLoadObjects.ContainsKey(activityPath))
                {
                    idsOfLazyLoadObjects[activityPath].ForEach(MTFDataTransferObject.RemoveFromLazyLoadCache);
                    if (idsOfLazyLoadObjects[activityPath].Count == 0)
                    {
                        idsOfLazyLoadObjects.Remove(activityPath);
                    }
                }
            }
        }

        private bool setupVariantRequested;
        private bool setupVariantCanceled;
        private IEnumerable<SetupVariantSelectionResult> variantSelectionResults;
        private void setupActivityParameters(MTFSequenceActivity activity, object[] parameters, object currentInstance, ScopeData scope)
        {
            handleSetupVariant(activity, scope);
            if (setupVariantCanceled || parameters == null)
            {
                return;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (!string.IsNullOrEmpty(activity.MTFParameters[i].SetupMethodName))
                {
                    parameters[i] = getParamBySetupMethod(activity.MTFParameters[i], currentInstance, parameters[i]);

                    if (parameters[i] == null)
                    {
                        activity.MTFParameters[i].Value = null;
                    }
                    else
                    {
                        MTFClientServerCommon.Helpers.TypeInfo typeInfo =
                            new MTFClientServerCommon.Helpers.TypeInfo(activity.MTFParameters[i].TypeName);
                        if (typeInfo.IsSimpleType)
                        {
                            activity.MTFParameters[i].Value = new ConstantTerm { Value = parameters[i] };
                        }
                        else
                        {
                            var newParamValue = new TermWrapper { Value = new GenericClassInstanceConfiguration(parameters[i]) };
                            var oldParamValue = activity.MTFParameters[i].Value as TermWrapper;
                            if (oldParamValue == null)
                            {
                                oldParamValue = new TermWrapper();
                                activity.MTFParameters[i].Value = oldParamValue;
                            }
                            if (oldParamValue.Value == null)
                            {
                                oldParamValue.Value = newParamValue.Value;
                            }
                            else
                            {
                                //copy back all non simple objects like activity results, variables ... - all terms, but don't replace activity result term
                                updateTermWrapperValue(oldParamValue.Value as GenericClassInstanceConfiguration,
                                    newParamValue.Value as GenericClassInstanceConfiguration);
                            }
                        }
                    }

                    RaiseAllowSaveExecutedSequence(true);
                }
            }
        }

        private void handleSetupVariant(MTFSequenceActivity activity, ScopeData scope)
        {
            setupVariantCanceled = false;
            if (activity.UsedDataNames == null || !activity.UsedDataNames.Any() || OnShowSetupVariantSelection == null)
            {
                return;
            }

            var classInfo = activity.ClassInfo;
            //if activity is form external sequence, get maped class info
            if (componentsMapping.ContainsKey(classInfo.Id))
            {
                classInfo = sequence.MTFSequenceClassInfos.FirstOrDefault(ci => ci.Id == componentsMapping[classInfo.Id]);
            }

            Dictionary<string, IEnumerable<SequenceVariant>> usedData = new Dictionary<string, IEnumerable<SequenceVariant>>();
            var variantSelectionResultsByActualVariant = new List<SetupVariantSelectionResult>();
            foreach (var activityDataName in activity.UsedDataNames)
            {
                usedData[activityDataName] = null;
            }

            if (classInfo != null && classInfo.Data != null)
            {
                foreach (var dataName in activity.UsedDataNames)
                {
                    if (classInfo.Data.ContainsKey(dataName))
                    {
                        usedData[dataName] = classInfo.Data[dataName].Select(d => d.SequenceVariant).Where(sv => sv != null).OrderBy(d => d);
                    }
                }
            }

            if (sequenceRuntimeContext.IsServiceModeActive)
            {
                foreach (var dataName in activity.UsedDataNames)
                {
                    SequenceVariant bestVariantSelector = null;

                    if (usedData[dataName] != null)
                    {
                        //exception switched off because of PAVEL FEXA
                        //throw new Exception("Variant isn't specified in sequence. Please run Teach mode first.");
                        bestVariantSelector = scope.SequenceVariant.GetBestMatch(usedData[dataName]);
                    }
                    //if (bestVariantSelector == null)
                    //{
                    //    throw new Exception("Variant isn't specified in sequence. Please run Teach mode first.");
                    //}

                    variantSelectionResultsByActualVariant.Add(new SetupVariantSelectionResult { DataName = dataName, SaveVariant = bestVariantSelector, UseVariant = bestVariantSelector });

                }
                variantSelectionResults = variantSelectionResultsByActualVariant;
                return;
            }

            setupVariantRequested = false;
            variantSelectionResults = null;

            var extendetUsedDataNames = new Dictionary<string, string>();
            if (activity.MTFParameters != null)
            {
                foreach (var param in activity.MTFParameters.Where(p => !string.IsNullOrEmpty(p.DataNameExtension)))
                {
                    if (param.Value != null)
                    {
                        extendetUsedDataNames[param.DataNameExtension] = evaluateParameterValue(param.Value, activity, new MTFActivityResult(), scope).ToString();
                    }
                }
            }

            OnShowSetupVariantSelection(activity.ActivityName, usedData, extendetUsedDataNames);
            while (!setupVariantRequested)
            {
                Thread.Sleep(500);
            }
        }

        private Guid GetMainSequenceComponentIdFromMapping(Guid guid)
        {
            var g = guid;
            if (componentsMapping.ContainsKey(guid))
            {
                var parentGuid = componentsMapping[guid];
                g = GetMainSequenceComponentIdFromMapping(parentGuid);
            }
            return g;
        }

        private void updateTermWrapperValue(GenericClassInstanceConfiguration oldValue, GenericClassInstanceConfiguration newValue)
        {
            if (oldValue != null && newValue != null)
            {
                for (int i = 0; i < oldValue.PropertyValues.Count; i++)
                {
                    var oldPropertyValue = oldValue.PropertyValues[i];
                    var newPropertyValue = newValue.PropertyValues[i];

                    if (oldPropertyValue.Value is GenericClassInstanceConfiguration)
                    {
                        updateTermWrapperValue(oldPropertyValue.Value as GenericClassInstanceConfiguration, newPropertyValue.Value as GenericClassInstanceConfiguration);
                    }
                    else if (oldPropertyValue.Value is ConstantTerm)
                    {
                        oldPropertyValue.Value = new ConstantTerm { Value = newPropertyValue.Value };
                    }
                    else if (!(oldPropertyValue.Value is Term))
                    {
                        oldPropertyValue.Value = newPropertyValue.Value;
                    }
                }
            }
        }

        private object getParamBySetupMethod(MTFParameterValue parameterValue, object instance, object originValue)
        {
            var setupMethod = instance.GetType().GetMethod(parameterValue.SetupMethodName);

            if (setupMethod.ReturnType.FullName != parameterValue.TypeName)
            {
                throw new Exception(string.Format("Setup mehtod {0} on parameter {1} has unexpected output type. Output type should be {2}.", parameterValue.SetupMethodName, parameterValue.DisplayName, parameterValue.TypeName));
            }

            var setupMethodParameters = setupMethod.GetParameters();
            if (setupMethodParameters.Length > 1)
            {
                throw new Exception(string.Format("Setup mehtod {0} has too much parameters.", parameterValue.SetupMethodName));
            }

            if (setupMethodParameters.Length == 1 && setupMethodParameters[0].ParameterType.FullName != parameterValue.TypeName)
            {
                throw new Exception(string.Format("Parameter {0} on setup mehtod {1} has unexpected type. Type should be {2}.", setupMethodParameters[0].Name, parameterValue.SetupMethodName, parameterValue.TypeName));
            }

            object[] param = null;
            if (setupMethodParameters.Length == 1)
            {
                param = new[] { originValue };
            }

            try
            {
                return setupMethod.Invoke(instance, param);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }
                throw;
            }
        }

        private bool activityPathEqual(List<Guid> path1, List<Guid> path2)
        {
            if (path1.Count != path2.Count)
            {
                return false;
            }

            for (int i = 0; i < path1.Count; i++)
            {
                if (path1[i] != path2[i])
                {
                    return false;
                }
            }
            return true;
        }

        private MTFParameterValue evaluateParameterValue(MTFParameterValue parameterValue, object evaluatedValue, MTFSequenceActivity currentActivity, MTFActivityResult activityResult, ScopeData scope)
        {
            var evaluatedParam = evaluateParameterValue(evaluatedValue, currentActivity, activityResult, scope);
            if (evaluatedParam == null)
            {
                return null;
            }
            
            MTFParameterValue newParameterValue = createCopy(parameterValue);
            newParameterValue.Value = prepareGenericClassInstanceInfo(evaluatedParam);

            return newParameterValue;
        }

        private object evaluateParameterValue(object val, MTFSequenceActivity currentActivity, MTFActivityResult activityResult, ScopeData scope)
        {
            if (val == null)
            {
                return null;
            }

            MTFClientServerCommon.Helpers.TypeInfo typeInfo = new MTFClientServerCommon.Helpers.TypeInfo(val.GetType().FullName);

            if (val is GenericClassInstanceConfiguration)
            {
                return evaluateGenericClassInstanceInfo((GenericClassInstanceConfiguration)val, currentActivity, activityResult, scope);
            }
            if (typeInfo.IsTerm)
            {
                return evaluateTerm(val as Term, currentActivity, activityResult, scope);
            }
            if (typeInfo.IsArrayOfSimpleType)
            {
                return val;
            }
            if (typeInfo.IsArray)
            {
                Array array = val as Array;
                if (array == null)
                {
                    return null;
                }

                Array newArray;
                if (typeInfo.Type != null)
                {
                    newArray = Array.CreateInstance(typeInfo.Type.GetElementType(), array.Length);
                    for (int i = 0; i < array.Length; i++)
                    {
                        newArray.SetValue(evaluateParameterValue(array.GetValue(i), currentActivity, activityResult, scope), i);
                    }
                }
                else
                {
                    newArray = new string[array.Length];
                    for (int i = 0; i < array.Length; i++)
                    {
                        newArray.SetValue(evaluateParameterValue(array.GetValue(i), currentActivity, activityResult, scope).ToString(), i);
                    }
                }

                return newArray;
            }
            else if (typeInfo.IsGenericType)
            {
                IList list = val as IList;
                if (list == null)
                {
                    return null;
                }

                IList newList = Activator.CreateInstance(val.GetType()) as IList;
                var listItemType = newList.GetType().GetGenericArguments()[0];
                bool isTermItem = listItemType == typeof(Term);
                foreach (var v in list)
                {
                    if (isTermItem)
                    {
                        newList.Add(new ConstantTerm { Value = evaluateParameterValue(v, currentActivity, activityResult, scope) });
                    }
                    else
                    {
                        newList.Add(evaluateParameterValue(v, currentActivity, activityResult, scope));
                    }
                }

                return newList;
            }

            return val;
        }


        private MTFParameterValue createCopy(MTFParameterValue param)
        {
            return new MTFParameterValue
            {
                AllowedValues = CreateCopy(param.AllowedValues),
                Description = param.Description,
                DisplayName = param.DisplayName,
                Name = param.Name,
                TypeName = param.TypeName,
                ValueListName = param.ValueListName,
                ValueListLevel = param.ValueListLevel,
                ValueListParentName = param.ValueListParentName,
                ValueName = param.ValueName,
            };
        }

        private GenericParameterValue createCopy(GenericParameterValue param)
        {
            return new GenericParameterValue
            {
                AllowedValues = CreateCopy(param.AllowedValues),
                DisplayName = param.DisplayName,
                Name = param.Name,
                TypeName = param.TypeName,
            };
        }

        private MTFObservableCollection<MTFAllowedValue> CreateCopy(MTFObservableCollection<MTFAllowedValue> source)
        {
            MTFObservableCollection<MTFAllowedValue> output = null;
            if (source != null)
            {
                output = new MTFObservableCollection<MTFAllowedValue>();
                foreach (var item in source)
                {
                    output.Add(item.Clone() as MTFAllowedValue);
                }
            }
            return output;
        }

        private GenericClassInstanceConfiguration evaluateGenericClassInstanceInfo(GenericClassInstanceConfiguration originValue, MTFSequenceActivity currentActivity, MTFActivityResult activityResult, ScopeData scope)
        {
            GenericClassInstanceConfiguration newValue = new GenericClassInstanceConfiguration
            {
                ClassInfo = (GenericClassInfo)originValue.ClassInfo.Copy(),
                PropertyValues = new ObservableCollection<GenericParameterValue>(),
            };

            foreach (GenericParameterValue propertyValue in originValue.PropertyValues)
            {
                GenericParameterValue newParameterValue = createCopy(propertyValue);
                newParameterValue.Value = evaluateParameterValue(propertyValue.Value, currentActivity, activityResult, scope);

                newValue.PropertyValues.Add(newParameterValue);
            }

            return newValue;
        }

        private object[] evaluateParameters(IEnumerable<MTFParameterValue> values, MTFSequenceActivity currentActivity, MTFActivityResult activityResult, ScopeData scope)
        {
            object[] parameters;
            try
            {
                if (values == null)
                {
                    return null;
                }

                parameters = new object[values.Count()];
                int i = 0;
                foreach (var val in values)
                {
                    try
                    {
                        if (val.Value is Term)
                        {
                            MTFSimpleParameterValue simpleParam = new MTFSimpleParameterValue(val.TypeName, evaluateTerm((Term)val.Value, currentActivity, activityResult, scope));
                            parameters[i] = componentsManager.Cast(simpleParam, currentActivity, activityResult, evaluateTerm, scope);
                        }
                        else
                        {
                            parameters[i] = componentsManager.Cast(val, currentActivity, activityResult, (valToFix, activity, res, scopeInternal) => { return evaluateTerm(valToFix, activity, res, scopeInternal); }, scope);
                        }
                        i++;
                    }
                    catch (Exception e)
                    {
                        var ex = new Exception(string.Format("Exception occured by processing parameter {0} of {1} type.", val.Name, val.TypeName), e);
                        SystemLog.LogException(ex);
                        if (asyncExecution)
                        {
                            RaiseOnError(DateTime.Now, ex.Message, StatusMessage.MessageType.Error, currentActivity, activityResult.ActivityIdPath.ToList());
                            StopExecution(MTFSequenceExecutionState.Aborted);
                            return null;
                        }
                        throw ex;
                    }
                }
            }
            catch (Exception e)
            {
                var ex = new Exception(string.Format("Evaluation of parameters for activity {0} failed.", currentActivity.ActivityName), e);
                SystemLog.LogException(ex);
                if (asyncExecution)
                {
                    RaiseOnError(DateTime.Now, ex.Message, StatusMessage.MessageType.Error, currentActivity, activityResult.ActivityIdPath.ToList());
                    StopExecution(MTFSequenceExecutionState.Aborted);
                    return null;
                }
                throw ex;
            }

            return parameters;
        }

        private MTFActivityResult ExecuteSequenceMessageActivity(MTFSequenceMessageActivity sequenceMessageActivity, ScopeData scope)
        {
            DateTime timestamp = DateTime.Now;
            MTFActivityResult activityResult = new MTFMessageActivityResult(sequenceMessageActivity);
            activityResult.TimestampMs = sequenceHandlingStopwatchMs();
            scope.ExecutingActivityPath.Add(sequenceMessageActivity.Id);
            raiseOnActivityChanged(scope.ExecutingActivityPath.ToArray(), sequenceMessageActivity, timestamp, scope);

            if (sequenceMessageActivity.MessageType == SequenceMessageType.Close)
            {
                if (OnCloseMessage != null)
                {
                    var output = false;
                    if (sequenceMessageActivity.ActivitySource != null)
                    {
                        OnCloseMessage(sequenceMessageActivity.ActivitySource.Id);
                        output = true;
                    }
                    else
                    {
                        RaiseOnError(DateTime.Now, "Source activity is not defined. Please check your sequence.", StatusMessage.MessageType.Error,
                            sequenceMessageActivity, scope.ExecutingActivityPath);
                    }

                    lock (lastActivityResultLock)
                    {
                        scope.SetActivityResult(sequenceMessageActivity.Id, output);
                        activityResult.ActivityIdPath = scope.ExecutingActivityPath.ToArray();
                        activityResult.Status = output ? MTFExecutionActivityStatus.Ok : MTFExecutionActivityStatus.Nok;
                        activityResult.ActivityResult = output;
                        activityResult.ActivityResultTypeName = sequenceMessageActivity.ReturnType;
                    }
                }
            }
            else if (OnShowMessage != null)
            {
                var messageInfo = new MessageInfo
                                  {
                                      Type = sequenceMessageActivity.MessageType,
                                      Buttons = GetMessageButtonsType(sequenceMessageActivity),
                                      ChoiceDisplayType = sequenceMessageActivity.DisplayType,
                                      ButtonValues = GetButtonValues(sequenceMessageActivity.Values, sequenceMessageActivity, scope),
                                      ActivityPath = scope.ExecutingActivityPath,
                                      AdditionalType = sequenceMessageActivity.NoBlockingMessageBoxType,
                                      IsFullScreen = sequenceMessageActivity.IsFullScreen,
                                  };

                if (sequenceMessageActivity.Header != null)
                {
                    object headerO = evaluateStringFormat(sequenceMessageActivity.Header, sequenceMessageActivity, activityResult, scope);
                    messageInfo.Header = headerO == null ? string.Empty : headerO.ToString();
                }

                if (sequenceMessageActivity.Message != null)
                {
                    object textO = evaluateStringFormat(sequenceMessageActivity.Message, sequenceMessageActivity, activityResult, scope);
                    messageInfo.Text = textO == null ? string.Empty : textO.ToString();
                }

                if (sequenceMessageActivity.MessageType == SequenceMessageType.Picture || sequenceMessageActivity.MessageType == SequenceMessageType.NoBlockingMessage)
                {
                    if (!sequenceMessageActivity.IsUseAbsolutePath)
                    {
                        if (sequenceMessageActivity.Image != null && !(sequenceMessageActivity.Image is EmptyTerm))
                        {
                            var image = evaluateTerm(sequenceMessageActivity.Image, sequenceMessageActivity, activityResult, scope) as MTFImage;
                            if (image != null)
                            {
                                messageInfo.ImageData = image.ImageData;
                            }
                            else
                            {
                                RaiseOnError(DateTime.Now, "Input image is not valid MTFImage", StatusMessage.MessageType.Error, sequenceMessageActivity, scope.ExecutingActivityPath);
                            }

                        }
                    }
                    else
                    {
                        if (File.Exists(sequenceMessageActivity.PathToImage) && (BaseConstants.ImageExtensions.Contains(Path.GetExtension(sequenceMessageActivity.PathToImage))
                             || BaseConstants.ImageExtensions.Contains(Path.GetExtension(sequenceMessageActivity.PathToImage.ToLower()))))
                        {
                            messageInfo.ImageData = imageToByteArray(new System.Drawing.Bitmap(sequenceMessageActivity.PathToImage));
                        }
                        else
                        {
                            RaiseOnError(DateTime.Now, "Input image is not valid MTFImage", StatusMessage.MessageType.Error, sequenceMessageActivity, scope.ExecutingActivityPath);
                        }
                    }
                }

                messageBoxResultReceived = false;

                OnShowMessage(messageInfo);

                if (sequenceMessageActivity.MessageType != SequenceMessageType.NoBlockingMessage)
                {
                    while (!messageBoxResultReceived)
                    {
                        Thread.Sleep(500);
                    }
                }
                else
                {
                    messageBoxResult = null;
                }

                lock (lastActivityResultLock)
                {
                    object output = null;
                    if (messageBoxResult != null)
                    {
                        if (messageBoxResult.Result == MTFDialogResultEnum.TextResult)
                        {
                            output = messageBoxResult.TextResult;
                        }
                        else
                        {
                            output = messageBoxResult.Result == MTFDialogResultEnum.Ok || messageBoxResult.Result == MTFDialogResultEnum.Yes;
                        }
                    }
                    scope.SetActivityResult(sequenceMessageActivity.Id, output);
                    activityResult.ActivityIdPath = scope.ExecutingActivityPath.ToArray();
                    activityResult.Status = MTFExecutionActivityStatus.Ok;
                    activityResult.ActivityResult = output;
                    activityResult.ActivityResultTypeName = sequenceMessageActivity.ReturnType;
                    ((MTFMessageActivityResult)activityResult).DisplayedMessage = messageInfo.Text;
                    ((MTFMessageActivityResult)activityResult).Header = messageInfo.Header;

                    var checkOutputValueResult = checkOutputValue(sequenceMessageActivity, output, activityResult, scope);
                    if (!string.IsNullOrEmpty(checkOutputValueResult))
                    {
                        handleCheckOutputValueBehavior(sequenceMessageActivity, null, checkOutputValueResult, ref activityResult, scope);
                    }
                    activityResult.ElapsedMs = (DateTime.Now - timestamp).TotalMilliseconds;
                }
            }

            raiseOnNewActivityresult(activityResult, scope);

            scope.ExecutingActivityPath.Remove(sequenceMessageActivity.Id);

            return activityResult;
        }

        private List<string> GetButtonValues(object value, MTFSequenceMessageActivity sequenceMessageActivity, ScopeData scope)
        {
            if (value == null)
            {
                return null;
            }
            var collection = value as IList<Term>;
            if (collection != null)
            {
                return collection.Select(x =>
                                         {
                                             var item = evaluateTerm(x, scope);
                                             return item != null ? item.ToString() : null;
                                         }).ToList();
            }
            var term = value as Term;
            if (term != null)
            {
                var result = evaluateTerm(term, scope);
                var resultCollection = result as ICollection;
                if (resultCollection != null)
                {
                    return (from object item in resultCollection select item != null ? item.ToString() : null).ToList();
                }
                if (result != null)
                {
                    return new List<string> { result.ToString() };
                }
            }
            throw new Exception("Cannot create MessageBox items. Please check your sequence");
        }

        private byte[] imageToByteArray(System.Drawing.Bitmap image)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(image, typeof(byte[]));
        }

        private MessageButtons GetMessageButtonsType(MTFSequenceMessageActivity activity)
        {
            if (activity.MessageType == SequenceMessageType.Choice)
            {
                if (activity.DisplayType == SequenceMessageDisplayType.ComboBox || activity.DisplayType == SequenceMessageDisplayType.ToggleButtons)
                {
                    return activity.ShowCancel ? MessageButtons.OkCancel : MessageButtons.Ok;
                }
                return activity.ShowCancel ? MessageButtons.Cancel : MessageButtons.None;
            }
            if (activity.MessageType == SequenceMessageType.Input)
            {
                return activity.ShowCancel ? MessageButtons.OkCancel : MessageButtons.Ok;
            }

            switch (activity.Buttons)
            {
                case MessageActivityButtons.Ok:
                    return MessageButtons.Ok;
                case MessageActivityButtons.OkCancel:
                    return MessageButtons.OkCancel;
                case MessageActivityButtons.YesNo:
                    return MessageButtons.YesNo;
                case MessageActivityButtons.None:
                    return MessageButtons.None;
            }
            return MessageButtons.Cancel;
        }

        private MTFActivityResult ExecuteVariableActivity(MTFVariableActivity variableActivity, ScopeData scope)
        {
            DateTime timestamp = DateTime.Now;
            scope.ExecutingActivityPath.Add(variableActivity.Id);
            raiseOnActivityChanged(scope.ExecutingActivityPath.ToArray(), variableActivity, timestamp, scope);

            var result = new MTFVariableActivityResult
            {
                ActivityId = variableActivity.Id,
                Status = MTFExecutionActivityStatus.Ok,
                ActivityIdPath = scope.ExecutingActivityPath.ToArray(),
                ActivityName = variableActivity.ActivityName,
                ElapsedMs = (DateTime.Now - timestamp).TotalMilliseconds,
                ExceptionMessage = string.Empty,

                VariableName = variableActivity.Variable.Name,
                VariableTypeName = variableActivity.Variable.TypeName,
                VariableId = variableActivity.Variable.Id,
            };

            if (variablesHandler.ContainsVariable(variableActivity.Variable.Id))
            {
                var newVariableValue = evaluateTerm(variableActivity.Value, variableActivity, result, scope);
                variablesHandler.SetVariableValue(variableActivity.Variable.Id, newVariableValue , scope.DeviceUnderTestInfo?.Id);
                result.Value = newVariableValue?.ToString();
            }
            else
            {
                throw new Exception(string.Format("Variable in activity {0} isn't defined. Please check your sequence.", variableActivity.ActivityName));
            }

            raiseOnNewActivityresult(result, scope);
            scope.ExecutingActivityPath.Remove(variableActivity.Id);

            return result;
        }

        private MTFActivityResult ExecuteFillValidationTableActivity(MTFFillValidationTableActivity activity, ScopeData scope)
        {
            DateTime timestamp = DateTime.Now;
            scope.ExecutingActivityPath.Add(activity.Id);
            raiseOnActivityChanged(scope.ExecutingActivityPath.ToArray(), activity, timestamp, scope);

            var result = new MTFActivityResult
            {
                ActivityId = activity.Id,
                Status = MTFExecutionActivityStatus.Ok,
                ActivityIdPath = scope.ExecutingActivityPath.ToArray(),
                ActivityName = activity.ActivityName,
                ElapsedMs = (DateTime.Now - timestamp).TotalMilliseconds,
                ExceptionMessage = string.Empty,
            };

            bool conditionResult;
            string errorMessage;   

            try
            {
                conditionResult = evaluateConditionTerm(activity.Term, activity, result, scope);
            }
            catch (Exception ex)
            {
                SystemLog.LogException(ex);
                errorMessage = activity.Term is ITermErrorHandling errorTerm
                    ? $"Your condition is incorrect. More info in SystemLog. Please check your sequence, {activity.Term}{Environment.NewLine}{errorTerm.ErrorText}"
                    : $"Your condition is incorrect. More info in SystemLog. Please check your sequence, {activity.Term}";

                activityErrorsHandler.AddError(scope.DeviceUnderTestInfo?.Id, CreateActivityError(timestamp, activity, errorMessage));

                throw new Exception(formatException(activity, errorMessage));
            }

            result.ActivityResult = conditionResult;

            if (!conditionResult)
            {
                errorMessage = activity.Term.ToString();
                handleCheckOutputValueBehavior(activity, null, errorMessage, result, true, true, scope);
            }

            raiseOnNewActivityresult(result, scope);
            scope.ExecutingActivityPath.Remove(activity.Id);
            return result;
        }

        private MTFActivityResult ExecuteDynamicActivity(MTFExecuteActivity dynamicActivity, ScopeData scope)
        {
            MTFActivityResult actResult = new MTFActivityResult(dynamicActivity);
            DateTime timestamp = DateTime.Now;
            actResult.TimestampMs = sequenceHandlingStopwatchMs();

            var sequenceName = evaluateStringFormat(dynamicActivity.DynamicSequence, dynamicActivity, actResult, scope);
            actResult.ActivityIdPath = scope.ExecutingActivityPath.ToArray();

            switch (dynamicActivity.DynamicActivityType)
            {
                case DynamicActivityTypes.Load:
                    LoadDynamicSequence(sequenceName, dynamicActivity, actResult);
                    break;
                case DynamicActivityTypes.Unload:
                    UnloadDynamicSequence(sequenceName, dynamicActivity, actResult);
                    break;
                case DynamicActivityTypes.Execute:
                    ExecuteDynamicSequence(sequenceName, dynamicActivity, actResult, actResult, scope);
                    break;
            }


            actResult.ElapsedMs = (DateTime.Now - timestamp).TotalMilliseconds;
            raiseOnNewActivityresult(actResult, scope);
            return actResult;
        }

        private void ExecuteDynamicSequence(string sequenceName, MTFExecuteActivity dynamicActivity, MTFActivityResult result, MTFActivityResult actResult, ScopeData scope)
        {
            actResult.SubActivityResults = new List<MTFActivityResult>();
            var dynamicSequence = dynamic.GetSequenceByName(sequenceName, Sequence.FullPath);
            if (dynamicSequence != null)
            {
                if (!string.IsNullOrEmpty(dynamicActivity.DynamicMethod.Text))
                {
                    bool subSequenceIsFound = false;
                    var methodName = evaluateStringFormat(dynamicActivity.DynamicMethod, dynamicActivity, result, scope);
                    if (dynamicSequence.ActivitiesByCall != null)
                    {
                        var subSequence =
                            dynamicSequence.ActivitiesByCall.FirstOrDefault(
                                x =>
                                    MTFSequenceActivityHelper.CombineTranslatedActivityName(
                                        SequenceLocalizationHelper.ActualDictionary.GetValue(x.ActivityName), x.UniqueIndexer) == methodName) as
                                MTFSubSequenceActivity;
                        if (subSequence != null)
                        {
                            subSequenceIsFound = true;
                            if (OnDynamicExecuteSequence != null)
                            {
                                OnDynamicExecuteSequence(dynamicSequence.Id, subSequence.Id, true, result.ActivityIdPath);
                            }
                            if (subSequence.IsExecuteAsOneActivity)
                            {
                                scope.DisableEvents(subSequence.Id);
                            }
                            actResult.SubActivityResults.Add(ExecuteActivity(subSequence, scope));
                            if (subSequence.IsExecuteAsOneActivity)
                            {
                                scope.EnableEvents(subSequence.Id);
                            }
                            AdjustResultStatusAndExceptions(actResult);
                        }
                    }
                    if (!subSequenceIsFound)
                    {
                        SetErrorMessage(actResult, string.Format("Call dynamic: SubSequence {0} was not found", methodName), dynamicActivity);
                    }
                }
                else
                {
                    if (OnDynamicExecuteSequence != null)
                    {
                        OnDynamicExecuteSequence(dynamicSequence.Id, Guid.Empty, false, result.ActivityIdPath);
                    }
                    foreach (var sequenceActivity in dynamicSequence.MTFSequenceActivities)
                    {
                        actResult.SubActivityResults.Add(ExecuteActivity(sequenceActivity, scope));
                    }
                    AdjustResultStatusAndExceptions(actResult);
                }

            }
            else
            {
                SetErrorMessage(actResult, string.Format("Call dynamic: Sequence {0} was not found", sequenceName), dynamicActivity);
            }
        }

        private void UnloadDynamicSequence(string sequenceName, MTFExecuteActivity dynamicActivity, MTFActivityResult actResult)
        {
            var dynamicSequence = dynamic.GetSequenceByName(sequenceName, Sequence.FullPath);
            if (dynamicSequence != null)
            {
                dynamic.ReleaseSequence(dynamicSequence, variablesHandler, componentsMapping);
                OnDynamicUnloadSequence?.Invoke(dynamicSequence.Id);
                actResult.Status = MTFExecutionActivityStatus.Ok;
            }
            else
            {
                SetWarningMessage(actResult, string.Format("Call dynamic: Sequence {0} was not found", sequenceName), dynamicActivity);
            }
        }

        private void LoadDynamicSequence(string sequenceName, MTFExecuteActivity dynamicActivity, MTFActivityResult actResult)
        {
            var persist = new MTFPersistToBin();
            var dynamicSequenceFullPath = dynamic.GetSequenceFullPath(sequenceName, Sequence.FullPath);
            var parentSequence = dynamicActivity.GetParent<MTFSequence>();
            if (!dynamic.ExistSequence(dynamicSequenceFullPath))
            {
                MTFDataTransferObject.RaiseNotifyPropertyChangedEvent = true;
                var s = persist.LoadSequenceProject(dynamicSequenceFullPath);
                MTFDataTransferObject.RaiseNotifyPropertyChangedEvent = false;
                if (s != null)
                {
                    s.FullPath = dynamicSequenceFullPath;
                    dynamic.AddSequence(s);
                    if (OnDynamicLoadSequence != null)
                    {
                        var sequenceDict = new Dictionary<Guid, MTFExternalSequenceInfo>();
                        s.ExtractExternalSequences(sequenceDict);
                        OnDynamicLoadSequence(s, sequenceDict);
                    }
                    s.ReplaceIdentityObjectsInSequenceProject();
                    CreateDynamicComponentMapping(s);
                    variablesHandler.InitVariablesMapping(s, parentSequence != null ? parentSequence.Id : Guid.Empty, dynamic);

                    actResult.Status = MTFExecutionActivityStatus.Ok;
                }
                else
                {
                    SetErrorMessage(actResult, string.Format("Call dynamic: Sequence {0} was not found", sequenceName), dynamicActivity);
                }
            }
            else
            {
                SetWarningMessage(actResult, string.Format("Call dynamic: Sequence {0} is already loaded", sequenceName), dynamicActivity);
            }

        }

        private void SetWarningMessage(MTFActivityResult actResult, string msg, MTFSequenceActivity activity)
        {
            actResult.ExceptionOccured = true;
            actResult.ExceptionMessage = msg;
            actResult.Status = MTFExecutionActivityStatus.Warning;
            RaiseOnError(DateTime.Now, actResult.ExceptionMessage, StatusMessage.MessageType.Warning, activity, actResult.ActivityIdPath.ToList());
        }

        private void SetErrorMessage(MTFActivityResult actResult, string msg, MTFSequenceActivity activity)
        {
            actResult.ExceptionOccured = true;
            actResult.ExceptionMessage = msg;
            actResult.Status = MTFExecutionActivityStatus.Nok;
            RaiseOnError(DateTime.Now, actResult.ExceptionMessage, StatusMessage.MessageType.Warning, activity, actResult.ActivityIdPath.ToList());
            ChangeExecutionState(MTFSequenceExecutionState.AborSubSequence);
        }


        private MTFActivityResult ExecuteExternalActivity(MTFExecuteActivity externalActivity, ScopeData scope)
        {
            MTFActivityResult actResult = new MTFActivityResult(externalActivity);
            var currentSequence = externalActivity.GetActivityParentOfType<MTFSequence>();
            if (currentSequence != null && currentSequence.ExternalSubSequences != null)
            {
                var externalInfo = currentSequence.ExternalSubSequences.FirstOrDefault(x => x.IsEnabled &&
                    externalActivity.ExternalCall != null &&
                    x.ExternalSequence.Name == externalActivity.ExternalCall.ExternalSequenceToCall);
                if (externalInfo != null)
                {
                    DateTime timestamp = DateTime.Now;
                    actResult.TimestampMs = sequenceHandlingStopwatchMs();
                    MTFExecutionActivityStatus statusForSubresults = MTFExecutionActivityStatus.None;
                    string exception = null;
                    bool exceptionOccured = false;
                    bool checkOutputValueFailed = false;
                    raiseOnActivityChanged(scope.ExecutingActivityPath.ToArray(), externalActivity, timestamp, scope);
                    var subSequence = externalInfo.ExternalSequence;
                    if (externalActivity.ExternalCall != null)
                    {
                        if (externalActivity.ExternalCall.InnerSubSequenceByCallId == Guid.Empty || externalActivity.ExternalCall.InnerSubSequenceByCallId == ActivityNameConstants.CallWholeSequenceId)
                        {
                            //Execute all SubSequence
                            if (subSequence.MTFSequenceActivities != null)
                            {
                                foreach (var activity in subSequence.MTFSequenceActivities)
                                {
                                    if (executionState == MTFSequenceExecutionState.Stopping || executionState == MTFSequenceExecutionState.Aborting)
                                    {
                                        return null;
                                    }
                                    if (activity.IsActive)
                                    {
                                        var res = ExecuteActivity(activity, scope);
                                        if (res != null)
                                        {
                                            statusForSubresults = AssignStatus(statusForSubresults, res);
                                            if (res.ExceptionOccured)
                                            {
                                                exceptionOccured = true;
                                                exception = res.ExceptionMessage;
                                            }
                                            if (res.CheckOutpuValueFailed)
                                            {
                                                checkOutputValueFailed = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Execute selected SubSequenceByCall
                            if (subSequence.ActivitiesByCall != null)
                            {
                                var subSequenceByCall = subSequence.ActivitiesByCall.FirstOrDefault(x => x.Id == externalActivity.ExternalCall.InnerSubSequenceByCallId);
                                if (subSequenceByCall is MTFSubSequenceActivity)
                                {
                                    var res = ExecuteActivity(subSequenceByCall, scope);
                                    if (res != null)
                                    {
                                        statusForSubresults = res.Status;
                                        exceptionOccured = res.ExceptionOccured;
                                        exception = res.ExceptionMessage;
                                        checkOutputValueFailed = res.CheckOutpuValueFailed;
                                    }
                                }
                            }
                        }
                    }
                    actResult.ActivityIdPath = scope.ExecutingActivityPath.ToArray();
                    actResult.ElapsedMs = (DateTime.Now - timestamp).TotalMilliseconds;
                    actResult.Status = statusForSubresults;
                    actResult.ExceptionMessage = exception;
                    actResult.ExceptionOccured = exceptionOccured;
                    actResult.CheckOutpuValueFailed = checkOutputValueFailed;
                    raiseOnNewActivityresult(actResult, scope);
                }
            }

            return actResult;
        }

        private MTFExecutionActivityStatus AssignStatus(MTFExecutionActivityStatus statusForSubresults, MTFActivityResult res)
        {
            if (statusForSubresults == MTFExecutionActivityStatus.None || statusForSubresults == MTFExecutionActivityStatus.Ok)
            {
                return res.Status;
            }
            else
            {
                if (statusForSubresults == MTFExecutionActivityStatus.Warning && res.Status == MTFExecutionActivityStatus.Nok)
                {
                    return res.Status;
                }
                else
                {
                    return statusForSubresults;
                }
            }
        }

        private MTFActivityError CreateActivityError(DateTime timeStamp, MTFSequenceActivity activity, string errorMessage)
        {
            var error = new MTFActivityError
            {
                TimeStamp = timeStamp,
                ErrorMessage = errorMessage,
            };

            if (activity!=null)
            {
               error.ActivityName = activity.ActivityName;
               error.ActivityPathShort = SequenceLocalizationHelper.TranslateActivityPath(activity.GenerateShortReportPath());
               error.ActivityPathLong = SequenceLocalizationHelper.TranslateActivityPath(activity.GenerateShortReportPath(-1));
            }

            return error;
        }

        private MTFActivityResult ExecuteErrorHandlingActivity(MTFErrorHandlingActivity errorHandlingActivity, ScopeData scope)
        {
            DateTime timestamp = DateTime.Now;
            scope.ExecutingActivityPath.Add(errorHandlingActivity.Id);
            raiseOnActivityChanged(scope.ExecutingActivityPath.ToArray(), errorHandlingActivity, timestamp, scope);

            MTFActivityResult result = new MTFActivityResult
            {
                ActivityId = errorHandlingActivity.Id,
                Status = MTFExecutionActivityStatus.Ok,
                ActivityIdPath = scope.ExecutingActivityPath.ToArray(),
                ActivityName = errorHandlingActivity.ActivityName,
                TimestampMs = sequenceHandlingStopwatchMs(),
                ExceptionMessage = string.Empty,
            };

            if (errorHandlingActivity.ErrorHandlingType == ErrorHandlingType.CleanErrors)
            {
                lock (lastActivityResultLock)
                {
                    scope.SetActivityResult(errorHandlingActivity.Id, IsSequencePass(scope.DeviceUnderTestInfo?.Id, true));
                }
                reportManager.ClearErrors(scope.DeviceUnderTestInfo?.Id);
                activityErrorsHandler.Clear(scope.DeviceUnderTestInfo?.Id);

                lastErrorActivity = null;
                if (errorHandlingActivity.CleanErrorWindow)
                {
                    RaiseActivityStringProgressChanged(new ActivityStringProgressChangedEventArgs { Command = ActivityStringProgressCommand.CleanErrorWindow });
                }
            }

            if (errorHandlingActivity.ErrorHandlingType == ErrorHandlingType.CheckErrors)
            {
                lock (lastActivityResultLock)
                {
                    result.ActivityResult = IsSequencePass(scope.DeviceUnderTestInfo?.Id, errorHandlingActivity.IncludeValidationTables);
                    result.ActivityResultTypeName = "System.Boolean";
                    scope.SetActivityResult(errorHandlingActivity.Id, result.ActivityResult);
                }

                AssignOutputValue(errorHandlingActivity, result.ActivityResult, result, scope);

                var checkOutputValueResult = checkOutputValue(errorHandlingActivity, result.ActivityResult, result, scope);
                if (!string.IsNullOrEmpty(checkOutputValueResult))
                {
                    handleCheckOutputValueBehavior(errorHandlingActivity, null, checkOutputValueResult, ref result, scope);
                }
            }

            if (errorHandlingActivity.ErrorHandlingType == ErrorHandlingType.RaiseError)
            {
                if (errorHandlingActivity.RaiseError != null && !errorHandlingActivity.IsPossibilityCatch)
                {
                    var msg = evaluateStringFormat(errorHandlingActivity.RaiseError, errorHandlingActivity, result, scope);

                    reportManager.AddError(timestamp, ErrorTypes.ComponentError, errorHandlingActivity.TranslateActivityName(), msg, scope.DeviceUnderTestInfo?.Id);

                    activityErrorsHandler.AddError(scope.DeviceUnderTestInfo?.Id, CreateActivityError(timestamp, errorHandlingActivity, msg));

                    result.ExceptionMessage = msg;
                    result.ExceptionOccured = true;
                    result.Status = MTFExecutionActivityStatus.Nok;
                }

                if (errorHandlingActivity.ErrorOutput != null && errorHandlingActivity.ErrorOutput.TypeName == typeof(string).FullName)
                {
                    var value = variablesHandler.GetVariableValue(errorHandlingActivity.ErrorOutput.Id, scope.DeviceUnderTestInfo?.Id);
                    variablesHandler.SetVariableValue(errorHandlingActivity.ErrorOutput.Id, $"{value}{result.ExceptionMessage}{Environment.NewLine}", scope.DeviceUnderTestInfo?.Id);
                }

                RaiseOnError(DateTime.Now, result.ExceptionMessage, StatusMessage.MessageType.Error, errorHandlingActivity, scope.ExecutingActivityPath);
            }

            if (errorHandlingActivity.ErrorHandlingType == ErrorHandlingType.GetErrors)
            {

                //build format string for exception object
                StringBuilder sb = new StringBuilder();
                if (errorHandlingActivity.ErrorRenderTimeStamp)
                    sb.Append("{TimeStamp} ");
                if (errorHandlingActivity.ErrorRenderActivityName)
                    sb.Append("{ActivityName} ");
                if (errorHandlingActivity.ErrorRenderActivityPathShort)
                    sb.Append("{ActivityPathShort} ");
                if (errorHandlingActivity.ErrorRenderActivityPathLong)
                    sb.Append("{ActivityPathLong} ");
                if (errorHandlingActivity.ErrorRenderErroMessage)
                    sb.Append("{ErrorMessage} ");

                lock (lastActivityResultLock)
                {
                    List<MTFActivityError> ex = activityErrorsHandler.GetErrors(scope.DeviceUnderTestInfo?.Id);
                    
                    result.ActivityResult = string.Join(Environment.NewLine, ex.Select(e => e.FormatActivityError(sb.ToString())));
                    result.ActivityResultTypeName = "System.String";
                    scope.SetActivityResult(errorHandlingActivity.Id, result.ActivityResult);
                }
            }

            if (errorHandlingActivity.ErrorHandlingType == ErrorHandlingType.GetInvalidValidationTables)
            {
                lock (lastActivityResultLock)
                {
                    var tableNames = variablesHandler.ValidationTables(scope.DeviceUnderTestInfo?.Id).Where(t => t.Status == MTFValidationTableStatus.Nok)
                        .Select(t => t.Name + " " + string.Join(" ", t.Rows.Where(r => r.Status == MTFValidationTableStatus.Nok).Select(r => r.Header)))
                        .ToArray();
                    result.ActivityResult = string.Join(Environment.NewLine, tableNames);
                    result.ActivityResultTypeName = "System.String";
                    scope.SetActivityResult(errorHandlingActivity.Id, result.ActivityResult);
                }
            }

            if (errorHandlingActivity.ErrorHandlingType == ErrorHandlingType.GetLastErrorActivityName)
            {
                lock (lastActivityResultLock)
                {
                    result.ActivityResult = lastErrorActivity == null ? null : lastErrorActivity.ActivityName;
                    result.ActivityResultTypeName = "System.String";
                    scope.SetActivityResult(errorHandlingActivity.Id, result.ActivityResult);
                }
            }

            if (errorHandlingActivity.ErrorHandlingType == ErrorHandlingType.GetErrorImages)
            {
                lock (lastActivityResultLock)
                {
                    result.ActivityResult = GetErrorImagesFromValidationTables(scope.DeviceUnderTestInfo?.Id);
                    result.ActivityResultTypeName = typeof(MTFImage[]).ToString();
                    scope.SetActivityResult(errorHandlingActivity.Id, result.ActivityResult);
                }
            }

            result.ElapsedMs = (DateTime.Now - timestamp).TotalMilliseconds;

            raiseOnNewActivityresult(result, scope);

            scope.ExecutingActivityPath.Remove(errorHandlingActivity.Id);

            if (executionState == MTFSequenceExecutionState.Executing && errorHandlingActivity.ErrorHandlingType == ErrorHandlingType.RaiseError)
            {
                handleErrorBehavior(errorHandlingActivity, null, result.ExceptionMessage, result, false, false, scope);
            }

            return result;
        }

        private MTFImage[] GetErrorImagesFromValidationTables(Guid? id)
        {
            var nokTables = variablesHandler.ValidationTables(id).Where(v => v.Status == MTFValidationTableStatus.Nok);
            List<MTFValidationTableRow> nokImageRows = new List<MTFValidationTableRow>();
            foreach (var table in nokTables)
            {
                nokImageRows.AddRange(table.Rows.Where(r => r.IsActualValueImage && r.Status == MTFValidationTableStatus.Nok));
            }

            return nokImageRows.Count == 0 ? new MTFImage[0] : nokImageRows.Select(r => r.Items.First(i => i.HasImage).Value as MTFImage).ToArray();
            //var nokRowImages = nokTables.Select(t => t.Rows.Where(r => r.IsActualValueImage));
        }

        #region obsolete
        private MTFActivityResult ExecuteLoggingActivity(MTFLoggingActivity loggingActivity, ScopeData scope)
        {
            DateTime timestamp = DateTime.Now;
            scope.ExecutingActivityPath.Add(loggingActivity.Id);
            raiseOnActivityChanged(scope.ExecutingActivityPath.ToArray(), loggingActivity, timestamp, scope);

            MTFActivityResult result;

            if (loggingActivity.LoggingType == LoggingType.LogMessage)
            {
                result = new MTFLogMessageResult
                {
                    ActivityId = loggingActivity.Id,
                    Status = MTFExecutionActivityStatus.Ok,
                    ActivityIdPath = scope.ExecutingActivityPath.ToArray(),
                    ActivityName = loggingActivity.ActivityName,
                    TimestampMs = sequenceHandlingStopwatchMs(),
                    ExceptionMessage = string.Empty,
                };
            }
            else
            {
                result = new MTFActivityResult
                {
                    ActivityId = loggingActivity.Id,
                    Status = MTFExecutionActivityStatus.Ok,
                    ActivityIdPath = scope.ExecutingActivityPath.ToArray(),
                    ActivityName = loggingActivity.ActivityName,
                    TimestampMs = sequenceHandlingStopwatchMs(),
                    ExceptionMessage = string.Empty,
                };
            }

            //if (loggingActivity.LoggingType == LoggingType.OpenLogFile)
            //{
            //    openLogFile(evaluateStringFormat(loggingActivity.LogFileName, loggingActivity, result));
            //}

            if (loggingActivity.LoggingType == LoggingType.LogMessage)
            {
                string message;
                try
                {
                    message = evaluateStringFormat(loggingActivity.LogMessage, loggingActivity, result, scope);
                }
                catch (Exception e)
                {
                    message = e.Message;
                }

                reportManager.SaveMessageAsync(DateTime.Now, message, scope.DeviceUnderTestInfo?.Id);
                //((MTFLogMessageResult)result).LoggedMessage = logMessage(message, loggingActivity.LogTimeStamp);
            }

            result.ElapsedMs = (DateTime.Now - timestamp).TotalMilliseconds;
            result.TimestampMs = sequenceHandlingStopwatchMs();

            raiseOnNewActivityresult(result, scope);

            scope.ExecutingActivityPath.Remove(loggingActivity.Id);

            return result;
        }
        #endregion

        private bool IsGoldSample(ScopeData scope) => scope.SequenceVariant.MatchGoldSample(sequence.GoldSampleSetting.GoldSampleSelector, sequence.VariantGroups);

        private SequenceVariant CurrentGoldSample(ScopeData scope) => scope.SequenceVariant.GetBestGoldSample(goldSampleData.GoldSampleList.Select(x => x.SequenceVariant), sequence.GoldSampleSetting.GoldSampleSelector);

        private bool GoldSampleRequired(ScopeData scope)
        {
            var sequenceVariantInfo = goldSampleData.GoldSampleList.FirstOrDefault(x => Equals(x.SequenceVariant, CurrentGoldSample(scope)));

            return sequenceVariantInfo != null && sequenceVariantInfo.GoldSampleExpired;
    }

        private void GoldSampleInvalidate(ScopeData scope)
        {
            var sequenceVariantInfo = goldSampleData.GoldSampleList.FirstOrDefault(x => Equals(x.SequenceVariant, CurrentGoldSample(scope)));
            if (sequenceVariantInfo != null)
            {
                sequenceVariantInfo.GoldSampleExpired = true;
            }
        }

        private MTFActivityResult ExecuteMTFExecuteActivity(MTFExecuteActivity executeActivity, ScopeData scope)
        {
            switch (executeActivity.Type)
            {
                case ExecuteActyvityTypes.Local:
                    return executeActivity.ActivityToCall == null ? null : ExecuteActivity(executeActivity.ActivityToCall, scope);
                case ExecuteActyvityTypes.External:
                    return ExecuteExternalActivity(executeActivity, scope);
                case ExecuteActyvityTypes.Dynamic:
                    return ExecuteDynamicActivity(executeActivity, scope);
            }
            return null;
        }



        private MTFActivityResult ExecuteSequenceHandlingActivity(MTFSequenceHandlingActivity sequenceHandlingActivity, ScopeData scope)
        {
            DateTime timestamp = DateTime.Now;
            bool canRaiseResult = true;

            scope.ExecutingActivityPath.Add(sequenceHandlingActivity.Id);
            raiseOnActivityChanged(scope.ExecutingActivityPath.ToArray(), sequenceHandlingActivity, timestamp, scope);


            MTFActivityResult result = new MTFActivityResult
            {
                ActivityId = sequenceHandlingActivity.Id,
                Status = MTFExecutionActivityStatus.Ok,
                ActivityIdPath = scope.ExecutingActivityPath.ToArray(),
                ActivityName = sequenceHandlingActivity.ActivityName,
                TimestampMs = sequenceHandlingStopwatchMs(),
                ExceptionMessage = string.Empty,
            };

            switch (sequenceHandlingActivity.SequenceHandlingType)
            {
                case SequenceHandlingType.SaveReportAndCleanErrors:
                    {
                        var sequenceHandlingResult = new MTFActivityResult
                        {
                            ActivityId = sequenceHandlingActivity.Id,
                            Status = MTFExecutionActivityStatus.Ok,
                            ActivityIdPath = scope.ExecutingActivityPath.ToArray(),
                            ActivityName = sequenceHandlingActivity.ActivityName,
                            TimestampMs = sequenceHandlingStopwatchMs(),
                            ElapsedMs = sequenceHandlingStopwatchMs(),
                        };

                        List<MTFActivityError> exceptionList = activityErrorsHandler.GetErrors(scope.DeviceUnderTestInfo?.Id);

                        sequenceHandlingResult.ExceptionMessage = string.Join(Environment.NewLine,
                            exceptionList.Select(
                                e => e.FormatActivityError("{TimeStamp} {ActivityName} {ActivityPathShort} {ActivityPathLong} {ErrorMessage}")));

                        if (sequenceHandlingActivity.SaveToTxt && (sequenceHandlingActivity.Logs != null))
                        {
                            foreach (var log in sequenceHandlingActivity.Logs)
                            {
                                string message;

                                try
                                {
                                    message = evaluateStringFormat(log, sequenceHandlingActivity, result, scope).Replace("\\n", Environment.NewLine);
                                }
                                catch (Exception e)
                                {
                                    message = e.Message;
                                }

                                reportManager.SaveMessageAsync(DateTime.Now, message, scope.DeviceUnderTestInfo?.Id);
                            }
                        }

                        if (sequenceHandlingActivity.SetStatus)
                        {
                            SetSequenceStatus(sequenceHandlingResult.ElapsedMs, new StatusLinesFontSize(), scope.DeviceUnderTestInfo?.Id, sequenceHandlingActivity.IncludeValidationTables);
                        }

                        if (sequenceHandlingActivity.UseCycleName)
                        {
                            try
                            {
                                var cycleName = evaluateStringFormat(sequenceHandlingActivity.LogCycleName, sequenceHandlingActivity, result, scope);
                                reportManager.SetCycleName(cycleName, scope.DeviceUnderTestInfo?.Id);
                            }
                            catch (Exception ex)
                            {
                                var timeStamp = DateTime.Now;
                                SystemLog.LogException(ex);
                                RaiseOnError(timeStamp, ex.Message, StatusMessage.MessageType.Error, sequenceHandlingActivity, scope.ExecutingActivityPath);
                                reportManager.AddError(timeStamp, ErrorTypes.SequenceError, sequenceHandlingActivity.TranslateActivityName(), ex.Message, scope.DeviceUnderTestInfo?.Id);
                            }
                        }

                        try
                        {
                            reportManager.CompleteReport(sequenceHandlingActivity.LogHiddenRows, true, sequenceHandlingActivity.SaveGraphicalView,
                                sequence.GraphicalViewSetting, variablesHandler.ValidationTables(scope.DeviceUnderTestInfo?.Id).ToDictionary(key => key.Id, value => value), scope.DeviceUnderTestInfo?.Id);
                        }
                        catch (Exception ex)
                        {
                            SystemLog.LogException(ex);
                            RaiseOnError(DateTime.Now, ex.Message, StatusMessage.MessageType.Error, sequenceHandlingActivity, scope.ExecutingActivityPath);

                            ChangeExecutionStateInternal(MTFSequenceExecutionState.Aborting);
                        }

                        //SaveSequenceIfModifiedBySetupMode();

                        result.ActivityResultTypeName = typeof(bool).FullName;
                        result.ActivityResult = IsSequencePass(scope.DeviceUnderTestInfo?.Id, sequenceHandlingActivity.IncludeValidationTables);

                        lock (lastActivityResultLock)
                        {
                            scope.SetActivityResult(sequenceHandlingActivity.Id, result.ActivityResult);
                        }

                        if (sequenceHandlingActivity.RestartTimer)
                        {
                            resetSequenceHandlingStopwatch();
                        }

                        if (sequenceHandlingActivity.CleanErrorMemory)
                        {
                            reportManager.ClearErrors(scope.DeviceUnderTestInfo?.Id);
                            activityErrorsHandler.Clear(scope.DeviceUnderTestInfo?.Id);
                            lastErrorActivity = null;
                        }

                        if (sequenceHandlingActivity.CleanErrorWindow)
                        {
                            RaiseActivityStringProgressChanged(new ActivityStringProgressChangedEventArgs { Command = ActivityStringProgressCommand.CleanErrorWindow });
                        }

                        if (sequenceHandlingActivity.CleanTables)
                        {
                            ClearValidationTables(sequenceHandlingActivity, scope);
                        }

                        SaveGoldSampleList();

                        break;
                    }
                case SequenceHandlingType.StartDurationCounter:
                    {
                        resetSequenceHandlingStopwatch();
                        reportManager.SetStartTime(scope.DeviceUnderTestInfo?.Id);
                        break;
                    }
                case SequenceHandlingType.SetSequenceStatusMessage:
                    {
                        string s1 = string.Empty;
                        string s2 = string.Empty;
                        string s3 = string.Empty;

                        if (sequenceHandlingActivity.StatusLines != null)
                        {
                            if (sequenceHandlingActivity.StatusLines.Count >= 1)
                            {
                                s1 = evaluateStringFormat(sequenceHandlingActivity.StatusLines[0], sequenceHandlingActivity, result, scope);
                            }

                            if (sequenceHandlingActivity.StatusLines.Count >= 2)
                            {
                                s2 = evaluateStringFormat(sequenceHandlingActivity.StatusLines[1], sequenceHandlingActivity, result, scope);
                            }

                            if (sequenceHandlingActivity.StatusLines.Count >= 3)
                            {
                                s3 = evaluateStringFormat(sequenceHandlingActivity.StatusLines[2], sequenceHandlingActivity, result, scope);
                            }
                        }

                        if (OnSequenceStatusMessage != null)
                        {
                            reportManager.SetSequenceStatus(s1, scope.DeviceUnderTestInfo?.Id);

                            RaiseSequenceStatusMessage(s1, s2, s3, new StatusLinesFontSize
                            {
                                Line1 = sequenceHandlingActivity.StatusLinesFontSize.Line1,
                                Line2 = sequenceHandlingActivity.StatusLinesFontSize.Line2,
                                Line3 = sequenceHandlingActivity.StatusLinesFontSize.Line3,
                            }, scope.DeviceUnderTestInfo?.Id);
                        }
                        
                        break;
                    }
                case SequenceHandlingType.ClearValidTables:
                    {
                        ClearValidationTables(sequenceHandlingActivity, scope);

                        break;
                    }
                case SequenceHandlingType.LogMessage:
                    {
                        string message;

                        try
                        {
                            message = evaluateStringFormat(sequenceHandlingActivity.LogMessage, sequenceHandlingActivity, result, scope);
                        }
                        catch (Exception e)
                        {
                            message = e.Message;
                        }

                        reportManager.SaveMessageAsync(DateTime.Now, message, scope.DeviceUnderTestInfo?.Id);

                        break;
                    }
                case SequenceHandlingType.ChangeCommandsStatus:
                    {
                        canRaiseResult = false;
                        if (sequenceRuntimeContext.IsServiceModeActive || sequenceRuntimeContext.IsTeachModeActive)
                        {
                            allowedCommands = sequenceHandlingActivity.CommandsSetting.Where(x => x.ServiceCommand != null && x.IsEnabled).Select(x => x.ServiceCommand.Id).ToList();
                            UpdateToggleCommandsState(sequenceHandlingActivity.CommandsSetting);
                        }
                        break;
                    }
                case SequenceHandlingType.GetVariant:
                    {
                        result.ActivityResultTypeName = typeof(string).FullName;
                        if (scope.SequenceVariant != null)
                        {
                            result.ActivityResult = scope.SequenceVariant.ToString();
                        }
                        lock (lastActivityResultLock)
                        {
                            scope.SetActivityResult(sequenceHandlingActivity.Id, result.ActivityResult);
                        }
                        break;
                    }
                case SequenceHandlingType.SetVariant:
                    {
                        //CheckSetVariant(sequenceHandlingActivity);
                        var currentVariant = sequenceHandlingActivity.SequenceVariant;

                        if ((currentVariant != null) && (currentVariant.VariantGroups != null))
                        {
                            foreach (var variantGroupValue in currentVariant.VariantGroups)
                            {
                                if (variantGroupValue.Term == null)
                                {
                                    continue;
                                }

                                var termResult = this.evaluateTerm(variantGroupValue.Term, sequenceHandlingActivity, result, scope);

                                if (termResult != null)
                                {
                                    variantGroupValue.Values = new List<SequenceVariantValue> { new SequenceVariantValue { Name = termResult.ToString() } };
                                }
                            }
                        }
                        scope.SequenceVariant = sequenceHandlingActivity.SequenceVariant;

                        CheckGoldSample(scope);
                        break;
                    }
                case SequenceHandlingType.PauseSequence:
                    {
                        if (sequenceHandlingActivity.ExecuteOnlyInDebug)
                        {
                            if (sequenceRuntimeContext.IsDebugModeActive)
                            {
                                Pause();
                            }
                        }
                        else
                        {
                            Pause();
                        }
                        break;
                    }
                case SequenceHandlingType.StopSequence:
                    {
                        if (sequenceHandlingActivity.ExecuteOnlyInDebug)
                        {
                            if (sequenceRuntimeContext.IsDebugModeActive)
                            {
                                Stop();
                            }
                        }
                        else
                        {
                            Stop();
                        }
                        break;
                    }
                case SequenceHandlingType.GetExecutionState:
                    {
                        result.ActivityResultTypeName = typeof(bool).FullName;
                        result.ActivityResult = false;

                        if (sequenceHandlingActivity.IsServiceMode && sequenceRuntimeContext.IsServiceMode)
                        {
                            result.ActivityResult = true;
                        }
                        if (sequenceHandlingActivity.IsTeachMode && sequenceRuntimeContext.IsTeachMode)
                        {
                            result.ActivityResult = true;
                        }
                        if (sequenceHandlingActivity.GoingToServiceMode && sequenceRuntimeContext.IsServiceModeActive)
                        {
                            result.ActivityResult = true;
                        }
                        if (sequenceHandlingActivity.GoingToTeachMode && sequenceRuntimeContext.IsTeachModeActive)
                        {
                            result.ActivityResult = true;
                        }

                        lock (lastActivityResultLock)
                        {
                            scope.SetActivityResult(sequenceHandlingActivity.Id, result.ActivityResult);
                        }
                        break;
                    }
                case SequenceHandlingType.GetLoggedUsers:
                    {
                        result.ActivityResultTypeName = typeof(string[]).FullName;
                        result.ActivityResult = loggedUsers == null || loggedUsers.Count == 0 ? new string[] { null } : loggedUsers.Values.ToArray(); //because of back compatibility array with one empty feeld is needed
                        lock (lastActivityResultLock)
                        {
                            scope.SetActivityResult(sequenceHandlingActivity.Id,result.ActivityResult);
                        }
                        break;
                    }

                case SequenceHandlingType.SynchronizationExecuteInParallel:
                    {
                        var nameOfActivity = sequenceHandlingActivity.ActivityName;
                        if (dictionaryOfSynchronization.ContainsKey(nameOfActivity))
                        {
                            dictionaryOfSynchronization[nameOfActivity]++;
                        }
                        else
                        {
                            dictionaryOfSynchronization.Add(nameOfActivity, 1);
                        }

                        while (dictionaryOfSynchronization[nameOfActivity] < int.Parse(sequenceHandlingActivity.ExpectedCount))
                        {
                            Thread.Sleep(500);

                            if (executionState == MTFSequenceExecutionState.Stopping)
                            {
                                return null;
                            }
                        }

                        break;
                    }

                case SequenceHandlingType.ChangeUserCommandsStatus:
                {
                    UpdateSequenceUserCommandSettings(sequenceHandlingActivity.UserCommandsSetting);
                    OnUserCommandsStatusChanged?.Invoke(sequenceUserCommandsStates);
                    break;
                }

                case SequenceHandlingType.SetUserIndicatorValue:
                {
                    var userCommandId = sequenceHandlingActivity.SetUserIndicatorSettings.IndicatorId;
                    var indicatorValue = (bool)evaluateTerm(sequenceHandlingActivity.SetUserIndicatorSettings.ValueTerm, scope);
                    OnUserIndicatorValueChanged?.Invoke(userCommandId, indicatorValue);
                    break;
                }

                case SequenceHandlingType.GetMTFVersion:
                {
                    result.ActivityResultTypeName = typeof(string).FullName;
                    result.ActivityResult = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
                
                    lock (lastActivityResultLock)
                    {
                        scope.SetActivityResult(sequenceHandlingActivity.Id, result.ActivityResult);
                    }
                    break;
                }

                case SequenceHandlingType.SwitchView:
                {
                    OnViewChanged?.Invoke(sequenceHandlingActivity.SwitchGraphicalView.SwitchToView,
                        sequenceHandlingActivity.SwitchGraphicalView.SwitchToView == SequenceExecutionViewType.GraphicalView
                            ? sequenceHandlingActivity.SwitchGraphicalView.SelectedGraphicalViewId
                            : (Guid?) null, scope.DeviceUnderTestInfo?.Id);
                    break;
                }
            }

            if (sequenceHandlingActivity.SequenceHandlingType == SequenceHandlingType.SaveReportAndCleanErrors
                || sequenceHandlingActivity.SequenceHandlingType == SequenceHandlingType.GetLoggedUsers)
            {
                var checkOutputValueResult = checkOutputValue(sequenceHandlingActivity, result.ActivityResult, result, scope);
                if (!string.IsNullOrEmpty(checkOutputValueResult))
                {
                    handleCheckOutputValueBehavior(sequenceHandlingActivity, null, checkOutputValueResult, ref result, scope);
                }
            }

            result.ElapsedMs = (DateTime.Now - timestamp).TotalMilliseconds;

            if (canRaiseResult)
            {
                raiseOnNewActivityresult(result, scope);
            }

            scope.ExecutingActivityPath.Remove(sequenceHandlingActivity.Id);

            return result;
        }
        
        private void SetSequenceVariantRuntimeContext(Dictionary<string, string> values, ScopeData scope)
        {
            var sequenceVariant = new SequenceVariant();
            if (values != null && sequence.VariantGroups != null)
            {
                foreach (var value in values)
                {
                    var groupName = value.Key;
                    var group = sequence.VariantGroups.FirstOrDefault(x => x.Name == groupName);
                    if (group != null)
                    {
                        var index = sequence.VariantGroups.IndexOf(group);
                        var selectedValues = new List<SequenceVariantValue> { new SequenceVariantValue { Name = value.Value } };
                        sequenceVariant.SetVariant(groupName, index, selectedValues);
                    }
                }
            }
            scope.SequenceVariant = sequenceVariant;

            CheckGoldSample(scope);
        }

        private void SendToClientSetupControl(object data, string dataName, IEnumerable<ClientSetupControlName> clientSetupControls)
        {
            IEnumerable<SimpleClientControlInfo> clientSetupControlInfos = null;

            if (clientSetupControls != null)
            {
                clientSetupControlInfos = clientSetupControls.Select(cci => new SimpleClientControlInfo { AssemblyName = cci.AssemblyName, TypeName = cci.TypeName });
            }

            if (clientSetupControlInfos == null)
            {
                return;
            }

            this.SendToClientControl(data, dataName, clientSetupControlInfos);
        }

        private void SendToClientControl(object data, string dataName, IEnumerable<ClientControlName> clientControls)
        {
            IEnumerable<SimpleClientControlInfo> clientControlInfos = null;
            if (clientControls != null)
            {
                clientControlInfos = clientControls.Select(cci => new SimpleClientControlInfo { AssemblyName = cci.AssemblyName, TypeName = cci.TypeName });
                //var activity = getActivityByThread();
                //clientControlInfos = activity.ClassInfo.MTFClass.ClientControlInfos != null
                //    ? activity.ClassInfo.MTFClass.ClientControlInfos.Select(
                //        cci => new SimpleClientControlInfo { AssemblyName = cci.AssemblyName, TypeName = cci.TypeName })
                //    : null;
            }
            //else
            //{
            //    clientControlInfos = clientControls.Select(cci => new SimpleClientControlInfo { AssemblyName = cci.AssemblyName, TypeName = cci.TypeName });
            //}

            if (clientControlInfos == null || sequence.SequenceExecutionUiSetting.SelectedClientUis == null ||
                !sequence.SequenceExecutionUiSetting.SelectedClientUis.Any(c => clientControlInfos.Any(ci => ci.AssemblyName == c.AssemblyName && ci.TypeName == c.TypeName)))
            {
                return;
            }

            this.SendToClientControl(data, dataName, clientControlInfos);
        }

        private void SendToClientControl(object data, string dataName, IEnumerable<SimpleClientControlInfo> clientControlInfos)
        {
            if (clientControlInfos == null)
            {
                return;
            }

            byte[] rawData;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, data);

                rawData = ms.ToArray();
            }

            this.OnUIControlSendData(rawData,
                new ClientUIDataInfo
                {
                    DataName = dataName,
                    Direction = ClientDataDirection.ToClient,
                    ClientControlInfos = clientControlInfos,
                    DataType = data.GetType().AssemblyQualifiedName,
                });
        }

        private OpenClientSetupControlArgs closedControl;
        public void ClientSetupControlClosed(OpenClientSetupControlArgs args)
        {
            closedControl = args;
        }

        private void OpenClientSetupControl(OpenClientSetupControlArgs args)
        {
            closedControl = null;
            if (OnOpenClientSetupControl != null)
            {
                OnOpenClientSetupControl(args);
            }

            while (!(closedControl != null && closedControl.TypeName == args.TypeName && closedControl.AssemblyName == args.AssemblyName))
            {
                Thread.Sleep(500);
            }
        }

        private void CheckGoldSample( ScopeData scope)
        {
            var gsInfo = GoldSampleHelper.CheckGoldSample(sequence, goldSampleData, scope.SequenceVariant,
                sequenceStart, checkGoldSampleAfterStart, requireGoldSampleAfterInactivity);
            if (checkGoldSampleAfterStart)
            {
                checkGoldSampleAfterStart = !gsInfo.SequenceVariantInfo.IsGoldSample;
            }
            requireGoldSampleAfterInactivity = gsInfo.ReguireAfterInactivity;

            reportManager.SetSequenceVariant(scope.SequenceVariant, scope.DeviceUnderTestInfo?.Id);
            reportManager.SetRemains(gsInfo.SequenceVariantInfo, sequence.GoldSampleSetting, scope.DeviceUnderTestInfo?.Id);

            if (sequence.GoldSampleSetting.UseGoldSample)
            {
                if (OnShowMessage != null && gsInfo.RaiseMessage)
                {
                    var messageInfo = new MessageInfo
                    {
                        Header = "Gold sample warning",
                        Text = gsInfo.Message,
                        Type = SequenceMessageType.Warning,
                        Buttons = MessageButtons.Ok,
                        ActivityPath = scope.ExecutingActivityPath,
                    };
                    OnShowMessage(messageInfo);

                    while (!messageBoxResultReceived)
                    {
                        Thread.Sleep(500);
                    }
                }
            }

            var variantInfo = gsInfo.SequenceVariantInfo.Clone() as SequenceVariantInfo;
            variantInfo.SequenceVariant = gsInfo.CurrentVariant;
            lock (eventHandlingLock)
            {
                OnSequenceVariantChanged?.Invoke(variantInfo, scope.DeviceUnderTestInfo?.Id);
            }
            goldSampleData.LastUsedVariantTime = DateTime.Now;
        }

        private void UpdateToggleCommandsState(IList<ServiceCommandsSetting> commandsSettings)
        {
            if (Sequence.ServiceCommands != null)
            {
                foreach (var commandSetting in commandsSettings)
                {
                    if (commandSetting.ServiceCommand != null && commandSetting.ServiceCommand.Type == MTFServiceCommandType.ToggleButton)
                    {
                        var command = Sequence.ServiceCommands.FirstOrDefault(x => x.Id == commandSetting.ServiceCommand.Id);
                        if (command != null)
                        {
                            if (commandSetting.IsChecked.HasValue)
                            {
                                toggleCommands[command.Id] = commandSetting.IsChecked.Value;
                            }
                        }
                    }
                }
            }
        }


        private void SaveGoldSampleList()
        {
            if (goldSampleData.IsNotEmpty && !string.IsNullOrEmpty(sequence.GoldSampleSetting.GoldSampleDataFile))
            {
                Task.Run(() => GoldSampleHelper.Save(sequence.GoldSampleSetting.GoldSampleDataFile, goldSampleData));
            }
        }

        private void ClearValidationTables(MTFSequenceHandlingActivity sequenceHandlingActivity, ScopeData scope)
        {
            var list = sequenceHandlingActivity.TablesSetting.Where(x => x.Clear).Select(x => x.ValidationTable.Id).ToList();
            var validationTablesToReset = sequenceHandlingActivity.ClearAllTables
                ? variablesHandler.ValidationTables(scope.DeviceUnderTestInfo?.Id)
                : variablesHandler.ValidationTables(scope.DeviceUnderTestInfo?.Id).Where(t => list.Contains(t.Id));
            foreach (var validationTale in validationTablesToReset.ToList())
            {
                multipleTableRowFillCheck.RemoveValidatedRowsId(scope.DeviceUnderTestInfo?.Id, validationTale.Rows.Select(r => r.Id));
                validationTale.Reset();
            }

            lock (eventHandlingLock)
            {
                OnClearValidationTables?.Invoke(sequenceHandlingActivity.ClearAllTables, list, scope.DeviceUnderTestInfo?.Id);
            }
        }

        private string evaluateStringFormat(MTFStringFormat stringFormat, MTFSequenceActivity currentActivity, MTFActivityResult activityResult, ScopeData scope)
        {
            if (stringFormat == null || string.IsNullOrEmpty(stringFormat.Text))
            {
                return string.Empty;
            }
            string renderedString;
            if (stringFormat.Parameters == null)
            {
                renderedString = stringFormat.Text;
            }
            else
            {
                List<object> param = new List<object>();
                foreach (Term t in stringFormat.Parameters)
                {
                    try
                    {
                        param.Add(evaluateTerm(t, currentActivity, activityResult, scope));
                    }
                    catch (Exception e)
                    {
                        param.Add(e.Message);
                    }
                }

                renderedString = string.Format(stringFormat.Text, param.ToArray());
            }

            return renderedString.Replace("\\n", Environment.NewLine);
        }

        private void resetSequenceHandlingStopwatch()
        {
            sequenceHandlingResultStopwatch = DateTime.Now;
        }

        private double sequenceHandlingStopwatchMs()
        {
            return (DateTime.Now - sequenceHandlingResultStopwatch).TotalMilliseconds;
        }

        private void AddToValidationTable(string tableName, string rowName, object value, IEnumerable<ValidationColumn> validationColumns, ScopeData scope)
        {
            var table = getTable(tableName, scope);

            var row = table.Rows.FirstOrDefault(r => r.Header == rowName) ?? table.CreateTableRow(rowName);

            if (validationColumns != null)
            {
                foreach (var valCol in validationColumns)
                {
                    var item = row.Items.FirstOrDefault(c => c.Column.Header == valCol.Name);
                    if (item == null)
                    {
                        throw new Exception($"Validation table {tableName} don't contains column {valCol.Name}. Check validation table in sequence editor.");
                    }
                    item.IsSet = true;
                    item.Value = valCol.Value;
                }
            }

            ObservableCollection<ExtendedRow> rows = new MTFObservableCollection<ExtendedRow>();
            rows.Add(new ExtendedRow { Row = row, InjectedRow = row, ActualValue = new ConstantTerm { Value = value } });
            ValidationTableTerm validationTableTerm = new ValidationTableTerm
            {
                ValidationTable = table,
                InjectedTable = table,
                Rows = rows,
                SequenceVariant = scope.SequenceVariant,
                IsGoldSample = scope.SequenceVariant.MatchGoldSample(sequence.GoldSampleSetting.GoldSampleSelector, sequence.VariantGroups)
            };
            validationTableTerm.Evaluate();
            raiseOnNewValidateRows(validationTableTerm.GetValidatedRowsForDisplaying(DateTime.Now, table.Id, 0, scope.DeviceUnderTestInfo?.Id, out var rowsToLog), false, scope);
            LogTable(table, rowsToLog, scope.DeviceUnderTestInfo?.Id);
        }


        private IEnumerable<ValidationColumn> GetFromValidationTable(string tableName, string rowName, ScopeData scope)
        {
            var tableRow = getTableRow(getTable(tableName, scope), rowName);
            MTFValidationTableRowVariant bestRow = null;
            if (scope.SequenceVariant != null && tableRow.RowVariants != null)
            {
                var bestVariant = scope.SequenceVariant.GetBestMatch(tableRow.RowVariants.Select(x => x.SequenceVariant));
                bestRow = bestVariant == null ? null : tableRow.RowVariants.FirstOrDefault(r => Equals(r.SequenceVariant, bestVariant));
            }
            var validationColumns = tableRow.Items.Select(r => new ValidationColumn { Name = r.Header, Value = r.Value }).ToArray();
            if (bestRow != null)
            {
                var columntToReplace = bestRow.Items.Where(i => i.Type == MTFTableColumnType.Value);
                foreach (var col in columntToReplace)
                {
                    var valCol = validationColumns.FirstOrDefault(c => c.Name == col.Header);
                    if (valCol != null)
                    {
                        valCol.Value = col.Value;
                    }
                }
            }

            return validationColumns;
        }

        private object GetFromConstantTable(string tableName, string rowName, ScopeData scope)
        {
            var table = getConstantTable(tableName, scope);
            var row = table.Rows.FirstOrDefault(r => r.Header == rowName);
            if (row == null)
            {
                throw new Exception($"Constant table {tableName} don't contains row {rowName}. Please check sequence.");
            }

            return getConstantTable(tableName, scope).GetResult(ValidationTableResultType.CellResult, row.Id, "Value", scope.SequenceVariant);
        }

        private MTFValidationTable getTable(string tableName, ScopeData scope)
        {
            var tableVariable = variablesHandler.GetVariable<MTFValidationTable>(tableName);
            if (tableVariable == null)
            {
                throw new Exception($"Validation table {tableName} is not found in sequence. Please check sequence.");
            }
            return (MTFValidationTable)variablesHandler.GetVariableValue(tableVariable.Id, scope.DeviceUnderTestInfo?.Id);
        }

        private MTFConstantTable getConstantTable(string tableName, ScopeData scope)
        {
            var tableVariable = variablesHandler.GetVariable<MTFConstantTable>(tableName);
            if (tableVariable == null)
            {
                throw new Exception($"Constant table {tableName} is not found in sequence. Please check sequence.");
            }

            return (MTFConstantTable)variablesHandler.GetVariableValue(tableVariable.Id, scope.DeviceUnderTestInfo?.Id);
        }

        private MTFValidationTableRow getTableRow(MTFValidationTable table, string rowName)
        {
            var row = table.Rows.FirstOrDefault(r => r.Header == rowName);
            if (row == null)
            {
                throw new Exception($"Validation table {table.Name} don't contains row {rowName}. Please check sequence.");
            }
            return row;
        }

        private IEnumerable<ValidationRowContainer> GetValidationTableRows(string tableName, ScopeData scope)
        {
            var table = getTable(tableName, scope);

            List<ValidationRowContainer> validationRowContainers = new List<ValidationRowContainer>();
            foreach (var row in table.Rows)
            {
                List<ValidationColumn> validationColumns = new List<ValidationColumn>();
                ValidationRowContainer validationRowContainer = new ValidationRowContainer
                {
                    ValidationStatus = row.Status,
                    ValidationColumns = validationColumns,
                };
                validationRowContainers.Add(validationRowContainer);
                foreach (var cell in row.Items)
                {
                    if (cell.Type == MTFTableColumnType.Identification)
                    {
                        validationRowContainer.RowName = cell.Value.ToString();
                    }
                    else if (cell.Type == MTFTableColumnType.ActualValue)
                    {
                        validationRowContainer.Value = cell.Value;
                    }
                    else if (cell.Type == MTFTableColumnType.Value)
                    {
                        validationColumns.Add(new ValidationColumn { Name = cell.Column.Header, Value = cell.Value });
                    }
                }
            }

            return validationRowContainers;
        }

        private void AddRangeToValidationTable(string tableName, IEnumerable<ValidationRowContainer> rows, ScopeData scope)
        {
            if (rows == null)
            {
                return;
            }
            var table = getTable(tableName, scope);

            ObservableCollection<ExtendedRow> outputRows = new MTFObservableCollection<ExtendedRow>();

            foreach (var validationRowContainer in rows)
            {
                var row = table.Rows.FirstOrDefault(r => r.Header == validationRowContainer.RowName) ??
                          table.CreateTableRow(validationRowContainer.RowName);
                row.IsEvaluated = validationRowContainer.ValidationStatus == MTFValidationTableStatus.Ok ||
                                  validationRowContainer.ValidationStatus == MTFValidationTableStatus.Nok;
                if (row.IsEvaluated)
                {
                    row.Status = validationRowContainer.ValidationStatus;
                }


                if (validationRowContainer.ValidationColumns != null)
                {
                    foreach (var validationColumn in validationRowContainer.ValidationColumns)
                    {
                        var item = row.Items.FirstOrDefault(c => c.Column.Header == validationColumn.Name);
                        if (item == null)
                        {
                            throw new Exception($"Validation table {tableName} don't contains column {validationColumn.Name}. Check validation table in sequence editor.");
                        }
                        item.IsSet = true;
                        item.Value = validationColumn.Value;
                        if (row.IsEvaluated)
                        {
                            item.Status = !validationColumn.IsWrong;
                        }
                    }
                }
                outputRows.Add(new ExtendedRow { Row = row, InjectedRow = row, ActualValue = new ConstantTerm { Value = validationRowContainer.Value } });
            }

            var validationTableTerm = new ValidationTableTerm
            {
                ValidationTable = table,
                InjectedTable = table,
                Rows = outputRows,
                SequenceVariant = scope.SequenceVariant,
                IsGoldSample = IsGoldSample(scope),
            };
            validationTableTerm.Evaluate();
            raiseOnNewValidateRows(validationTableTerm.GetValidatedRowsForDisplaying(DateTime.Now, table.Id, 0, scope.DeviceUnderTestInfo?.Id, out var rowsToLog), false, scope);
            LogTable(table, rowsToLog, scope.DeviceUnderTestInfo?.Id);
        }


        private void updateTermByActivityResultsAndVariables(Term term, ScopeData scope)
        {
            //add last activity result to terms
            term.ForEachTerm<ActivityResultTerm>(t =>
            {
                lock (lastActivityResultLock)
                {
                    if (t.Value == null)
                    {
                        throw new Exception("Activity Result in your condition is not assigned. Please drag the Activity Result Target to the activity you want.");
                    }
                    if (!scope.ContainsActivityResult(t.Value.Id))
                    {
                        throw new Exception($"Result of activity {t.Value.ActivityName}({t.Value.Id}) is unknown. Please check your sequence.");
                    }
                    t.ActivityResult = scope.GetActivityResult(t.Value.Id);
                }
            });

            term.ForEachTerm<ValidationTableTerm>(t =>
            {
                var variable = variablesHandler.AllVariables.FirstOrDefault(x => x.Key.Value == t.ValidationTable).Key;
                if (variable != null)
                {
                    t.InjectedTable = variablesHandler.GetVariableValue(variable.Id, scope.DeviceUnderTestInfo?.Id) as MTFValidationTable;
                    UpdateRows(t.Rows, t.InjectedTable);
                }

                t.SequenceVariant = scope.SequenceVariant;
                t.IsGoldSample = IsGoldSample(scope);
                //t.ActivityPath = activityPath;
            });

            term.ForEachTerm<ValidationTableResultTerm>(t =>
            {
                var variable = variablesHandler.AllVariables.FirstOrDefault(x => x.Key.Value == t.ValidationTable).Key;
                if (variable != null)
                {
                    t.InjectedValue = variablesHandler.GetVariableValue(variable.Id, scope.DeviceUnderTestInfo?.Id) as IMTFTable;
                }
                t.SequenceVariant = scope.SequenceVariant;
            });

            term.ForEachTerm<VariableTerm>(t =>
            {
                if (t.MTFVariable != null && !variablesHandler.AllVariables.Keys.Any(sv => t.MTFVariable != null || sv.Id == t.MTFVariable.Id))
                {
                    throw new Exception($"Variable {t.ToString()} is not declared. Please check your sequence.");
                }

                var variable = variablesHandler.AllVariables.FirstOrDefault(x => x.Key == t.MTFVariable).Key;
                if (variable != null)
                {
                    t.InjectedValue = variablesHandler.GetVariableValue(variable.Id, scope.DeviceUnderTestInfo?.Id);
                }

            });

            term.ForEachTerm<ExecuteActivityTerm>(t =>
                                                  {
                                                      t.EvaluatedParameters = evaluateParameters(t.MTFParameters, null, new MTFActivityResult(),
                                                          scope);
                                                      t.Instance = instances[t.ClassInfoId].Instance;// instances.FirstOrDefault(x=>x.Key).Value?.Instance;
                                                      t.Method = GetMethodInfo(t.Instance, t.MethodName, null);
                                                  });
        }



        private void UpdateRows(ObservableCollection<ExtendedRow> rows, MTFValidationTable mtfValidationTable)
        {
            if (rows != null && rows.Count > 0)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    rows[i].InjectedRow = mtfValidationTable.Rows.FirstOrDefault(x => x.Header == rows[i].Row.Header);
                }
            }
        }

        object termEvaluationLock = new object();
        private object evaluateTerm(Term term, ScopeData scope)
        {
            lock (termEvaluationLock)
            {
                updateTermByActivityResultsAndVariables(term, scope);
                return term.Evaluate();
            }
        }

        private object evaluateTerm(Term term, MTFSequenceActivity currentActivity, MTFActivityResult activityResult, ScopeData scope)
        {
            if (term == null)
            {
                return null;
            }
            DateTime timeStamp = DateTime.Now;
            lock (termEvaluationLock)
            {
                object result = evaluateTerm(term, scope);
                term.ForEachTerm<ValidationTableTerm>(x =>
                {
                    if (x.InjectedTable != null)
                    {
                        if (x.IsGoldSample)
                        {
                            StoreValues(x.Rows, x.InjectedTable.Id);
                        }

                        var validatedRows = x.GetValidatedRowsForDisplaying(timeStamp, x.InjectedTable.Id,
                            activityResult.NumberOfRepetition, scope.DeviceUnderTestInfo.Id, out var rowsToLog);
                        if (x.InjectedTable.CheckMultipleFilling)
                        {
                            CheckValidatedRows(validatedRows, currentActivity, timeStamp, scope);
                        }

                        raiseOnNewValidateRows(validatedRows, false, scope);
                        LogTable(x.InjectedTable, rowsToLog, scope.DeviceUnderTestInfo?.Id);
                    }
                });
                return result;
            }
        }

        private void CheckValidatedRows(List<MTFValidationTableRowResult> validatedRows, MTFSequenceActivity currentActivity, DateTime timeStamp, ScopeData scope)
        {
            foreach (var rowResult in validatedRows)
            {
                if (multipleTableRowFillCheck.IsRowUsed(scope.DeviceUnderTestInfo?.Id, rowResult.Row.Id))
                {
                    var activityId = multipleTableRowFillCheck.GetActivityOfUsage(scope.DeviceUnderTestInfo?.Id, rowResult.Row.Id);
                    if (activityId != currentActivity.Id)
                    {
                        var msg = string.Format("The Row ({1}) in the validation table ({0}) has been filled from multiple locations. Last location: {2}",
                            rowResult.TableName, rowResult.Row.Header, currentActivity.GetActivityPath() + currentActivity.ActivityName);

                        RaiseOnError(timeStamp, msg, StatusMessage.MessageType.Warning, currentActivity, scope.ExecutingActivityPath);

                        reportManager.AddError(timeStamp, ErrorTypes.SequenceError, currentActivity.TranslateActivityName(), msg, scope.DeviceUnderTestInfo?.Id);
                    }
                }
                else
                {
                    multipleTableRowFillCheck.Add(scope.DeviceUnderTestInfo?.Id, rowResult.Row.Id, currentActivity.Id);
                }
            }
        }

        private void StoreValues(ObservableCollection<ExtendedRow> rows, Guid tableId)
        {
            foreach (var row in rows)
            {
                if (row != null && row.IsSet)
                {
                    goldSampleData.AddTableValue(tableId, row.InjectedRow.Id, row.InjectedRow.GetGoldSampleValues());

                }
            }
        }

        private void LogTable(MTFValidationTable table, List<MTFValidationTableRow> rowsToLog, Guid? dut)
        {
            reportManager.SaveTableAsync(table, rowsToLog, dut);
        }

        private bool evaluateConditionTerm(Term term, MTFSequenceActivity currentActivity, MTFActivityResult activityResult, ScopeData scope)
        {
            if (term == null || term is EmptyTerm)
            {
                return true;
            }

            if (term.ResultType != typeof(bool))
            {
                return false;
            }

            lock (termEvaluationLock)
            {
                DateTime timeStamp = DateTime.Now;
                bool result = (bool) evaluateTerm(term, scope);

                term.ForEachTerm<ValidationTableTerm>(x =>
                {
                    if (x.InjectedTable != null)
                    {
                        if (x.IsGoldSample)
                        {
                            StoreValues(x.Rows, x.InjectedTable.Id);
                        }

                        var validatedRows = x.GetValidatedRowsForDisplaying(timeStamp, x.InjectedTable.Id,
                            activityResult.NumberOfRepetition, scope.DeviceUnderTestInfo?.Id, out var rowsToLog);
                        if (x.InjectedTable.CheckMultipleFilling)
                        {
                            CheckValidatedRows(validatedRows, currentActivity, timeStamp, scope);
                        }

                        raiseOnNewValidateRows(validatedRows,
                            !result && (currentActivity.RepeatOnCheckOutputFailed &&
                                        currentActivity.NumberOfAttemptsOnCheckOutputFailed > 0), scope);
                        LogTable(x.InjectedTable, rowsToLog, scope.DeviceUnderTestInfo?.Id);
                    }
                });
                return result;
            }
        }

        private object prepareGenericClassInstanceInfo(object o)
        {
            if (o is GenericClassInstanceConfiguration)
            {
                return o;
            }

            var typeInfo = new MTFClientServerCommon.Helpers.TypeInfo(o.GetType().FullName);
            if (typeInfo.IsSimpleType || typeInfo.IsArrayOfSimpleType || typeInfo.IsImage)
            {
                return o;
            }

            if (o.GetType().GetCustomAttribute<MTFKnownClassAttribute>() != null)
            //if (typeInfo.IsUnknownType && MTFContext.AvailableClasses.Any(c => c.FullName == o.GetType().FullName))
            {
                return new GenericClassInstanceConfiguration(o);
            }
            if (typeInfo.IsCollection)
            {
                List<object> l = new List<object>();
                foreach (var item in (IEnumerable)o)
                {
                    l.Add(prepareGenericClassInstanceInfo(item));
                }

                return l;
            }


            try
            {
                return o.ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private Dictionary<Guid, object> methodLocks = new Dictionary<Guid, object>();
        private object lockMethodLocks = new object();
        private int executedMethodCounter;
        private Dictionary<string, List<Guid>> idsOfLazyLoadObjects = new Dictionary<string, List<Guid>>();//<activity path, list of dto ids>
        private void executeMethod(MTFSequenceActivity activity, object instance, object[] parameters, MethodInfo method, MTFActivityResult activityResult, ScopeData scope)
        {

            try
            {
                object methodLock = null;

                Guid lockId = Guid.Empty;
                //no paralel calls - lock by class type
                if (activity.ClassInfo.MTFClass.ThreadSafeLevel == ThreadSafeLevel.No)
                {
                    lockId = activity.ClassInfo.MTFClass.Id;
                }
                //can execute diferent instances in parallel
                else if (activity.ClassInfo.MTFClass.ThreadSafeLevel == ThreadSafeLevel.Class)
                {
                    lockId = activity.ClassInfo.Id;
                }
                if (lockId != Guid.Empty)
                {
                    lock (lockMethodLocks)
                    {
                        if (!methodLocks.ContainsKey(lockId))
                        {
                            methodLocks[lockId] = new object();
                        }
                    }
                    methodLock = methodLocks[lockId];
                }

                object output;
                if (methodLock == null)
                {
                    output = method.Invoke(instance, parameters);
                    if (executionState == MTFSequenceExecutionState.Aborting)
                    {
                        activityResult.Status = MTFExecutionActivityStatus.None;
                    }
                }
                else
                {
                    lock (methodLock)
                    {
                        output = method.Invoke(instance, parameters);
                    }
                }

                activityResult.ExceptionMessage = string.Empty;

                if (activity.ReturnType != "System.Void" && !string.IsNullOrEmpty(activity.ReturnType))
                {
                    lock (lastActivityResultLock)
                    {
                        scope.SetActivityResult(activity.Id, output);
                    }
                    if (output != null)
                    {
                        var typeInfo = new MTFClientServerCommon.Helpers.TypeInfo(output.GetType().FullName);
                        if (!typeInfo.IsSimpleType && !typeInfo.IsArrayOfSimpleType)
                        {
                            output = prepareGenericClassInstanceInfo(output);
                            if (output is GenericClassInstanceConfiguration)
                            {
                                addToLazyLoad((MTFDataTransferObject)output, scope.ExecutingActivityPahtAsString);
                            }
                        }
                    }
                    activityResult.ActivityResult = output;
                    activityResult.ActivityResultTypeName = activity.ReturnType;

                    AssignOutputValue(activity, output, activityResult, scope);

                    var checkOutputValueResult = checkOutputValue(activity, output, activityResult, scope);
                    if (!string.IsNullOrEmpty(checkOutputValueResult))
                    {
                        handleCheckOutputValueBehavior(activity, () => executeMethod(activity, instance, parameters, method, activityResult, scope), checkOutputValueResult, ref activityResult, scope);
                    }
                }
            }
            catch (Exception e)
            {
                scope.RemoveActivityResult(activity.Id);

                handleErrorBehavior(activity, () => executeMethod(activity, instance, parameters, method, activityResult, scope), getExceptionMessage(e), activityResult, scope);
            }
        }

        private void AssignOutputValue(MTFSequenceActivity activity, object output, MTFActivityResult activityResult, ScopeData scope)
        {
            if (activity.Variable != null)
            {
                var variable = variablesHandler.GetVariable(activity.Variable.Id);
                if (variable != null)
                {
                    variablesHandler.SetVariableValue(activity.Variable.Id, output, scope.DeviceUnderTestInfo?.Id);
                    var result = new MTFVariableActivityResult
                    {
                        ActivityIdPath = activityResult.ActivityIdPath,
                        VariableId = variable.Id,
                        Value = output == null ? string.Empty : output.ToString(),
                    };
                    RaiseActivityResult(result);
                }
            }
        }

        private string checkOutputValue(MTFSequenceActivity activity, object output, MTFActivityResult activityResult, ScopeData scope)
        {
            if (activity.Term != null)
            {
                if (!evaluateConditionTerm(activity.Term, activity, activityResult, scope))
                {
                    var stringOutput = string.Empty;
                    if (output != null)
                    {
                        if (!(output is GenericClassInstanceConfiguration))
                        {
                            stringOutput = string.Format(" Output value is {0}", output);
                        }
                    }
                    if (activity.Term is ITermErrorHandling)
                    {
                        return activity.Term.ToString();
                    }
                    return string.Format("{0} {1}", activity.Term, stringOutput);
                    //throw new Exception(string.Format("Check output value failed, {0} isn't true.{1}", activity.Term, stringOutput));
                }
            }

            return string.Empty;
        }

        private void handleCheckOutputValueBehavior(MTFSequenceActivity activity, Action repeatMethod, string errorMessage, ref MTFActivityResult activityResult, ScopeData scope)
        {
            handleCheckOutputValueBehavior(activity, repeatMethod, errorMessage, activityResult, true, true, scope);
        }

        private void handleCheckOutputValueBehavior(MTFSequenceActivity activity, Action repeatMethod, string errorMessage,
            MTFActivityResult activityResult, bool raiseErrorEvent, bool saveEx, ScopeData scope)
        {
            activityResult.Status = MTFExecutionActivityStatus.Nok;

            if (activity.RepeatOnCheckOutputFailed && activity.NumberOfAttemptsOnCheckOutputFailed > 0 &&
                (executionState == MTFSequenceExecutionState.Executing || executionState == MTFSequenceExecutionState.Pause || executionState == MTFSequenceExecutionState.AborSubSequence))
            {
                //currentActivityResult.ExceptionMessage = null;
                activityResult.NumberOfRepetition++;
                activityResult.Status = MTFExecutionActivityStatus.Warning;
                activity.NumberOfAttemptsOnCheckOutputFailed--;
                if (activity.RepeatDelayOnCheckOutputFailed > 0)
                {
                    Thread.Sleep((int)activity.RepeatDelayOnCheckOutputFailed);
                }
                //executeMethod(activity, instance, parameters, method, ref activityResult);
                if (repeatMethod != null)
                {
                    if (executionState == MTFSequenceExecutionState.Aborting || executionState == MTFSequenceExecutionState.AborSubSequence)
                    {
                        ChangeExecutionState(MTFSequenceExecutionState.Executing);
                    }
                    repeatMethod();
                }
                activity.NumberOfAttemptsOnCheckOutputFailed++;
                return;
            }

            var timeStamp = DateTime.Now;

            if (raiseErrorEvent)
            {
                reportManager.AddError(ErrorTypes.CheckOutputValue, activity.TranslateActivityName(), errorMessage, scope.DeviceUnderTestInfo?.Id);
                RaiseOnError(timeStamp, errorMessage, StatusMessage.MessageType.Error, activity, activityResult.ActivityIdPath.ToList());
            }

            if (saveEx && !activity.IsPossibilityCatch)
            {
                activityErrorsHandler.AddError(scope.DeviceUnderTestInfo?.Id, CreateActivityError(timeStamp, activity, errorMessage));
            }
            activityResult.ExceptionMessage = formatException(activity, errorMessage);

            if (activity.OnCheckOutputFailed != MTFErrorBehavior.Continue)
            {
                activityResult.CheckOutpuValueFailed = true;
            }
            if (activity.OnCheckOutputFailed == MTFErrorBehavior.AbortTest)
            {
                raiseOnNewActivityresult(activityResult, scope);
                Abort();
            }
            else if (activity.OnCheckOutputFailed == MTFErrorBehavior.HandledByParent)
            {
                raiseOnNewActivityresult(activityResult, scope);
                if (executionState != MTFSequenceExecutionState.Aborting && executionState != MTFSequenceExecutionState.Stopping && executionState != MTFSequenceExecutionState.Pause)
                {
                    //executionState = MTFSequenceExecutionState.AborSubSequence;
                    ChangeExecutionStateInternal(MTFSequenceExecutionState.AborSubSequence);
                }
            }
            else if (activity.OnCheckOutputFailed == MTFErrorBehavior.Continue)
            {
                if (executionState == MTFSequenceExecutionState.AborSubSequence)
                {
                    ChangeExecutionState(MTFSequenceExecutionState.Executing);
                }
                activityResult.CheckOutpuValueFailed = false;
            }
        }

        private void handleErrorBehavior(MTFSequenceActivity activity, Action repeatMethod, string errorMessage, MTFActivityResult activityResult, ScopeData scope)
        {
            handleErrorBehavior(activity, repeatMethod, errorMessage, activityResult, true, true, scope);
        }

        private void handleErrorBehavior(MTFSequenceActivity activity, Action repeatMethod, string errorMessage, MTFActivityResult activityResult, bool raiseErrorEvent, bool saveEx, ScopeData scope)
        {
            activityResult.Status = MTFExecutionActivityStatus.Nok;

            if (activity.Repeat && activity.NumberOfAttempts > 0 &&
                (executionState == MTFSequenceExecutionState.Executing || executionState == MTFSequenceExecutionState.Pause || executionState == MTFSequenceExecutionState.AborSubSequence))
            {
                //currentActivityResult.ExceptionMessage = null;
                activityResult.NumberOfRepetition++;
                activityResult.Status = MTFExecutionActivityStatus.Warning;
                activity.NumberOfAttempts--;
                if (activity.RepeatDelay > 0)
                {
                    Thread.Sleep((int)activity.RepeatDelay);
                }
                //executeMethod(activity, instance, parameters, method, ref activityResult);
                if (repeatMethod != null)
                {
                    if (executionState == MTFSequenceExecutionState.Aborting || executionState == MTFSequenceExecutionState.AborSubSequence)
                    {
                        ChangeExecutionState(MTFSequenceExecutionState.Executing);
                    }
                    repeatMethod();
                }
                activity.NumberOfAttempts++;
                return;
            }

            var timeStamp = DateTime.Now;
            if (raiseErrorEvent)
            {
                reportManager.AddError(ErrorTypes.ComponentError, activity.TranslateActivityName(), errorMessage, scope.DeviceUnderTestInfo?.Id);
                RaiseOnError(timeStamp, errorMessage, StatusMessage.MessageType.Error, activity, activityResult.ActivityIdPath.ToList());
            }

            if (saveEx && !activity.IsPossibilityCatch)
            {
                activityErrorsHandler.AddError(scope.DeviceUnderTestInfo?.Id, CreateActivityError(timeStamp, activity, errorMessage));
            }
            activityResult.ExceptionMessage = formatException(activity, errorMessage);

            if (activity.OnError != MTFErrorBehavior.Continue)
            {
                activityResult.ExceptionOccured = true;
            }

            if (activity.OnError == MTFErrorBehavior.AbortTest)
            {
                raiseOnNewActivityresult(activityResult, scope);
                Abort();
            }
            else if (activity.OnError == MTFErrorBehavior.HandledByParent)
            {
                raiseOnNewActivityresult(activityResult, scope);
                if (executionState != MTFSequenceExecutionState.Aborting && executionState != MTFSequenceExecutionState.Stopping && executionState != MTFSequenceExecutionState.Pause)
                {
                    //executionState = MTFSequenceExecutionState.AborSubSequence;
                    ChangeExecutionStateInternal(MTFSequenceExecutionState.AborSubSequence);
                }
            }
            else if (activity.OnError == MTFErrorBehavior.Continue)
            {
                if (executionState == MTFSequenceExecutionState.AborSubSequence)
                {
                    ChangeExecutionState(MTFSequenceExecutionState.Executing);
                }
                activityResult.ExceptionOccured = false;
            }
        }

        private string formatException(MTFSequenceActivity activity, string message)
        {

            return string.Format("{0} : Activity {1} Error: {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                SequenceLocalizationHelper.TranslateActivityPath(activity.GenerateShortReportPath()), message);
        }

        private string getExceptionMessage(Exception e)
        {
            return e.InnerException != null ? e.InnerException.Message : e.Message;
        }

        private void executeSubActivityCover(MTFSubSequenceActivity subActivity, ScopeData scope, MTFActivityResult subSeqResult)
        {
            executeSubActivity(subActivity, scope, subSeqResult);
        }

        private MTFActivityResult executeSubActivity(MTFSubSequenceActivity subActivity, ScopeData scope, MTFActivityResult subSeqResult)
        {
            //MTFActivityResult subSeqResult = new MTFActivityResult(subActivity);
            var timeStamp = DateTime.Now;

            MTFActivityResult result = null;

            subSeqResult.TimestampMs = sequenceHandlingStopwatchMs();

            switch (subActivity.ExecutionType)
            {
                case ExecutionType.ExecuteAlways:
                    {
                        result = doExecuteSubActivity(subActivity, subActivity.Activities, scope);
                        return evaluateResult(subSeqResult, result, subActivity, scope, timeStamp);
                    }

                case ExecutionType.ExecuteByCall:
                    {
                        result = doExecuteSubActivity(subActivity, subActivity.Activities, scope);
                        return evaluateResult(subSeqResult, result, subActivity, scope, timeStamp);
                    }

                case ExecutionType.ExecuteIf:
                    {
                        if (State == MTFSequenceExecutionState.DebugGoToNewPosition ||
                            evaluateConditionTerm(subActivity.Term, subActivity, subSeqResult, scope))
                        {
                            result = doExecuteSubActivity(subActivity, subActivity.Activities, scope);
                        }
                        else
                        {
                            subSeqResult.ActivityIdPath = scope.ExecutingActivityPath.ToArray();
                            raiseOnNewActivityresult(subSeqResult, scope);
                            return null;
                        }

                        return evaluateResult(subSeqResult, result, subActivity, scope, timeStamp);
                    }

                case ExecutionType.ExecuteUntil:
                    {
                        do
                        {
                            raiseOnRepeatSubSequence(scope.ExecutingActivityPath.ToArray());

                            result = doExecuteSubActivity(subActivity, subActivity.Activities, scope);
                            if (State == MTFSequenceExecutionState.Stopping || State == MTFSequenceExecutionState.Aborting ||
                                State == MTFSequenceExecutionState.DebugGoToTopPosition)
                            {
                                return null;
                            }
                        } while ((State == MTFSequenceExecutionState.Executing || State == MTFSequenceExecutionState.Pause) &&
                               evaluateConditionTerm(subActivity.Term, subActivity, result, scope));

                        return evaluateResult(subSeqResult, result, subActivity, scope, timeStamp);
                    }

                case ExecutionType.ExecuteInParallel:
                    {
                        result = doExecuteSubActivity(subActivity, subActivity.Activities, true, scope);
                        return evaluateResult(subSeqResult, result, subActivity, scope, timeStamp);
                    }

                case ExecutionType.ExecuteWhile:
                    {
                        while ((State == MTFSequenceExecutionState.Executing || State == MTFSequenceExecutionState.Pause) && evaluateConditionTerm(subActivity.Term, subActivity, result, scope))
                        {
                            raiseOnRepeatSubSequence(scope.ExecutingActivityPath.ToArray());

                            result = doExecuteSubActivity(subActivity, subActivity.Activities, scope);

                            if (State == MTFSequenceExecutionState.Stopping || State == MTFSequenceExecutionState.Aborting || State == MTFSequenceExecutionState.DebugGoToTopPosition)
                            {
                                return null;
                            }
                        }

                        if (result == null)
                        {
                            subSeqResult.ActivityIdPath = scope.ExecutingActivityPath.ToArray();

                            raiseOnNewActivityresult(subSeqResult, scope);

                            return null;
                        }

                        return evaluateResult(subSeqResult, result, subActivity, scope, timeStamp);
                    }

                case ExecutionType.ExecuteByCase:
                    {
                        bool executed = false;
                        MTFCase executedCase = null;
                        if (subActivity.Cases != null)
                        {
                            if (subActivity.VariantSwitch)
                            {
                                if (scope.SequenceVariant != null)
                                {
                                    executedCase = SequenceVariantHelper.SelectCaseByVariant(scope.SequenceVariant, subActivity.Cases);
                                }
                            }
                            else
                            {
                                var termResult = evaluateTerm(subActivity.Term, subActivity, subSeqResult, scope);
                                executedCase = subActivity.Cases.FirstOrDefault(c => Equals(c.Value, termResult)) ??
                                               subActivity.Cases.FirstOrDefault(c => c.IsDefault);
                            }

                            if (executedCase != null)
                            {
                                var start = DateTime.Now;
                                scope.ExecutingActivityPath.Add(executedCase.Id);
                                var caseResult = new MTFActivityResult
                                {
                                    ActivityId = executedCase.Id,
                                    ActivityIdPath = scope.ExecutingActivityPath.ToArray(),
                                    TimestampMs = sequenceHandlingStopwatchMs()
                                };
                                raiseOnActivityChanged(scope.ExecutingActivityPath.ToArray(), executedCase, start, scope);
                                result = doExecuteSubActivity(subActivity, executedCase.Activities, scope);
                                executed = true;
                                caseResult.Status = result.Status;
                                caseResult.ExceptionMessage = result.ExceptionMessage;
                                caseResult.ExceptionOccured = result.ExceptionOccured;
                                caseResult.CheckOutpuValueFailed = result.CheckOutpuValueFailed;
                                caseResult.NumberOfRepetition = result.NumberOfRepetition;
                                caseResult.ElapsedMs = (DateTime.Now - start).TotalMilliseconds;

                                raiseOnNewActivityresult(caseResult, scope);
                                scope.ExecutingActivityPath.Remove(executedCase.Id);
                            }
                        }

                        if (!executed)
                        {
                            subSeqResult.ActivityIdPath = scope.ExecutingActivityPath.ToArray();
                            raiseOnNewActivityresult(subSeqResult, scope);
                            return null;
                        }
                        return evaluateResult(subSeqResult, result, subActivity, scope, timeStamp);
                    }
                case ExecutionType.ExecuteOnBackground:
                {
                    var scopeBackground = scope.CreateChild();
                    scopeBackground.ExecutingActivityPath.Select(item => item).ToList();
                    scopeBackground.DisableNotifyEvents(subActivity.Id);
                    
                    var t = new Thread(() =>
                                       {
                                           result = doExecuteSubActivity(subActivity, subActivity.Activities, scopeBackground);
                                           evaluateResult(subSeqResult, result, subActivity, scopeBackground, timeStamp);
                                           scope.RemoveChild(scopeBackground);
                                       });
                    listActivitiesOnBackground.Add(t);
                    t.Start();
                    return null;
                }

            }
            return null;
        }

        private MTFActivityResult evaluateResult(MTFActivityResult subSeqResult,
            MTFActivityResult result, MTFSubSequenceActivity subActivity,
            ScopeData scope, DateTime timeStamp)
        {
            subSeqResult.ActivityIdPath = scope.ExecutingActivityPath.ToArray();
            if (subSeqResult.Status == MTFExecutionActivityStatus.None)
            {
                subSeqResult.Status = result.Status;
            }
            subSeqResult.ExceptionMessage = result.ExceptionMessage;
            subSeqResult.ExceptionOccured = result.ExceptionOccured;
            subSeqResult.CheckOutpuValueFailed = result.CheckOutpuValueFailed;
            //subSeqResult.NumberOfRepetition = result.NumberOfRepetition;
            //subSeqResult.ElapsedMs = result.ElapsedMs;

            if (result.ExceptionOccured)
            {
                var newScope = scope.CreateChild();
                handleErrorBehavior(subActivity, () => executeSubActivityCover(subActivity, newScope, subSeqResult), result.ExceptionMessage, subSeqResult, false, false, scope);
                scope.RemoveChild(newScope);
            }

            if (result.CheckOutpuValueFailed)
            {
                var newScope = scope.CreateChild();
                handleCheckOutputValueBehavior(subActivity, () => executeSubActivityCover(subActivity, newScope, subSeqResult), result.ExceptionMessage, subSeqResult, false, false, scope);
                scope.RemoveChild(newScope);
            }

            subSeqResult.ElapsedMs = (DateTime.Now - timeStamp).TotalMilliseconds;
            raiseOnNewActivityresult(subSeqResult, scope);

            return subSeqResult;
        }

        private MTFActivityResult doExecuteSubActivity(MTFSubSequenceActivity subActivity, IEnumerable<MTFSequenceActivity> innerActivities, ScopeData scope)
        {
            return doExecuteSubActivity(subActivity, innerActivities, false, scope);
        }

        private MTFActivityResult doExecuteSubActivity(MTFSubSequenceActivity subActivity, IEnumerable<MTFSequenceActivity> innerActivities, bool executeInParallel, ScopeData scope)
        {
            if ((goingToServiceMode || goingToTeachMode) && subActivity.CanExecuteService && !serviceCommandInprogress && executionState == MTFSequenceExecutionState.Executing)
            {
                //sequenceRuntimeContext.IsServiceModeActive = goingToServiceMode;
                //sequenceRuntimeContext.IsTeachModeActive = goingToTeachMode;
                goingToServiceMode = false;
                goingToTeachMode = false;

                serviceCommandInprogress = true;
                scopeForService = scope;
                executeActivityInScope(subActivity.PrepaireServiceActivity, scopeForService);
                serviceCommandInprogress = false;

                executedCommands = new List<Guid>();
                sequenceRuntimeContext.IsServiceExecutionAllowed = true;

                RaiseSerivceExecutionCommandsStateChanged(allowedCommands, toggleCommands.Where(x => x.Value).Select(x => x.Key));

                while (sequenceRuntimeContext.IsServiceModeActive || sequenceRuntimeContext.IsTeachModeActive)
                {
                    Thread.Sleep(500);
                }
                executeActivityInScope(subActivity.CleanupServiceActivity, scopeForService);
                scopeForService = null;
                sequenceRuntimeContext.IsServiceExecutionAllowed = false;

                if (subActivity.ServiceExecutionBehavior == ServiceExecutionBehavior.SkipInner)
                {
                    return null;
                }
            }

            MTFActivityResult subSeqResult = new MTFActivityResult(subActivity);
            subSeqResult.SubActivityResults = new MTFObservableCollection<MTFActivityResult>();
            //currentActivityResults.Add(subSeqResult);
            //var resultBackup = currentActivityResults;
            //currentActivityResults = subSeqResult.SubActivityResults;
            var stopwatchSubActivity = System.Diagnostics.Stopwatch.StartNew();

            if (subActivity.IsExecuteAsOneActivity)
            {
                scope.DisableEvents(subActivity.Id);
            }

            if (executeInParallel)
            {
                asyncExecution = true;
                List<Thread> activitiesInParallel = new List<Thread>();
                scope.DisableNotifyEvents(subActivity.Id);
                var activityPath = scope.ExecutingActivityPath.ToList();
                foreach (var innerActivity in innerActivities)
                {
                    var activity = innerActivity;
                    Thread t = new Thread(() =>
                                          {
                                              try
                                              {
                                                  var newScope = scope.CreateChild();
                                                  subSeqResult.SubActivityResults.Add(ExecuteActivity(activity, newScope));
                                                  scope.RemoveChild(newScope);
                                              }
                                              catch (Exception ex)
                                              {
                                                  Abort();
                                                  SystemLog.LogException(ex);
                                                  RaiseOnError(DateTime.Now, ex.Message, StatusMessage.MessageType.Error, activity, activityPath);
                                              }
                                          });
                    activitiesInParallel.Add(t);
                    t.Start();
                }

                //wait fore all activities
                foreach (Thread t in activitiesInParallel)
                {
                    t.Join();
                }
                scope.EnableNotifyEvents(subActivity.Id);
                asyncExecution = false;
            }
            else
            {
                foreach (var innerActivity in innerActivities)
                {
                    // if error occurred and catch is used -> non execute another activities in this subsequence 
                    if (actualExecuteCatch != null)
                    {
                        var parentActivity = innerActivity.Parent as MTFSubSequenceActivity;
                        if (parentActivity?.ActivityOnCatch == actualExecuteCatch)
                        {
                            break;
                        }
                    }

                    var isInnerActivitySubSequence = false;
                    if (subActivity.ActivityOnCatch != null && subActivity != subActivity.ActivityOnCatch)
                    {
                        innerActivity.IsPossibilityCatch = true;
                        if (innerActivity is MTFSubSequenceActivity activity)
                        {
                            activity.ActivityOnCatch = subActivity.ActivityOnCatch;
                            isInnerActivitySubSequence = true;
                        }
                    }

                    if (subActivity.CanExecuteService)
                    {
                        if (!goingToServiceMode && !goingToTeachMode &&
                            executionState != MTFSequenceExecutionState.Stopping &&
                            executionState != MTFSequenceExecutionState.AborSubSequence &&
                            executionState != MTFSequenceExecutionState.Aborting)
                        {
                            subSeqResult.SubActivityResults.Add(ExecuteActivity(innerActivity, scope));
                        }
                    }
                    else if (executionState != MTFSequenceExecutionState.Stopping &&
                        executionState != MTFSequenceExecutionState.AborSubSequence &&
                        executionState != MTFSequenceExecutionState.Aborting)
                    {
                        subSeqResult.SubActivityResults.Add(ExecuteActivity(innerActivity, scope));
                    }

                    if (subSeqResult.SubActivityResults?.Count > 0 && subSeqResult.SubActivityResults.Last() != null 
                        && subSeqResult.SubActivityResults.Last().ExceptionOccured && innerActivity.IsPossibilityCatch && !isInnerActivitySubSequence)
                    {
                        var catchSubSequence = createActivityToCall(subActivity);
                        this.executionState = MTFSequenceExecutionState.Executing;
                        var resultCatchSubsequence = this.ExecuteActivity(catchSubSequence, scope);
                        subSeqResult.SubActivityResults.Last().Status = resultCatchSubsequence.Status;
                  
                        // ToDo - show catch subsequence
                        //subSeqResult.SubActivityResults.Add(resultCatchSubsequence);
                        //subActivity.Activities.Add(catchSubSequence);

                        actualExecuteCatch = subActivity.ActivityOnCatch;
                        break;
                    }
                }
            }

            if (subActivity.IsExecuteAsOneActivity)
            {
                scope.EnableEvents(subActivity.Id);
            }

            if ((goingToServiceMode || goingToTeachMode) && subActivity.CanExecuteService && !serviceCommandInprogress && executionState == MTFSequenceExecutionState.Executing)
            {
                return doExecuteSubActivity(subActivity, innerActivities, executeInParallel, scope);
            }

            stopwatchSubActivity.Stop();
            subSeqResult.ElapsedMs = stopwatchSubActivity.Elapsed.TotalMilliseconds;
            //currentActivityResults = resultBackup;
            AdjustResultStatusAndExceptions(subSeqResult);

            if ((subSeqResult.ExceptionOccured && subActivity.OnError == MTFErrorBehavior.Continue && executionState == MTFSequenceExecutionState.AborSubSequence) ||
                (subSeqResult.CheckOutpuValueFailed && subActivity.OnCheckOutputFailed == MTFErrorBehavior.Continue && executionState == MTFSequenceExecutionState.AborSubSequence))
            {
                //executionState = MTFSequenceExecutionState.Executing;
                ChangeExecutionStateInternal(MTFSequenceExecutionState.Executing);
                subSeqResult.ExceptionMessage = string.Empty;
                subSeqResult.ExceptionOccured = false;
                subSeqResult.CheckOutpuValueFailed = false;
            }

            return subSeqResult;
        }

        private MTFExecuteActivity createActivityToCall(MTFSubSequenceActivity subActivity)
        {
            return new MTFExecuteActivity
              {
                  MTFClassAlias = "ExecuteSubSequence",
                  IsActive = true,
                  ActivityName = "CatchSubSequence",
                  MTFMethodName = string.Empty,
                  MTFMethodDisplayName = string.Empty,
                  SetupModeSupport = false,
                  ActivityToCall = subActivity.ActivityOnCatch,
                  DynamicMethod = new MTFStringFormat(),
                  DynamicActivityType = DynamicActivityTypes.Load,
                  DynamicSequence = new MTFStringFormat(),
                  Type = ExecuteActyvityTypes.Local,
              };
        }

        private void AdjustResultStatusAndExceptions(MTFActivityResult result)
        {
            result.Status = MTFExecutionActivityStatus.Ok;
            if (result.SubActivityResults != null)
            {
                if (result.SubActivityResults.Any(x => x != null && x.Status == MTFExecutionActivityStatus.Warning))
                {
                    result.Status = MTFExecutionActivityStatus.Warning;
                }
                if (result.SubActivityResults.Any(x => x != null && x.Status == MTFExecutionActivityStatus.Nok))
                {
                    result.Status = MTFExecutionActivityStatus.Nok;
                    var ex = result.SubActivityResults.LastOrDefault(x => x != null && !string.IsNullOrEmpty(x.ExceptionMessage));
                    if (ex != null)
                    {
                        result.ExceptionMessage = ex.ExceptionMessage;
                        result.ExceptionOccured = ex.ExceptionOccured;
                        result.CheckOutpuValueFailed = ex.CheckOutpuValueFailed;
                    }
                }
            }
        }

        private void raiseOnRepeatSubSequence(Guid[] activityPath)
        {
            lock (eventHandlingLock)
            {
                if (OnRepeatSubSequence != null)
                {
                    OnRepeatSubSequence(activityPath);
                }
            }
        }

        private void raiseOnActivityChanged(Guid[] executingActivityPath, MTFDataTransferObject dto, DateTime timeStamp, ScopeData scope)
        {
            if (!scope.RaiseEvents || !scope.RaiseNotifyEvents)
            {
                return;
            }

            lock (eventHandlingLock)
            {
                OnActivityChanged?.Invoke(executingActivityPath.ToArray());
            }
        }

        private void raiseOnNewActivityresult(MTFActivityResult result, ScopeData scope)
        {
            if (!scope.RaiseEvents)
            {
                return;
            }

            if ((executionState == MTFSequenceExecutionState.Executing ||
                executionState == MTFSequenceExecutionState.Pause ||
                executionState == MTFSequenceExecutionState.Aborting ||
                executionState == MTFSequenceExecutionState.AborSubSequence))
            {
                RaiseActivityResult(result);
            }
        }

        private void raiseOnNewValidateRows(List<MTFValidationTableRowResult> rows, bool activityWillBeRepeated, ScopeData scope)
        {
            if (rows == null || rows.Count < 1)
            {
                return;
            }
            UpdateHeadersInItems(rows);
            if ((executionState == MTFSequenceExecutionState.Executing || executionState == MTFSequenceExecutionState.Pause) && OnNewValidatiteRows != null)
            {
                lock (eventHandlingLock)
                {
                    OnNewValidatiteRows(rows, activityWillBeRepeated, scope.DeviceUnderTestInfo?.Id);
                }
            }
        }

        private void UpdateHeadersInItems(List<MTFValidationTableRowResult> rows)
        {
            foreach (var rowResult in rows)
            {
                foreach (var item in rowResult.Row.Items)
                {
                    item.Header = item.Column.Header;
                }
            }
        }

        private MethodInfo GetMethodInfo(object instance, string methodName, string classInfoAlias)
        {
            MethodInfo method = null;
            if (!string.IsNullOrEmpty(methodName))
            {
                if (instance == null)
                {
                    //get static method of static class
                    var mtfClass = componentsManager.GetType(sequence.MTFSequenceClassInfos.FirstOrDefault(ci => ci.Alias == classInfoAlias).MTFClass);
                    //TODO change
                    method = mtfClass.GetMethod(methodName);
                }
                else
                {
                    //get instance class form given instance
                    method = instance.GetType().GetMethod(methodName);
                }
                if (method == null)
                {
                    var property = instance.GetType().GetProperty(methodName.Split('.')[0]);
                    if (property != null)
                    {
                        method = methodName.EndsWith(".Set") ? property.GetSetMethod() : property.GetGetMethod();
                    }
                }
            }

            return method;
        }

        private MethodInfo getMethod(object instance, MTFSequenceActivity activity, ref MTFActivityResult activityResult, ScopeData scope)
        {
            var method = GetMethodInfo(instance, activity.MTFMethodName, activity.ClassInfo.Alias);

            activityResult.TimestampMs = sequenceHandlingStopwatchMs();
            if (method == null)
            {
                activityResult.ExceptionMessage =
                    $"Can't find method or property {activity.MTFMethodName}. Please check your sequence. Sequence execution stopped.";
                reportManager.AddError(ErrorTypes.SequenceError, activity.TranslateActivityName(), activityResult.ExceptionMessage, scope.DeviceUnderTestInfo?.Id);
                activityResult.Status = MTFExecutionActivityStatus.Nok;

                RaiseOnError(DateTime.Now, activityResult.ExceptionMessage, StatusMessage.MessageType.Error, activity, activityResult.ActivityIdPath.ToList());

                return null;
            }

            return method;
        }

        void handleTextNotification(string message, StatusMessage.MessageType type)
        {
            if (OnActivityStringProgressChanged != null)
            {
                var activityResult = getActivityResultByThread();
                RaiseActivityStringProgressChanged(new ActivityStringProgressChangedEventArgs
                {
                    Message =
                        new StatusMessage
                        {
                            ActivityNames =
                                new List<ActivityIdentifier>
                                {
                                    new ActivityIdentifier
                                    {
                                        ActivityKey = activityResult.ActivityName,
                                        UniqueIndexer = activityResult.ActivityIndexer
                                    }
                                },
                            Text = message,
                            Type = type,
                            TimeStamp = DateTime.Now,
                            ActivityPath = activityResult.ActivityIdPath.ToList()
                        },
                    ExecutionPath = getActivityPathByThread()
                });
            }
        }

        void handleProgressNotification(int percent, string text, ScopeData scope)
        {
            if (!scope.RaiseEvents || !scope.RaiseNotifyEvents)
            {
                return;
            }

            RaiseActivityPercentProgressChanged(new ActivityPercentProgressChangedEventArgs { Percent = percent, Text = text, ExecutionPath = getActivityPathByThread() });
        }


        private void HandleProgressNotificationIndeterminate(string text, bool isStarted, ScopeData scope)
        {
            if (!scope.RaiseEvents || !scope.RaiseNotifyEvents)
            {
                return;
            }

            RaiseActivityPercentProgressChanged(new ActivityPercentProgressChangedEventArgs { Percent = isStarted ? -1 : 0, Text = text, ExecutionPath = getActivityPathByThread(), IsStarted = isStarted });
        }

        void handleImageNotification(byte[] imageData, List<Guid> executionPath)
        {
            if (OnActivityImageProgressChanged != null)
            {
                if (executionPath == null)
                {
                    executionPath = getActivityPathByThread(false);
                }

                RaiseActivityImageProgressChanged(new ActivityImageProgressChangedEventArgs { ImageData = imageData, ExecutionPath = executionPath });
            }
        }

        public void SetValidationTableRow(Guid tableId, MTFValidationTableRow row)
        {
            variablesHandler.SetValidationTableRow(tableId, row);
            Task.Run(() =>
            {
                RaiseAllowSaveExecutedSequence(true);
            });
        }


        public void SetIsServiceMode(bool isServiceMode)
        {
            Task.Run(() =>
            {
                while (!isServiceMode && serviceCommandInprogress)
                {
                    Thread.Sleep(500);
                }

                if (sequenceRuntimeContext!=null)
                {
                    if (isServiceMode)
                    {
                        sequenceRuntimeContext.IsServiceModeActive = true;
                        goingToServiceMode = true;
                    }
                    else
                    {
                        sequenceRuntimeContext.IsServiceModeActive = false;
                    }
                }
            });
        }

        public void SetIsTeachingMode(bool isTeachingMode)
        {
            Task.Run(() =>
            {
                while (!isTeachingMode && serviceCommandInprogress)
                {
                    Thread.Sleep(500);
                }

                if (sequenceRuntimeContext!=null)
                {
                    if (isTeachingMode)
                    {
                        sequenceRuntimeContext.IsTeachModeActive = true;
                        goingToTeachMode = true;
                    }
                    else
                    {
                        sequenceRuntimeContext.IsTeachModeActive = false;
                    }
                }
            });
        }

        //this is execution entry point for service and user command activities
        private void executeActivityInScope(MTFSequenceActivity activity, ScopeData scope)
        {
            if (activity == null)
            {
                return;
            }

            try
            {
                var result = ExecuteActivity(activity, scope);
                if (result != null)
                {
                    if (result.ExceptionOccured)
                    {
                        if (State == MTFSequenceExecutionState.Aborting)
                        {
                            StopExecution(MTFSequenceExecutionState.Aborted);
                            return;
                        }
                        if (OnShowMessage != null)
                        {
                            var messageInfo = new MessageInfo
                            {
                                Header = "Command crashed",
                                Text = result.ExceptionMessage,
                                Type = SequenceMessageType.Error,
                                Buttons = MessageButtons.Ok,
                            };
                            OnShowMessage(messageInfo);
                        }
                        executionState = MTFSequenceExecutionState.Executing;
                    }
                    else if (result.CheckOutpuValueFailed)
                    {
                        if (State == MTFSequenceExecutionState.Aborting)
                        {
                            StopExecution(MTFSequenceExecutionState.Aborted);
                            return;
                        }
                        if (OnShowMessage != null)
                        {
                            var messageInfo = new MessageInfo
                            {
                                Header = "Command check output value failed",
                                Text = result.ExceptionMessage,
                                Type = SequenceMessageType.Error,
                                Buttons = MessageButtons.Ok,
                            };
                            OnShowMessage(messageInfo);
                        }
                        executionState = MTFSequenceExecutionState.Executing;
                    }
                }
            }
            catch (Exception e)
            {
                SystemLog.LogException(e);
                RaiseOnError(DateTime.Now, e.Message, StatusMessage.MessageType.Error, activity, scope.ExecutingActivityPath);
            }
        }



        private void RaiseOnError(DateTime timeStamp, string message, StatusMessage.MessageType type, MTFSequenceActivity activity, List<Guid> executingPath)
        {
            lock (eventHandlingLock)
            {
                if (activity != null)
                {
                    lastErrorActivity = activity;
                }
                
                Guid activityId = activity?.Id ?? Guid.Empty;

                var activityPath = executingPath ?? (getActivityPathByThread(false) ?? new List<Guid> { activityId });
                
                if (this.OnError != null)
                {
                    if (activity != null)
                    {
                        if (!activity.IsPossibilityCatch)
                        {
                            OnError(new StatusMessage
                                    {
                                        TimeStamp = timeStamp,
                                        Text = message,
                                        Type = type,
                                        ActivityNames = activity.GenerateShortReportPath(),
                                        ActivityPath = activity.GetActivityGuidPath(),
                                    });
                        }
                    }
                    else
                    {
                        OnError(new StatusMessage
                                {
                                    TimeStamp = timeStamp,
                                    Text = message,
                                    Type = type,
                                    ActivityPath = rootScopeData?.ExecutingActivityPath.ToList(),
                        });
                    }
                }

                if (sequenceRuntimeContext.IsDebugModeActive && serverSettings.PauseAfterError)
                {
                    ChangeExecutionState(MTFSequenceExecutionState.Pause);
                }

                if (type == StatusMessage.MessageType.Error && OnShowMessage != null && serverSettings.ErrorMessageType != ErrorMessageType.None)
                {
                    if (serverSettings.ErrorMessageType == ErrorMessageType.NonBlockingWindow)
                    {
                        var messageInfo = new MessageInfo
                        {
                            Header = $"Error{activity.GenerateShortReportPath()}",
                            Text = message,
                            Type = SequenceMessageType.NoBlockingMessage,
                            AdditionalType = SequenceMessageType.Error,
                            Buttons = MessageButtons.Ok,
                            ActivityPath = activityPath,
                        };
                        OnShowMessage(messageInfo);
                    }
                    else if (serverSettings.ErrorMessageType == ErrorMessageType.BlockingWindow)
                    {
                        messageBoxResultReceived = false;
                        var messageInfo = new MessageInfo
                        {
                            Header = $"Error{activity.GenerateShortReportPath()}",
                            Text = message,
                            Type = SequenceMessageType.Error,
                            Buttons = MessageButtons.Ok,
                            ActivityPath = activityPath,
                        };
                        OnShowMessage(messageInfo);
                        while (!messageBoxResultReceived)
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
            }
        }

        private List<Guid> executedCommands = new List<Guid>();
        private bool serviceCommandInprogress;
        public void ExecuteServiceCommand(Guid commandId)
        {
            Task.Run(() =>
            {
                if (sequence.ServiceCommands == null)
                {
                    return;
                }

                serviceCommandInprogress = true;
                var command = sequence.ServiceCommands.FirstOrDefault(c => c.Id == commandId);
                if (command != null)
                {
                    switch (command.Type)
                    {
                        case MTFServiceCommandType.Button:
                            executeActivityInScope(command.PrepairActivity, scopeForService);
                            executeActivityInScope(command.ExecuteActivity, scopeForService);
                            executeActivityInScope(command.CleaunupActivity, scopeForService);
                            break;
                        case MTFServiceCommandType.ToggleButton:
                            PerformToggleButton(command);
                            break;
                    }

                }

                if (!executedCommands.Contains(commandId))
                {
                    executedCommands.Add(commandId);
                }

                RaiseSerivceExecutionCommandsStateChanged(allowedCommands, toggleCommands.Where(x => x.Value).Select(x => x.Key));

                serviceCommandInprogress = false;
            });
        }

        private void PerformToggleButton(MTFServiceCommand toggleCommand)
        {
            if (!toggleCommands.ContainsKey(toggleCommand.Id))
            {
                toggleCommands[toggleCommand.Id] = true;
            }
            else
            {
                toggleCommands[toggleCommand.Id] = !toggleCommands[toggleCommand.Id];
            }
            var newState = toggleCommands[toggleCommand.Id];

            if (newState)
            {
                executeActivityInScope(toggleCommand.PrepairActivity, scopeForService);
                executeActivityInScope(toggleCommand.ExecuteActivity, scopeForService);
            }
            else
            {
                executeActivityInScope(toggleCommand.CleaunupActivity, scopeForService);
            }
        }

        public void NotifyAndStopSequence(Exception exception)
        {
            if (exception != null)
            {
                SystemLog.LogException(exception);
                RaiseOnError(DateTime.Now, exception.Message, StatusMessage.MessageType.Error, null, null);
            }
            if (sequenceRuntimeContext.IsServiceModeActive)
            {
                sequenceRuntimeContext.IsServiceModeActive = false;
            }
            if (sequenceRuntimeContext.IsTeachModeActive)
            {
                sequenceRuntimeContext.IsTeachModeActive = false;
            }
            lock (eventHandlingLock)
            {
                if (OnStateChanged != null)
                {
                    OnStateChanged(MTFSequenceExecutionState.CriticalAbort);
                }
            }
            Abort();
        }

        private void UpdateSequenceUserCommandSettings(IEnumerable<UserCommandsState> userCommandsSetting)
        {
            foreach (var commandSetting in userCommandsSetting)
            {
                var command = commandSetting.UserCommand;
                if (command != null)
                {
                    var sequenceUserCommandSettings = sequenceUserCommandsStates.FirstOrDefault(c => c.UserCommand.Id == command.Id);
                    if (sequenceUserCommandSettings == null)
                    {
                        sequenceUserCommandSettings = new UserCommandsState { UserCommand = command };
                        sequenceUserCommandsStates.Add(sequenceUserCommandSettings);
                    }
                    if (commandSetting.IsEnabled != null)
                    {
                        sequenceUserCommandSettings.IsEnabled = commandSetting.IsEnabled == true;
                    }

                    if (sequenceUserCommandSettings.UserCommand.Type == MTFUserCommandType.ToggleButton && commandSetting.IsChecked != null)
                    {
                        sequenceUserCommandSettings.IsChecked = commandSetting.IsChecked == true;
                    }
                }
            }
        }

        private IList<UserCommandsState> sequenceUserCommandsStates = new List<UserCommandsState>();
        public void ExecuteUserCommand(Guid commandId)
        {
            var command = sequenceUserCommandsStates.FirstOrDefault(c => c.UserCommand.Id == commandId);

            if (command?.UserCommand.ExecuteActivity == null || 
                (command.IsChecked == true && command.UserCommand.Type == MTFUserCommandType.ToggleButton && command.UserCommand.ToggleOffActivity == null))
            {
                return;
            }
            
            command.IsExecuting = true;
            OnUserCommandsStatusChanged?.Invoke(sequenceUserCommandsStates);

            var userCommandScope = rootScopeData.CreateChild();
            userCommandScope.DisableEvents(commandId);
            if (command.UserCommand.Type == MTFUserCommandType.Button)
            {
                executeActivityInScope(command.UserCommand.ExecuteActivity, userCommandScope);
            }

            if (command.UserCommand.Type == MTFUserCommandType.ToggleButton)
            {
                if (command.IsChecked == true)
                {
                    executeActivityInScope(command.UserCommand.ExecuteActivity, userCommandScope);
                    command.IsChecked = false;
                }
                else
                {
                    executeActivityInScope(command.UserCommand.ToggleOffActivity, userCommandScope);
                    command.IsChecked = true;
                }
            }

            userCommandScope.EnableEvents(commandId);
            rootScopeData.RemoveChild(userCommandScope);
            command.IsExecuting = false;
            OnUserCommandsStatusChanged?.Invoke(sequenceUserCommandsStates);
        }
    }

    public class GuidContainer
    {
        public Guid CurrentSequenceId { get; set; }
        public Guid ParentSequenceId { get; set; }

        public GuidContainer(Guid currentSequenceId, Guid parentsequenceId)
        {
            CurrentSequenceId = currentSequenceId;
            ParentSequenceId = parentsequenceId;
        }

        public override string ToString()
        {
            return string.Format("{0} | {1}", CurrentSequenceId, ParentSequenceId);
        }
    }
}
