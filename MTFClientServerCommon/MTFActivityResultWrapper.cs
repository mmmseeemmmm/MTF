using System;
using System.Collections.Generic;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFActivityResultWrapper
    {
        private Guid activityId;
        public Guid ActivityId
        {
            get => activityId;
            set => activityId = value;
        }

        private Guid[] activityIdPath;
        public Guid[] ActivityIdPath
        {
            get => activityIdPath;
            set => activityIdPath = value;
        }

        private string activityName;
        public string ActivityName
        {
            get => activityName;
            set => activityName = value;
        }

        private int activityIndexer;
        public int ActivityIndexer
        {
            get => activityIndexer;
            set => activityIndexer = value;
        }

        private object activityResult;
        public object ActivityResult
        {
            get => activityResult;
            set => activityResult = value;
        }

        private string activityResultTypeName;
        public string ActivityResultTypeName
        {
            get => activityResultTypeName;
            set => activityResultTypeName = value;
        }

        private double elapsedMs;
        public double ElapsedMs
        {
            get => elapsedMs;
            set => elapsedMs = value;
        }

        private double timestampMs;
        public double TimestampMs
        {
            get => timestampMs;
            set => timestampMs = value;
        }
        
        private string exceptionMessage;
        public string ExceptionMessage
        {
            get => exceptionMessage;
            set => exceptionMessage = value;
        }

        private uint numberOfRepetition;
        public uint NumberOfRepetition
        {
            get => numberOfRepetition;
            set => numberOfRepetition = value;
        }

        private MTFExecutionActivityStatus status;
        public MTFExecutionActivityStatus Status
        {
            get => status;
            set => status = value;
        }
        
        private IList<MTFParameterValue> mtfParameters;
        public IList<MTFParameterValue> MTFParameters
        {
            get => mtfParameters;
            set => mtfParameters = value;
        }

        private string variableValue;
        public string VariableValue
        {
            get => variableValue;
            set => variableValue = value;
        }

        private string variableName;
        public string VariableName
        {
            get => variableName;
            set => variableName = value;
        }

        private string variableTypeName;
        public string VariableTypeName
        {
            get => variableTypeName;
            set => variableTypeName = value;
        }
    }
}
