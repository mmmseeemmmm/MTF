using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class PowerTerm : UnaryExponentTerm
    {
        public PowerTerm()
            : base()
        {
        }

        public PowerTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            if (this.Value.ResultType == typeof(Int16))
            {
                return (Int16)Math.Pow((Int16)Value.Evaluate(), Exponent);
            }
            if (this.Value.ResultType == typeof(Int32))
            {
                return (Int32)Math.Pow((Int32)Value.Evaluate(), Exponent);
            }
            if (this.Value.ResultType == typeof(Int64))
            {
                return (Int64)Math.Pow((Int64)Value.Evaluate(), Exponent);
            }
            if (this.Value.ResultType == typeof(UInt16))
            {
                return (UInt16)Math.Pow((UInt16)Value.Evaluate(), Exponent);
            }
            if (this.Value.ResultType == typeof(UInt32))
            {
                return (UInt32)Math.Pow((UInt32)Value.Evaluate(), Exponent);
            }
            if (this.Value.ResultType == typeof(UInt64))
            {
                return (UInt64)Math.Pow((UInt64)Value.Evaluate(), Exponent);
            }
            if (this.Value.ResultType == typeof(float))
            {
                return (float)Math.Pow((float)Value.Evaluate(), Exponent);
            }
            if (this.Value.ResultType == typeof(double))
            {
                return Math.Pow((double)Value.Evaluate(), Exponent);
            }
            if (this.Value.ResultType == typeof(sbyte))
            {
                return (sbyte)Math.Pow((sbyte)Value.Evaluate(), Exponent);
            }
            if (this.Value.ResultType == typeof(byte))
            {
                return (byte)Math.Pow((byte)Value.Evaluate(), Exponent);
            }

            throw new Exception("MTF Power operation isn't supportetd with given type.");
        }

        public override string ToString()
        {
            return this.Value != null ? string.Format("{0} {1} {2}", this.Value, Symbol, Exponent) : string.Empty;
        }

        public override string Symbol
        {
            get { return "^"; }
        }

        public override MTFIcons Icon
        {
            get { return MTFIcons.TermPower; }
        }

        public override string Label
        {
            get { return "y-th power"; }
        }
    }
}
