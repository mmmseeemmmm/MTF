using MTFClientServerCommon;
using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.MTFValidationTable;
using System.Threading.Tasks;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.GoldSamplePersist;
using MTFClientServerCommon.GraphicalView;
using MTFClientServerCommon.Import;
using MTFClientServerCommon.SequenceLocalization;
using MTFCommon;
using MTFCore.DbReporting;
using MTFCore.Managers;
using MTFCore.Managers.Components;
using MTFCore.Services;
using MTFActivityResult = MTFClientServerCommon.MTFActivityResult;
using System.Threading;

namespace MTFCore
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    class Core : ServiceBase, IMTFCoreService, IMTFExtendedCoreService
    {
        
        private readonly Dictionary<string, MTFImportSetting> settingsToImport = new Dictionary<string, MTFImportSetting>();
        private static readonly MTFPersistToBin persist = new MTFPersistToBin();
        private MTFSequenceExecutor sequenceExecutor;
        private readonly object callbacksLock = new object();
        private readonly object treeResultCallbackLock = new object();
        private readonly object tableViewResultCallbackLock = new object();
        private readonly Dictionary<string, IMTFCoreServiceCallback> treeExecutionCallbacks = new Dictionary<string, IMTFCoreServiceCallback>();
        private readonly Dictionary<string, IMTFCoreServiceCallback> tableExecutionCallbacks = new Dictionary<string, IMTFCoreServiceCallback>();
        private readonly Dictionary<string, IMTFCoreServiceCallback> treeResultsCallbacks = new Dictionary<string, IMTFCoreServiceCallback>();
        private readonly Dictionary<string, IMTFCoreServiceCallback> tableViewResultsCallbacks = new Dictionary<string, IMTFCoreServiceCallback>();
        private Dictionary<string, MTFActivityResult> activityResults;
        private TableViewResults tableViewResults;
        private ServerSettings serverSettings;
        private bool sequenceIsFinished = true;
        private Guid[] currentExecutingPath;
        private MTFSequenceExecutionState currentExecutionState = MTFSequenceExecutionState.None;
        private IEnumerable<List<Guid>> setupModeActivityPaths;
        private IEnumerable<List<Guid>> breakPointActivityPaths;
        private bool isSetupActive;
        private bool isDebugActive;
        private bool isServiceModeActive;
        private bool isTeachModeActive;
        private string loadedSequenceName;
        private readonly object activityResultLock = new object();
        private readonly bool isDataFolderCreated;
        private readonly string previousVersionFolder;

        public event OnMessageEventHandler OnMessage;
        public delegate void OnMessageEventHandler(string header, string message, int level);

        public event RequestServerStopHandler RequestServerStop;
        public delegate void RequestServerStopHandler();

        public event ServerSettingsChangedHandler ServerSettingsChanged;
        public delegate void ServerSettingsChangedHandler(ServerSettings settings);

        public static MTFPersistToBin Persist => persist;

        public ServerSettings ServerSettings
        {
            get => serverSettings;
            set => serverSettings = value;
        }

        public Core()
        {
            isDataFolderCreated = Directory.Exists(Path.Combine(BaseConstants.DataPath, BaseConstants.MTFVersion));
            previousVersionFolder = string.Empty;
            if (Directory.Exists(BaseConstants.DataPath))
            {
                previousVersionFolder = Directory.GetDirectories(BaseConstants.DataPath)
                    .Except(new[] { BaseConstants.ServerSystemLogsPath, BaseConstants.GoldSampleBasePath,
                        Path.Combine(BaseConstants.DataPath, BaseConstants.GraphicalViewSources),
                        Path.Combine(BaseConstants.DataPath, BaseConstants.ReportImageBasePath),
                        Path.Combine(BaseConstants.DataPath, BaseConstants.ReportGraphicalViewBasePath),})
                    .OrderBy(d => d).LastOrDefault();
            }

            MTFDataTransferObject.RaiseNotifyPropertyChangedEvent = false;
            serverSettings = XmlOperations.LoadServerSetting();

            if (!isDataFolderCreated && string.IsNullOrEmpty(previousVersionFolder))
            {
                CreateDirectories();
            }

            if (isDataFolderCreated || (!isDataFolderCreated && string.IsNullOrEmpty(previousVersionFolder)))
            {
                ReportManager.InitializeDatabase();
            }
            //addLibsPaths();
        }

        public override void Init()
        {
        }

        public override IEnumerable<ServiceWcfEndpointConfiguration> EndpointConfigurations => new[]
        {
            new ServiceWcfEndpointConfiguration {Interface = typeof(IMTFCoreService), Address = ""},
            new ServiceWcfEndpointConfiguration {Interface = typeof(IMTFExtendedCoreService), Address = ""},
        };

        private void addLibsPaths()
        {
            StringBuilder sb = new StringBuilder();
            libsPaths(BaseConstants.AssembliesPath, sb);
            var path = Environment.GetEnvironmentVariable("path");
            Environment.SetEnvironmentVariable("path", sb.Append(path).ToString());
        }

        private void CreateDirectories()
        {
            FileHelper.CreateDirectory(Path.Combine(BaseConstants.DataPath, BaseConstants.ClassInstanceConfigBasePath));
            FileHelper.CreateDirectory(Path.Combine(BaseConstants.DataPath, BaseConstants.SequenceBasePath));
            FileHelper.CreateDirectory(Path.Combine(BaseConstants.DataPath, BaseConstants.LogsBasePath));

            FileHelper.CreateDirectory(Path.Combine(BaseConstants.DataPath, BaseConstants.GraphicalViewSources));
            FileHelper.CreateDirectory(Path.Combine(BaseConstants.DataPath, BaseConstants.ReportImageBasePath));
            FileHelper.CreateDirectory(Path.Combine(BaseConstants.DataPath, BaseConstants.ReportGraphicalViewBasePath));
            FileHelper.CreateDirectory(BaseConstants.GoldSampleBasePath);
        }

        private void libsPaths(string path, StringBuilder allPaths)
        {
            if (Directory.Exists(path))
            {
                allPaths.Append(path).Append(";");
                foreach (string directory in Directory.GetDirectories(path))
                {
                    libsPaths(directory, allPaths);
                }
            }
        }


        public List<MTFClassInfo> AvailableMonsterClasses() => ManagersContainer.Get<ComponentsManager>().AvailableMonsterClasses;
        
        public List<MTFClassInstanceConfiguration> ClassInstanceConfigurations(MTFClassInfo classInfo)
        {
            var instanceConfigs = persist.LoadDataList<MTFClassInstanceConfiguration>(ClassInstanceConfigPersistPath(classInfo));

            //fix if configuration is stored in old MTF and there isn't information about assembly path,
            //this can be removed when data is in DB and there isn't data redundancy in sequence file
            foreach (var instanceConfig in instanceConfigs)
            {
                if (string.IsNullOrEmpty(instanceConfig.ClassInfo.RelativePath))
                {
                    var storedMonsterClass = AvailableMonsterClasses().FirstOrDefault(m => m.FullName == instanceConfig.ClassInfo.FullName);
                    instanceConfig.ClassInfo.RelativePath = storedMonsterClass?.RelativePath;
                }
            }

            return instanceConfigs;
        }
        
        public static string ClassInstanceConfigPersistPath(MTFClassInfo classInfo) =>
            Path.Combine(BaseConstants.ClassInstanceConfigBasePath,
                classInfo.AssemblyName,
                classInfo.FullName + ".xml");



        private bool sequenceSavingInProgress;
        private bool sequenceSavingByExecutionInProgress;
        public void SaveSequence(MTFSequence sequence, string fileName, bool replaceGuids)
        {
            try
            {
                sequence.MTFVersion = new MTFClientServerCommon.Version(Assembly.GetExecutingAssembly().GetName().Version);
                sequenceSavingInProgress = true;
                sequence.LastPersistTime = DateTime.Now;
                if (replaceGuids)
                {
                    sequence.ReplaceIdentityObjects();
                    sequence.ReplaceGuids();
                }
                if (string.IsNullOrEmpty(sequence.Name))
                {
                    sequence.Name = fileName.Split('.')[0];
                }
                persist.SaveData(sequence, Path.Combine(BaseConstants.SequenceBasePath, fileName));
            }
            finally
            {
                sequenceSavingInProgress = false;
            }
        }

        public Stream OpenSequenceStream(string fileName) => persist.GetFileStream(Path.Combine(BaseConstants.SequenceBasePath, fileName));

        public IList<MTFSequenceClassInfo> LoadSequenceClassInfo(string fileName)
        {
            var sequence = loadSequence(fileName);
            return sequence?.MTFSequenceClassInfos;
        }

        [OperationBehavior(AutoDisposeParameters = true)]
        public bool ImportSequence(Stream stream)
        {
            string sessionId = OperationContext.Current.SessionId;
            if (settingsToImport.ContainsKey(sessionId))
            {
                var setting = settingsToImport[sessionId];
                settingsToImport.Remove(sessionId);
                return ImportHelper.ImportSequences(stream, setting);
            }
            return false;
        }

        public void ImportSequenceSetting(MTFImportSetting setting)
        {
            string sessionId = OperationContext.Current.SessionId;
            settingsToImport.Add(sessionId, setting);
        }

        public List<GraphicalViewImg> SaveGraphicalViewImages(List<GraphicalViewImg> images)
        {
            try
            {
                return GraphicalViewFileHelper.SaveGraficalViewImages(images);
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine(ex.Message);
                if (ex.InnerException != null)
                {
                    sb.AppendLine(ex.InnerException.Message);
                }
                throw new FaultException(sb.ToString());
            }
        }

        public List<GraphicalViewImg> LoadGraphicalViewImages(bool loadAll, IEnumerable<string> fileNames)
        {
            try
            {
                return loadAll ? GraphicalViewFileHelper.LoadAllGraphicalViewImages() : GraphicalViewFileHelper.LoadGraphicalViewImages(fileNames);
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine(ex.Message);
                if (ex.InnerException != null)
                {
                    sb.AppendLine(ex.InnerException.Message);
                }
                throw new FaultException(sb.ToString());
            }
        }

        public List<string> GetGraphicalViewImageNames() => GraphicalViewFileHelper.GetGraphicalViewImageNames()?.ToList();

        public Guid LastSequencePersistId(string fileName) => loadSequence(fileName).LastPersistId;

        private MTFSequence loadSequence(string fileName)
        {
            var sequence = persist.LoadData<MTFSequence>(Path.Combine(BaseConstants.SequenceBasePath, fileName));
            if (sequence != null)
            {
                sequence.ReplaceIdentityObjects();
                UpdateSequenceClassInfos(sequence);
                sequence.IsModified = false;
            }
            return sequence;
        }

        public string CreateDirectory(string path, string newDirName, bool useDefaultMtfDirectory) => FileOperation.CreateNewFolder(path, newDirName, useDefaultMtfDirectory);

        public string RenameItem(string newName, string oldFullName, string root, bool useDefaultMtfDirectory) => FileOperation.RenameItem(newName, oldFullName, root, useDefaultMtfDirectory);

        public void RemoveFile(string name, string root, bool useDefaultMtfDirectory) => FileOperation.RemoveFile(name, root, useDefaultMtfDirectory);

        public void RemoveDirectory(string name, string root, bool useDefaultMtfDirectory) => FileOperation.RemoveDirectory(name, root, useDefaultMtfDirectory);

        public string GetServerFullDirectoryPath(string relativePath) => FileOperation.GetServerFullDirectoryPath(relativePath);

        public List<MTFPersistDataInfo> GetSequencesInfo(string basePath, string path) => persist.GetPersistInfo(Path.Combine(basePath, path));

        public List<MTFPersistDataInfo> GetServerFileAndFolders(string path, bool getFiles) => FileOperation.GetAllServerItems(path, getFiles);

        public void SetIsSetupMode(bool isSetupMode) => sequenceExecutor?.SetIsSetupMode(isSetupMode);

        public void AddSetupActivityPath(List<Guid> activityPath) => sequenceExecutor?.AddSetupActivityPath(activityPath);

        public void RemoveSetupActivityPath(List<Guid> activityPath) => sequenceExecutor?.RemoveSetupActivityPath(activityPath);

        public void SetIsDebugMode(bool isDebugMode) => sequenceExecutor?.SetIsDebugMode(isDebugMode);

        public void AddBreakPointActivityPath(List<Guid> activityPath) => sequenceExecutor?.AddBreakPointActivityPath(activityPath);

        public void RemoveBreakPointActivityPath(List<Guid> activityPath) => sequenceExecutor?.RemoveBreakPointActivityPath(activityPath);

        public void StartSequence(string sequenceName, bool isSetupMode, IEnumerable<List<Guid>> setupModeActivityPaths, bool isDebug, IEnumerable<List<Guid>> breakPointActivityPaths, bool isServiceMode, bool isTeachMode)
        {
            isSetupActive = isSetupMode;
            this.setupModeActivityPaths = setupModeActivityPaths;
            isDebugActive = isDebug;
            this.breakPointActivityPaths = breakPointActivityPaths;
            isServiceModeActive = isServiceMode;
            isTeachModeActive = isTeachMode;

            PrepaireSequence(sequenceName);
        }

        public void ResultReuqest(ResultRequestTypes resultType)
        {
            switch (resultType)
            {
                case ResultRequestTypes.TreeResults:
                    lock (treeResultCallbackLock)
                    {
                        treeResultsCallbacks[OperationContext.Current.SessionId] = OperationContext.Current.GetCallbackChannel<IMTFCoreServiceCallback>();
                    }
                    GetTreeResults();
                    break;
                case ResultRequestTypes.TableResults:
                    lock (tableViewResultCallbackLock)
                    {
                        tableViewResultsCallbacks[OperationContext.Current.SessionId] = OperationContext.Current.GetCallbackChannel<IMTFCoreServiceCallback>();
                    }
                    GetTableViewResults();
                    break;
            }
        }

        public Dictionary<Guid, object> GetActualVariableValues(IEnumerable<Guid> variables) => sequenceExecutor?.GetActualVariableValues(variables);

        public void DebugStepOver() => sequenceExecutor?.DebugStepOver();

        public void DebugStepInto() => sequenceExecutor?.DebugStepInto();

        public void DebugStepOut() => sequenceExecutor?.DebugStepOut();

        public void SetNewExecutionPointer(Guid[] executionPath) => sequenceExecutor?.SetNewExecutionPointer(executionPath);

        public void SetNewVariableValue(Guid variableId, object value) => sequenceExecutor?.SetNewVariableValue(variableId, value);

        public GenericClassInstanceConfiguration GetObjectFormLazyLoadCache(Guid id)
        {
            if (MTFDataTransferObject.LazyLoadCache.ContainsKey(id))
            {
                MTFDataTransferObject.LazyLoadCache[id].IsLazyLoad = false;
                return MTFDataTransferObject.LazyLoadCache[id] as GenericClassInstanceConfiguration;
            }
            return null;
        }


        private void PrepaireSequence(string sequenceName)
        {
            activityResults = new Dictionary<string, MTFActivityResult>();
            tableViewResults = new TableViewResults();
            if (!sequenceIsFinished)
            {
                sequenceExecutor.Start(isSetupActive, setupModeActivityPaths, isDebugActive, breakPointActivityPaths, isServiceModeActive, isTeachModeActive, loggedUsers);
                return;
            }

            sequenceIsFinished = false;
            raiseExeucutionEvents = true;

            loadedSequenceName = sequenceName;
            Message("Sequence starting", "Sequence " + loadedSequenceName + " is loaded and will be started.", 0);

            sequenceExecutor?.CleanLazyLoadCache();
            PrepaireSequenceToStart(sequenceName);
        }

        public void StopSequence() => sequenceExecutor?.Stop();

        public void PauseSequence() => sequenceExecutor?.Pause();

        public string GetExecutingSequenceName() => loadedSequenceName;

        public Guid[] GetExecutingActivityPath() => currentExecutingPath;

        public MTFSequenceExecutionState GetSequenceExecutingState() => currentExecutionState;

        

        public void ClientUISendData(byte[] data, ClientUIDataInfo info) => sequenceExecutor?.ClientUIDataSend(data, info);

        private Dictionary<string, string> loggedUsers = new Dictionary<string, string>();
        public void LogedUserChanged(string userName)
        {
            try
            {
                loggedUsers[OperationContext.Current.SessionId] = userName;
                sequenceExecutor?.LogedUserChanged(OperationContext.Current.SessionId, userName);
            }
            catch (Exception e)
            {
                SystemLog.LogException(e);
            }
        }

        public void SetupControlClosed(OpenClientSetupControlArgs args)
        {
            try
            {
                sequenceExecutor?.ClientSetupControlClosed(args);
            }
            catch (Exception e)
            {
                SystemLog.LogException(e);
            }
        }

        public List<MTFActivityResult> GetMTFActivityResult()
        {
            lock (activityResultLock)
            {
                return activityResults?.Values.ToList();
            }
        }

        public DataMigrationInfo CheckPossibleDataMigration()
        {
            return new DataMigrationInfo
            {
                MigrationPossible = !isDataFolderCreated,
                PreviousMTFVersion = previousVersionFolder.Split('\\').Last(),
            };
        }

        public Task DoDataMigration(DataMigrationType dataMigrationType)
        {
            return Task.Run(() => {
                switch (dataMigrationType)
                {
                    case DataMigrationType.DoNothing: CreateDirectories(); break;
                    case DataMigrationType.Copy: FileHelper.CopyDirectory(previousVersionFolder, Path.Combine(BaseConstants.DataPath, BaseConstants.MTFVersion)); break;
                    case DataMigrationType.Move: FileHelper.MoveDirectory(previousVersionFolder, Path.Combine(BaseConstants.DataPath, BaseConstants.MTFVersion)); ; break;
                }
                ReportManager.InitializeDatabase();
            });
        }

        private void cleanCallbacks()
        {
            lock (callbacksLock)
            {
                var removeKeys = treeExecutionCallbacks.Keys.Where(k => ((ICommunicationObject)treeExecutionCallbacks[k]).State != CommunicationState.Opened).ToArray();
                foreach (string key in removeKeys)
                {
                    treeExecutionCallbacks.Remove(key);
                }

                removeKeys = tableExecutionCallbacks.Keys.Where(k => ((ICommunicationObject)tableExecutionCallbacks[k]).State != CommunicationState.Opened).ToArray();
                foreach (string key in removeKeys)
                {
                    tableExecutionCallbacks.Remove(key);
                }
            }
        }

        private IEnumerable<IMTFCoreServiceCallback> allCallbacks()
        {
            lock (callbacksLock)
            {
                return treeExecutionCallbacks.Where(p => ((ICommunicationObject)p.Value).State == CommunicationState.Opened).Select(p => p.Value)
                    .Concat(tableExecutionCallbacks.Where(p => !treeExecutionCallbacks.ContainsKey(p.Key) && ((ICommunicationObject)p.Value).State == CommunicationState.Opened).Select(p => p.Value)).ToArray();
            }
        }

        private IEnumerable<IMTFCoreServiceCallback> GetOpenedTreeCallbacks()
        {
            lock (callbacksLock)
            {
                return treeExecutionCallbacks.Where(p => ((ICommunicationObject)p.Value).State == CommunicationState.Opened)
                    .Select(p => p.Value);
            }
        }

        private IEnumerable<IMTFCoreServiceCallback> GetOpenedTableCallbacks()
        {
            lock (callbacksLock)
            {
                return tableExecutionCallbacks.Where(p => ((ICommunicationObject)p.Value).State == CommunicationState.Opened)
                    .Select(p => p.Value);
            }
        }

        private void GetTreeResults()
        {
            IMTFCoreServiceCallback callback = null;
            try
            {

                lock (treeResultCallbackLock)
                {
                    callback = treeResultsCallbacks[OperationContext.Current.SessionId];
                }
                if (callback != null)
                {
                    lock (activityResultLock)
                    {
                        callback.OnTreeResults(activityResults?.Values.ToArray()); 
                    }
                }
                lock (treeResultCallbackLock)
                {
                    treeResultsCallbacks.Remove(OperationContext.Current.SessionId);
                }
            }
            catch (ArgumentException)
            {
                if (callback != null)
                {
                    callback.OnTreeResults(null);
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconected!");
                SystemLog.LogException(e);
            }
        }

        private void GetTableViewResults()
        {
            try
            {
                IMTFCoreServiceCallback callback;
                lock (treeResultCallbackLock)
                {
                    callback = tableViewResultsCallbacks[OperationContext.Current.SessionId];
                }

                callback?.OnTableViewResults(tableViewResults?.GetAllResultsByTables());

                lock (treeResultCallbackLock)
                {
                    tableViewResultsCallbacks.Remove(OperationContext.Current.SessionId);
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconected!");
                SystemLog.LogException(e);
            }
        }

        private void UpdateSequenceClassInfos(MTFSequence sequence)
        {
            if (sequence.MTFSequenceClassInfos != null)
            {
                foreach (var classInfo in sequence.MTFSequenceClassInfos)
                {
                    var cl = AvailableMonsterClasses().FirstOrDefault(c => c.FullName == classInfo.MTFClass.FullName);
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
                sequence.IsModified = false;
            }
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

        private static void UpdateActivitiesByClassInfo(MTFSequenceActivity activity, MTFClassInfo classInfo)
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

        private void Message(string header, string message, int level) => OnMessage?.Invoke(header, message, level);

        public ServerSettings GetServerSettings() => serverSettings;

        public void SaveServerSettings() => XmlOperations.SaveXmlData(BaseConstants.ServerSettingsPath, serverSettings);

        public void UpdateServerSettings(ServerSettings settings)
        {
            serverSettings = settings;
            ServerSettingsChanged?.Invoke(serverSettings);
        }

        private bool raiseExeucutionEvents = true;
        private bool stopServerAfterStopSequence;

        public string RequestStopServer()
        {
            if (sequenceSavingByExecutionInProgress || sequenceSavingInProgress)
            {
                return "Storing a sequence in progress, try closing the application later";
            }

            cleanCallbacks();
            if (treeExecutionCallbacks.Keys.Count <= 1 || tableExecutionCallbacks.Keys.Count <= 1)
            {
                if (currentExecutionState == MTFSequenceExecutionState.Executing)
                {
                    stopServerAfterStopSequence = true;
                    raiseExeucutionEvents = false;

                    if (isServiceModeActive)
                    {
                        SetIsServiceMode(false);
                    }

                    if (isTeachModeActive)
                    {
                        SetIsTeachingMode(false);
                    }

                    StopSequence();

                    return string.Empty;
                }
                if (RequestServerStop != null)
                {
                    RequestServerStop();
                }
                else
                {
                    Message("Server stop requested", "Server stop was requested, but hosting application don't handled RequestServerStop event.", 1);
                }
            }
            return string.Empty;
        }

        public void SendMessageBoxResult(MTFDialogResult dialogResult, List<Guid> executingActivityPath)
        {

            sequenceExecutor?.SendMessageBoxResult(dialogResult, executingActivityPath);
            //send message to close dialog to all clients
            foreach (var client in allCallbacks())
            {
                Task.Run(() => { client.OnCloseMessage(executingActivityPath); });
            }
        }

        public void SetupVariantSelectionResult(IEnumerable<SetupVariantSelectionResult> variantSelectionResults) => sequenceExecutor?.SetSetupVariantSelection(variantSelectionResults);

        public void PrepaireSequenceToStart(string sequenceName)
        {
            MTFDataTransferObject.RaiseNotifyPropertyChangedEvent = true;
            var sequence = LoadSequenceProject(sequenceName);

            var currentVersion = new MTFClientServerCommon.Version(1, 8, 2, 0);
            if (sequence.MTFVersion == null || sequence.MTFVersion < currentVersion)
            {
                if (sequence.ExternalSubSequences != null)
                {
                    var list = new List<MTFSequence> { sequence };
                    list.AddRange(sequence.ExternalSubSequences.Select(x => x.ExternalSequence));
                    VersionConvertHelper.ConvertCallActivities(list);
                }
            }
            MTFDataTransferObject.RaiseNotifyPropertyChangedEvent = false;

            UnregisterExecutorEvents();

            sequenceExecutor = new MTFSequenceExecutor(sequence, ClassInstanceConfigurations, serverSettings);
            sequenceExecutor.SaveSequneceCallback = (s, modifiedByUser) =>
            {
                try
                {
                    sequenceSavingByExecutionInProgress = true;
                    sequence.LastPersistId = Guid.NewGuid();
                    foreach (var seq in s)
                    {
                        seq.SequenceVersion.IncreaseRevision();
                        seq.AddChange(modifiedByUser);

                        SaveSequence(seq, seq.FullPath, false);
                    }
                }
                catch (Exception ex)
                {
                    sequenceExecutor.NotifyAndStopSequence(ex);
                }
                finally
                {
                    sequenceSavingByExecutionInProgress = false;
                }
            };

            sequenceExecutor.OnError += OnSequenceExecutionError;
            sequenceExecutor.OnFinish += OnSequenceExecutionFinish;
            sequenceExecutor.OnActivityChanged += OnSequenceExecutionActivityChanged;
            sequenceExecutor.OnActivityPercentProgressChanged += OnSequenceExecutionActivityPercentProgressChanged;
            sequenceExecutor.OnActivityStringProgressChanged += OnSequenceExecutionActivityStringProgressChanged;
            sequenceExecutor.OnActivityImageProgressChanged += OnSequenceExecutionActivityImageProgressChanged;
            sequenceExecutor.OnStateChanged += OnSequenceExecutionStateChanged;
            sequenceExecutor.OnNewActivityResult += OnSequenceExecutionNewActivityResult;
            sequenceExecutor.OnNewValidatiteRows += OnSequenceExecutionNewValidateRows;
            sequenceExecutor.OnRepeatSubSequence += OnSequenceExecutionRepeatSubSequence;
            sequenceExecutor.OnSequenceStatusMessage += OnSequenceExecutionSequenceStatusMessage;
            sequenceExecutor.OnShowMessage += OnSequenceExecutionShowMessage;
            sequenceExecutor.OnClearValidationTables += OnClearValidationTables;
            sequenceExecutor.OnShowSetupVariantSelection += OnSequenceExecutionShowSetupVariantSelection;
            sequenceExecutor.OnSequenceVariantChanged += OnSequenceExecutionVariantChanged;
            sequenceExecutor.OnSerivceExecutionCommandsStateChanged += OnSerivceExecutionCommandsStateChanged;
            sequenceExecutor.OnLoadGoldSamples += OnSequenceExecutionLoadGoldSamples;
            sequenceExecutor.OnAllowSaveExecutedSequence += OnSequenceExecutionAllowSaveExecutedSequence;
            sequenceExecutor.OnUIControlSendData += OnClientUIDataSend;
            sequenceExecutor.OnDynamicLoadSequence += OnSequenceExecutionDynamicLoadSequence;
            sequenceExecutor.OnDynamicUnloadSequence += OnSequenceExecutionDynamicUnLoadSequence;
            sequenceExecutor.OnDynamicExecuteSequence += OnSequenceExecutionDynamicExecuteSequence;
            sequenceExecutor.OnCloseMessage += OnSequenceExecutionCloseMessage;
            sequenceExecutor.OnOpenClientSetupControl += OnSequenceExecutionOpenClientSetupControl;
            sequenceExecutor.OnUserCommandsStatusChanged += SequenceExecutorUserCommandsStatusChanged;
            sequenceExecutor.OnUserIndicatorValueChanged += SequenceExecutorUserIndicatorValueChanged;
            sequenceExecutor.OnViewChanged += SequenceExecutorOnOnViewChanged;

            sequenceExecutor?.Start(isSetupActive, setupModeActivityPaths, isDebugActive, breakPointActivityPaths, isServiceModeActive, isTeachModeActive, loggedUsers);
        }

        private void SequenceExecutorOnOnViewChanged(SequenceExecutionViewType view, Guid? graphicalViewId, Guid? dutId)
        {
            try
            {
                foreach (var client in allCallbacks())
                {
                    try
                    {
                        client.OnSequenceExecutionViewChanged(view, graphicalViewId, dutId);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconected!");
                SystemLog.LogException(ex);
            }
        }

        private void UnregisterExecutorEvents()
        {
            if (sequenceExecutor == null)
            {
                return;
            }
            sequenceExecutor.OnError -= OnSequenceExecutionError;
            sequenceExecutor.OnFinish -= OnSequenceExecutionFinish;
            sequenceExecutor.OnActivityChanged -= OnSequenceExecutionActivityChanged;
            sequenceExecutor.OnActivityPercentProgressChanged -= OnSequenceExecutionActivityPercentProgressChanged;
            sequenceExecutor.OnActivityStringProgressChanged -= OnSequenceExecutionActivityStringProgressChanged;
            sequenceExecutor.OnActivityImageProgressChanged -= OnSequenceExecutionActivityImageProgressChanged;
            sequenceExecutor.OnStateChanged -= OnSequenceExecutionStateChanged;
            sequenceExecutor.OnNewActivityResult -= OnSequenceExecutionNewActivityResult;
            sequenceExecutor.OnNewValidatiteRows -= OnSequenceExecutionNewValidateRows;
            sequenceExecutor.OnRepeatSubSequence -= OnSequenceExecutionRepeatSubSequence;
            sequenceExecutor.OnSequenceStatusMessage -= OnSequenceExecutionSequenceStatusMessage;
            sequenceExecutor.OnShowMessage -= OnSequenceExecutionShowMessage;
            sequenceExecutor.OnClearValidationTables -= OnClearValidationTables;
            sequenceExecutor.OnShowSetupVariantSelection -= OnSequenceExecutionShowSetupVariantSelection;
            sequenceExecutor.OnSequenceVariantChanged -= OnSequenceExecutionVariantChanged;
            sequenceExecutor.OnSerivceExecutionCommandsStateChanged -= OnSerivceExecutionCommandsStateChanged;
            sequenceExecutor.OnLoadGoldSamples -= OnSequenceExecutionLoadGoldSamples;
            sequenceExecutor.OnAllowSaveExecutedSequence -= OnSequenceExecutionAllowSaveExecutedSequence;
            sequenceExecutor.OnUIControlSendData -= OnClientUIDataSend;
            sequenceExecutor.OnDynamicLoadSequence -= OnSequenceExecutionDynamicLoadSequence;
            sequenceExecutor.OnDynamicUnloadSequence -= OnSequenceExecutionDynamicUnLoadSequence;
            sequenceExecutor.OnDynamicExecuteSequence -= OnSequenceExecutionDynamicExecuteSequence;
            sequenceExecutor.OnCloseMessage -= OnSequenceExecutionCloseMessage;
            sequenceExecutor.OnOpenClientSetupControl -= OnSequenceExecutionOpenClientSetupControl;
            sequenceExecutor.OnUserCommandsStatusChanged -= SequenceExecutorUserCommandsStatusChanged;
            sequenceExecutor.OnUserIndicatorValueChanged -= SequenceExecutorUserIndicatorValueChanged;
            sequenceExecutor.OnViewChanged -= SequenceExecutorOnOnViewChanged;
            sequenceExecutor.SaveSequneceCallback = null;
        }

        private MTFSequence LoadSequenceProject(string sequenceName)
        {
            var sequence = loadSequence(sequenceName);
            if (sequence != null)
            {
                sequence.FullPath = sequenceName;
                if (sequence.ExternalSubSequencesPath != null && sequence.ExternalSubSequencesPath.Count > 0)
                {
                    sequence.ExternalSubSequences = new List<MTFExternalSequenceInfo>();
                    foreach (var item in sequence.ExternalSubSequencesPath)
                    {
                        sequence.ExternalSubSequences.Add(new MTFExternalSequenceInfo(LoadSequenceProject(DirectoryPathHelper.GetFullPathFromRelative(sequenceName, item.Key)), item.Value));
                    }
                }
            }
            return sequence;
        }

        public void ChangeExecutionEventListener(SequenceExecutionEventType[] listenersToAdd, SequenceExecutionEventType[] listenersToRemove)
        {
            var context = OperationContext.Current;
            if (context != null)
            {
                IEnumerable<SequenceExecutionEventType> toRemove = listenersToRemove;

                if (listenersToAdd != null && listenersToRemove != null)
                {
                    toRemove = listenersToRemove.Except(listenersToAdd).ToArray();
                }

                if (listenersToAdd != null)
                {
                    if (listenersToAdd.Contains(SequenceExecutionEventType.Tree))
                    {
                        lock (callbacksLock)
                        {
                            treeExecutionCallbacks[OperationContext.Current.SessionId] = OperationContext.Current.GetCallbackChannel<IMTFCoreServiceCallback>();
                        }
                    }
                    if (listenersToAdd.Contains(SequenceExecutionEventType.Table))
                    {
                        lock (callbacksLock)
                        {
                            tableExecutionCallbacks[OperationContext.Current.SessionId] = OperationContext.Current.GetCallbackChannel<IMTFCoreServiceCallback>();
                        }
                    }
                }

                if (toRemove != null)
                {
                    if (toRemove.Contains(SequenceExecutionEventType.Tree))
                    {
                        lock (callbacksLock)
                        {
                            if (treeExecutionCallbacks.ContainsKey(OperationContext.Current.SessionId))
                            {
                                treeExecutionCallbacks.Remove(OperationContext.Current.SessionId);
                            }
                        }
                    }
                    if (toRemove.Contains(SequenceExecutionEventType.Table))
                    {
                        lock (callbacksLock)
                        {
                            if (tableExecutionCallbacks.ContainsKey(OperationContext.Current.SessionId))
                            {
                                tableExecutionCallbacks.Remove(OperationContext.Current.SessionId);
                            }
                        }
                    }
                }
            }
            cleanCallbacks();
        }

        private void OnSequenceExecutionError(StatusMessage errorMessage)
        {
            //tell client throw callback object about error
            foreach (var client in allCallbacks())
            {
                try
                {
                    client.OnSequenceExecutionError(errorMessage);
                }
                catch (Exception ex)
                {
                    SystemLog.LogException(ex);
                }
            }
            //raise local enent for server GUI
            Message("Sequence Execution Error",$"{errorMessage.TimeStamp} {SequenceLocalizationHelper.TranslateActivityPath(errorMessage.ActivityNames)} : {errorMessage.Text}", 2);
        }

        private void OnSequenceExecutionFinish(MTFSequenceResult sequenceResult)
        {
            sequenceIsFinished = true;
            raiseExeucutionEvents = false;

            Message("Sequence finished", "Sequence " + loadedSequenceName + " is finished.", 0);
            loadedSequenceName = string.Empty;
            currentExecutingPath = null;
            currentExecutionState = MTFSequenceExecutionState.None;

            //tell client throw callback object about sequence is finished
            foreach (var client in allCallbacks())
            {
                client.OnSequenceFinished(sequenceResult);
            }

            if (stopServerAfterStopSequence)
            {
                RequestStopServer();
            }
        }

        private void OnSequenceExecutionActivityChanged(Guid[] executingActivityPath)
        {
            if (!raiseExeucutionEvents)
            {
                return;
            }
            try
            {
                currentExecutingPath = executingActivityPath;
                foreach (var client in allCallbacks())
                {
                    try
                    {
                        client.OnSequenceActivityChanged(executingActivityPath);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogException(ex);
                    }
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconected!");
                SystemLog.LogException(e);
            }
        }

        private void OnSequenceExecutionActivityPercentProgressChanged(ActivityPercentProgressChangedEventArgs e)
        {
            if (!raiseExeucutionEvents)
            {
                return;
            }
            try
            {
                foreach (var client in allCallbacks())
                {
                    try
                    {
                        client.OnActivityPercentProgress(e);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(ex);
            }
        }

        private void OnSequenceExecutionActivityStringProgressChanged(ActivityStringProgressChangedEventArgs e)
        {
            if (!raiseExeucutionEvents)
            {
                return;
            }
            try
            {
                foreach (var client in allCallbacks())
                {
                    try
                    {
                        client.OnActivityStringProgress(e);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(ex);
            }
        }

        private void OnSequenceExecutionActivityImageProgressChanged(ActivityImageProgressChangedEventArgs e)
        {
            if (!raiseExeucutionEvents)
            {
                return;
            }
            try
            {
                foreach (var client in allCallbacks())
                {
                    try
                    {
                        client.OnActivityImageProgress(e);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogException(ex);
                    }

                }
            }
            catch (Exception ex)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(ex);
            }
        }

        private void OnSequenceExecutionStateChanged(MTFSequenceExecutionState newState)
        {
            if (!raiseExeucutionEvents)
            {
                return;
            }
            try
            {
                //lock (callbacksLock)
                {
                    currentExecutionState = newState;
                    foreach (var client in allCallbacks())
                    {
                        try
                        {
                            client.OnSequenceExecutionStateChanged(newState);
                        }
                        catch (Exception ex)
                        {
                            SystemLog.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        private void OnSequenceExecutionNewActivityResult(MTFActivityResult result)
        {
            activityResults[string.Join("->", result.ActivityIdPath)] = result;

            if (!raiseExeucutionEvents)
            {
                return;
            }
            try
            {
                foreach (var c in GetOpenedTreeCallbacks())
                {
                    try
                    {
                        c.OnNewActivityResult(result);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogException(ex);
                    }
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        private void OnSequenceExecutionNewValidateRows(List<MTFValidationTableRowResult> rows, bool activityWillBeRepeated, Guid? dutId)
        {
            if (rows == null || rows.Count < 1)
            {
                return;
            }

            //lock (callbacksLock)
            {
                tableViewResults.UpdateTableRows(rows, dutId);
                //time view rows handling
                //similiar algoritm is on client in ExecutionGloalTable in method AddNewRows
                if (!activityWillBeRepeated)
                {
                    var newRows = rows.Where(r => r.IsValidated).ToList();
                    if (newRows.Count > 0)
                    {
                        newRows[0] = newRows[0].Clone() as MTFValidationTableRowResult;
                        newRows[0].HasTimeStamp = true;
                        newRows[0].TimeStamp = rows.First(r => r.HasTimeStamp).TimeStamp;
                    }
                }

                if (!raiseExeucutionEvents)
                {
                    return;
                }
                try
                {
                    foreach (var c in GetOpenedTableCallbacks())
                    {
                        try
                        {
                            c.OnNewValidateRows(rows, activityWillBeRepeated, dutId);
                        }
                        catch (Exception ex)
                        {
                            SystemLog.LogException(ex);
                        }
                    }
                }
                catch (Exception e)
                {
                    SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                    SystemLog.LogException(e);
                }
            }
        }

        private void OnSequenceExecutionRepeatSubSequence(Guid[] executingActivityPath)
        {
            try
            {
                string repeatPrefix = string.Join("->", executingActivityPath);
                var keysToRemove = activityResults.Keys.Where(k => k.StartsWith(repeatPrefix)).ToArray();
                foreach (var key in keysToRemove)
                {
                    activityResults.Remove(key);
                }

                if (!raiseExeucutionEvents)
                {
                    return;
                }

                //lock (callbacksLock)
                {
                    foreach (var client in allCallbacks())
                    {
                        try
                        {
                            client.RepeatSubSequence(executingActivityPath);
                        }
                        catch (Exception ex)
                        {
                            SystemLog.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        private void OnSequenceExecutionSequenceStatusMessage(string line1, string line2, string line3, StatusLinesFontSize fontSize, Guid? dutId)
        {
            if (!raiseExeucutionEvents)
            {
                return;
            }
            try
            {
                foreach (var client in allCallbacks())
                {
                    try
                    {
                        client.OnSequenceStatusMessage(line1, line2, line3, fontSize, dutId);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogException(ex);
                    }
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        private void OnSequenceExecutionShowMessage(MessageInfo messageInfo)
        {
            if (!raiseExeucutionEvents)
            {
                return;
            }
            try
            {
                foreach (var client in allCallbacks())
                {
                    try
                    {
                        client.OnShowMessage(messageInfo);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogException(ex);
                    }
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        private void OnSequenceExecutionCloseMessage(Guid activityId)
        {
            foreach (var client in allCallbacks())
            {
                Task.Run(() => { client.OnCloseMessage(new List<Guid> { activityId }); });
            }
        }

        private void OnSequenceExecutionOpenClientSetupControl(OpenClientSetupControlArgs args)
        {
            foreach (var client in allCallbacks())
            {
                client.OnSequenceExecutionOpenClientSetupControl(args);
            }
        }

        private void OnClearValidationTables(bool clearAllTables, List<Guid> tablesForClearing, Guid? dutId)
        {
            if (!raiseExeucutionEvents)
            {
                return;
            }
            try
            {
                if (clearAllTables)
                {
                    tableViewResults.Clear(dutId);
                }
                else
                {
                    foreach (var tableId in tablesForClearing)
                    {
                        tableViewResults.RemoveTable(dutId, tableId);
                    }
                }

                foreach (var client in allCallbacks())
                {
                    try
                    {
                        client.OnClearValidationTables(clearAllTables, tablesForClearing, dutId);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogException(ex);
                    }
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        private void OnSequenceExecutionShowSetupVariantSelection(string activityName, Dictionary<string, IEnumerable<SequenceVariant>> dataVariants, Dictionary<string, string> extendetUsedDataNames)
        {
            if (!raiseExeucutionEvents)
            {
                return;
            }
            foreach (var client in allCallbacks())
            {
                try
                {
                    client.OnShowSetupVariantSelection(activityName, dataVariants, extendetUsedDataNames);
                }
                catch (Exception ex)
                {
                    SystemLog.LogException(ex);
                }
            }
        }

        private void OnSequenceExecutionLoadGoldSamples(List<SequenceVariantInfo> goldSampleList)
        {
            if (!raiseExeucutionEvents)
            {
                return;
            }
            try
            {
                //lock (callbacksLock)
                {
                    foreach (var client in allCallbacks())
                    {
                        try
                        {
                            client.OnLoadGoldSamples(goldSampleList);
                        }
                        catch (Exception ex)
                        {
                            SystemLog.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        private void OnSequenceExecutionDynamicLoadSequence(MTFSequence sequence, Dictionary<Guid, MTFExternalSequenceInfo> externalSequences)
        {
            if (!raiseExeucutionEvents)
            {
                return;
            }
            try
            {
                foreach (var c in allCallbacks())
                {
                    try
                    {
                        c.OnDynamicLoadSequence(sequence, externalSequences);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogException(ex);
                    }
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        private void OnSequenceExecutionDynamicUnLoadSequence(Guid sequenceId)
        {
            if (!raiseExeucutionEvents)
            {
                return;
            }
            try
            {
                foreach (var c in allCallbacks())
                {
                    try
                    {
                        c.OnDynamicUnloadSequence(sequenceId);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogException(ex);
                    }
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        private void OnSequenceExecutionDynamicExecuteSequence(Guid sequenceId, Guid subSequenceId, bool callSubSequence, Guid[] activityIdPath)
        {
            if (!raiseExeucutionEvents)
            {
                return;
            }
            try
            {
                foreach (var c in allCallbacks())
                {
                    try
                    {
                        c.OnDynamicExecuteSequence(sequenceId, subSequenceId, callSubSequence, activityIdPath);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.LogException(ex);
                    }
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        public bool ExistSequence(string fileName)
        {
            return FileOperation.ExistFile(Path.Combine(BaseConstants.SequenceBasePath, fileName));
        }

        private void OnSequenceExecutionVariantChanged(SequenceVariantInfo sequenceVariantInfo, Guid? dutId)
        {
            if (!raiseExeucutionEvents)
            {
                return;
            }
            try
            {
                foreach (var client in allCallbacks())
                {
                    Task.Run(() =>
                             {
                                 try
                                 {
                                     client.OnSequenceVariantChanged(sequenceVariantInfo, dutId);
                                 }
                                 catch (Exception ex)
                                 {
                                     SystemLog.LogException(ex);
                                 }
                             });
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        private void OnSerivceExecutionCommandsStateChanged(IEnumerable<Guid> allowedCommands, IEnumerable<Guid> checkedCommands)
        {
            try
            {
                foreach (var client in allCallbacks())
                {
                    Task.Run(() =>
                             {
                                 try
                                 {
                                     client.ServiceCommandsStateChanged(allowedCommands, checkedCommands);
                                 }
                                 catch (Exception ex)
                                 {
                                     SystemLog.LogException(ex);
                                 }
                             });
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        private void SequenceExecutorUserIndicatorValueChanged(Guid indicatorId, bool value)
        {
            try
            {
                foreach (var client in allCallbacks())
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            client.OnSequenceExecutionUserIndicatorValueChanged(indicatorId, value);
                        }
                        catch (Exception ex)
                        {
                            SystemLog.LogException(ex);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        private object userCommandStatusLock = new object();
        private void SequenceExecutorUserCommandsStatusChanged(IEnumerable<UserCommandsState> commandsSettings)
        {
            try
            {
                lock (userCommandStatusLock)
                {
                    foreach (var client in allCallbacks())
                    {
                        try
                        {
                            client.OnSequenceExecutionUserCommandsStatusChanged(commandsSettings);
                        }
                        catch (Exception ex)
                        {
                            SystemLog.LogException(ex);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        public void SetValidationTableRow(Guid tableId, MTFValidationTableRow row) => sequenceExecutor?.SetValidationTableRow(tableId, row);

        public void SaveExecutedSequence(string modifiedByUser) => sequenceExecutor?.SaveSequence(modifiedByUser);

        public void SetIsServiceMode(bool isServiceMode)
        {
            if (sequenceExecutor!=null)
            {
                isServiceModeActive = isServiceMode;
                sequenceExecutor.SetIsServiceMode(isServiceMode);
            }
        }

        public void SetIsTeachingMode(bool isTeachingMode)
        {
            if (sequenceExecutor!=null)
            {
                isTeachModeActive = isTeachingMode;
                sequenceExecutor.SetIsTeachingMode(isTeachingMode);
            }
        }

        public void ExecuteServiceCommand(Guid commandId)
        {
            sequenceExecutor?.ExecuteServiceCommand(commandId);
        }

        public void RemoveGoldSampleData(string fileName) => GoldSampleHelper.Remove(fileName);

        public FileInfo GetGoldSampleDataFileInfo(string fileName) => GoldSampleHelper.GetGoldSampleDataFileInfo(fileName);

        public void ExecuteUserCommand(Guid commandId) => sequenceExecutor?.ExecuteUserCommand(commandId);

        private void OnSequenceExecutionAllowSaveExecutedSequence(bool state)
        {
            try
            {
                foreach (var client in allCallbacks())
                {
                    Task.Run(() =>
                             {
                                 try
                                 {
                                     client.OnAllowSaveExecutedSequence(state);
                                 }
                                 catch (Exception ex)
                                 {
                                     SystemLog.LogException(ex);
                                 }
                             });
                }
            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }

        public List<MTFFileInfo> GetSequenceFileInfos(string path)
        {
            path = path ?? string.Empty;
            var persistsInfo = persist.GetPersistInfo(Path.Combine(BaseConstants.SequenceBasePath, path));
            return persistsInfo.Select(x => new MTFFileInfo { FullName = Path.Combine(path, x.Name), IsDirectory = x.Type != MTFDialogItemTypes.File }).ToList();
        }

        public MTFSequenceVariant GetSequenceVariant(string sequenceFullName)
        {
            var variant = new MTFSequenceVariant();
            var sequence = persist.LoadData<MTFSequence>(Path.Combine(BaseConstants.SequenceBasePath, sequenceFullName));
            if (sequence != null && sequence.VariantGroups != null)
            {
                foreach (var variantGroup in sequence.VariantGroups)
                {
                    var group = new MTFSequenceVariantGroup();
                    group.Name = variantGroup.Name;
                    if (variantGroup.Values != null)
                    {
                        foreach (var sequenceVariantValue in variantGroup.Values)
                        {
                            group.Values.Add(new MTFSequenceVariantValue { Name = sequenceVariantValue.Name });
                        }
                    }
                    variant.VariantGroups.Add(group);
                }
            }
            return variant;
        }

        private void OnClientUIDataSend(byte[] data, ClientUIDataInfo info)
        {
            try
            {
                Task.Run(() =>
                         {
                             foreach (var client in allCallbacks())
                             {

                                 try
                                 {
                                     client.ClientUIReceiveData(data, info);
                                 }
                                 catch (Exception ex)
                                 {
                                     SystemLog.LogException(ex);
                                 }
                             }
                         });

            }
            catch (Exception e)
            {
                SystemLog.LogMessage("Callback call raised exception (see bellow), client's callback is disconnected!");
                SystemLog.LogException(e);
            }
        }
    }
}
