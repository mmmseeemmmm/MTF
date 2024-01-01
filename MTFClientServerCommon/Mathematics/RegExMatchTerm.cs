using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using AutomotiveLighting.MTFCommon;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class RegExMatchTerm : UnaryLogicalTerm
    {
        public RegExMatchTerm()
            : base()
        {
        }

        public RegExMatchTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            if (string.IsNullOrEmpty(Parameter))
            {
                throw new Exception("MTF RegExMatch operation - pattern isn't set.");
            }
            var value = this.Value.Evaluate().ToString();
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception("MTF RegExMatch operation - value isn't set.");
            }

            try
            {
                var match = Regex.Match(value, Parameter);
                return match.Success;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("MTF RegExMatch operation - {0}", ex.Message));
            }
        }

        public override string ToString()
        {
            return this.Value != null ? string.Format("{0} ({1}, '{2}')", Symbol, this.Value, Parameter) : string.Empty;
        }

        public override string Symbol
        {
            get { return "RegExMatch"; }
        }

        public override MTFIcons Icon
        {
            get { return MTFIcons.TermRegEx; }
        }

        public override string Label
        {
            get { return "RegEx Match"; }
        }

        public string Parameter
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }
}
