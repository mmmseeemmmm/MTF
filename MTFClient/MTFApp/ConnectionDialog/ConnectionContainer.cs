using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFApp.ConnectionDialog
{
    [Serializable]
    public class ConnectionContainer
    {
        private string alias;
        private string host;
        private string port;

        public string Alias
        {
            get { return alias; }
            set { alias = value; }
        }

        public string Host
        {
            get { return host; }
            set { host = value; }
        }

        public string Port
        {
            get { return port; }
            set { port = value; }
        }
    }
}
