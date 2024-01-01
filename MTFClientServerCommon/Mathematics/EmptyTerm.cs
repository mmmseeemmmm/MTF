using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class EmptyTerm : Term
    {
        public EmptyTerm()
            : base()
        {
        }

        public EmptyTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public EmptyTerm(string targetTypeName)
            : base(targetTypeName)
        {

        }

        public IEnumerable<string> ChildrenTermSymbols => TermFactory.GetTermSymbols(TermGroups.None);

        public override object Evaluate()
        {
            return null;
        }

        public override Type ResultType => TargetType!=null ? Type.GetType(TargetType) : null;

        public override string Symbol => string.Empty;

        public override string ToString()
        {
            return string.Empty;
        }

        public override TermGroups TermGroup => TermGroups.None;

        public override TermGroups ChildrenTermGroup => TermGroups.None;

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon => AutomotiveLighting.MTFCommon.MTFIcons.None;

        public override string Label => string.Empty;
    }
}
