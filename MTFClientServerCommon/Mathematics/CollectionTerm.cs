using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class CollectionTerm : UnaryTerm
    {
        public CollectionTerm()
            : base()
        {
        }

        public CollectionTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            return Value;
        }

        public override Type ResultType
        {
            get
            {
                if (Value != null)
                {
                    return Value.GetType();
                }

                return null;
            }
        }

        public override string Symbol
        {
            get { return string.Empty; }
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
