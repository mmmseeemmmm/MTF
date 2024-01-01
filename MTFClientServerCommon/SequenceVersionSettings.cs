using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [Serializable]
    public class SequenceVersionSettings : MTFDataTransferObject
    {
        public string EmailServer
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
    }
}
