using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;

namespace ClimaChamberDriver
{
    [MTFClass(Name = "Clima chamber", Description = "Driver for clima chamber connected via serial port", Icon = MTFIcons.Clima)]
    [MTFClassCategory("Control & Measurement")]
    public class ClimaChamber : IDisposable, ICanStop
    {
        public IMTFSequenceRuntimeContext runtimeContext;

        private string type = string.Empty;
        private string comPort = string.Empty;
        private int baudRate=9600;
        private IClimaChamber climaControl;

        [MTFConstructor(Description = "Create SubZero Clima chamber driver on given serial port.")]
        //[MTFAllowedParameterValue("type", "Votsch", "Votsch")]
        //[MTFAllowedParameterValue("type", "SubZero", "SubZero")]
        //public ClimaChamber(string type, string comPort)
        public ClimaChamber(string comPort)
        {
            this.comPort = comPort;
            this.type = "SubZero";
            //this.type = type;
            //this.baudRate = baudRate;
            Init();
        }

        [MTFConstructor(Description = "Create Votch Clima chamber driver on given serial port.")]
        //[MTFAllowedParameterValue("type", "Votsch", "Votsch")]
        //[MTFAllowedParameterValue("type", "SubZero", "SubZero")]
        public ClimaChamber(string comPort, int baudRate)
        {
            this.comPort = comPort;
            this.type = "Votsch";
            this.baudRate = baudRate;
            Init();
            //Start = true;
        }

        //[MTFMethod]
        //[MTFProgressEvent("Progress", Index = 0)]
        //public event MTFPercentProgressEventHandler Progress1;
        //[MTFRaiseProgressEvent("StdConsoleProgress")]
        public void Init()
        {
            StopClima = false;
            try
            {
                //StdConsoleProgress("Creating connection with clima chamber on " + comPort + " ...");
                switch (this.type)
                {
                    case "Votsch":
                        climaControl = new ClimaVotchAscii2Control(comPort, baudRate);
                        climaControl.RuntimeContext = runtimeContext;
                        //climaControl.InitProgress = Progress1;
                        break;
                    case "SubZero":
                        climaControl = new ClimaSubZeroControl(comPort);
                        climaControl.RuntimeContext = runtimeContext;
                        break;
                    default:
                        break;  
                }

                //StdConsoleProgress("DONE.");
            }
            catch (Exception e)
            {
                if (climaControl != null)
                    climaControl.Close();
                throw new Exception("Error during init: "+e.ToString());
            }
        }

        public void Dispose()
        {
            if (climaControl != null)
            {
                climaControl.Close();
            }
            GC.SuppressFinalize(this);
        }

        ~ClimaChamber()
        {
            this.Dispose();
        }

             
        [MTFProperty(Name = "Temperature", Description = "This methods sets or reads out actual temperature from the Clima Chamber")]
        public double Temperature
        {
            get
            {
                return climaControl.Temperature;
            }
            set
            {
                climaControl.Temperature = value;
            }
        }

        [MTFProperty(Name = "Fan Speed", Description = "This methods sets reads out fan speed's set point in % from the Clima Chamber")]
        public double Fan
        {
            get
            {
                return climaControl.Fan;
            }
            set
            {
                climaControl.Fan = value;
            }
        }

        [MTFProperty(Name = "Run", Description = "This methods starts (Run=true) or stop (Run=false) clima chamber")]
        public bool Run
        {
            get
            {
                return climaControl.Run;
            }
            set
            {
                climaControl.Run = value;
            }
        }



        [MTFProperty(Name = "Humidity", Description = "This methods sets reads out actual humidity from the Clima Chamber. Votch Clima chamber need several minutes to read proper value. Please check the manual to clima chamber. Zero humidity means that humidity is not cotrolled")]
        public double Humidity
        {
            get
            {
                return climaControl.Humidity;
            }
            set
            {
                climaControl.Humidity = value;
            }
        }

        [MTFProperty(Name = "Clima Alarms", Description = "This methods reads out alarms. Votch 2nd generation of clima chamber shows only that  critical error ocured (without text description). Votch 3rd generation shows critical error text. 2nd generation and 3rd generation does show warnings")]
        public List<string> Alarms
        {
            get
            {
                return climaControl.Alarms;
            }
        }



        [MTFMethod(DisplayName = "Temperature with gradient", Description = "This methods set the gradient and final temperature. Gradient is set with 1 decimal point resolution.")]
        //[MTFRaiseProgressEvent("StdConsoleProgress")]
        public void TemperatureWithGradient(double temperature, double gradient)
        {
            climaControl.TemperatureWithGradient(temperature, gradient);
        }

        [MTFMethod(DisplayName = "Humidity with gradient", Description = "This methods set the gradient and final humidity. If humidity was zero before gradient was set. Humidity was not cotrolled. Set humidity and wait several minutes to let humidity sensor measure properly its value. To prevent warnings from clima chamber, humidity is set to minimal value if gradient is set from zero. Gradient is set with 1 decimal point resolution.")]
        //[MTFRaiseProgressEvent("StdConsoleProgress")]
        public void HumidityWithGradient(double humidity, double gradient)
        {
            climaControl.HumidityWithGradient(humidity, gradient);
        }

        [MTFMethod(DisplayName = "Acknowledge Alarms", Description = "This methods reset all alarms in the clima chamber")]
        public void AckAlarms()
        {
            climaControl.AckAlarms();
        }

        public static bool StopClima=false;
        
        bool ICanStop.Stop
        {
            set
            {
                StopClima = true; 
            }
        }

        #region SWramps
        //[MTFMethod]
        //public void TemperatureRamp(double startTemperature, double finTemperature, double dwell)
        //{
        //    climaControl.TemperatureRamp(startTemperature, finTemperature, dwell);
        //}

        //[MTFMethod]
        //public void HumidityRamp(double startHumidity, double finHumidity, double dwell)
        //{
        //    climaControl.HumidityRamp(startHumidity, finHumidity, dwell);
        //}

        //[MTFMethod]
        //public void TempAndHumidRamp(double startTemperature, double startHumidity, double finTemperature, double finHumidity, double dwell)
        //{
        //    climaControl.TempAndHumidRamp(startTemperature, startHumidity, finTemperature, finHumidity, dwell);
        //}
        #endregion
    }

}
