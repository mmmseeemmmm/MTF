using System;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class SequenceVariantValue : MTFDataTransferObject
    {
        public SequenceVariantValue()
        {
        }

        public SequenceVariantValue(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }
}
