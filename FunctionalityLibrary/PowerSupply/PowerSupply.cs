using AutomotiveLighting.MTFCommon;
using System;

namespace PowerSupply
{
    [MTFClass(Description = "Power Supply Driver", Icon = MTFIcons.PowerSupply)]
    [MTFClassCategory("Control & Measurement")]
    public class PowerSupply:IPowerSupply, IDisposable,ICanStop
    {
        private PowerSupplyParent powerSupply = null;
        private string ip;
        private int boardNumber;
        private int port;
        private string localIp;
        private int localPort;
        private int channel;

        private int baudrate;
        private int adress;
        private byte syskonadress;
        private string syskontype;
        private string comport;
        private string supplyType;
        
        public static object locker = new object();
        public IMTFSequenceRuntimeContext runtimeContext;

        [MTFConstructor(Description = "Syskon Power Supply Connected via GPIB")]
        //[MTFAllowedParameterValue("type", "Syskon", "syskon")]
        public PowerSupply(int boardNumber, byte adressGPIB)
        {
            this.boardNumber = boardNumber;
            this.syskonadress = adressGPIB;
            this.supplyType = "syskon";
            this.syskontype = "GPIB";
            this.Init();
        }


        [MTFConstructor(Description = "Lambda Power Supply Connected via LAN Port")]
        //[MTFAllowedParameterValue("type", "Lambda", "lambda")]
        public PowerSupply(int port, string ip)
        {
            this.ip = ip;
            this.port = port;
            this.supplyType = "lambda";
            this.Init();
        }

        [MTFConstructor(Description = "TTi Power Supply Connected via LAN Port")]
        [MTFAdditionalParameterInfo(ParameterName = "port", DefaultValue = "9221")]
        [MTFAdditionalParameterInfo(ParameterName = "channel", DefaultValue = "1")]
        public PowerSupply(int port, string ip, int channel)
        {
            this.ip = ip;
            this.port = port;
            this.supplyType = "TTi";
            this.channel = channel;
            this.Init();
        }


        [MTFConstructor(Description = "Lambda Power Supply Connected Serial Port")]
        //[MTFAllowedParameterValue("type", "Lambda", "lambda")]
        //[MTFAllowedParameterValue("type", "Syskon", "syskon")]
        public PowerSupply(string comport, int baudrate, int serial_adress)
        {
            this.comport = comport;
            this.supplyType = "lambda";
            this.baudrate = baudrate;
            this.adress = serial_adress;
            //if (type == "syskon") this.syskontype = "serial";
            this.Init();
        }


        [MTFConstructor(Description = "Syskon or SSP Power Supply Connected Serial Port")]        
        [MTFAllowedParameterValue("type", "Syskon", "syskon")]
        [MTFAllowedParameterValue("type", "SSP", "ssp")]
        public PowerSupply(string comport, int baudrate, string type)
        {
            this.comport = comport;
            this.supplyType = type;
            this.baudrate = baudrate;
            //this.adress = serial_adress;
            this.syskontype = "serial";
            this.Init();
        }

        [MTFConstructor(Description = "APM Power Supply Connected via LAN Port")]
        public PowerSupply(int devicePort, string deviceIP, int localPort, string localIP)
        {
            this.ip = deviceIP;
            this.port = devicePort;
            this.localIp = localIP;
            this.localPort = localPort;
            this.supplyType = "apm";
            this.Init();
        }       

        public void Init()
        {
            //if (StdConsoleProgress != null)
            //{
            //    StdConsoleProgress("Waiting for initialization(Resetting the device) ");
            //}
            //else
            //{

            //}
            lock (locker)
            {
                if (supplyType == "lambda")
                {
                    if (!string.IsNullOrEmpty(ip))
                    {

                        powerSupply = new LambdaTCP(ip, port);
                        powerSupply.RuntimeContext = runtimeContext;
                        powerSupply.Init();
                        powerSupply.identification = powerSupply.Idn;
                    }
                    else if (!string.IsNullOrEmpty(comport))
                    {
                        powerSupply = new LambdaSerial(comport, baudrate, adress);
                        powerSupply.RuntimeContext = runtimeContext;
                        powerSupply.Init();
                        powerSupply.identification = powerSupply.Idn;
                    }
                }
                else if (supplyType == "syskon")
                {
                    if (syskontype == "GPIB")
                    {
                        powerSupply = new SyskonGPIB(boardNumber, syskonadress);
                        powerSupply.RuntimeContext = runtimeContext;
                        powerSupply.Init();
                        powerSupply.identification = powerSupply.Idn;
                    }
                    else if (syskontype == "serial")
                    {
                        powerSupply = new SyskonSerial(comport, baudrate);
                        powerSupply.RuntimeContext = runtimeContext;
                        powerSupply.Init();
                        powerSupply.identification = powerSupply.Idn;
                    }
                }
                else if (supplyType == "apm")
                {
                    if (!string.IsNullOrEmpty(ip))
                    {
                        powerSupply = new APMUDP(ip, port, this.localIp, this.localPort);
                        powerSupply.RuntimeContext = runtimeContext;
                        powerSupply.Init();
                        powerSupply.identification = powerSupply.Idn;
                    }
                }
                else if(supplyType == "ssp")
                {
                    powerSupply = new SSPSerial(comport, baudrate);
                    powerSupply.RuntimeContext = runtimeContext;
                    powerSupply.Init();
                    powerSupply.identification = powerSupply.Idn;
                }
                else if (supplyType == "TTi")
                {
                    powerSupply = new TTiTCP(ip, port, channel);
                    powerSupply.RuntimeContext = runtimeContext;
                    powerSupply.Init();
                    powerSupply.identification = powerSupply.Idn;
                }
                else
                {
                    throw new Exception("unsuported power source type");
                }
            }
        }
       

