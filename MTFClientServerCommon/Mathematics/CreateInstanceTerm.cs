using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    public class CreateInstanceTerm : UnaryTerm
    {
        public override object Evaluate()
        {
            throw new NotImplementedException();
        }

        public override Type ResultType
        {
            get { return typeof(GenericClassInstanceConfiguration); }
        }

        public override string Symbol
        {
            get { return "new"; }
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
