using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class NotIsInListTerm : ListTerm
    {
        public NotIsInListTerm()
            : base()
        {
        }

        public NotIsInListTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            return !(bool)base.Evaluate();
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.TermNotIsInList; }
        }

        public override string Symbol
        {
            get { return "Not In"; }
        }

        public override string Label
        {
            get { return "Excluded in list"; }
        }
    }
}
