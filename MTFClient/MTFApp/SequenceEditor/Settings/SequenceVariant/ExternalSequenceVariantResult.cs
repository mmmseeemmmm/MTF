using System;
using System.Collections.Generic;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.Settings.SequenceVariant
{
    public class ExternalSequenceVariantResult
    {
        public Guid SequenceId { get; set; }
        public string SequenceName { get; set; }
        public List<SequenceVariantValue> SequenceVariantValues { get; set; }
    }
}