        [MTFMethod(DisplayName="Reset",Description="This activity reset the device. Use this if you want to be sure that device is in the default state")]
        public void Rst()
        {
                powerSupply.Rst();
            
        }

        [MTFProperty(Name="Identification",Description="This activity show the identification of the power supply")]
        public string Idn
        {
            get
            {
                return powerSupply.identification;
            }
        }

        [MTFProperty(Name = "Voltage Limit", Description = "This activity set or read output voltage")]
        //[MTFAdditionalParameterInfo(ParameterName = "Voltage", DisplayName = "Voltage [V]")]
        public double Volt
        {
            get
            {
                lock (locker)
                {
                    return powerSupply.Volt;

                }
            }
            set
            {
                lock (locker)
                {
                    powerSupply.Volt = value;
                }
            }
        }

        [MTFProperty(Name="Current Limit",Description = "This activity set or read output current limit")]
        //[MTFAdditionalParameterInfoAttribute(ParameterName="Current",DisplayName="Current [A]")]
        public double Current
        {
            get
            {
                lock (locker)
                {
                    return powerSupply.Current;
                }
            }
            set
            {
                lock (locker)
                {
                    powerSupply.Current = value;
                }
            }
        }

        [MTFProperty(Name="Power Limit",Description = "This activity sets the power limit of the power supply. If syskon is used the power is limited by hardware. In other cases this parameter check condition that power<current*voltage whenever power,current, voltage is set.")]
        //[MTFAdditionalParameterInfoAttribute(ParameterName="Power",DisplayName="Power [W]")]
        public double Power
        {
            get
            {
                lock (locker)
                {
                    return powerSupply.Power;
                }
            }
            set
            {
                lock (locker)
                {
                    powerSupply.Power = value;
                }
            }
        }

        [MTFProperty(Name = "Output", Description = "This activity set output or read output status")]
        public bool Output
        {
            get
            {
                lock (locker)
                {
                    return powerSupply.Output;
                }
            }
            set
            {
                lock (locker)
                {
                    powerSupply.Output = value;
                }
            }
        }


        ~PowerSupply()
        {
            this.Dispose();
        }



        public void Dispose()
        {

            if (powerSupply != null)
            {
                    powerSupply.Dispose();
            }
            GC.SuppressFinalize(this);
        }


        //[MTFMethod(Description="Voltage Ramp",DisplayName="Voltage Ramp")]
        //[MTFAdditionalParameterInfo(ParameterName="startVolt",Description="Initial Voltage [V]")]
        //[MTFAdditionalParameterInfo(ParameterName = "finVolt", Description = "Final Voltage [V]")]
        //[MTFAdditionalParameterInfo(ParameterName = "current", Description = "Current [A]")]
        //[MTFAdditionalParameterInfo(ParameterName = "dwell", Description = "Dwell Time [s]")]
        public void VoltageRamp(double startVolt, double finVolt, double current, double dwell)
        {
            powerSupply.VoltageRamp(startVolt, finVolt, current, dwell);
        }
        //[MTFMethod(Description = "Current Ramp", DisplayName = "Current Ramp")]
        //[MTFAdditionalParameterInfo(ParameterName = "startCurrent", Description = "Initial Current [A]")]
        //[MTFAdditionalParameterInfo(ParameterName = "finCurrent", Description = "Final Current [A]")]
        //[MTFAdditionalParameterInfo(ParameterName = "volt", Description = "Voltage [V]")]
        //[MTFAdditionalParameterInfo(ParameterName = "dwell", Description = "Dwell Time [s]")]
        public void CurrentRamp(double startCurrent, double finCurrent, double volt, double dwell)
        {
            throw new NotImplementedException();
        }
        //[MTFMethod(Description = "Voltage and Current Ramp", DisplayName = "Voltage and Current Ramp")]
        //[MTFAdditionalParameterInfo(ParameterName = "startVolt", Description = "Initial Voltage [V]")]
        //[MTFAdditionalParameterInfo(ParameterName = "finVolt", Description = "Final Voltage [V]")]
        //[MTFAdditionalParameterInfo(ParameterName = "startCurrent", Description = "Initial Current [A]")]
        //[MTFAdditionalParameterInfo(ParameterName = "finCurrent", Description = "Final Current [A]")]
        //[MTFAdditionalParameterInfo(ParameterName = "dwell", Description = "Dwell Time [s]")]
        public void VoltageCurrentRamp(double startVolt, double finVolt, double startCurrent, double finCurrent, double dwell)
        {
            throw new NotImplementedException();
        }

        [MTFProperty(Name = "Measured Current", Description = "This activity measure output current")]
        public double MeasCurrent
        {
            get 
            {
                lock (locker)
                {
                    return powerSupply.MeasCurrent;
                }
            }
        }
        [MTFProperty(Name = "Measured Voltage", Description = "This activity measure output voltage")]
        public double MeasVolt
        {
            get
            {
                lock (locker)
                {
                    return powerSupply.MeasVolt;
                }
            }
        }

        bool ICanStop.Stop
        {
            set
            {
                powerSupply.stop = value;
            }
        }
    }
}
