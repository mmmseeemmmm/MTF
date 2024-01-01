using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace PowerSupply
{

    //[MTFClass(Description = "Lambda Power Supply Driver")]
    //[MTFClassCategory("Power Supply")]
    public class MTFLambda : IPowerSupply //PowerSupply_Old, IDisposable,IPowerSupply,ISyskon
    {

        private LambdaBase lamba = null;



        //[MTFConstructor(Descritpion = "Lambda Power Supply Connected via LAN")]
        //[MTFAllowedParameterValue("type", "Syskon", "syskon")]
        public MTFLambda(string ipAdress, int port)
        {
            //lamba = new LambdaTCP(ipAdress, port) { InitProgress = Progress1 };

        }


        //[MTFConstructor(Descritpion = "Lambda Power Supply Connected Serial Port")]
        public MTFLambda(string comPort, int baudRate, int serialAdress)
        {
            //lamba = new LambdaSerial(comPort, baudRate, serialAdress) { InitProgress = Progress1 };

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
            //lamba.InitProgress = Progress1;
            //lamba.Init();
            //lamba.identification = lamba.Idn;
            //if (StdConsoleProgress != null)
            //{
            //    StdConsoleProgress("Power supply initialized");
            //}
            //else
            //{

            //}
        }

         //[MTFMethod]
        public void Rst()
        {
            lamba.Rst();
        }




        //[MTFProperty]
        public string Idn
        {
            get
            {
                return lamba.identification;
            }
        }
        //[MTFProperty]
        public double Volt
        {
            get
            {
                return lamba.Volt;
            }
            set
            {
                lamba.Volt = value;
            }
        }
        //[MTFProperty]
        public double Current
        {
            get
            {
                return lamba.Current;
            }
            set
            {
                lamba.Current = value;
            }
        }

        //[MTFProperty]
        public double Power
        {
            get
            {
                return lamba.Power;
            }
            set
            {
                lamba.Power = value;
            }
        }

        //[MTFProperty]
        //[MTFAllowedPropertyValue("Output ON", "ON")]
        //[MTFAllowedPropertyValue("Output OFF", "OFF")]
        public bool Output
        {
            get
            {
                return lamba.Output;
            }
            set
            {
                lamba.Output = value;
            }
        }
        

        public void Dispose()
        {
            if (lamba != null)
            {
                lamba.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        ~MTFLambda()
        {
            this.Dispose();
        }



        public void VoltageRamp(double startVolt, double finVolt, double current, double dwell)
        {
            throw new NotImplementedException();
        }

        public void CurrentRamp(double startCurrent, double finCurrent, double volt, double dwell)
        {
            throw new NotImplementedException();
        }

        public void VoltageCurrentRamp(double startVolt, double finVolt, double startCurrent, double finCurrent, double dwell)
        {
            throw new NotImplementedException();
        }


        public double MeasCurrent
        {
            get 
            {
                return lamba.MeasCurrent;
            }
        }


        public double MeasVolt
        {
            get
            {
                return lamba.MeasVolt;
            }
        }
    }
}
