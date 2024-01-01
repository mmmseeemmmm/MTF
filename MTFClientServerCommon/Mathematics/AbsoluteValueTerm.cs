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
    public class AbsoluteValueTerm : UnaryNumberTerm
    {
        public AbsoluteValueTerm()
            : base()
        {
        }

        public AbsoluteValueTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            if (this.Value.ResultType == typeof(Int16))
            {
                return Math.Abs((Int16)this.Value.Evaluate());
            }
            if (this.Value.ResultType == typeof(Int32))
            {
                return Math.Abs((Int32)this.Value.Evaluate());
            }
            if (this.Value.ResultType == typeof(Int64))
            {
                return Math.Abs((Int64)this.Value.Evaluate());
            }
            if (this.Value.ResultType == typeof(UInt16))
            {
                return Math.Abs((UInt16)this.Value.Evaluate());
            }
            if (this.Value.ResultType == typeof(UInt32))
            {
                return Math.Abs((UInt32)this.Value.Evaluate());
            }
            if (this.Value.ResultType == typeof(float))
            {
                return Math.Abs((float)this.Value.Evaluate());
            }
            if (this.Value.ResultType == typeof(decimal))
            {
                return Math.Abs((decimal)this.Value.Evaluate());
            }
            if (this.Value.ResultType == typeof(double))
            {
                return Math.Abs((double)this.Value.Evaluate());
            }
            if (this.Value.ResultType == typeof(sbyte))
            {
                return Math.Abs((sbyte)this.Value.Evaluate());
            }
            if (this.Value.ResultType == typeof(byte))
            {
                return Math.Abs((byte)this.Value.Evaluate());
            }

            throw new Exception("MTF Absolute value operation isn't supported with given type.");
        }

        public override string Symbol
        {
            get { return "||"; }
        }

        public override string ToString()
        {
            return this.Value != null ? string.Format("|{0}|", this.Value.ToStringAsSubterm()) : string.Empty;
        }

        public override MTFIcons Icon
        {
            get { return MTFIcons.TermAbsoluteValue; }
        }

        public override string Label
        {
            get { return "Absolute value"; }
        }
    }
}
