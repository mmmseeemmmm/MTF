using System;
using System.Collections.Generic;

namespace MTFApp.SequenceEditor.Settings.SequenceVariant
{
    public class SequenceDataPackage
    {
        public Guid SequenceId { get; set; }
        public List<GroupDataPackage> Groups { get; set; }
    }
}