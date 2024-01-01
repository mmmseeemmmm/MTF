using System;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class SetupVariantSelectionResult : MTFDataTransferObject
    {
        public SetupVariantSelectionResult()
        {
        }

        public SetupVariantSelectionResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string DataName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public SequenceVariant SaveVariant
        {
            get { return GetProperty<SequenceVariant>(); }
            set { SetProperty(value); }            
        }

        public SequenceVariant UseVariant
        {
            get { return GetProperty<SequenceVariant>(); }
            set { SetProperty(value); }
        }
    }
}
