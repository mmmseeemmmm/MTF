using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Mathematics;

namespace MTFClientServerCommon
{
    [Serializable]
    public class SequenceVariantGroupValue : MTFDataTransferObject
    {
        public SequenceVariantGroupValue()
        {
        }

        public SequenceVariantGroupValue(SerializationInfo info, StreamingContext context)
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
                if (Name == "Mouting side")
                {
                    Name = SequenceVariantConstants.MountingSideCategory;
                }
                fromVersion = "1.0.1";
            }
        }

        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public List<SequenceVariantValue> Values
        {
            get { return GetProperty<List<SequenceVariantValue>>(); }
            set { SetProperty(value); }
        }

        public Term Term
        {
            get { return GetProperty<Term>(); }
            set { SetProperty(value); }
        }

        public int Index
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public bool IsEmpty => (Values == null || Values.Count == 0) && Term == null;

        public string GetJoinedValues()
        {
            return Values != null ? string.Join(";", Values.Select(g => g.Name)) : null;
        }
    }
}
