using System;

namespace MTFClientServerCommon
{
    [Serializable]
    public class MTFExternalSequenceInfo
    {
        public MTFSequence ExternalSequence { get; set; }
        public bool IsEnabled { get; set; }

        public MTFExternalSequenceInfo(MTFSequence externalSequence)
            : this(externalSequence, true)
        {
        }

        public MTFExternalSequenceInfo(MTFSequence externalSequence, bool isEnabled)
        {
            ExternalSequence = externalSequence;
            IsEnabled = isEnabled;
        }
    }
}
