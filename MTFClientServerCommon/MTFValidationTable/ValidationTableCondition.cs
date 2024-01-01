using MTFClientServerCommon.Mathematics;
using System;
using System.Runtime.Serialization;
using MTFClientServerCommon.Helpers;

namespace MTFClientServerCommon.MTFValidationTable
{
    [Serializable]
    public class ValidationTableCondition : MTFDataTransferObject
    {
        public ValidationTableCondition(string alias, string displayKey, Term term)
            : base()
        {
            this.Alias = alias;
            this.Term = term;
            this.DisplayKey = displayKey;
        }

        public ValidationTableCondition(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override string ObjectVersion
        {
            get { return "1.0.1"; }
        }

        protected override void VersionConvert(string fromVersion)
        {
            base.VersionConvert(fromVersion);
            if (fromVersion == "1.0.0")
            {
                DisplayKey = ValidationTableHelper.SetDisplayKey(Alias);
                fromVersion = "1.0.1";
            }
        }

        public string Alias
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string DisplayKey
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public Term Term
        {
            get { return GetProperty<Term>(); }
            set { SetProperty(value); }
        }
    }
}
