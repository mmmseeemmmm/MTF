using System;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    //Must stay because of deserialize of old object, in future can be removed
    [Serializable]
    public class MTFProgressEventInfo : MTFDataTransferObject
    {
        public MTFProgressEventInfo() : base()
        {
        }

        public MTFProgressEventInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        //    public string Name
        //    {
        //        get { return GetProperty<string>(); }
        //        set { SetProperty(value); }
        //    }

        //    public string Title
        //    {
        //        get { return GetProperty<string>(); }
        //        set { SetProperty(value); }
        //    }

        //    public string EventHandlerTypeName
        //    {
        //        get { return GetProperty<string>(); }
        //        set { SetProperty(value); }
        //    }

        //    public int Index
        //    {
        //        get { return GetProperty<int>(); }
        //        set { SetProperty(value); }
        //    }
    }
}
