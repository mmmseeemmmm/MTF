using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.MTFAccessControl
{
    [Serializable]
    public class AccessRole
    {
        public string Role { get; set; }

        public AccessRole(string Role)
        {
            this.Role = Role;
        }
    }
}
