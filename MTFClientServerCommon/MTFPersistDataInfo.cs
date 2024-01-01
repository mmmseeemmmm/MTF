using System;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFPersistDataInfo : MTFDataTransferObject
    {
        public MTFPersistDataInfo()
            : base()
        {
        }

        public MTFPersistDataInfo(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        //public bool IsDirectory
        //{
        //    get { return Type != MTFDialogItemTypes.File; }
        //}

        public MTFDialogItemTypes Type
        {
            get { return GetProperty<MTFDialogItemTypes>(); }
            set { SetProperty(value); }
        }
    }

    
}
