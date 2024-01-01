using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MTFClientServerCommon.Helpers;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class ConstantTerm : Term
    {

        private Type type;

        public ConstantTerm()
            : base()
        {

        }
        public ConstantTerm(Type type)
            : base()
        {
            this.type = type;
        }

        public ConstantTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public object Value
        {
            get { return GetProperty<object>(); }
            set { setValue(value); }
        }

        private void setValue(object val)
        {
            if (val == null)
            {
                SetProperty(val, "Value");

                return;
            }

            if (type != null && type != typeof(void) && !string.IsNullOrEmpty(val.ToString()))
            {
                Type conversionType = Nullable.GetUnderlyingType(type) ?? type;

                SetProperty(Convert.ChangeType(val, conversionType), "Value");

                return;
            }

            SetProperty(val, "Value");
        }

        public override string ToString()
        {
            if (Value == null)
            {
                return string.Empty;
            }

            return Value.ToString();
        }

        public override object Evaluate()
        {
            return Value;
        }

        public override Type ResultType
        {
            get
            {
                Type resultType = null;

                if (Value != null)
                {
                    resultType = Value.GetType();
                }

                if (string.IsNullOrWhiteSpace(this.TargetType))
                {
                    return resultType;
                }

                Type targetType = Type.GetType(this.TargetType);

                if (targetType!=null)
                {
                    if (resultType != null)
                    {
                        if (resultType == targetType)
                        {
                            return resultType;
                        }

                        resultType = targetType;
                    }
                    else
                    {
                        resultType = targetType;
                    } 
                }

                return resultType;
            }
        }

        public override string Symbol
        {
            get { return "Value"; }
        }

        public override string ToStringAsSubterm()
        {
            return ToString();
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
            get { return AutomotiveLighting.MTFCommon.MTFIcons.TermVal; }
        }

        public override string Label
        {
            get { return "Value"; }
        }
    }
}
