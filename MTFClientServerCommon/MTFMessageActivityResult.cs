using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFMessageActivityResult : MTFActivityResult
    {
        public MTFMessageActivityResult()
            : base()
        {
        }

        public MTFMessageActivityResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public MTFMessageActivityResult(MTFSequenceActivity activity)
            : base(activity)
        {
        }

        public string DisplayedMessage
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Header
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }
}
