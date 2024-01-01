using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AutomotiveLighting.MTFCommon;

namespace ClimaChamberDriver
{
    internal class ClimaVotschControl : IClimaChamber
    {

        private IMTFSequenceRuntimeContext runtimeContext = null;
        internal string readedFromCOM = string.Empty;
        internal bool comOpen = false;
        private System.IO.Ports.SerialPort serialPort1;
        internal string openedComPort = string.Empty;
        bool dataComming = false;
        private bool closing = false;

        private ValuesClima data = new ValuesClima();
        private string name = string.Empty;


        public ClimaVotschControl(string comPort)
        {
            try
            {
                if (InitializeSerialPort(comPort))
                {
                    this.serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);
                    this.openedComPort = comPort;
                    this.comOpen = this.serialPort1.IsOpen;
                }

                ValuesClima data = Actual;


                if (data != null)
                {
                    name = "Clima_" + openedComPort;
                    Data = data;
                    //StdConsoleProgress("DONE");
                }
                else
                {
                    Close();
                    throw new Exception("Clima could not open Clima chamber on " + comPort);
                }
            }
            catch (Exception)
            {
                Close();
                throw new Exception("Clima could not open Clima chamber on " + comPort);
            }
        }

        public void Close()
        {
            if (this.serialPort1 != null)
            {
                this.serialPort1.Close();
                this.openedComPort = string.Empty;
                this.comOpen = false;
                this.serialPort1 = null;
            }
        }

        ~ClimaVotschControl()
        {
            if (this.serialPort1 != null)
            {
                this.serialPort1.Close();
                this.serialPort1 = null;
            }
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            System.IO.Ports.SerialPort port = sender as System.IO.Ports.SerialPort;
            byte[] data = new byte[port.BytesToRead];
            port.Read(data, 0, data.Length);

            int startIndex = 0, endIndex = 0;
            if (data[0] == 2)
            {
                this.dataComming = true;
                startIndex = 1;
            }
            if (data[data.Length - 1] == 3)
            {
                if (data.Length >= 3)
                    endIndex = 3;
                else
                    endIndex = data.Length;
            }

            byte[] dataToAscii = new byte[data.Length];
            for (int i = startIndex; i < data.Length - endIndex; i++)
            {
                dataToAscii[i - startIndex] = data[i];
            }
            if (endIndex != 0 && endIndex < 3)
            {
                this.readedFromCOM = this.readedFromCOM.Substring(0, this.readedFromCOM.Length - (3 - endIndex));
            }
            else
            {
                this.readedFromCOM = this.readedFromCOM + Encoding.ASCII.GetString(dataToAscii);
            }
            this.readedFromCOM = this.readedFromCOM.Replace("\0", "");
            if (data[data.Length - 1] == 3)
            {
                this.dataComming = false;
            }
        }

        private ValuesClima Actual
        {
            get { return getActual(); }
        }

        public List<string> Alarms
        {
            get { return getAlarms(); }
        }

        private ValuesClima Data
        {
            set
            {
                string command = "1";
                command = command + "T" + String.Format("{0:000.0}", value.temperatureSet).Replace(",", ".");
                command = command + "F" + String.Format("{0:00}", value.humiditySet);
                command = command + "R";
                for (int i = 0; i < value.digitalOuts.Length; i++)
                {
                    string com = "0";
                    if (value.digitalOuts[i])
                        com = "1";
                    command = command + com;
                }

                set(command);
                if (!closing)
                {
                    this.checkResponse();
                    Thread.Sleep(1000);
                }
            }
        }

        private ValuesClima Gradients
        {
            set
            {
                string command = "01U";
                command = command + String.Format("{0:0000.0}", value.temperatureGradientUp).Replace(",", ".");
                command = command + String.Format("{0:0000.0}", value.temperatureGradientDown).Replace(",", ".");
                command = command + String.Format("{0:0000.0}", value.humidityGradientUp).Replace(",", ".");
                command = command + String.Format("{0:0000.0}", value.humidityGradientDown).Replace(",", ".");
                //command = "01?";
                //setASCII2(command);
            }
        }

        public void AckAlarms()
        {
            string command = "1:Set:ErrorQuit:";
            set(command);
            this.checkResponse();
        }

        private void checkResponse()
        {
            int counter = 0;
            while ((this.dataComming == true || this.readedFromCOM == string.Empty) && counter < 10)
            {
                counter++;
                Thread.Sleep(100);
            }
            string response = this.readedFromCOM;
            this.readedFromCOM = string.Empty;
            byte[] asciiResp = Encoding.ASCII.GetBytes(response);
            if (!response.StartsWith("1") || asciiResp[1] != 6)
            {
                throw new Exception("Negative respons from during Set Nominam value!!!");
            }
        }

