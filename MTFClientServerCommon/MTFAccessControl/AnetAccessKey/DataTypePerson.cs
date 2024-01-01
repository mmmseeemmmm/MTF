using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.MTFAccessControl.AnetAccessKey
{
    [Serializable]
    public class DataTypePerson
    {
        private List<Person> person;
        public List<Person> Person
        {
            get { return person; }
            set { person = value; }
        }

        private List<Role> allRoles;
        public List<Role> AllRoles
        {
            get { return allRoles; }
            set { allRoles = value; }
        }
    }
}
