using System;
using System.Collections.Generic;
using MTFCommon;
using MTFCommon.ClientControls;

namespace AutomotiveLighting.MTFCommon
{
    public interface IMTFSequenceRuntimeContext
    {
        void RaiseException(object sender, Exception exception, ExceptionLevel exceptionLevel);
        void LogMessage(string message, LogLevel logLevel);
        void TextNotification(string message);
        void TextNotification(string message, MessageType type);
        void ImageNotification(byte[] imageData);
        void ImageNotification(byte[] imageData, List<Guid> executionPath);
        void ProgressNotification(int percent);
        void ProgressNotification(int percent, string text);
        void ProgressNotificationIndeterminate(string text, bool isStarted);
        void SaveData(string dataName, object data);
        object LoadData(string dataName);
        T LoadData<T>(string dataName);
        object GetTargetData(string dataName);
        T GetTargetData<T>(string dataName);
        string ActivityName { get; }
        string ActivityPath { get; }
        List<Guid> ActivityPathIds { get; }
        bool IsSetupMode { get; }
        bool IsServiceMode { get; }
        bool IsTeachMode { get; }
        string SequenceName { get; }
        IEnumerable<string> SequenceVariantGroups { get; }
        IEnumerable<string> SequenceVariantValue(string groupName);
        void AddToValidationTable(string tableName, string rowName, object value, IEnumerable<ValidationColumn> validationColumns);
        IEnumerable<ValidationColumn> GetFromValidationTable(string tableName, string rowName);
        object GetFromConstantTable(string tableName, string rowName);
        MTFValidationTableStatus GetValidationTableStatus(string tableName);
        MTFValidationTableStatus GetValidationTableRowStatus(string tableName, string rowName);
        string GetValidationTableErrorText(string tableName);
        string GetValidationTableRowErrorText(string tableName, string rowName);
        IEnumerable<ValidationRowContainer> GetValidationTableRows(string tableName);
        void AddRangeToValidationTable(string tableName, IEnumerable<ValidationRowContainer> rows);
        string GetVariantForSaveData(string paramName);
        string GetVariantForLoadData(string paramName);
        bool IsGoldSample { get; }
        bool GoldSampleRequired { get; }
        void GoldSampleInvalidate();
        void SetSequenceVariant(Dictionary<string, string> values);
        void SendToClientControl(object data, string dataName, IEnumerable<ClientControlName> clientControls);
        void SendToClientSetupControl(object data, string dataName, IEnumerable<ClientSetupControlName> clientSetupControls);
        void OpenClientSetupControl(string assemblyName, string className);
        double RoundValue(double value);
        string ComponentFullPath { get; }
        string ComponentFullName { get; }
    }

    //delegate void SaveDataDelegate<T>()
    delegate T LoadDataDelegate<T>(string dataName);

    public enum ExceptionLevel
    {
        CriticalAsynchronousException,
        Warning,
        JustInfo
    }

    public class ValidationColumn
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public bool IsWrong { get; set; }
    }

    public class ValidationRowContainer
    {
        public string RowName { get; set; }
        public object Value { get; set; }
        public IEnumerable<ValidationColumn> ValidationColumns { get; set; }
        public MTFValidationTableStatus ValidationStatus { get; set; }
    }

    public enum MessageType
    {
        Error,
        Warning,
        Info,
    }
}
