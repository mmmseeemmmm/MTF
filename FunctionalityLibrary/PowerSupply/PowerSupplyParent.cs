using System;
using AutomotiveLighting.MTFCommon;

namespace PowerSupply
{
    public abstract class PowerSupplyParent:IDisposable,IPowerSupply
    {
        public abstract void Init();
        public abstract void Dispose();
        public abstract void Rst();
        public abstract void VoltageRamp(double startVolt, double finVolt, double current, double dwell);
        public abstract void CurrentRamp(double startCurrent, double finCurrent, double volt, double dwell);
        public abstract void VoltageCurrentRamp(double startVolt, double finVolt, double startCurrent, double finCurrent, double dwell);
        //public abstract string SendQuery(string query);
        //public abstract void SendCmd(string cmd);

        public string identification = string.Empty;
        public bool connectionSucceed = false;
        public bool stop=false;
        //protected 

        public abstract string Idn { get; }
        public abstract double Volt { get; set; }
        public abstract double Current { get; set; }
        public abstract double MeasCurrent { get; }
        public abstract double MeasVolt { get; }
        public abstract double Power { get; set; }
        public abstract bool Output { get; set; }
        

        public IMTFSequenceRuntimeContext RuntimeContext { get; set; }

    }
}
