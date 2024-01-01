using System;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class RoundingRule : MTFDataTransferObject
    {
        public RoundingRule()
        {
        }

        public RoundingRule(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public int Min
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }
        
        public int Max
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int Digits
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }
    }
}
