using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    public abstract class UnaryTerm : Term
    {
        public UnaryTerm()
            : base()
        {
        }

        public UnaryTerm(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public virtual Term Value
        {
            get { return GetProperty<Term>(); }
            set { SetProperty(value); }
        }
        public override string ToString()
        {
            return this.Value != null ? string.Format("{0}{1}", this.Symbol, this.Value.ToStringAsSubterm()) : string.Empty;
        }
    }
}
