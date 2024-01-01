using System;
using System.IO;
using Version = MTFClientServerCommon.Version;

namespace MTFApp.ImportSequence
{
    public class SequencesInZip
    {
        public Guid Id { get; set; }
        public string SequenceFullName { get; set; }
        public string SequenceStoredName { get; set; }
        public string ParentFullName { get; set; }
        public Version MTFVersion { get; set; }

        public string SequenceName => Path.GetFileNameWithoutExtension(SequenceFullName);

        public string ParentName => Path.GetFileNameWithoutExtension(ParentFullName);
    }
}