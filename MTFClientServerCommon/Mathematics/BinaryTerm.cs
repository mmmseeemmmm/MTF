using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    public abstract class BinaryTerm : Term
    {
        public BinaryTerm()
            : base()
        {
        }

        public BinaryTerm(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public virtual Term Value1
        {
            get { return GetProperty<Term>(); }
            set { SetProperty(value); }
        }

        public virtual Term Value2
        {
            get { return GetProperty<Term>(); }
            set { SetProperty(value); }
        }

        public override string ToString()
        {
            if (Value1!=null && Value2!=null)
            {
                return string.Format("{0} {1} {2}", Value1.ToStringAsSubterm(), Symbol, Value2.ToStringAsSubterm());
            }
            return string.Empty;
            
        }
    }
}
