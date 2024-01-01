using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFVariableActivityResult : MTFActivityResult
    {
        public MTFVariableActivityResult()
            : base()
        {
        }

        public MTFVariableActivityResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public MTFVariableActivityResult(MTFSequenceActivity activity)
            : base(activity)
        {
        }

        public string Value
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string VariableName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string VariableTypeName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public Guid VariableId
        {
            get { return GetProperty<Guid>(); }
            set { SetProperty(value); }
        }
    }
}
