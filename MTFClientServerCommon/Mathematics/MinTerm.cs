using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class MinTerm : ListOperationTerm
    {
        public MinTerm()
            : base()
        {
        }

        public MinTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            if (Value == null || Value.Parameters == null)
            {
                throw new Exception("Term Min isn't create.");
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
                
            var result = float.MaxValue;
            foreach (var item in param)
            {
                if (float.Parse(item.ToString()) < result)
                {
                    result = float.Parse(item.ToString());
                }
            }
            return result;
        }

        public override Type ResultType
        {
            get { return typeof(float); }
        }

        public override string Symbol
        {
            get { return "Min"; }
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.TermMin; }
        }

        public override string Label
        {
            get { return "Min value"; }
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
