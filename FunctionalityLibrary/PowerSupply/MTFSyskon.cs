using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace PowerSupply
{

    //[MTFClass(Description = "Syskon Power Supply Driver")]
    //[MTFClassCategory("Power Supply")]
    public class MTFSyskon:IPowerSupply //PowerSupply_Old, IDisposable,IPowerSupply,ISyskon
    {

        private SyskonBase syskon = null;



        //[MTFConstructor(Descritpion = "Syskon Power Supply Connected via GPIB")]
        //[MTFAllowedParameterValue("type", "Syskon", "syskon")]
         public MTFSyskon(int boardNumber, byte adress)
        {
            //syskon = new SyskonGPIB(boardNumber,adress) { InitProgress = Progress1 };

        }

        
        //[MTFConstructor(Descritpion = "Syskon Power Supply Connected Serial Port")]
        public MTFSyskon(string comport, int baudrate)
        {
            //syskon = new SyskonSerial(comport, baudrate) { InitProgress = Progress1 };

        }


        //[MTFProgressEvent("Standard output", Index = 1)]
        //public event MTFStringProgressEventHandler StdConsoleProgress;
        //[MTFProgressEvent("Progress", Index = 0)]
        //public event MTFPercentProgressEventHandler Progress1;
        //[MTFRaiseProgressEvent("StdConsoleProgress")]
        //[MTFRaiseProgressEvent("Progress1")]
        //[MTFMethod]
        public void Init()
        {
            //syskon.InitProgress = Progress1;
            syskon.Init();
            syskon.identification = syskon.Idn;
            /*if (StdConsoleProgress != null)
            {
                    StdConsoleProgress("Power supply initialized");
            }
            else
            {

            }*/
          }

        public void Rst()
        {
            syskon.Rst();
        }
    
        

        //[MTFProperty]
        public string Idn
        {
            get
            {
                return syskon.identification;
            }
        }
        //[MTFProperty]
        public double Volt
        {
            get
            {
                return syskon.Volt;
            }
            set
            {
                syskon.Volt=value;
            }
        }
        //[MTFProperty]
        public double Current
        {
            get
            {
                return syskon.Current;
            }
            set
            {
                syskon.Current=value;
            }
        }

        //[MTFProperty]
        public double Power
        {
            get
            {
                return syskon.Power;
            }
            set
            {
                syskon.Power = value;
            }
        }

        //[MTFProperty]
        //[MTFAllowedPropertyValue("Output ON", "ON")]
        //[MTFAllowedPropertyValue("Output OFF", "OFF")]
        public bool Output
        {
            get
            {
                return syskon.Output;
            }
            set
            {
                syskon.Output=value;
            }
        }
        //[MTFMethod]
        public void VoltageRamp(double startVolt, double finVolt, double current, double dwell)
        {
            syskon.VoltageRamp(startVolt,finVolt,current,dwell);
        }
        //[MTFMethod]
        public void CurrentRamp(double startCurrent, double finCurrent, double volt, double dwell)
        {
            throw new NotImplementedException();
        }
        //[MTFMethod]
        public void VoltageCurrentRamp(double startVolt, double finVolt, double startCurrent, double finCurrent, double dwell)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (syskon != null)
            {
                syskon.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        ~MTFSyskon()
        {
            this.Dispose();
        }



        public double MeasCurrent
        {
            get 
            {
                return syskon.MeasCurrent;
            }
        }

        public double MeasVolt
        {
            get
            {
                return syskon.MeasVolt;
            }
        }
    }
}

