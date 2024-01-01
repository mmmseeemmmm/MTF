using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class IsNullTerm : UnaryTerm
    {
        public IsNullTerm()
            : base()
        {
        }

        public IsNullTerm(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            return this.Value == null || this.Value.Evaluate() == null;
        }

        public override Type ResultType
        {
            get { return typeof(bool); }
        }

        public override string Symbol
        {
            get { return "IsNull"; }
        }

        public override string ToString()
        {
            if (Value != null)
            {
                return string.Format("{0}({1})", Symbol, Value.ToStringAsSubterm());
            }
            return string.Empty;
        }

        public override TermGroups TermGroup
        {
            get { return TermGroups.None; }
        }

        public override TermGroups ChildrenTermGroup
        {
            get { return TermGroups.None; }
        }

        public override MTFIcons Icon
        {
            get { return MTFIcons.TermIsNull; }
        }

        public override string Label
        {
            get { return "IsNull"; }
        }
    }
}
