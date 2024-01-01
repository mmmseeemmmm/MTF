using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFCase : MTFDataTransferObject
    {
        public MTFCase()
            : base()
        {
            Activities = new ObservableCollection<MTFSequenceActivity>();
        }

        public MTFCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public IList<MTFSequenceActivity> Activities
        {
            get { return GetProperty<IList<MTFSequenceActivity>>(); }
            set { SetProperty(value); }
        }

        public object Value
        {
            get { return GetProperty<object>(); }
            set { SetProperty(value); }
        }

        public bool IsDefault
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
    }
}
