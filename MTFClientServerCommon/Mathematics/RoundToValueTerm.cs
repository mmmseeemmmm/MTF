using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;
using MTFClientServerCommon.Helpers;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class RoundToValueTerm : UnaryRoundTerm
    {
        public RoundToValueTerm()
            : base()
        {
            SelectedRoundMode = RoundModes.RoundUp;
        }

        public RoundToValueTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            // ToDo - UnaryNumberTerm is always double now
            if (this.Value.ResultType == typeof(double) || this.Value.ResultType == typeof(int))
            {
                switch (this.SelectedRoundMode)
                {
                    case RoundModes.RoundUp:
                        {
                            return (int)Math.Ceiling(Convert.ToDouble(Value.Evaluate()) / RoundValue) * RoundValue;
                        }
                    case RoundModes.RoundDown:
                        {
                            return (int)Math.Floor(Convert.ToDouble(Value.Evaluate()) / RoundValue) * RoundValue;
                        }
                }
            }
            throw new Exception("MTF RoundToValue operation isn't supported with given type.");
        }

        public override string Symbol
        {
            get { return "Round to value"; }
        }

        public override MTFIcons Icon
        {
            get { return MTFIcons.TermRoundToValue; }
        }

        public override string Label
        {
            get { return "Round to value"; }
        }

        public override string ToString()
        {
            return this.Value != null ? string.Format("{0}To{1} ({2})", SelectedRoundMode, RoundValue, Value) : string.Empty;
        }

        public IEnumerable<EnumValueDescription> ListRoundModes
        {
            get
            {
                var listRoundModes = EnumHelper.GetAllValuesAndDescriptions<RoundModes>().ToList();
                listRoundModes.Remove(listRoundModes.First(x => (RoundModes)x.Value == RoundModes.Round));
                return listRoundModes;
            }
        }
    }
}
