using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    public abstract class UnaryLogicalTerm : UnaryTerm
    {
        public UnaryLogicalTerm()
            : base()
        {
        }

        public UnaryLogicalTerm(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public override Type ResultType
        {
            get { return typeof(bool); }
        }

        public override TermGroups TermGroup
        {
            get
            {
                return TermGroups.None
                    | TermGroups.LogicalTerm;
            }
        }

        public override TermGroups ChildrenTermGroup
        {
            get
            {
                return TermGroups.LogicalTerm;
            }
        }
    }
}
