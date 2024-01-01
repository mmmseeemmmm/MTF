using System;
using System.Runtime.Serialization;
using MTFClientServerCommon.Constants;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    class GoldSampleTerm : Term
    {
        public GoldSampleTerm()
            : base()
        {

        }

        public GoldSampleTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public Term ActualValue
        {
            get { return GetProperty<Term>(); }
            set { SetProperty(value); }
        }

        public object GoldSampleValue
        {
            get { return GetProperty<object>(); }
            set { SetProperty(value); }
        }

        public double Percentage
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public override object Evaluate()
        {
            double gs;
            if (GoldSampleValue!=null && double.TryParse(GoldSampleValue.ToString(), out gs))
            {
                double actualValue = double.Parse(ActualValue.Evaluate().ToString());
                var gsDispersion = Math.Abs(gs * Percentage / 100);
                return gs - gsDispersion <= actualValue && actualValue <= gs + gsDispersion;
            }
            return true;
        }

        public override string Symbol => ValidationTableConstants.ColumnGs;

        public override TermGroups ChildrenTermGroup => TermGroups.NumberTerm | TermGroups.StringTerm;

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon => AutomotiveLighting.MTFCommon.MTFIcons.None;

        public override string Label => "GoldSample";


        public override Type ResultType
        {
            get { return typeof(bool); }
        }

        public override TermGroups TermGroup
        {
            get
            {
                return TermGroups.None
                  | TermGroups.LogicalTerm;
            }
        }
    }
}
