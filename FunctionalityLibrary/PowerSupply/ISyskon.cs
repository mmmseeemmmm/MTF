using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerSupply
{
    interface ISyskon
    {
        //syskon specific functions
        void VoltageRamp(double startVolt, double finVolt, double current, double dwell);
        void CurrentRamp(double startCurrent, double finCurrent, double volt, double dwell);
        void VoltageCurrentRamp(double startVolt, double finVolt, double startCurrent, double finCurrent, double dwell);
    }
}
