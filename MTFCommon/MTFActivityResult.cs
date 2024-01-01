using System;

namespace AutomotiveLighting.MTFCommon
{
    public class MTFActivityResult
    {
        public MTFActivity Activity { get; set; }
        public double ElapsedMs { get; set; }
        public double TimestampMs { get; set; }
        public bool CheckOutpuValueFailed { get; set; }
        public bool ExceptionOccured { get; set; }
        public string ExceptionMessage { get; set; }
        public uint NumberOfRepetition { get; set; }
        public Guid[] ActivityIdPath { get; set; }
        public MTFActivityResultStatus Status { get; set; }
    }

    public enum MTFActivityResultStatus
    {
        None,
        Ok,
        Nok,
        Warning
    }
}
