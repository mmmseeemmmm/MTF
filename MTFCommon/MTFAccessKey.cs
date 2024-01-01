using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFCommon
{
    public class MTFAccessKey
    {
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string Type { get; set; }
        public DateTime Expiration { get; set; }
    }
}
