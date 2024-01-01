using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class NotTerm : UnaryLogicalTerm
    {
        public NotTerm()
            : base()
        {
        }

        public NotTerm(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            if (Value.ResultType == typeof(bool))
            {
                return !(bool)Value.Evaluate();
            }

            throw new Exception("Value must be bool.");
        }

        public override string Symbol
        {
            get { return "!"; }
        }



        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.TermNot; }
        }

        public override string Label
        {
            get { return "Negation"; }
        }
    }
}
