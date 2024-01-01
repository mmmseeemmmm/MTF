using System;
using System.Collections.Generic;

namespace MTFClientServerCommon
{
    [Serializable]
    public class StatusMessage
    {
        public List<Guid> ActivityPath { get; set; }
        public List<ActivityIdentifier> ActivityNames { get; set; }
        public string Text { get; set; }
        public MessageType Type { get; set; }
        public DateTime TimeStamp { get; set; }


        public enum MessageType
        {
            Error,
            Warning,
            Info,
        }
    }
}
