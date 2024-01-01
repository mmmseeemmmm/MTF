using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class RangeTerm : ListOperationTerm
    {
        public RangeTerm()
            : base()
        {
        }

        public RangeTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            if (Value == null || Value.Parameters == null)
            {
                throw new Exception("Term Range isn't create.");
            }
            
            List<object> param = new List<object>();
            foreach (Term t in this.Value.Parameters)
            {
                try
                {
                    param.Add(t.Evaluate());
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }

            var max = float.MinValue;
            var min = float.MaxValue;
            foreach (var item in param)
            {
                if (float.Parse(item.ToString()) > max)
                {
                    max = float.Parse(item.ToString());
                }
                if (float.Parse(item.ToString()) < min)
                {
                    min = float.Parse(item.ToString());
                }
            }
            return (max - min);
        }

        public override Type ResultType
        {
            get { return typeof(float); }
        }

        public override string Symbol
        {
            get { return "Range"; }
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.TermRange; }
        }

        public override string Label
        {
            get { return "Range"; }
        }

        public override TermGroups TermGroup
        {
            get { return TermGroups.None | TermGroups.LogicalTerm; }
        }

        public override TermGroups ChildrenTermGroup
        {
            get { return TermGroups.NumberTerm; }
        }
    }
}
