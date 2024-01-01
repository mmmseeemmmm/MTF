using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class IsInListTerm : ListTerm
    {
        public IsInListTerm()
            : base()
        {
        }

        public IsInListTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string Symbol
        {
            get { return "In"; }
        }

        public override object Evaluate()
        {
            return base.Evaluate();
        }


        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.TermIsInList; }
        }

        public override string Label
        {
            get { return "Included in list"; }
        }
    }
}
