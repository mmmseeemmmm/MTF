using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFApp
{
    interface IControlStatus
    {
        ControlStatus ControlStatus { get; set; }
    }

    enum ControlStatus
    {
        None,
        Ok,
        Error,
        Warning,
        GoldSample,
    }
}
