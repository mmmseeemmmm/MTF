using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerSupplyExtensionMethods;
using AutomotiveLighting.MTFCommon;

namespace PowerSupply
{
    public abstract class LambdaBase:PowerSupplyParent
    {
        protected double maxPower = 0;
        protected double power = 0;
        protected double volt = 0;
        protected double current = 0;
        //public abstract string Read();
        //public abstract void Write(string cmd);
        //public MTFPercentProgressEventHandler InitProgress { get; set; }
     
        public override string Idn
        {
            get { return identification; }
        }

        public override double Power
        {
            get
            {
                return power;
            }

            set
            {

                if ((value <= maxPower) && (value>= (current*volt)))
                {
                    power = value;
                }

                else
                {
                    throw new Exception("Maximum power limit exceeded or current settings of output current and volt is higher than Demanded Power Limit");
                }
            }
        }

        protected void WaitAfterReset()
        {

            int steps = 4;
            for (int i = 0; i < steps; i++) //60
            {
                System.Threading.Thread.Sleep(500);
                RuntimeContext.ProgressNotification(i * 100 / steps);
            }
        
        }

  /*      public override double Volt
        {
            get
            {
                return SendQuery(":VOLT?").GetDouble();
            }

            set
            {
                SendCmd(":VOLT " + value.FormatIntoString());
            }
        }

        public override double Current
        {
            get
            {
                return SendQuery(":CURR?").GetDouble();
            }

            set
            {
                SendCmd(":CURR " + value.FormatIntoString());
            }
        }


        public override string Output
        {

            get
            {
                return SendQuery("OUTP:STAT?");
            }

            set
            {
                SendCmd("OUTP:STAT " + value);
            }

        }*/



 


    }
}
