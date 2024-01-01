using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFApp.OpenSaveSequencesDialog
{
    class NameStructure
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string fullName;
        public string FullName
        {
            get { return fullName; }
            set { fullName = value; }
        }

        public NameStructure(string name, string fullName)
        {
            this.name = name;
            this.fullName = fullName;
        }
    }
}
