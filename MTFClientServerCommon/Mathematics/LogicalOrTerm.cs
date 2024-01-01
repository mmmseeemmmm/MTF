using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class LogicalOrTerm : BinaryLogicalTerm
    {
        public LogicalOrTerm() 
            : base()
        {
        }

        public LogicalOrTerm(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            return (bool)Value1.Evaluate() || (bool)Value2.Evaluate();
        }

        public override string Symbol
        {
            get { return "||"; }
        }

        public override TermGroups ChildrenTermGroup
        {
            get { return TermGroups.LogicalTerm; }
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.TermOr; }
        }

        public override string Label
        {
            get { return "Logical OR"; }
        }
    }
}
