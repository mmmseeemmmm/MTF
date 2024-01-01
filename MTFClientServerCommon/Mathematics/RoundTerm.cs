using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon.Helpers;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class RoundTerm : UnaryRoundTerm
    {
        public RoundTerm()
            : base()
        {
        }

        public RoundTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            // ToDo - UnaryNumberTerm is always double now
            if (this.Value.ResultType == typeof(double))
            {
                switch (this.SelectedRoundMode)
                {
                    case RoundModes.Round:
                        {
                            return Math.Round((double)Value.Evaluate(), RoundValue);
                        }
                    case RoundModes.RoundUp:
                        {
                            return Math.Ceiling((double)Value.Evaluate() * Math.Pow(10, RoundValue)) / Math.Pow(10, RoundValue);
                        }
                    case RoundModes.RoundDown:
                        {
                            return Math.Floor((double)Value.Evaluate() * Math.Pow(10, RoundValue)) / Math.Pow(10, RoundValue);
                        }
                }
            }
            throw new Exception("MTF Round operation isn't supported with given type.");
        }

        public override string Symbol
        {
            get { return "Round"; }
        }

        public override MTFIcons Icon
        {
            get { return MTFIcons.TermRound; }
        }

        public override string Label
        {
            get { return "Round"; }
        }

        public override string ToString()
        {
            return this.Value != null ? string.Format("{0} ({1}, {2})", SelectedRoundMode, Value, RoundValue) : string.Empty;
        }
    }
}
