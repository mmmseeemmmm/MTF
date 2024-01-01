using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFLogMessageResult : MTFActivityResult
    {
        public MTFLogMessageResult()
            : base()
        {
        }

        public MTFLogMessageResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public MTFLogMessageResult(MTFSequenceActivity activity)
            : base(activity)
        {
        }

        public string LoggedMessage
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }
}
