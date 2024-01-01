using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class TermWrapper : Term
    {
        public TermWrapper()
            : base()
        {
        }

        public TermWrapper(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public object Value
        {
            get { return GetProperty<object>(); }
            set { SetProperty(value); }
        }

        public string TypeName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public override object Evaluate()
        {
            if (Value is Term)
            {
                return ((Term)Value).Evaluate();
            }
            
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

        public override string ToString()
        {
            if (Value!=null)
            {
                return Value.ToString();
            }
            return string.Empty;
        }
    }
}
