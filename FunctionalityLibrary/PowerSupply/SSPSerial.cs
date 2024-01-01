using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerSupplyExtensionMethods;

namespace PowerSupply
{
    class SSPSerial : SyskonSerial
    {
        protected double power = 0;
        protected double maxPower = 0;
        protected double current = 0;
        protected double volt = 0;
        public SSPSerial(string comPort, int baudRate) : base( comPort, baudRate)
        {

        }

        
        public override double Volt
        {
            get
            {
                return SendQuery("USET?").GetDouble();
            }

            set
            {
                if (current * value <= power)
                {
                    SendCmd("USET " + value.FormatIntoString());
                    volt = Volt;
                }

                else
                {
                    throw new Exception("Power Limit Exceeded");
                }
            }
        }

        public override double Current
        {
            get
            {
                return SendQuery("ISET?").GetDouble();
            }

            set
            {
                if (value * volt <= power)
                {

                    SendCmd("ISET " + value.FormatIntoString());
                    current = Current;
                }
                else
                {
                    throw new Exception("Power Limit Exceeded");
                }
            }
        }

        protected override void DoClose()
        {
            Output = false;
        }

        public  override void Rst()
        {
            current = 0;
            volt = 0;
            base.Rst();
            this.DoInit();
        }

        protected override string DoInit()
        {            
            Write("*CLS; *ESE 60; ERAE 124; ERBE 16; *SRE 16"); //enable masks of error registers Event status register, Event error A, Event error B, and let generate service request for operation complete.
            System.Threading.Thread.Sleep(100);
            string idn=base.DoInit();
            if (idn.Contains("SSP62N052RU050P"))
            {
                maxPower = 1000;
            }
            else if (idn.Contains("SSP62N080RU125P"))
            {
                maxPower = 500;
            }
            else if (idn.Contains("SSP62N080RU025P"))
            {
                maxPower = 1000;
            }
            else if (idn.Contains("SSP64N052RU100P"))
            {
                maxPower = 2000;
            }
            else if (idn.Contains("SSP64N080RU075P"))
            {
                maxPower = 3000;
            }
            else 
            {
                throw new Exception("SSP type was not recognized");
            }
            power = maxPower;
            return idn;
        }

        public override double Power
        {
            get
            {
                return power;
            }

            set
            {

                if ((value <= maxPower) && (value >= (current * volt)))
                {
                    power = value;
                }

                else
                {
                    throw new Exception("Maximum power limit exceeded or current settings of output current and volt is higher than Demanded Power Limit");
                }
            }
        }

        public override void CheckErrors()
        {
            string response;
            string error = "";
            int status;
            Write("*STB?");
            response = Read().Trim();
            if (!int.TryParse(response, out status))
            {
                throw new Exception("Read status byte failed");
            }
            if ((status & 32) == 32) //Event Status Register is not empty
            {
                int ESR;
                Write("*ESR?");
                response = Read().Trim();
                if (!int.TryParse(response, out ESR))
                {
                    throw new Exception("Read event status register failed");
                }
                if((ESR&4)==4)
                {
                    error = error + "Query error;";
                }
                if ((ESR & 8) == 8)
                {
                    error = error + "Device dependecy error;";
                }
                if ((ESR & 16) == 16)
                {
                    error = error + "Execution error;";
                }
                if ((ESR & 32) == 32)
                {
                    error = error + "Command error;";
                }

            }
            if ((status & 8) == 8) //Event Status Register is not empty
            {
                int ERA;
                Write("*ERA?");
                response = Read().Trim();
                if (!int.TryParse(response, out ERA))
                {
                    throw new Exception("Read event register A failed");
                }
                if ((ERA & 4) == 4)
                {
                    error = error + "Overload/powerlimit;";
                }
                if ((ERA & 8) == 8)
                {
                    error = error + "Over current;";
                }
                if ((ERA & 16) == 16)
                {
                    error = error + "Over volatage;";
                }
                if ((ERA & 32) == 32)
                {
                    error = error + "Over temperature warning;";
                }
                if ((ERA & 64) == 64)
                {
                    error = error + "Over temperature shutdown;";
                }

            }
            if ((status & 4) == 4) //Event Status Register is not empty
            {
                int ERB;
                Write("*ERB?");
                response = Read().Trim();
                if (!int.TryParse(response, out ERB))
                {
                    throw new Exception("Read event register B failed");
                }                
                if ((ERB & 16) == 16)
                {
                    error = error + "Output On error;";
                }                
            }
            

            if(!string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }            
        }
        
    }
}
