using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.MTFAccessControl
{
    [Serializable]
    public class AccessSequence
    {
        public string Sequence { get; private set; }
        public AccessSequence(string Sequence)
        {
            this.Sequence = Sequence;
        }
    }
}
