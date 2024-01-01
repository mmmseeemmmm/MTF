using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public abstract class UnaryExponentTerm : UnaryNumberTerm
    {
        public UnaryExponentTerm()
            : base()
        {
            this.Exponent = 2;
        }

        public UnaryExponentTerm(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public int Exponent
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }
    }
}
