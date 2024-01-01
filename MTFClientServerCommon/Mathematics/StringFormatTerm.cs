using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.Mathematics
{
    [Serializable]
    public class StringFormatTerm : Term
    {
        public StringFormatTerm()
            : base()
        {
            if (this.Value == null)
            {
                this.Value = new MTFStringFormat();
            }
        }

        public StringFormatTerm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public MTFStringFormat Value
        {
            get { return GetProperty<MTFStringFormat>(); }
            set { SetProperty(value); }
        }

        public override object Evaluate()
        {
            if (Value == null || string.IsNullOrEmpty(Value.Text))
            {
                return string.Empty;
            }
            string renderedString = string.Empty;
            if (Value.Parameters == null)
            {
                renderedString = Value.Text;
            }
            else
            {
                List<object> param = new List<object>();
                foreach (Term t in Value.Parameters)
                {
                    try
                    {
                        param.Add(t.Evaluate());
                    }
                    catch (Exception e)
                    {
                        param.Add(e.Message);
                    }
                }

                renderedString = string.Format(Value.Text, param.ToArray());
            }

            return renderedString.Replace("\\n", Environment.NewLine);
        }

        public override Type ResultType
        {
            get { return typeof(string); }
        }

        public override string Symbol
        {
            get { return "StringFormat"; }
        }

        public override TermGroups TermGroup
        {
            get { return TermGroups.None | TermGroups.StringTerm; }
        }

        public override TermGroups ChildrenTermGroup
        {
            get { return TermGroups.None; }
        }

        public override string ToString()
        {
            if (Value!=null)
            {
                return Value.ToString();
            }
            return string.Empty;
        }

        public override AutomotiveLighting.MTFCommon.MTFIcons Icon
        {
            get { return AutomotiveLighting.MTFCommon.MTFIcons.TermStringFormat; }
        }

        public override string Label
        {
            get { return "String format"; }
        }
    }
}
