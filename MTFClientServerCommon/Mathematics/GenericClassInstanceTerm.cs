using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class GenericClassInstanceTerm : UnaryTerm
    {
        public GenericClassInstanceTerm()
            : base()
        {
        }

        public GenericClassInstanceTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            return null;
        }

        public override Type ResultType
        {
            get { return typeof(GenericClassInstanceConfiguration); }
        }

        public override string Symbol
        {
            get { return "Create "; }
        }

        public override TermGroups TermGroup
        {
            get { return TermGroups.None; }
        }

        public override TermGroups ChildrenTermGroup
        {
            get { return TermGroups.None; }
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.None; }
        }

        public override string Label
        {
            get { return string.Empty; }
        }
    }
}
