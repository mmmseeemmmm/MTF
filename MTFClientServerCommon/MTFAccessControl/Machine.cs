using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.MTFAccessControl
{
    [Serializable]
    public class AccessMachine
    {
        public string MachineName { get; set; }
        public List<AccessRole> Roles { get; set; }
        public List<AccessSequence> Sequences { get; set; }

        public AccessMachine(string MachineName, List<AccessRole> Roles, List<AccessSequence> Sequences)
        {
            this.MachineName = MachineName;
            this.Roles = Roles;
            this.Sequences = Sequences;
        }        
    }
}
