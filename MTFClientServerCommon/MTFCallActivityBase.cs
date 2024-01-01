using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    public abstract class MTFCallActivityBase : MTFSequenceActivity
    {

        public MTFCallActivityBase()
            : base()
        {
        }

        public MTFCallActivityBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public bool IsCollapsed
        {
            get { return GetProperty<bool>(); }
            set { ExecuteWithoutSetIsModified(() => SetProperty(value)); }
        }
    }
}
