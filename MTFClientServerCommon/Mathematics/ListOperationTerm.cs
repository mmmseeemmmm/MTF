using System;
using System.Runtime.Serialization;
using System.Text;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public abstract class ListOperationTerm : Term
    {
        public ListOperationTerm()
            : base()
        {
            if (this.Value == null)
            {
                this.Value = new MTFListOperation();
            }
        }

        public ListOperationTerm(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public MTFListOperation Value
        {
            get { return GetProperty<MTFListOperation>(); }
            set { SetProperty(value); }
        }
 
        public override Type ResultType
        {
            get { return typeof(float); }
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]", Symbol, objArrayToStr(Value as MTFListOperation));
        }

        private static string objArrayToStr(MTFListOperation arr)
        {
            if (arr == null || arr.Parameters == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (object o in arr.Parameters)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }
                sb.Append(o);
            }
            return sb.ToString();
        }

        public override TermGroups TermGroup
        {
            get { return TermGroups.None | TermGroups.LogicalTerm; }
        }

        public override TermGroups ChildrenTermGroup
        {
            get { return TermGroups.NumberTerm | TermGroups.LogicalTerm; }
        }
    }
}
