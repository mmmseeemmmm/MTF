using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MTFClientServerCommon;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.SequenceLocalization;
using MTFCommon;
using MTFCommon.ClientControls;
using MessageType = AutomotiveLighting.MTFCommon.MessageType;

namespace MTFCore
{
    class MTFSequenceRuntimeContext : IMTFSequenceRuntimeContext, IDisposable
    {
        private readonly string sequenceName;
        public MTFSequenceRuntimeContext(string sequenceName)
        {
            this.sequenceName = sequenceName;
            ThreadIdToActivity = new Dictionary<int, MTFSequenceActivity>();
            ThreadIdToScopeData = new Dictionary<int, ScopeData>();
        }

        public void RaiseException(object sender, Exception exception, ExceptionLevel exceptionLevel)
        {
            RaiseExceptionMethod(sender, exception, exceptionLevel);
        }

        public Action<object, Exception, ExceptionLevel> RaiseExceptionMethod { get; set; }

        public string ActivityName
        {
            get
            {
                checkThread();
                var dict = SequenceLocalizationHelper.ActualDictionary;
                var activity = ThreadIdToActivity[System.Threading.Thread.CurrentThread.ManagedThreadId];
                return activity != null ? MTFSequenceActivityHelper.CombineTranslatedActivityName(dict.GetValue(activity.ActivityName), activity.UniqueIndexer) : null;
            }
        }

        public string ActivityPath
        {
            get { return string.Join(" - ", getActivityPathByThread()); }
        }

        public List<Guid> ActivityPathIds
        {
            get { return getActivityPathByThread(); }
        }

        public void LogMessage(string message, LogLevel logLevel)
        {
            LogMessageMethod(message, logLevel);
        }

        public Action<string, LogLevel> LogMessageMethod { get; set; }

        public void TextNotification(string message)
        {
            TextNotificationMethod(message, StatusMessage.MessageType.Info);
        }

        public void TextNotification(string message, MessageType type)
        {
            TextNotificationMethod(message, (StatusMessage.MessageType)(int)type);
        }

        public Action<string, StatusMessage.MessageType> TextNotificationMethod { get; set; }

        public void ImageNotification(byte[] imageData)
        {
            ImageNotificationMethod(imageData, null);
        }

        public void ImageNotification(byte[] imageData, List<Guid> executionPath)
        {
            ImageNotificationMethod(imageData, executionPath);
        }

        public Action<byte[], List<Guid>> ImageNotificationMethod { get; set; }

        public void ProgressNotification(int percent)
        {
            ProgressNotification(percent, string.Empty);
        }

        public void ProgressNotification(int percent, string text)
        {
            ProgressNotificationMethod(percent, text, getScopeByThread());
        }

        public void ProgressNotificationIndeterminate(string text, bool isStarted)
        {
            ProgressNotificationIndeterminateMethod(text, isStarted, getScopeByThread());
        }

        public Action<int, string, ScopeData> ProgressNotificationMethod { get; set; }

        public Action<string, bool, ScopeData> ProgressNotificationIndeterminateMethod { get; set; }

        public Dictionary<int, ScopeData> ThreadIdToScopeData { get; set; }

        public Dictionary<int, MTFSequenceActivity> ThreadIdToActivity { get; set; }

        public List<List<Guid>> SetupModeActivityPaths { get; set; }

        public bool IsSetupModeActive { get; set; }

        public List<List<Guid>> BreakPointActivityPaths { get; set; }

        public bool IsDebugModeActive { get; set; }

        public bool IsServiceModeActive { get; set; }

        public bool IsTeachModeActive { get; set; }

        public bool IsServiceExecutionAllowed { get; set; }

        private List<Guid> getActivityPathByThread()
        {
            return getScopeByThread().ExecutingActivityPath;
        }

        private ScopeData getScopeByThread()
        {
            checkThread();

            return ThreadIdToScopeData[System.Threading.Thread.CurrentThread.ManagedThreadId];
        }

