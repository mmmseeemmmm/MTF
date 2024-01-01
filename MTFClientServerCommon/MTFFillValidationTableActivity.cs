using System;
using System.Runtime.Serialization;
using AutomotiveLighting.MTFCommon;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFFillValidationTableActivity : MTFSequenceActivity
    {
        public MTFFillValidationTableActivity()
            : base()
        {

        }

        public MTFFillValidationTableActivity(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override MTFIcons Icon
        {
            get { return MTFIcons.TermFillValidationTable; }
        }
    }
}
