using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class EqualTerm : BinaryLogicalTerm
    {
        public EqualTerm()
            : base()
        {
        }

        public EqualTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            return areEqualValues(Value1.Evaluate(), Value2.Evaluate());
        }

        public override string Symbol
        {
            get { return "=="; }
        }

        public override TermGroups ChildrenTermGroup
        {
            get
            {
                return TermGroups.NumberTerm
                    | TermGroups.StringTerm
                    | TermGroups.LogicalTerm;
            }
        }



        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.TermEqual; }
        }

        public override string Label
        {
            get { return "Equality"; }
        }
    }
}
