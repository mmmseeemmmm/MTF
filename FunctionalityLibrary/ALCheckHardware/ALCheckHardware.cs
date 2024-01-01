using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;
using ALCheck.Hardware;
using System.IO;

namespace ALCheckHardwareDriver
{
    [MTFClassCategory("Control & Measurement")]

    [MTFClass(Icon = MTFIcons.ALCheck)]
    public class ALCheckHardware : IDisposable
    {
        PLC aLCheckHW;

        [MTFConstructor(ParameterHelperClassName = "ALCheckHardwareDriver.ALCheckConfigHelper")]
        public ALCheckHardware(string comPort, string requestedPLCVersion)
        {
            string portFullName = "USB_" + comPort + "_57600";
            string conFile = Path.Combine(Directory.GetCurrentDirectory(), "mtfLibs", "onlsvr.con");
            string pcdFile = Path.Combine(Directory.GetCurrentDirectory(), "mtfLibs", "RESOURCE.PCD");

            aLCheckHW = new PLC(portFullName, conFile,pcdFile);
            aLCheckHW.RequestedPlcVersion = requestedPLCVersion;
            aLCheckHW.Connect();
            aLCheckHW.StartRemoteMode();
        }

        [MTFAllowedParameterValue("analogOutput", "Analog out 0", "MAIN.ANALOGOUTPUTS.ANALOGOUT0")]
        [MTFAllowedParameterValue("analogOutput", "Analog out 1", "MAIN.ANALOGOUTPUTS.ANALOGOUT1")]
        [MTFAdditionalParameterInfo(ParameterName = "value", Description = "Desired output value in [mV]")]
        [MTFMethod]
        public void SetAnalogOutput(string analogOutput, uint value)
        {
            aLCheckHW.SetValue(analogOutput, (uint)value);
        }

        [MTFAllowedParameterValue("errorLed", "Master LED", "MAIN.ERRORLEDS.MASTERISON")]
        [MTFAllowedParameterValue("errorLed", "Slave LED", "MAIN.ERRORLEDS.SLAVEISON")]
        [MTFMethod]
        public bool GetErrorLEDs(string errorLed)
        {
            bool result;

            aLCheckHW.GetValue(errorLed, out result);

            return result;
        }

        [MTFAllowedParameterValue("switchName", "Ignition", "MAIN.POWERSWITCHES.IGNITIONISON")]
        [MTFAllowedParameterValue("switchName", "Low beam", "MAIN.POWERSWITCHES.LOWBEAMISON")]
        [MTFAllowedParameterValue("switchName", "High beam", "MAIN.POWERSWITCHES.HIGHBEAMISON")]
        [MTFAllowedParameterValue("switchName", "High beam spot", "MAIN.POWERSWITCHES.HIGHBEAMSPOTISON")]
        [MTFAllowedParameterValue("switchName", "Fog light", "MAIN.POWERSWITCHES.FOGLAMPISON")]
        [MTFAllowedParameterValue("switchName", "Position lamp", "MAIN.POWERSWITCHES.POSITIONLAMPISON")]
        [MTFAllowedParameterValue("switchName", "Turnindicator", "MAIN.POWERSWITCHES.TURNINDISON")]
        [MTFAllowedParameterValue("switchName", "Daytime running", "MAIN.POWERSWITCHES.DAYTIMERUNISON")]
        [MTFAllowedParameterValue("switchName", "ECU supply", "MAIN.POWERSWITCHES.ECUSUPPLYISON")]
        [MTFAllowedParameterValue("switchName", "Infrared", "MAIN.POWERSWITCHES.INFRAREDISON")]
        [MTFAllowedParameterValue("switchName", "Sidemarker", "MAIN.POWERSWITCHES.SIDEMARKERISON")]
        [MTFAllowedParameterValue("switchName", "Switch 12", "MAIN.POWERSWITCHES.SWITCH12ISON")]
        [MTFAllowedParameterValue("switchName", "Switch 13", "MAIN.POWERSWITCHES.SWITCH13ISON")]
        [MTFAllowedParameterValue("switchName", "Switch 14", "MAIN.POWERSWITCHES.SWITCH14ISON")]
        [MTFAllowedParameterValue("switchName", "Switch 15", "MAIN.POWERSWITCHES.SWITCH15ISON")]
        [MTFAllowedParameterValue("switchName", "Switch 16", "MAIN.POWERSWITCHES.SWITCH16ISON")]
        [MTFMethod]
        public void SetPowerSwitch(string switchName, bool state)
        {
            aLCheckHW.SetValue(switchName, state);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("voltageName", "Ubat Master", "MAIN.VOLTAGES.UBATMASTER")]
        [MTFAllowedParameterValue("voltageName", "Ubat Slave", "MAIN.VOLTAGES.UBATSLAVE")]
        [MTFAllowedParameterValue("voltageName", "Analog in 0", "MAIN.VOLTAGES.ANALOGIN0")]
        [MTFAllowedParameterValue("voltageName", "Analog in 1", "MAIN.VOLTAGES.ANALOGIN1")]
        [MTFAllowedParameterValue("voltageName", "Analog in 2", "MAIN.VOLTAGES.ANALOGIN2")]
        [MTFAllowedParameterValue("voltageName", "Analog in 3", "MAIN.VOLTAGES.ANALOGIN3")]
        [MTFAllowedParameterValue("voltageName", "Analog in 4", "MAIN.VOLTAGES.ANALOGIN4")]
        [MTFAllowedParameterValue("voltageName", "Analog in 5", "MAIN.VOLTAGES.ANALOGIN5")]
        [MTFAllowedParameterValue("voltageName", "Analog in 6", "MAIN.VOLTAGES.ANALOGIN6")]
        [MTFAllowedParameterValue("voltageName", "Analog in 7", "MAIN.VOLTAGES.ANALOGIN7")]
        public uint GetVoltage(string voltageName)
        {
            uint result;
            aLCheckHW.GetValue(voltageName, out result);

            return result;
        }
        
