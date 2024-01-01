using System;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFAllowedValue : MTFDataTransferObject, ISemradList
    {
        public MTFAllowedValue()
            : base()
        {
        }

        public MTFAllowedValue(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string DisplayName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public object Value
        {
            get => GetProperty<object>();
            set => SetProperty(value);
        }
    }

    public interface ISemradList
    {
        string DisplayName { get; set; }
        object Value { get; set; }
    }
}
