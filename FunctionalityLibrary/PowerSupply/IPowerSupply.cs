using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerSupply
{
    
    public interface IPowerSupply:IDisposable
    {
        void Init();

        void Rst();
        
        string Idn { get; }
                
        double Volt { get; set; }

        double Current { get; set; }

        double MeasCurrent { get; }

        double MeasVolt { get; }

        double Power { get; set; }

        bool Output { get; set; }

        void VoltageRamp(double startVolt, double finVolt, double current, double dwell);

        void CurrentRamp(double startCurrent, double finCurrent, double volt, double dwell);

        void VoltageCurrentRamp(double startVolt, double finVolt, double startCurrent, double finCurrent, double dwell);

    }
}