        public Action<string, string, object, IEnumerable<ValidationColumn>, ScopeData> AddToValidationTableMethod { get; set; }

        public Func<string, string, ScopeData, IEnumerable<ValidationColumn>> GetFromValidationTableMethod { get; set; }
        public Func<string, string, ScopeData, object> GetFromConstantTableMethod { get; set; }
        public Func<string, ScopeData, MTFValidationTableStatus> GetValidationTableStatusMethod { get; set; }
        public Func<string, ScopeData, string> GetValidationTableErrorTextMethod { get; set; }
        public Func<string, string, ScopeData, MTFValidationTableStatus> GetValidationTableRowStatusMethod { get; set; }
        public Func<string, string, ScopeData, string> GetValidationTableRowErrorTextMethod { get; set; }

        public void SaveData(string dataName, object data)
        {
            SaveDataMethod(dataName, data);
        }

        public Action<string, object> SaveDataMethod { get; set; }

        public object LoadData(string dataName)
        {
            return LoadDataMethod(dataName, null, getScopeByThread());
        }

        public T LoadData<T>(string dataName)
        {
            return (T)LoadDataMethod(dataName, typeof(T), getScopeByThread());
        }

        public Func<string, Type, object> GetTargetDataMethod { get; set; }

        public object GetTargetData(string dataName)
        {
            return GetTargetDataMethod(dataName, null);
        }

        public T GetTargetData<T>(string dataName)
        {
            return (T)GetTargetDataMethod(dataName, typeof(T));
        }

        public Func<string, Type, ScopeData, object> LoadDataMethod { get; set; }

        public bool IsSetupMode
        {
            get
            {
                if (IsServiceExecutionAllowed)
                {
                    return true;
                }
                if (!IsSetupModeActive || SetupModeActivityPaths == null)
                {
                    return false;
                }

                return SetupModeActivityPaths.Any(l => string.Join(" - ", getActivityPathByThread()) == string.Join(" - ", l));
            }
        }

        public bool IsServiceMode
        {
            get { return IsServiceModeActive && IsServiceExecutionAllowed; }
        }

        public bool IsTeachMode
        {
            get { return IsTeachModeActive && IsServiceExecutionAllowed; }
        }

        private void checkThread()
        {
            if (!ThreadIdToScopeData.ContainsKey(System.Threading.Thread.CurrentThread.ManagedThreadId))
            {
                throw new Exception("This kind of call must be executed on MTF thread. Please check driver implementation!");
            }
        }

        public string SequenceName
        {
            get { return sequenceName; }
        }

        public IEnumerable<string> SequenceVariantGroups => getScopeByThread()?.SequenceVariantGroups;

        public IEnumerable<string> SequenceVariantValue(string groupName) => getScopeByThread()?.SequenceVariantValue(groupName);

        public void AddToValidationTable(string tableName, string rowName, object value, IEnumerable<ValidationColumn> validationColumns)
        {
            AddToValidationTableMethod(tableName, rowName, value, validationColumns, getScopeByThread());
        }

        public IEnumerable<ValidationColumn> GetFromValidationTable(string tableName, string rowName)
        {
            return GetFromValidationTableMethod(tableName, rowName, getScopeByThread());
        }

        public object GetFromConstantTable(string tableName, string rowName)
        {
            return GetFromConstantTableMethod(tableName, rowName, getScopeByThread());
        }

        public MTFValidationTableStatus GetValidationTableStatus(string tableName)
        {
            return GetValidationTableStatusMethod(tableName, getScopeByThread());
        }

        public MTFValidationTableStatus GetValidationTableRowStatus(string tableName, string rowName)
        {
            return GetValidationTableRowStatusMethod(tableName, rowName, getScopeByThread());
        }

        public string GetValidationTableRowErrorText(string tableName, string rowName)
        {
            return GetValidationTableRowErrorTextMethod(tableName, rowName, getScopeByThread());
        }

