using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class SubtractTerm : BinaryTerm
    {
        public SubtractTerm()
            : base()
        {
        }

        public SubtractTerm(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            if (Value1.ResultType == typeof(Int16))
            {
                return Convert.ToInt16(Value1.Evaluate()) - Convert.ToInt16(Value2.Evaluate());
            }
            else if (Value1.ResultType == typeof(Int32))
            {
                return Convert.ToInt32(Value1.Evaluate()) - Convert.ToInt32(Value2.Evaluate());
            }
            else if (Value1.ResultType == typeof(Int64))
            {
                return Convert.ToInt64(Value1.Evaluate()) - Convert.ToInt64(Value2.Evaluate());
            }
            else if (Value1.ResultType == typeof(UInt16))
            {
                return Convert.ToUInt16(Value1.Evaluate()) - Convert.ToUInt16(Value2.Evaluate());
            }
            else if (Value1.ResultType == typeof(UInt32))
            {
                return Convert.ToUInt32(Value1.Evaluate()) - Convert.ToUInt32(Value2.Evaluate());
            }
            else if (Value1.ResultType == typeof(UInt64))
            {
                return Convert.ToUInt64(Value1.Evaluate()) - Convert.ToUInt64(Value2.Evaluate());
            }
            else if (Value1.ResultType == typeof(float))
            {
                return Convert.ToSingle(Value1.Evaluate()) - Convert.ToSingle(Value2.Evaluate());
            }
            else if (Value1.ResultType == typeof(decimal))
            {
                return Convert.ToDecimal(Value1.Evaluate()) - Convert.ToDecimal(Value2.Evaluate());
            }
            else if (Value1.ResultType == typeof(double))
            {
                return Convert.ToDouble(Value1.Evaluate()) - Convert.ToDouble(Value2.Evaluate());
            }
            else if (Value1.ResultType == typeof(sbyte))
            {
                return Convert.ToSByte(Value1.Evaluate()) - Convert.ToSByte(Value2.Evaluate());
            }
            else if (Value1.ResultType == typeof(byte))
            {
                return Convert.ToByte(Value1.Evaluate()) - Convert.ToByte(Value2.Evaluate());
            }

            throw new Exception("MTF Subtract operation isn't supported with given type.");
        }

        public override Type ResultType
        {
            get { return Value1.ResultType; }
        }

        public override string Symbol
        {
            get { return "-"; }
        }

        public override TermGroups TermGroup
        {
            get
            {
                return TermGroups.None
                    | TermGroups.NumberTerm;
            }
        }

        public override TermGroups ChildrenTermGroup
        {
            get { return TermGroups.NumberTerm; }
        }


        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.TermSubtract; }
        }

        public override string Label
        {
            get { return "Subtraction"; }
        }
    }
}
