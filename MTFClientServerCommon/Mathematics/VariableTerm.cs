using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class VariableTerm : Term
    {
        public VariableTerm()
            : base()
        {
        }

        public VariableTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            return InjectedValue;
        }

        public override Type ResultType
        {
            get { return MTFVariable == null ? null : Type.GetType(MTFVariable.TypeName); }
        }

        public override string Symbol
        {
            get { return "Variable"; }
        }

        [MTFPersistIdOnly]
        public MTFVariable MTFVariable
        {
            get { return GetProperty<MTFVariable>(); }
            set { SetProperty(value); }
        }


        public override string ToString()
        {
            return MTFVariable == null ? "Null" : "{"+MTFVariable.Name+"}";
        }


        public override TermGroups TermGroup
        {
            get
            {
                return TermGroups.None
                    | TermGroups.NumberTerm
                    | TermGroups.StringTerm
                    | TermGroups.LogicalTerm;
            }
        }

        public override TermGroups ChildrenTermGroup
        {
            get { return TermGroups.None; }
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.TermVariable; }
        }

        public override string Label
        {
            get { return "Variable"; }
        }

        public object InjectedValue { get; set; }
    }
}
