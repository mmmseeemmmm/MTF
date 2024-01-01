using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    public abstract class UnaryNumberTerm : UnaryTerm
    {
        public UnaryNumberTerm()
            : base()
        {
        }

        public UnaryNumberTerm(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public override Type ResultType
        {
            get { return this.Value != null ? this.Value.ResultType : null; }
        }

        public override TermGroups TermGroup
        {
            get
            {
                return TermGroups.None
                    | TermGroups.NumberTerm;
            }
        }

        public override TermGroups ChildrenTermGroup
        {
            get { return TermGroups.NumberTerm; }
        }
    }
}
