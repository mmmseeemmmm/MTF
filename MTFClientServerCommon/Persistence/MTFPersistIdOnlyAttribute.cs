using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MTFPersistIdOnlyAttribute : Attribute
    {
        public MTFPersistIdOnlyAttribute()
        {
            IdentifierName = "Id";
        }

        public string IdentifierName
        {
            get;
            set;
        }
    }
}
