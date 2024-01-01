using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    public class ActivityStringProgressChangedEventArgs
    {
        public StatusMessage Message { get; set; }
        public string StringName { get; set; }
        public List<Guid> ExecutionPath { get; set; }
        public ActivityStringProgressCommand Command { get; set; }
    }
    public enum ActivityStringProgressCommand
    {
        ShowMessage,
        CleanErrorWindow,
    }
}
