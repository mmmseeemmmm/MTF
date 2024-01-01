using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFClientServerCommon.MTFValidationTable
{
    public enum MTFValidationTableExecutionMode
    {
        [Description("All rows")]
        AllRows,
        [Description("Only set rows")]
        OnlySet,
    }
}
