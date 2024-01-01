using MTFClientServerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFCore
{
    class MTFSimpleParameterValue : IParameterValue
    {
        public MTFSimpleParameterValue(string typeName, object value)
        {
            TypeName = typeName;
            Value = value;
        }

        public string TypeName
        {
            get;
            private set;
        }

        public object Value
        {
            get;
            private set;
        }
    }
}
