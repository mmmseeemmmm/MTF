using MTFClientServerCommon.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFStringFormat : MTFDataTransferObject
    {
        public MTFStringFormat()
            : base()
        {
        }

        public MTFStringFormat(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        } 

        public string Text
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public IList<Term> Parameters
        {
            get { return GetProperty<IList<Term>>(); }
            set { SetProperty(value); }
        }

        public override string ToString()
        {
            return "StringFormat(" + Text + ")";
        }
    }
}
