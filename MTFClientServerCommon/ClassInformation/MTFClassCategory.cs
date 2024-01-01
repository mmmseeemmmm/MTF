using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFClassCategory : MTFDataTransferObject
    {
        public MTFClassCategory()
            : base()
        {
        }

        public MTFClassCategory(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public string Name
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public ObservableCollection<MTFClassInfo> Classes
        {
            get => GetProperty<ObservableCollection<MTFClassInfo>>();
            set => SetProperty(value);
        }
    }
}
