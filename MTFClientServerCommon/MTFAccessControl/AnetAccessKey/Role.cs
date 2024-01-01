using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MTFClientServerCommon.MTFAccessControl.AnetAccessKey
{
    [Serializable]
    public class Role : ICloneable
    {
        public Role()
        {
            id = Guid.NewGuid();    
        }

        private Guid id;
        public Guid Id
        {
            get { return id; }
            set { id = value; }
        }
        
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private bool isModify = false;
        [XmlIgnore]
        public bool IsModify
        {
            get { return isModify; }
            set { isModify = value; }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