        [MTFAllowedParameterValue("currentName", "GND LIT Master", "MAIN.CURRENTS.GNDLITMASTER")]
        [MTFAllowedParameterValue("currentName", "GND FIL1 Master", "MAIN.CURRENTS.GNDFIL1MASTER")]
        [MTFAllowedParameterValue("currentName", "GND FIL2 Master", "MAIN.CURRENTS.GNDFIL2MASTER")]
        [MTFAllowedParameterValue("currentName", "GND LIT Slave", "MAIN.CURRENTS.GNDLITSLAVE")]
        [MTFAllowedParameterValue("currentName", "GND FIL1 Slave", "MAIN.CURRENTS.GNDFIL1SLAVE")]
        [MTFAllowedParameterValue("currentName", "GND FIL2 Slave", "MAIN.CURRENTS.GNDFIL2SLAVE")]
        [MTFMethod(Description = "Measure current flow through the given line. Returned value is in [mA]")]
        public uint GetCurrent(string currentName)
        {
            uint result;
            aLCheckHW.GetValue(currentName,out result);

            return result;
        }

        [MTFAllowedParameterValue("pwmName", "Sensor 0", "MAIN.PWMOUTPUTS.SENS0PWM")]
        [MTFAllowedParameterValue("pwmName", "Sensor 1", "MAIN.PWMOUTPUTS.SENS1PWM")]
        [MTFAllowedParameterValue("pwmName", "V Signal", "MAIN.PWMOUTPUTS.VSIGNAL")]
        [MTFAllowedParameterValue("pwmName", "PWM out 0", "MAIN.PWMOUTPUTS.PWMOUT0")]
        [MTFAllowedParameterValue("pwmName", "PWM out 1", "MAIN.PWMOUTPUTS.PWMOUT1")]
        [MTFAllowedParameterValue("pwmName", "PWM out 2", "MAIN.PWMOUTPUTS.PWMOUT2")]
        [MTFAllowedParameterValue("pwmName", "PWM out 3", "MAIN.PWMOUTPUTS.PWMOUT3")]
        [MTFAllowedParameterValue("pwmName", "PWM out 4", "MAIN.PWMOUTPUTS.PWMOUT4")]
        [MTFAllowedParameterValue("pwmName", "PWM out 5", "MAIN.PWMOUTPUTS.PWMOUT5")]
        [MTFAllowedParameterValue("pwmName", "PWM out 6", "MAIN.PWMOUTPUTS.PWMOUT6")]
        [MTFAllowedParameterValue("pwmName", "PWM out 7", "MAIN.PWMOUTPUTS.PWMOUT7")]
        [MTFAllowedParameterValue("fixedSlope", "Rising", 0)]
        [MTFAllowedParameterValue("fixedSlope", "Falling", 1)]
        [MTFAdditionalParameterInfo(ParameterName = "frequency", Description = "Frequency of PWM [Hz]. Allowed range is 0,6 - 4000 Hz")]
        [MTFAdditionalParameterInfo(ParameterName = "dutyCycle", Description = "Duty cycle of PWM [%]")]
        [MTFAdditionalParameterInfo(ParameterName = "fixedSlope")]
        [MTFMethod]
        public void SetPWM(string pwmName, double frequency, uint dutyCycle, uint fixedSlope)
        {
            checkRange(dutyCycle, 0, 100);
            checkRange(frequency, 0.6, 4000);

            frequency = frequency * 1000;

            aLCheckHW.SetValue(pwmName + ".FREQUENCY", (uint)frequency);
            aLCheckHW.SetValue(pwmName + ".DUTYCYCLE", dutyCycle);
            aLCheckHW.SetValue(pwmName + ".FIXEDSLOPE", fixedSlope);
        }

        [MTFProperty]
        public string FirmwareVersion
        {
            get { return aLCheckHW.FirmwareVersion; }
        }

        [MTFProperty]
        public string RequestedPLCVersion
        {
            get { return aLCheckHW.RequestedPlcVersion; }
            set { aLCheckHW.RequestedPlcVersion = value; }
        }

        [MTFProperty]
        public string PLCVersion
        {
            get { return aLCheckHW.PLCVersion; }
        }

        [MTFProperty]
        public bool IspMode
        {
            set { aLCheckHW.SetValue("MAIN.ISPMODE.ISACTIVE", value); }
        }

        [MTFProperty]
        public bool CANTermination
        {
            set { aLCheckHW.SetValue("MAIN.CANTERMINATION.ISACTIVE",value); }
        }

        [MTFProperty]
        public byte LINConnection
        {
            set { aLCheckHW.SetValue("MAIN.LINCONNECTION.ISCONNECTED", value); }
        }

        [MTFProperty]
        public byte AnalogSensorsPossition
        {
            set { aLCheckHW.SetValue("MAIN.ANALOGSENSORS.POSITION", value); }
        }
        

        private void checkRange(double value, double min, double max)
        {
            if (value < min || value > max)
                throw new Exception("Value " + value + " is out of range. Range is <" + min + "," + max + ">");
        }

        public void Dispose()
        {
            if (aLCheckHW != null)
            {
                aLCheckHW.StopRemoteMode();
                aLCheckHW.Disconnect();
                aLCheckHW.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