        private void set(string command)
        {
            byte[] asciicharacters = Encoding.ASCII.GetBytes(command);

            byte[] data = new byte[asciicharacters.Length + 4];
            int sum = 256;
            data[0] = 2;    //{STX}
            sum = sum - data[0];
            for (int i = 1; i < data.Length - 3; i++)
            {
                data[i] = asciicharacters[i - 1];
                sum = sum - data[i];
                if (sum < 0)
                {
                    sum = sum + 256;
                }

            }
            byte[] checkSum = Encoding.ASCII.GetBytes(sum.ToString("X"));

            if (checkSum.Length == 2)
            {
                data[data.Length - 3] = checkSum[0];
                data[data.Length - 2] = checkSum[1];
            }
            else
            {
                data[data.Length - 3] = 0;
                data[data.Length - 2] = checkSum[0];
            }
            data[data.Length - 1] = 3;    //{ETX}
            this.serialPort1.Write(data, 0, data.Length);
        }

        private void setASCII2(string command)
        {
            byte[] asciicharacters = Encoding.ASCII.GetBytes(command);

            byte[] data = new byte[asciicharacters.Length + 2];

            data[0] = 36; //$

            for (int i = 1; i < data.Length - 1; i++)
            {
                data[i] = asciicharacters[i - 1];
            }

            data[data.Length - 1] = 13;    //{CR}
            this.serialPort1.Write(data, 0, data.Length);
        }

        private List<string> getAlarms()
        {
            List<string> result = null;
            string command = "1:Get:Errors:";
            set(command);

            int counter = 0;
            while ((this.dataComming == true || this.readedFromCOM == string.Empty) && counter < 10)
            {
                counter++;
                Thread.Sleep(100);
            }
            if (counter >= 10)
                return null;


            string readed = this.readedFromCOM;
            this.readedFromCOM = string.Empty;

            int index = readed.IndexOf(command) + command.Length;
            readed = readed.Substring(index, readed.Length - index - 1);

            index = readed.IndexOf(":");
            string toParse = readed.Substring(0, index);

            int numberErrors;
            int.TryParse(toParse, out numberErrors);
            if (numberErrors > 0)
            {
                readed = readed.Substring(index + 1, readed.Length - index - 1);
                char[] errorArray = readed.Replace(":", "").ToCharArray();
                for (int errorIndex = 0; errorIndex < errorArray.Length; errorIndex++)
                {
                    if (errorArray[errorIndex] == '1')
                    {
                        command = "1:Get:Errors:Text:" + String.Format("{0:00}", errorIndex + 1) + ":";
                        set(command);

                        counter = 0;
                        while ((this.dataComming == true || this.readedFromCOM == string.Empty) && counter < 10)
                        {
                            counter++;
                            Thread.Sleep(100);
                        }
                        if (counter >= 10)
                            return null;


                        readed = this.readedFromCOM;
                        this.readedFromCOM = string.Empty;

                        if (readed.IndexOf("1:Get:Errors:Text:") != -1)
                        {
                            index = readed.IndexOf("1:Get:Errors:Text:") + command.Length;
                            readed = readed.Substring(index, readed.Length - index - 1);
                            if (result == null)
                                result = new List<string>();
                            result.Add((errorIndex + 1).ToString() + ":" + readed);
                        }
                    }
                }


            }

            return result;
        }

