using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MTFCommon.Helpers;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class LessOrEqualTerm : BinaryLogicalTerm
    {
        public LessOrEqualTerm() 
            : base()
        {
        }

        public LessOrEqualTerm(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            object val1 = Value1.Evaluate();
            object val2 = Value2.Evaluate();
            if (val1 is string)
            {
                val1 = ((string)val1).ToNumeric();
            }
            if (val2 is string)
            {
                val2 = ((string)val2).ToNumeric();
            }
            if (val1 == null || val2 == null)
            {
                throw new Exception(string.Format("{0} : Can't compare values.", this.ToString()));
            }
            Type evaluationType = TypeHelper.GetCommonType(val1.GetType(), val2.GetType());
            if (evaluationType == null)
            {
                throw new Exception(string.Format("{0} : Can't compare values.", this.ToString()));
            }

            IComparable comparable1;
            IComparable comparable2;
            comparable1 = Convert.ChangeType(val1, evaluationType) as IComparable;
            comparable2 = Convert.ChangeType(val2, evaluationType) as IComparable;

            if (comparable1 == null || comparable2 == null)
            {
                throw new Exception(string.Format("{0} : Can't compare values.", this.ToString()));
            }

            return comparable1.CompareTo(comparable2) <= 0;
        }

        public override string Symbol
        {
            get { return "<="; }
        }

        public override TermGroups ChildrenTermGroup
        {
            get { return TermGroups.NumberTerm | TermGroups.StringTerm; }
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.TermLessOrEqual; }
        }

        public override string Label
        {
            get { return "Less than or equal to"; }
        }
    }
}
