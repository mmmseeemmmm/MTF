using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFIdentityObject : MTFDataTransferObject
    {
        public MTFIdentityObject()
            : base()
        {
        }

        public MTFIdentityObject(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public string IdentifierName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string TypeName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }
}
