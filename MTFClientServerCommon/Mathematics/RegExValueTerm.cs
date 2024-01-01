using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using AutomotiveLighting.MTFCommon;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class RegExValueTerm : UnaryTerm
    {
        public RegExValueTerm()
            : base()
        {
        }

        public RegExValueTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override object Evaluate()
        {
            if (string.IsNullOrEmpty(Pattern))
            {
                throw new Exception("MTF RegExValue operation - Pattern isn't set.");
            }
            var value = this.Value.Evaluate().ToString();
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception("MTF RegExValue operation - Value isn't set.");
            }
            if (string.IsNullOrWhiteSpace(Substring))
            {
                throw new Exception("MTF RegExValue operation - Name isn't set.");
            }

            try
            {
                var match = Regex.Match(value, Pattern);
                if (!match.Success)
                {
                    throw new Exception("MTF RegExValue operation - operation Success failed.");
                }

                return match.Groups[Substring].Value;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("MTF RegExValue operation - {0}", ex.Message));
            }
        }

        public override string Symbol
        {
            get { return "RegExValue"; }
        }

        public override MTFIcons Icon
        {
            get { return MTFIcons.TermRegExValue; }
        }

        public override string Label
        {
            get { return "RegEx Value"; }
        }

        public override Type ResultType
        {
            get { return typeof(string); }
        }

        public override string ToString()
        {
            return this.Value != null ? string.Format("{0} ('{1}', {2}) ({3})", Symbol, Pattern, Substring, this.Value) : string.Empty;
        }

        public override TermGroups TermGroup
        {
            get { return TermGroups.None | TermGroups.StringTerm; }
        }

        public override TermGroups ChildrenTermGroup
        {
            get { return TermGroups.StringTerm; }
        }

        public string Pattern
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Substring
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }
}
