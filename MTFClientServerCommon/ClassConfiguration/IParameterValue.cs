using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon
{
    public interface IParameterValue
    {
        string TypeName { get; }
        object Value { get; }
    }
}
