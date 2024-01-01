using System;
using System.Collections.Generic;

namespace MTFClientServerCommon
{
    public class ActivityPercentProgressChangedEventArgs
    {
        public int Percent { get; set; }
        public string Text { get; set; }
        public string ProgressBarName { get; set; }
        public List<Guid> ExecutionPath { get; set; }
        public bool IsStarted { get; set; }
    }
}
