using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MTFClientServerCommon
{
    [Serializable]
    [KnownType(typeof(MTFMessageActivityResult))]
    [KnownType(typeof(MTFVariableActivityResult))]
    [KnownType(typeof(MTFLogMessageResult))]
    public class MTFActivityResult : MTFDataTransferObject
    {
        public MTFActivityResult()
            : base()
        {
            //Messages = new Dictionary<MTFProgressEventInfo, List<string>>();
            //Progress = new Dictionary<MTFProgressEventInfo, int>();
        }

        public MTFActivityResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public MTFActivityResult(MTFSequenceActivity activity)
            : base()
        {
            //Messages = new Dictionary<MTFProgressEventInfo, List<string>>();
            //Progress = new Dictionary<MTFProgressEventInfo, int>();

            ActivityId = activity.Id;
            ActivityName = activity.ActivityName;
            ActivityIndexer = activity.UniqueIndexer;
            if (activity.ClassInfo != null)
            {
                MTFClassAlias = activity.ClassInfo.Alias;
            }
            MTFMethodName = activity.MTFMethodName;
        }

        public Guid ActivityId
        {
            get { return GetProperty<Guid>(); }
            set { SetProperty(value); }
        }

        public Guid[] ActivityIdPath
        {
            get { return GetProperty<Guid[]>(); }
            set { SetProperty(value); }
        }

        public string ActivityName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public int ActivityIndexer
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public string MTFClassAlias
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string MTFMethodName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public object ActivityResult
        {
            get { return GetProperty<object>(); }
            set { SetProperty(value); }
        }

        public string ActivityResultTypeName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public IList<MTFParameterValue> MTFParameters
        {
            get { return GetProperty<IList<MTFParameterValue>>(); }
            set { SetProperty(value); }
        }

        //public Dictionary<MTFProgressEventInfo, List<string>> Messages
        //{
        //    get { return GetProperty<Dictionary<MTFProgressEventInfo, List<string>>>(); }
        //    set { SetProperty(value); }
        //}

        //public Dictionary<MTFProgressEventInfo, int> Progress
        //{
        //    get { return GetProperty<Dictionary<MTFProgressEventInfo, int>>(); }
        //    set { SetProperty(value); }
        //}

        public double ElapsedMs
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double TimestampMs
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public string ExceptionMessage
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public StringBuilder ProgressString
        {
            get { return GetProperty<StringBuilder>(); }
            set { SetProperty(value); }
        }

        public IList<MTFActivityResult> SubActivityResults
        {
            get { return GetProperty<IList<MTFActivityResult>>(); }
            set { SetProperty(value); }
        }

        public uint NumberOfRepetition
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public MTFExecutionActivityStatus Status
        {
            get { return GetProperty<MTFExecutionActivityStatus>(); }
            set { SetProperty(value); }
        }

        public bool CheckOutpuValueFailed
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool ExceptionOccured
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
    }
}