        private ValuesClima getActual()
        {
            set("1?");

            int counter = 0;
            while ((this.dataComming == true || this.readedFromCOM == string.Empty) && counter < 10)
            {
                counter++;
                Thread.Sleep(100);
            }
            if (counter >= 10)
                return null;

            ValuesClima result = null;
            string readed = this.readedFromCOM;
            this.readedFromCOM = string.Empty;
            if (readed.StartsWith("1T"))
            {
                result = new ValuesClima();

                int index = readed.IndexOf("1T") + 2;
                readed = readed.Substring(index, readed.Length - 2);

                index = readed.IndexOf("F");
                string toParse = readed.Substring(0, index);
                readed = readed.Substring(index + 1, readed.Length - index - 1);
                double.TryParse(toParse.Replace(".", ","), out result.temperatureActual);

                index = readed.IndexOf("P");
                toParse = readed.Substring(0, index);
                readed = readed.Substring(index + 1, readed.Length - index - 1);
                double.TryParse(toParse.Replace(".", ","), out result.humidityActual);

                index = readed.IndexOf("T");
                toParse = readed.Substring(0, index);
                readed = readed.Substring(index + 1, readed.Length - index - 1);
                bool.TryParse(toParse.Replace(".", ","), out result.printer);

                index = readed.IndexOf("$");
                if (index == -1)
                {
                    result.unitSwitch = true;
                    index = readed.IndexOf("#");
                }
                else
                {
                    result.unitSwitch = false;
                }
                toParse = readed.Substring(0, index);
                readed = readed.Substring(index + 1, readed.Length - index - 1);
                bool.TryParse(toParse.Replace(".", ","), out result.printer);

                index = readed.IndexOf("T");
                toParse = readed.Substring(0, index);
                readed = readed.Substring(index + 1, readed.Length - index - 1);
                if (toParse == "--")
                {
                    result.errorsNr = 0;
                }
                else
                {
                    int.TryParse(toParse.Replace(".", ","), out result.errorsNr);
                }

                index = readed.IndexOf("F");
                toParse = readed.Substring(0, index);
                readed = readed.Substring(index + 1, readed.Length - index - 1);
                double.TryParse(toParse.Replace(".", ","), out result.temperatureSet);

                index = readed.IndexOf("R");
                toParse = readed.Substring(0, index);
                readed = readed.Substring(index + 1, readed.Length - index - 1);
                double.TryParse(toParse.Replace(".", ","), out result.humiditySet);

                bool bit = false;
                for (int i = 0; i < 16; i++)
                {
                    if (readed[i].ToString() == "1")
                        bit = true;
                    else
                        bit = false;
                    result.digitalOuts[i] = bit;
                }

                //  NumberOfReportedErrors

            }
            return result;
        }

        private bool InitializeSerialPort(string comPort)
        {
            if (comPort == null || comPort == string.Empty)
            {
                return false;
            }
            if (General.AvailableComPorts.Contains(comPort))
            {
                try
                {
                    this.serialPort1 = new System.IO.Ports.SerialPort();
                    this.serialPort1.PortName = comPort;
                    this.serialPort1.BaudRate = 19200;
                    this.serialPort1.Open();
                    serialPort1.WriteLine("");
                }
                catch (Exception)
                {
                    return false;


                }
            }
            else
            {
                // MessageBox.Show(comPort + " is not Available in system!!!\nPlease, check Scanner COM setting.",
                //              "EAP6 Storage system - Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return true;
        }

        private void SetClimaSetting()
        {
            Data = data;
        }

        private ValuesClima GetClimaSetting()
        {
            ValuesClima result = null;
            result = Actual;

            return result;
        }

        internal class ValuesClima
        {
            internal double temperatureSet = 0.0;
            internal double humiditySet = 0.0;
            internal double temperatureActual = 0.0;
            internal double humidityActual = 0.0;
            internal bool printer = false;
            internal bool[] digitalOuts = new bool[16];
            internal bool unitSwitch = false;
            internal int errorsNr = 0;
            internal double temperatureGradientUp = 0.0;
            internal double temperatureGradientDown = 0.0;
            internal double humidityGradientUp = 0.0;
            internal double humidityGradientDown = 0.0;
        }


        public double Temperature
        {
            get
            {
                data = GetClimaSetting();
                return data.temperatureActual;
            }
            set
            {
                data.temperatureSet = value;
                SetClimaSetting();
            }
        }

        public double Humidity
        {
            get
            {
                data = GetClimaSetting();
                return data.humidityActual;
            }
            set
            {
                data.humiditySet = value;
                SetClimaSetting();
            }
        }

        public bool Run
        {
            get
            {
                data = GetClimaSetting();
                return data.unitSwitch;
            }
            set
            {
                data.unitSwitch = value;
                data.digitalOuts[0] = value;
                SetClimaSetting();
            }
        }

        public void TemperatureWithGradient(double temperature, double gradient)
        {
            throw new NotImplementedException();
        }

        public void HumidityWithGradient(double humidity, double gradient)
        {
            throw new NotImplementedException();
        }


        //List<string> IClimaChamber.Alarms
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //void IClimaChamber.AckAlarms()
        //{
        //    throw new NotImplementedException();
        //}


        //public void TemperatureRamp(double startTemperature, double finTemperature, double dwell)
        //{
        //    throw new NotImplementedException();
        //}

        //public void HumidityRamp(double startHumidity, double finHumidity, double dwell)
        //{
        //    throw new NotImplementedException();
        //}


        //public void TempAndHumidRamp(double startTemp, double startHumid, double finTemp, double finHumid, double dwell)
        //{
        //    throw new NotImplementedException();
        //}


        public double Fan
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public IMTFSequenceRuntimeContext RuntimeContext
        {
            get
            {
                return runtimeContext;
            }
            set
            {
                runtimeContext = value;
            }
        }
    }
}
