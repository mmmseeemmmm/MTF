using System;
using System.Collections.Generic;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.Settings.SequenceVariant
{
    public class MainSequenceVariantResult
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public Guid SequenceId { get; set; }
        public string SequenceName { get; set; }
        public List<SequenceVariantValue> SequenceVariantValues { get; set; }
        public List<ExternalSequenceVariantResult> ExternalSequenceVariantResults { get; set; }
    }
}