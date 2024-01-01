using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using PowerSupplyExtensionMethods;

namespace PowerSupply
{
    internal class APMUDP : LambdaBase
    {
        private UdpClientWrapper udpClientWrapper;
        private double maxVoltage = 0;
        private double maxCurrent = 0;
        private const string defaultError = "0";
        private Dictionary<string, string> errrorCodes = new Dictionary<string, string>
        {
            {defaultError, "Normal"},
            {"1", "OVP"},
            {"2", "OCP"},
            {"3", "OPP"},
            {"4", "CVC (CV TO CC)"},
            {"5", "CCV (CC TO CV)"},
            {"6", "SLAVE OUT LINE"},
            {"7", "CURRCOUNT_NOT READY"},
            {"8", "CURRCOUNT_FAIL TEST"},
            {"9", "OVER VOLTAGE PROTECTION"},
            {"A", "SHORT CIRCUIT"},
            {"B", "FAN FAULT"},
            {"C", "OVER TEMPERATURE"},
            {"D", "NTC_FAIL"},
            {"E", "PRIMARY_FAIL"},
        };

        #region Properties

        public override double Volt
        {
            get
            {
                return this.SendQuery("VOLT?").GetDouble();
            }
            set
            {
                if (value > this.maxCurrent)
                {
                    throw new Exception(string.Format("Set voltage '{0}V' is greater than max voltage '{1}V'.", value, this.maxVoltage));
                }

                if ((value * this.current) <= power)
                {
                    this.SendCommnad("VOLT " + value.FormatIntoString());

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
                return this.SendQuery("CURR?").GetDouble();
            }
            set
            {
                if (value > this.maxCurrent)
                {
                    throw new Exception(string.Format("Set current '{0}A' is greater than max current '{1}V'.", value, this.maxCurrent));
                }

                if ((value * volt) <= power)
                {
                    this.SendCommnad("CURR " + value.FormatIntoString());

                    current = Current;
                }
                else
                {
                    throw new Exception("Power Limit Exceeded");
                }
            }
        }

        public override bool Output
        {
            get
            {
                string result = this.SendQuery("OUTP?");

                switch (result)
                {
                    case "1":
                    {
                        return true;
                    }
                    case "0":
                    {
                        return false;
                    }
                }

                throw new Exception("Wrong response obtained");
            }
            set
            {
                string result = this.SendQuery("OUTP?");
                if (value)
                {
                    if (result == "0")
                        this.SendCommnad("OUTP 1");
                }
                else
                {
                    if (result == "1")
                        this.SendCommnad("OUTP 0");
                }
            }

        }

        public override double MeasCurrent
        {
            get
            {
                return SendQuery("MEAS:CURR?").GetDouble();
            }
        }

        public override double MeasVolt
        {
            get
            {
                return SendQuery("MEAS:VOLT?").GetDouble();
            }
        }
        
        #endregion Properties

        public APMUDP(string deviceIpAdress, int devicePort, string localIpAddress, int localPort)
        {
            this.udpClientWrapper = new UdpClientWrapper(new IPEndPoint(IPAddress.Parse(localIpAddress), localPort), new IPEndPoint(IPAddress.Parse(deviceIpAdress), devicePort));
        }
        
        public override void Init()
        {
            try
            {
                this.connectionSucceed = true;

                identification = this.SendQuery("*IDN?").Replace("\0", "");
                
                this.SendCommnad("SYST:ETR");
                
                maxPower = this.GetPower();
                maxVoltage = this.SendQuery("VOLT?MAX").GetDouble();
                maxCurrent = this.SendQuery("CURR?MAX").GetDouble();
                power = maxPower;
                //Output = false; // No disable output, required from Reutlingen.
            }

            catch (Exception e)
            {
                this.connectionSucceed = false;

                throw new Exception("Communication failed: " + e.ToString());
            }
        }

        private double GetPower()
        {
            if (string.IsNullOrWhiteSpace(this.identification))
            {
                throw new Exception("Identification not set.");
            }

            var strings = this.identification.Split(',');

            if (strings.Length == 4 || (strings.Length == 5))
            {
                var result = Regex.Match(strings[1], @"(\d+)W", RegexOptions.IgnoreCase);

                if (!result.Success)
                {
                    throw new Exception("Identification has not information about power.");
                }

                double value = Double.Parse(result.Value.Substring(0, result.Value.Length - 1));

                return value;
            }

            throw new Exception("Identification has wrong length. Identification string is: " + identification);
        }

        private void SendCommnad(string command)
        {
            if (!this.connectionSucceed)
            {
                throw new Exception("Connection was not established");
            }

            try
            {
                this.udpClientWrapper.Send(this.FilterCommand(command));

                this.VerifyErrors();
            }
            catch (Exception exception)
            {
                throw new Exception("Command send failed");
            }
        }

        private string SendQuery(string query)
        {
            if (!this.connectionSucceed)
            {
                throw new Exception("Connection was not established");
            }

            try
            {
                string data = this.udpClientWrapper.SendReceive(this.FilterCommand(query));
                string result = data.Trim('\n').Trim('\r');

                this.VerifyErrors();

                return result;
            }
            catch (Exception exception)
            {
                throw new Exception("Command send failed");
            }
        }

        private void VerifyErrors()
        {
            if (!this.connectionSucceed)
            {
                throw new Exception("Connection was not established");
            }

            Thread.Sleep(100);

            ////TODO: NOT FUNCTION COMMAND
            //string errorData = this.udpClientWrapper.SendReceive(this.FilterCommand("ASWR?"));
            //string errorResult = errorData.Trim('\n').Trim('\r');

            //if (errorResult != "0")
            //{
            //    string errorMessage;

            //    if (this.errrorCodes.TryGetValue(errorResult, out errorMessage))
            //    {

            //    }

            //    throw new Exception(string.Format("APM Error: {0}-{1}", errorResult, errorMessage));
            //}

            //Thread.Sleep(100);
        }

        private string FilterCommand(string command)
        {
            if (!string.IsNullOrWhiteSpace(command))
            {
                if (command.Last() != '\n')
                {
                    command += '\n';
                }
            }

            return command;
        }

        public override void Rst()
        {
            throw new NotImplementedException();
        }

        public override void VoltageRamp(double startVolt, double finVolt, double current, double dwell)
        {
            throw new NotImplementedException();
        }

        public override void CurrentRamp(double startCurrent, double finCurrent, double volt, double dwell)
        {
            throw new NotImplementedException();
        }

        public override void VoltageCurrentRamp(double startVolt, double finVolt, double startCurrent, double finCurrent, double dwell)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            try
            {
                //if (this.udpClientWrapper != null) // No disable output, required from Reutlingen.
                //{
                //    Output = false;
                //}
            }
            catch
            {
                // ignored
            }
            finally
            {
                if (udpClientWrapper != null)
                {
                    udpClientWrapper.Dispose();
                    udpClientWrapper = null;
                }
            }
        }

        ~APMUDP()
        {
            this.Dispose();
        }
    }
}
