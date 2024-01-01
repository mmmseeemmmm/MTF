using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MTFClientServerCommon
{
    [Serializable]
    public class SequenceVariantGroup : MTFDataTransferObject
    {
        public SequenceVariantGroup()
        {
        }

        public SequenceVariantGroup(SerializationInfo info, StreamingContext context)
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
                    Name = "Mounting side";
                }
                fromVersion = "1.0.0";
            }
        }

        public string Name
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public IList<SequenceVariantValue> Values
        {
            get { return GetProperty<IList<SequenceVariantValue>>(); }
            set { SetProperty(value); }
        }
    }
}