        public string GetValidationTableErrorText(string tableName)
        {
            return GetValidationTableErrorTextMethod(tableName, getScopeByThread());
        }

        public Func<string, string> GetVariantForSaveDataMethod { get; set; }
        public string GetVariantForSaveData(string paramName)
        {
            return GetVariantForSaveDataMethod(paramName);
        }

        public Func<string, ScopeData, string> GetVariantForLoadDataMethod { get; set; }
        public string GetVariantForLoadData(string paramName)
        {
            return GetVariantForLoadDataMethod(paramName, getScopeByThread());
        }

        public Func<string, ScopeData, IEnumerable<ValidationRowContainer>> GetValidationTableRowsMethod { get; set; }
        public IEnumerable<ValidationRowContainer> GetValidationTableRows(string tableName)
        {
            return GetValidationTableRowsMethod(tableName, getScopeByThread());
        }

        public Action<string, IEnumerable<ValidationRowContainer>, ScopeData> AddRangeToValidationTableMethod { get; set; }
        public void AddRangeToValidationTable(string tableName, IEnumerable<ValidationRowContainer> rows)
        {
            AddRangeToValidationTableMethod(tableName, rows, getScopeByThread());
        }

        public Func<ScopeData, bool> IsGoldSampleMethod { get; set; }
        public bool IsGoldSample
        {
            get { return IsGoldSampleMethod(getScopeByThread()); }
        }

        public Func<ScopeData, bool> GoldSampleRequiredMethod { get; set; }
        public Action<ScopeData> GoldSampleInvalidateMethod { get; set; }

        public bool GoldSampleRequired { get { return GoldSampleRequiredMethod(getScopeByThread()); } }
        public void GoldSampleInvalidate()
        {
            GoldSampleInvalidateMethod(getScopeByThread());
        }


        public void SetSequenceVariant(Dictionary<string, string> values)
        {
            SetSequenceVariantMethod(values, getScopeByThread());
        }

        public Action<Dictionary<string, string>, ScopeData> SetSequenceVariantMethod { get; set; }

        public void SendToClientControl(object data, string dataName)
        {
            SendToClientControlMethod(data, dataName, null);
        }

        public void SendToClientControl(object data, string dataName, IEnumerable<ClientControlName> clientControls)
        {
            SendToClientControlMethod(data, dataName, clientControls);
        }

        public void SendToClientSetupControl(object data, string dataName, IEnumerable<ClientSetupControlName> clientSetupControls)
        {
            SendToClientSetupControlMethod(data, dataName, clientSetupControls);
        }

        public Action<object, string, IEnumerable<ClientControlName>> SendToClientControlMethod { get; set; }

        public Action<object, string, IEnumerable<ClientSetupControlName>> SendToClientSetupControlMethod { get; set; }

        public void OpenClientSetupControl(string assemblyName, string className)
        {
            OpenClientSetupContorolMethod(new OpenClientSetupControlArgs { AssemblyName = assemblyName, TypeName = className });
        }

        public Action<OpenClientSetupControlArgs> OpenClientSetupContorolMethod { get; set; }

        public Func<double, double> RoundValueMethod { get; set; }

        public double RoundValue(double value)
        {
            return RoundValueMethod(value);
        }

        public string ComponentFullPath => Path.GetDirectoryName(ComponentFullName);

        public string ComponentFullName
        {
            get
            {
                checkThread();
                var activity = ThreadIdToActivity[System.Threading.Thread.CurrentThread.ManagedThreadId];
                return activity.ClassInfo.MTFClass.FullPathName;
            }
        }

        public void Dispose()
        {
            foreach (var propertyInfo in GetType().GetProperties())
            {
                if (propertyInfo.CanWrite && propertyInfo.CanRead && !propertyInfo.PropertyType.IsValueType)
                {
                    propertyInfo.SetValue(this, null);
                }
            }
        }
    }
}
