using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    public class ActivityImageProgressChangedEventArgs
    {
        public byte[] ImageData { get; set; }
        public string ImageName { get; set; }
        public List<Guid> ExecutionPath { get; set; }
    }
}
