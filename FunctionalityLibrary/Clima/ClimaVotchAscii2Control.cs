using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ClimaVotchExtension;
using AutomotiveLighting.MTFCommon;
using System.Threading;

namespace ClimaChamberDriver
{
    internal class VotchState
    {
        internal double temperature;
        internal bool tempFound = false;
        internal double gradientTemp=0;
        internal double humid;
        internal bool humidFound = false;
        internal double gradientHumid=0;
        internal bool digitalHumidStart = false;
        internal bool digitalHumidFound = false;
        internal double fan;
        internal bool fanFound = false;
        internal bool start=true;
        internal bool startFound = false;
        internal bool condProtect;
        internal bool condensationProtectFound = false;
        internal bool clarific;
        internal bool clarifFound = false;
        internal bool compAir;
        internal bool compAirFound = false;
        internal bool dryer;
        internal bool dryerFound = false;
        internal bool opt60068_2_30;
        internal bool opt60068_2_30Found = false;
        internal bool opt60068_2_38;
        internal bool opt60068_2_38Found = false;
        internal double maxTemp;
        internal double minTemp;
        internal double maxHumid;
        internal double minHumid;
        internal double maxFan;
        internal double minFan;
        internal bool humidIsControlled = false;


    }

    internal class VotchSet
    {
        internal double temperature;
        internal double gradientTemp = 0;
        internal double humid;
        internal double gradientHumid = 0;
        internal bool digitalHumidStart = false;
        internal double fan;
        internal bool start = false;
        internal bool condProtect=false;
        internal bool clarific = false;
        internal bool compAir = false;
        internal bool dryer = false;
        internal bool opt60068_2_30 = false;
        internal bool opt60068_2_38 = false;
    }

    internal class ClimaVotchAscii2Control:IClimaChamber,ICanStop
    {
        #region definitions
        private IMTFSequenceRuntimeContext runtimeContext;
        private bool stop = false;
        private int timeOut = 10000;
        private string comPort;
        private int baudRate = -1;
        private bool connectionSucceed=false;
        private SerialPort serialPort;
        private VotchState votchState;
        private VotchSet votchSetPoints;
        private enum controlledValues {temp, humid, fan, start,condProtect, clarific,
        compAir, dryer, opt60068_2_30, opt60068_2_38, humidStart, undefined}
        private enum source {writeGradient,writeValues}        
        //private int cmdlength = -1; 
        //private int answerlength = -1;
        private int posDigWrite = -1;
        private int posDigRead = -1;
        controlledValues[] answerArray;
        controlledValues[] commandArray;
        string commandStringFormat = string.Empty;
        string answerStringFormat = string.Empty;
        private int percent = 0;
        #endregion

        #region ctor
        public ClimaVotchAscii2Control(string comPort, int baudRate)
        {
            ClimaChamber.StopClima = false;
            this.comPort = comPort;
            this.baudRate = baudRate;
            Init();

        }
        #endregion
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
        protected void Init()
        {
            if ((comPort != null) && (comPort != string.Empty) && (baudRate != -1))
            {
                InitSerialPort(this.comPort, this.baudRate);
                this.connectionSucceed = true;
                votchState=new VotchState();
                votchSetPoints = new VotchSet();
                GetConfiguration();
                ResetGradients();
                BuildCommandFormatString();

                if (votchState.digitalHumidFound)
                {
                    votchSetPoints.humid = votchState.minHumid;
                    votchSetPoints.digitalHumidStart = false;
                }
                else
                {
                    votchState.minHumid = 10; //25 for 3rd generation of clima chambers
                }
                votchState.humidIsControlled = false;
                votchSetPoints.start = false;
                votchSetPoints.fan = votchState.maxFan;
                votchSetPoints.temperature = 0;
                


            }
        }

        private void SaveConfigForDebug(string s)
        {
            string fileName = AppDomain.CurrentDomain.BaseDirectory.ToString() + @"climaConfig.txt";
            File.WriteAllText(fileName, s);

        }
    
        
        private void InitSerialPort(string comPort, int baudRate)
        {

            try
            {
                this.serialPort = new SerialPort();
                this.serialPort.PortName = comPort;
                this.serialPort.BaudRate = baudRate;
                this.serialPort.WriteTimeout = this.timeOut;
                this.serialPort.ReadTimeout = this.timeOut;
                //this.serialAdress = "ADR " + Convert.ToString(serialAdress);
                this.serialPort.NewLine = "\r";
                this.serialPort.Open();


            }
            catch (Exception e)
            {
                throw new Exception("Serial port was not initialized:" + e.ToString());

            }


        }


        private void ReadValues()
        {
            
            Write("$01I");
            string[] values = Read(5000).Split(' ');

            int pos = 0;
            for (int i = 0; i < posDigRead; i++)
            {
                
                if (answerArray[pos] != controlledValues.undefined)
                {
                    if (answerArray[pos] == controlledValues.temp)
                    {
                        votchState.temperature = values[pos].GetDouble();
                    }
                    else if (answerArray[pos] == controlledValues.humid)
                    {
                        votchState.humid = values[pos].GetDouble();
                    }
                    else if (answerArray[pos] == controlledValues.fan)
                    {
                        votchState.fan = values[pos].GetDouble();
                    }
                }
                pos++;
            }
            char[] bit = values.Last().ToCharArray();
            for (int i = 0; i < bit.Length; i++)
            {
          
                if (answerArray[pos] != controlledValues.undefined)
                {
                    if (answerArray[pos] == controlledValues.start)
                    {
                        votchState.start = bit[i] == '1' ? true : false;
                    }
                    if (answerArray[pos] == controlledValues.humidStart)
                    {
                        votchState.digitalHumidStart = bit[i] == '1' ? true : false;
                    }
                    if (answerArray[pos] == controlledValues.condProtect)
                    {
                        votchState.condProtect = bit[i] == '1' ? true : false;
                    }
                    if (answerArray[pos] == controlledValues.clarific)
                    {
                        votchState.clarific = bit[i] == '1' ? true : false;
                    }
                    if (answerArray[pos] == controlledValues.compAir)
                    {
                        votchState.compAir = bit[i] == '1' ? true : false;
                    }
                    if (answerArray[pos] == controlledValues.dryer)
                    {
                        votchState.dryer = bit[i] == '1' ? true : false;
                    }
                    if (answerArray[pos] == controlledValues.opt60068_2_30)
                    {
                        votchState.opt60068_2_30 = bit[i] == '1' ? true : false;
                    }
                    if (answerArray[pos] == controlledValues.opt60068_2_38)
                    {
                        votchState.opt60068_2_38 = bit[i] == '1' ? true : false;
                    }

                }
                pos++;
            }
            
        }

        private void BuildCommandFormatString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("$01E ");
            //int pos = 0;
            for (int i = 0; i < posDigWrite; i++)
            {
                if (commandArray[i] != controlledValues.undefined)
                {
                    sb.Append("{");
                    sb.AppendFormat("{0:d}", commandArray[i]);
                    sb.Append(":0000.0} ");
                    //pos++;
                }
                else
                {
                    sb.Append("0000.0 ");
                }
            }

            for (int i = posDigWrite; i < posDigWrite + 32; i++)
            {
                {
                    if (commandArray[i] != controlledValues.undefined)
                    {
                        sb.Append("{");
                        sb.AppendFormat("{0:d}", commandArray[i]);
                        sb.Append("}");
                        //pos++;
                    }
                    else
                    {
                        sb.Append("0");
                    }
                }
            }
            commandStringFormat = sb.ToString();
        }



        private void WriteValues()
        {
            StringBuilder cmd = new StringBuilder();
            cmd.AppendFormat(commandStringFormat, votchSetPoints.temperature, votchSetPoints.humid, votchSetPoints.fan, votchSetPoints.start ? "1" : "0",
                votchSetPoints.condProtect ? "1" : "0", votchSetPoints.clarific ? "1" : "0", votchSetPoints.compAir ? "1" : "0", votchSetPoints.dryer ? "1" : "0",
                votchSetPoints.opt60068_2_30 ? "1" : "0", votchSetPoints.opt60068_2_38 ? "1" : "0",votchSetPoints.digitalHumidStart ? "1":"0");
            cmd.Replace(",", ".");
            #if DEBUG
            Console.WriteLine(cmd.ToString());
            #endif
            Write(cmd.ToString());
            CheckWrite(source.writeValues);
        }

        private void WriteGradient()
        {
            StringBuilder cmd = new StringBuilder();
            double plusGradTemp;
            double minusGradTemp;
            double plusGradHumid;
            double minusGradHumid;
            if (votchSetPoints.gradientTemp == 0)
            {
                plusGradTemp = 0;
                minusGradTemp = 0;
            }
            else if (votchSetPoints.gradientTemp > 0)
            {
                plusGradTemp = votchSetPoints.gradientTemp;
                minusGradTemp = 0;
            }
            else
            {
                plusGradTemp = 0;
                minusGradTemp = -votchSetPoints.gradientTemp;
            }

            if (votchSetPoints.gradientHumid == 0)
            {
                plusGradHumid = 0;
                minusGradHumid = 0;
            }
            else if (votchSetPoints.gradientHumid > 0)
            {
                plusGradHumid = votchSetPoints.gradientHumid;
                minusGradHumid = 0;
            }
            else
            {
                plusGradHumid = 0;
                minusGradHumid = -votchSetPoints.gradientHumid;
            }


            cmd.AppendFormat("$01U {0:0000.0} {1:0000.0} {2:0000.0} {3:0000.0}", plusGradTemp, minusGradTemp, plusGradHumid, minusGradHumid);
            cmd.Replace(",", ".");
            #if DEBUG
            Console.WriteLine(cmd.ToString());
            #endif
            Write(cmd.ToString());
            CheckWrite(source.writeGradient);
        }

        private void ResetGradients()
        {
            votchSetPoints.gradientHumid = 0;
            votchSetPoints.gradientTemp = 0;
            #if DEBUG
            Console.WriteLine("$01U 0000.0 0000.0 0000.0 0000.0");
            #endif
            Write("$01U 0000.0 0000.0 0000.0 0000.0");
            CheckWrite(source.writeGradient);
        }

        private void ResetTempGradients()
        {

            votchSetPoints.gradientTemp = 0;
            WriteGradient();
        }

        private void ResetHumidGradients()
        {
            votchSetPoints.gradientHumid = 0;
            WriteGradient();

        }
        


        # region get configuration
        private void GetConfiguration()
        {
            char[] delimiters = new char[] { '\n' };

            string type_string = string.Empty;
            bool descriptionFoundE = false;
            bool descriptionFoundI = false;
            int startDescriptionI = -1;
            int startDescriptionE = -1;
            Write("$01?");
            bool run = true;
            bool foundEstring = false;
            bool foundIstring = false;
            bool fanSpeedFound = false;
            string s = string.Empty;
            while (run && !ClimaChamber.StopClima)
            {
                UpdateProgress();
                try
                {
                    s = s + Read(5000);
                    System.Threading.Thread.Sleep(100);

                }
                catch
                {
                    run = false;
                    serialPort.DiscardInBuffer();
                }
            }
#if !DEBUG
            SaveConfigForDebug(s);
#endif
            //s = ParsingTestHelper.GetNewClimaString();
            var split = s.Split('\n');
            
            for (int i = 0; i < split.Length; i++)
            {
                if (ClimaChamber.StopClima)
                {
                    break;
                }
                UpdateProgress();
                string line = split[i];
                if (line.ToLower().Contains("e-string"))
                {
                    type_string = "E";
                }
                else if (line.ToLower().Contains("i-string"))
                {
                    type_string = "I";
                }
                else
                {
                }
                if (type_string == "E")
                {
                    if (line.Contains("$01E")&&!foundEstring)
                    {
                        foundEstring = true;
                        commandArray = FillCommandArray(split, i, out posDigWrite);


                        //var commandLine = line.Replace("$01E ", "").Replace(" <CR>", "");
                        //string[] words = commandLine.Split(' ');
                        //words = words.Where(x => !string.IsNullOrEmpty(x)).ToArray(); //clear empty arrays
                        //var cmdlength = words.Length;
                        //var prefix = "DO00";
                        //posDigWrite = Array.IndexOf(words, prefix);
                        //if (posDigWrite == -1)
                        //{
                        //    var item = words.FirstOrDefault(x => x.StartsWith(prefix));
                        //    if (item != null)
                        //    {
                        //        posDigWrite = Array.IndexOf(words, item);
                        //    }
                        //}

                        //var array = new controlledValues[posDigWrite + 32];
                        //for (int j = 0; j < posDigWrite + 32; j++)
                        //{
                        //    array[j] = controlledValues.undefined;
                        //}
                        //commandArray = array;
                    }
                    if (line.ToLower().Contains("description:"))
                    {
                        descriptionFoundE = true;
                        startDescriptionE = i + 1;
                    }
                    if (descriptionFoundE)
                    {
                        if (line.ToLower().Contains("temperature"))
                        {
                            votchState.tempFound = true;
                            commandArray[i - startDescriptionE] = controlledValues.temp;
                            char decimalPoint = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToCharArray()[0];
                            line = line.Replace('.', decimalPoint);
                            string[] descriptions = line.Split(' ');
                            double number;
                            int indx = 0;
                            foreach (string word in descriptions)
                            {

                                if (Double.TryParse(word, out number))
                                {
                                    if (indx == 0)
                                    {
                                        votchState.minTemp = number;
                                    }
                                    else if (indx == 1)
                                    {
                                        votchState.maxTemp = number;
                                    }
                                    else
                                    {
                                        throw new Exception("Read temperature range from clima failed");
                                    }
                                    indx++;
                                }

                            }
                            if (indx != 2)
                            {
                                throw new Exception("Read fan range from clima failed");
                            }


                        }
                        else if (line.ToLower().Contains("humidity") && line.Contains("CV"))
                        {
                            votchState.humidFound = true;
                            commandArray[i - startDescriptionE] = controlledValues.humid;
                            char decimalPoint = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToCharArray()[0];
                            line = line.Replace('.', decimalPoint);
                            string[] descriptions = line.Split(' ');
                            double number;
                            int indx = 0;
                            foreach (string word in descriptions)
                            {

                                if (Double.TryParse(word, out number))
                                {
                                    if (indx == 0)
                                    {
                                        votchState.minHumid = number;
                                    }
                                    else if (indx == 1)
                                    {
                                        votchState.maxHumid = number;
                                    }
                                    else
                                    {
                                        throw new Exception("Read humidity range from clima failed");
                                    }
                                    indx++;
                                }
                            }
                            if (indx != 2)
                            {
                                throw new Exception("Read fan range from clima failed");
                            }
                        }
                        else if (line.ToLower().Contains("fan speed"))
                        {
                            votchState.fanFound = true;
                            commandArray[i - startDescriptionE] = controlledValues.fan;
                            char decimalPoint = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToCharArray()[0];
                            line = line.Replace('.', decimalPoint);
                            string[] descriptions = line.Split(' ');
                            double number;
                            int indx = 0;
                            foreach (string word in descriptions)
                            {

                                if (Double.TryParse(word, out number))
                                {
                                    if (indx == 0)
                                    {
                                        votchState.minFan = number;
                                    }
                                    else if (indx == 1)
                                    {
                                        votchState.maxFan = number;
                                    }
                                    else
                                    {
                                        throw new Exception("Read fan range from clima failed");
                                    }
                                    indx++;
                                }
                            }
                            if (indx != 2)
                            {
                                throw new Exception("Read fan range from clima failed");
                            }
                        }
                        else if (line.ToLower().Contains("start"))
                        {
                            votchState.startFound = true;
                            commandArray[i - startDescriptionE] = controlledValues.start;
                        }
                        else if (line.ToLower().Contains("humidity") && line.Contains("DO"))
                        {
                            votchState.digitalHumidFound = true;
                            commandArray[i - startDescriptionE] = controlledValues.humidStart;
                        }
                        else if (line.ToLower().Contains("cond.protect") || line.ToLower().Contains("condensation protection"))
                        {
                            votchState.condensationProtectFound = true;
                            commandArray[i - startDescriptionE] = controlledValues.condProtect;
                        }
                        else if (line.ToLower().Contains("clarificat"))
                        {
                            votchState.clarifFound = true;
                            commandArray[i - startDescriptionE] = controlledValues.clarific;
                        }
                        else if (line.ToLower().Contains("comp.air") || line.ToLower().Contains("compressed air"))
                        {
                            votchState.compAirFound = true;
                            commandArray[i - startDescriptionE] = controlledValues.compAir;
                        }
                        else if (line.ToLower().Contains("dryer"))
                        {
                            votchState.dryerFound = true;
                            commandArray[i - startDescriptionE] = controlledValues.dryer;
                        }
                        else if (line.Contains("60068-2-30"))
                        {
                            votchState.opt60068_2_30Found = true;
                            commandArray[i - startDescriptionE] = controlledValues.opt60068_2_30;
                        }
                        else if (line.Contains("60068-2-38"))
                        {
                            votchState.opt60068_2_38Found = true;
                            commandArray[i - startDescriptionE] = controlledValues.opt60068_2_38;
                        }
                    }
                }
                else if (type_string == "I")
                {


                    if (line.Contains("$01I")&&!foundIstring)
                    {
                        foundIstring = true;
                        answerArray = FillCommandArray(split, i, out posDigRead);
                        //var informationLine = split[i + 1].Replace("$01E ", "").Replace(" <CR>", "").Replace("DESCRIPTION:", "").TrimEnd();
                        //string[] words = informationLine.Split(' ');
                        //words = words.Where(x => !string.IsNullOrEmpty(x)).ToArray(); //clear empty arrays
                        //answerlength = words.Length;
                        //posDigRead = Array.IndexOf(words, "DO00");
                        //var array = new controlledValues[posDigRead + 32];
                        //for (int j = 0; j < posDigRead + 32; j++)
                        //{
                        //    array[j] = controlledValues.undefined;
                        //}
                        //answerArray = array;
                    }
                    if (line.ToLower().Contains("description:"))
                    {
                        descriptionFoundI = true;
                        startDescriptionI = i + 1;
                    }
                    

                    if (descriptionFoundI)
                    {
                        if (line.ToLower().Contains("temperature") && line.ToLower().Contains("actual"))
                        {
                            answerArray[i - startDescriptionI] = controlledValues.temp;
                        }
                        else if (line.ToLower().Contains("humidity") && line.ToLower().Contains("actual"))
                        {
                            answerArray[i - startDescriptionI] = controlledValues.humid;
                        }
                        else if (line.ToLower().Contains("fan speed") && !fanSpeedFound)
                        {
                            fanSpeedFound = true;
                            answerArray[i - startDescriptionI] = controlledValues.fan;
                        }
                        else if (line.ToLower().Contains("start"))
                        {
                            answerArray[i - startDescriptionI] = controlledValues.start;
                        }
                        else if (line.ToLower().Contains("humidity") && line.Contains("DO"))
                        {
                            answerArray[i - startDescriptionI] = controlledValues.humidStart;
                        }
                        else if (line.ToLower().Contains("cond.protect") || line.ToLower().Contains("condensation protection"))
                        {
                            answerArray[i - startDescriptionI] = controlledValues.condProtect;
                        }
                        else if (line.ToLower().Contains("clarificat"))
                        {
                            answerArray[i - startDescriptionI] = controlledValues.clarific;
                        }
                        else if (line.ToLower().Contains("comp.air"))
                        {
                            answerArray[i - startDescriptionI] = controlledValues.compAir;
                        }
                        else if (line.ToLower().Contains("dryer"))
                        {
                            answerArray[i - startDescriptionI] = controlledValues.dryer;
                        }
                        else if (line.Contains("60068-2-30"))
                        {
                            answerArray[i - startDescriptionI] = controlledValues.opt60068_2_30;
                        }
                        else if (line.Contains("60068-2-38"))
                        {
                            answerArray[i - startDescriptionI] = controlledValues.opt60068_2_38;
                        }
                    }

                }


            }
        }

        private controlledValues[] FillCommandArray(string[] lines, int index, out int foundIndex)
        {
            foundIndex = -1;
            var foundDesc = false;

            while (!foundDesc && index<lines.Length)
            {
                if (lines[index].StartsWith("Description:"))
                {
                    foundDesc = true;
                    var indexOfFirstCommand = index + 1;
                    if (indexOfFirstCommand<lines.Length)
                    {
                        var firstCommand = lines[indexOfFirstCommand].Split(' ').FirstOrDefault();
                        var commandLength = firstCommand?.Length;
                        if (commandLength!=0)
                        {
                            int i = indexOfFirstCommand;
                            bool stopParsingCommands = false;

                            while (!stopParsingCommands && i<lines.Length)
                            {
                                var cmd = lines[i].Split(' ').FirstOrDefault();
                                if (cmd!=null && cmd.Length == commandLength)
                                {
                                    if (cmd == "DO00")
                                    {
                                        foundIndex = i- indexOfFirstCommand;
                                        stopParsingCommands = true;
                                    }
                                    i++;
                                }
                                else
                                {
                                    stopParsingCommands = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    index++;
                }
            }

            return Enumerable.Range(0, foundIndex+32).Select(x=>controlledValues.undefined).ToArray();
        }

        #endregion

        public double Temperature
        {
            get
            {
                if (votchState.tempFound)
                {
                    ReadValues();
                }
                else
                {
                    throw new Exception("Function not supported or was not found in configuration string");
                }
                return votchState.temperature;
            }
            set
            {
                if (votchState.tempFound)
                {
                    votchSetPoints.temperature = value.CheckLimits(votchState.minTemp, votchState.maxTemp);
                    ResetTempGradients();
                    WriteValues();
                }
                else
                {
                    throw new Exception("Function not supported or was not found in configuration string");
                }
                
            }
        }

        public double Humidity
        {
            get
            {
                if (votchState.humidFound)
                {
                    
                    ReadValues();
                    if (votchState.digitalHumidFound)//2nd gen clima
                    {
                        if (!votchState.digitalHumidStart)
                        {
                            return 0;
                        }
                        else
                        {
                            return votchState.humid;
                        }
                    }
                    else //3rd gen clima
                    {
                        return votchState.humid;
                    }

                }
                else
                {
                    throw new Exception("Function not supported or was not found in configuration string");
                }
                
            }
            set
            {
                if (votchState.humidFound)
                {

                    if (value != 0)
                    {
                        votchState.humidIsControlled = true;
                        if (votchState.digitalHumidFound)//Controlled Humidity (Clima 2nd Generation with Humid Start)
                        {
                            votchSetPoints.humid = value.CheckLimits(votchState.minHumid, votchState.maxHumid);
                            votchSetPoints.digitalHumidStart = true;
                        }
                        else //Clima 3rd generation
                        {
                            votchSetPoints.humid = value.CheckLimits(votchState.minHumid, votchState.maxHumid);
                        }


                    }
                    else //value is 0
                    {
                        votchState.humidIsControlled = false;
                        if (votchState.digitalHumidFound) //Humid is not controlled (Clima 2nd Generation with Humid Start)
                        {
                            votchSetPoints.humid = votchState.minHumid;
                            votchSetPoints.digitalHumidStart = false;

                        }
                        else //3rd gen clima chamber
                        {
                            votchSetPoints.humid = 0;
                        }

                    
                    }
                    ResetHumidGradients();
                    WriteValues();
                }
                else
                {
                    throw new Exception("Function not supported or was not found in configuration string");
                }
            }
        }

        public double Fan
        {
            get
            {
                if (votchState.fanFound)
                {
                    ReadValues();
                }
                else
                {
                    throw new Exception("Function not supported or was not found in configuration string");
                }
                return votchState.fan;
            }
            set
            {
                if (votchState.fanFound)
                {
                    votchSetPoints.fan = value.CheckLimits(votchState.minFan, votchState.maxFan);
                    WriteValues();
                }
                else
                {
                    throw new Exception("Function not supported or was not found in configuration string");
                }

            }
        }

        public bool Run
        {
            get
            {
                if (votchState.startFound)
                {
                    ReadValues();
                    //CheckErrors();
                }
                else 
                {
                    throw new Exception("Function not supported or was not found in configuration string");
                }
                return votchState.start;
            }
            set
            {
                if (votchState.startFound)
                {
                    votchSetPoints.start = value;
                    WriteValues();
                }
                else
                {
                    throw new Exception("Function not supported or was not found in configuration string");
                }
            }
        }

        public void TemperatureWithGradient(double temperature, double gradient)
        {
            temperature.CheckLimits(votchState.minTemp, votchState.maxTemp);
            ResetTempGradients();
            if (Math.Sign(temperature - votchSetPoints.temperature) != Math.Sign(temperature - Temperature))
            {
                throw new Exception("Gradient was not set. Temperature Setpoint differs from actual value");
            }
            
            if ((temperature - votchSetPoints.temperature) >= 0)
            {
                
                votchSetPoints.gradientTemp = gradient;

            }
            else
            {
                votchSetPoints.gradientTemp = -gradient;
            }
            votchSetPoints.temperature = temperature;
            WriteGradient();
            WriteValues();
        }

        public void HumidityWithGradient(double humidity, double gradient)
        {
            humidity.CheckLimits(votchState.minHumid, votchState.maxHumid);
            ResetHumidGradients();

            if (!votchState.humidIsControlled)
            {
                votchState.humidIsControlled = true;
                votchSetPoints.humid = votchState.minHumid;
                WriteValues();
            
            }

            if (Math.Sign(humidity - votchSetPoints.humid) != Math.Sign(humidity - Humidity))
            {
                throw new Exception("Gradient was not set. Humidity Setpoint differs from actual value");
            }
            if ((humidity - votchSetPoints.humid) >= 0)
            {

                votchSetPoints.gradientHumid = gradient;

            }
            else
            {
                votchSetPoints.gradientHumid = -gradient;
            }
                      
            votchSetPoints.humid = humidity;
            WriteGradient();
            if (votchState.digitalHumidFound)
            {
                votchSetPoints.digitalHumidStart = true;
            }
            
            WriteValues();           
        }

        public void AckAlarms()
        {
            if (!votchState.digitalHumidFound)
            {
                //Mixec Mode Ascii2 & Ascii1 is not supported yet
                //string command = "1:Set:ErrorQuit:";
                //set(command);
                //this.checkResponse();
                AckAlarmsAscii2();
            }
            else
           {
               AckAlarmsAscii2();
           }
        }

        public void Close()
        {
            if (this.serialPort != null)
            {
                try
                {
                    //TODO: set chamber to defauld mode
                    //votch.temperature = 23;
                    //votch.humid = 0;
                    //votch.fan = 100;
                    //votch.start = false; //comment for debug
                    //votch.clarific = false;
                    //votch.compAir = false;
                    //votch.condProtect=false;
                    //votch.dryer = false;
                    //votch.opt60068_2_30 = false;
                    //votch.opt60068_2_38 = false;
                    votchSetPoints.start = false;
                    ResetGradients();
                    WriteValues();
                    connectionSucceed = false;
                }
                catch
                {
                    //TODO jde udelat neco jineho nez to zazdit?
                }
                finally
                {
                    this.serialPort.Close();
                    this.serialPort = null;
                   
                }
            }
        }

        #region write read
        private void Write(string command)
        {
            if (connectionSucceed)
            {
                #if DEBUG
                Console.WriteLine(command);
                #endif
                serialPort.WriteLine(command);
                System.Threading.Thread.Sleep(1000);
                //WaitWithProgressBar(1000);//Wait until command is processed by clima chamber
            }
            else
            {
                throw new Exception("Connection was not established");
            }
        }
        private string Read(int timeout)
        {
            if (connectionSucceed)
            {
                serialPort.ReadTimeout = timeout;
                string s = serialPort.ReadLine().TrimEnd();
                #if DEBUG
                Console.WriteLine(s);
                #endif
                return s;
            }
            else
            {
                throw new Exception("Connection was not established");
            }
        }
        #endregion
   
        private void CheckWrite(source source)
        {
            //if (votchState.digitalHumidFound||source==source.writeGradient)
//{
                try
                {
                    string error = Read(1000);
                    if (error != "0")
                    {
                        throw new Exception("Write " + source + "Values Failed: " + error);
                    }
                }
                catch (TimeoutException)
                {

                    //new chamber does not confirm write values with 0
                }

        }

        public void AckAlarmsAscii2()
        { 
            Write("$01Q");
            string s=Read(5000);
            //string errors = string.Empty;
            //    if (s!= "00")
            //    {
            //        errors+=", Number of errors after confirmation all error:"+s;
            //    }
                
            
            //if (!string.IsNullOrEmpty(errors))
            //{
                          
            
            //    throw new Exception("Nr of errors:"+s+", Error list: "+errors);
            //}
        }

        public List<string> getAlarmsAscii2()
        {
            List<string> result = null;
            string errors = string.Empty;
            string s = string.Empty;
            Write("$01F");
            try
            {
                s = Read(1000);

                if (s != "0")
                {
                    if (result == null)
                    {
                        result = new List<string>();
                    }
                    result.Add(s);

                }
            }
            catch (Exception e)
            {
                if (e.HResult == -2146233083)
                {
                    
                    //if no error old chamber send 0
                    //if error chambers does not respond (timeout occurs)
                    if (result == null)
                    {
                        result = new List<string>();
                    }
                    result.Add("Clima chamber 2nd generation: critical error ocured please check the clima chamber");
                }
                else
                {
                    throw new Exception(e.ToString());
                }
            }
            return result;
        }

    
        public bool Stop
        {
            set 
            { 
                stop=value; 
            }
        }

        public List<string> Alarms
        {
            get 
            {
                if (!votchState.digitalHumidFound)
                {
                    //return getAlarmsAscii1(); //sofar readout error memory in mixed mode is not supported
                    return getAlarmsAscii2();
                }
                else
                {
                    return getAlarmsAscii2();
                }
            }
        }


        private void UpdateProgress()
        {
            if (percent < 100)
            {
            }
            else
            {
                percent = 0;
            }
            runtimeContext.ProgressNotification(percent);
            percent++;
        }

        private void WaitWithProgressBar(int timeMiliseconds)
        {
            int steps = timeMiliseconds / 100;
            for (int i = 0; i < timeMiliseconds/100; i++) //60
            {
                Thread.Sleep(100);
                runtimeContext.ProgressNotification(i * 100 / steps);
            }
            if ((timeMiliseconds - steps * 100) > 30)
            {
                Thread.Sleep(timeMiliseconds - steps * 100);
            }
        }

        #region checkResponse
        //private void checkResponse()
        //{

        //    string response = ReadAscii1();
        //    byte[] asciiResp = Encoding.ASCII.GetBytes(response);
        //    if (!response.StartsWith("1") || asciiResp[1] != 6)
        //    {
        //        throw new Exception("Negative respons obtained");
        //    }
        //}
        #endregion
        #region ReadAscoo1()
        //private string ReadAscii1()
        //{
           
        //    string answer = serialPort.ReadTo("\u0003");
        //    char[] remChars={'\u0002','\u0003'};
        //    answer = answer.Trim(remChars);
        //    return answer;
        //}
        #endregion
        #region getAlarmsAscii1
        //private List<string> getAlarmsAscii1()
        //{
        //    List<string> result = null;
        //    string command = "1:Get:Errors:";
        //    set(command);
        //    string readed = ReadAscii1();
        //    string[] answer = readed.Split(':');
        //    int numberErrors; 
        //    int.TryParse(answer[3],out numberErrors);
        //    if (numberErrors > 0)
        //    {
        //        char[] errors = answer[4].ToCharArray();
        //        for (int i = 0; i < errors.Length; i++) 
        //        {
        //            if (errors[i] == '1')
        //            {
        //                command = string.Format("1:Get:Errors:Text:{0:00}", i + 1);
        //                set(command);
        //                readed = ReadAscii1();
        //                if (!string.IsNullOrEmpty(readed))
        //                {
        //                    answer = readed.Split(':');                            
        //                    if (result == null)                            
        //                    {                                
        //                        result = new List<string>();                            
        //                        //answer[0]:answer[1]:answer[2]:answer[3]:answer[4]:answer[5]}                            
        //                        //z:Get:ErrorText:ErrNr:ErrorDescription:CC                            
        //                        result.Add(answer[3] + ":" + answer[4]);                       
        //                    }                    
        //                }                
        //            }                     
        //        }
        //    }
        //    return result;
        //}
        #endregion
        #region set(string command)
        //private void set(string command)
        //{
        //    byte[] asciicharacters = Encoding.ASCII.GetBytes(command);

        //    byte[] data = new byte[asciicharacters.Length + 4];
        //    int sum = 256;
        //    data[0] = 2;    //{STX}
        //    sum = sum - data[0];
        //    for (int i = 1; i < data.Length - 3; i++)
        //    {
        //        data[i] = asciicharacters[i - 1];
        //        sum = sum - data[i];
        //        if (sum < 0)
        //        {
        //            sum = sum + 256;
        //        }

        //    }
        //    byte[] checkSum = Encoding.ASCII.GetBytes(sum.ToString("X"));

        //    if (checkSum.Length == 2)
        //    {
        //        data[data.Length - 3] = checkSum[0];
        //        data[data.Length - 2] = checkSum[1];
        //    }
        //    else
        //    {
        //        data[data.Length - 3] = 0;
        //        data[data.Length - 2] = checkSum[0];
        //    }
        //    data[data.Length - 1] = 3;    //{ETX}
        //    serialPort.Write(data, 0, data.Length);
        //}
        #endregion
        #region Ramps -Currently not supported
        //private Timer aTimer = new Timer();

        //aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        //private double temp;
        //private double humid;
        //private int iRamp = 0;
        //int steps;
        //double dTemp=0.1;
        //double dHumid = 0.1;
        //double startTemp;
        //double startHumid;
        //double dt;
        //int wait = 0;
        //bool rampIsRunning=false;
        //RampType rampType;
        //enum RampType {temp,humid, tempAndHumid}
        //double temp;


        //double minTime = 10;
        //System.Diagnostics.Stopwatch sw= new  System.Diagnostics.Stopwatch();



        //private void CalculateVoltageAndHumidRamp(double startTemperature, double startHumididy, double stopTemperature, double stopHumidity, double dwell)
        //{
        //    double dt1, dt2;

        //    double dValue = 0.1;
        //    startTemperature = Math.Round(startTemperature, 1);
        //    stopTemperature = Math.Round(stopTemperature, 1);
        //    double range = Math.Abs(stopTemperature - startTemperature);
        //    steps = (int)(range / dValue);
        //    dt1 = dwell / steps;
        //    startHumididy = Math.Round(startHumididy, 1);
        //    stopHumidity = Math.Round(stopHumidity, 1);
        //    range = Math.Abs(stopHumidity - startHumididy);
        //    steps = (int)(range / dValue);
        //    dt2 = dwell / steps;
        //    if (dt1 < dt2)
        //    {
        //        dt = dt1;
        //    }
        //    else 
        //    {
        //        dt = dt2;

        //    }

        //    if (dt < minTime)
        //    {
        //        dt = minTime;
        //        steps = (int)(dwell / dt);
        //        dTemp = (stopTemperature - startTemperature) * dt / dwell;
        //        dHumid = (stopHumidity - startHumididy) * dt / dwell;
        //    }
        //    else
        //    {

        //            dTemp = (stopTemperature - startTemperature) / steps;
        //            dHumid = (stopHumidity - startHumididy) / steps;

        //    }

        //    votchState.gradientTemp = Math.Ceiling(dTemp * 600 / dt) / 10;
        //    votchState.gradientHumid = Math.Ceiling(dHumid * 600 / dt) / 10;

        //    startTemp = startTemperature;
        //    startHumid = startHumididy;            
        //    wait = (int)((dwell - dt * steps) * 1000);
        //    if (wait < 100)
        //    {
        //        wait = 0;
        //    }
        //    #if DEBUG
        //    Console.WriteLine("Wait: " + wait.ToString());
        //    Console.WriteLine("dt: " + dt.ToString());
        //    Console.WriteLine("dTemp: " + dTemp.ToString());
        //    Console.WriteLine("dHumid: " + dHumid.ToString());
        //    Console.WriteLine("Steps: " + steps.ToString());
        //    Console.WriteLine("Gradient Temperature: " + votchState.gradientTemp.ToString());
        //    Console.WriteLine("Gradient Humid: " + votchState.gradientHumid.ToString());
        //    #endif

        //    rampType = RampType.tempAndHumid;
        //}

        //private void CalculateRamp(double startValue, double stopValue, double dwell,RampType type)
        //{
        //    //double minTime = 5;
        //    double dValue = 0.1;
        //    startValue = Math.Round(startValue, 1);
        //    stopValue = Math.Round(stopValue, 1);
        //    double range = Math.Abs(stopValue - startValue);
        //    steps = (int)(range / dValue);
        //    dt = dwell / steps;
        //    if (dt < minTime)
        //    {
        //        dt = minTime;
        //        steps = (int)(dwell / dt);
        //        dValue = (stopValue - startValue) * dt / dwell;

        //    }
        //    else
        //    {
        //        dValue = (stopValue - startValue) / steps;
        //    }
        //    if (type == RampType.temp)
        //    {
        //        startTemp = startValue;
        //        dTemp = dValue;
        //        rampType = type;
        //        votchState.gradientTemp = Math.Ceiling(dTemp * 600 / dt) / 10;
        //        votchState.gradientHumid = 0;
        //    }
        //    else if (type == RampType.humid)
        //    {
        //        startHumid = startValue;
        //        dHumid = dValue;
        //        rampType = type;
        //        votchState.gradientHumid = Math.Ceiling(dHumid * 600 / dt)/10;
        //        votchState.gradientTemp = 0;
        //    }
        //    else if(type==RampType.tempAndHumid)
        //    {
        //        //startTemp = startValue;
        //        //startHumid = startValue;
        //        rampType = type;
        //    }
        //    wait = (int)((dwell - dt * steps) * 1000);
        //    if (wait < 100)
        //    {
        //        wait = 0;
        //    }
        //    #if DEBUG
        //    Console.WriteLine("Wait: "+wait.ToString());
        //    Console.WriteLine("dt: " + dt.ToString());
        //    Console.WriteLine("dTemp: " + dTemp.ToString());
        //    Console.WriteLine("dHumid: " + dHumid.ToString());
        //    Console.WriteLine("Steps: " + steps.ToString());
        //    Console.WriteLine("Gradient Temperature: " + votchState.gradientTemp.ToString());
        //    Console.WriteLine("Gradient Humid: " + votchState.gradientHumid.ToString());
        //    #endif
        //}

        //public void TemperatureRamp(double startTemperature, double finTemperature, double dwell)
        //{

        //    CalculateRamp(startTemperature, finTemperature, dwell, RampType.temp);
        //    //WriteGradient();


        //    //aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        //    aTimer.OnTimer += OnTimedEvent;
        //    aTimer.Interval = dt;
        //    aTimer.Start();
        //    //aTimer.Enabled = true;
        //    //aTimer.AutoReset = true;


        //    rampIsRunning = true;
        //    while (rampIsRunning)
        //    {
        //        if(stop)
        //        {
        //            //aTimer.Enabled=false;
        //            aTimer.Stop();
        //            rampIsRunning = false;


        //        }
        //        System.Threading.Thread.Sleep(1000);
        //    }
        //    aTimer.OnTimer -= OnTimedEvent;
        //    //ResetGradients();
        //}



        //public void HumidityRamp(double startHumidity, double finHumidity, double dwell)
        //{

        //    CalculateRamp(startHumidity, finHumidity, dwell, RampType.humid);
        //    //WriteGradient();



        //    aTimer.OnTimer += OnTimedEvent;
        //    aTimer.Interval = dt;
        //    aTimer.Start();



        //    rampIsRunning = true;
        //    while (rampIsRunning)
        //    {
        //        if (stop)
        //        {

        //            aTimer.Stop();
        //            rampIsRunning = false;

        //        }
        //        System.Threading.Thread.Sleep(1000);
        //    }
        //    aTimer.OnTimer -= OnTimedEvent;
        //    //ResetGradients();
        //}

        //public void TempAndHumidRamp(double startTemp, double startHumid, double finTemp, double finHumid, double dwell)
        //{
        //    CalculateVoltageAndHumidRamp(startTemp, startHumid, finTemp, finHumid, dwell);
        //    //WriteGradient();

        //    aTimer.OnTimer += OnTimedEvent;
        //    aTimer.Interval = dt;
        //    aTimer.Start();

        //    rampIsRunning = true;
        //    while (rampIsRunning)
        //    {
        //        if (stop)
        //        {
        //            //aTimer.Enabled = false;
        //            aTimer.Stop();
        //            rampIsRunning = false;

        //        }
        //        System.Threading.Thread.Sleep(1000);
        //    }
        //    aTimer.OnTimer -= OnTimedEvent;
        //    //ResetGradients();
        //}

        //private void OnTimedEvent(object source)
        //{

        //    //aTimer.Enabled = false;
        //    if (iRamp < steps)
        //    {
        //        if (rampType == RampType.temp)
        //        {
        //            temp = startTemp + dTemp * (iRamp + 1);
        //            votchState.temperature = temp;
        //        }
        //        else if (rampType == RampType.humid)
        //        {
        //            humid = startHumid + dHumid * (iRamp + 1);
        //            votchState.humid = humid;
        //        }
        //        else if (rampType == RampType.tempAndHumid)
        //        {
        //            temp = startTemp + dTemp * (iRamp + 1);
        //            votchState.temperature = temp;
        //            humid = startHumid + dHumid * (iRamp + 1);
        //            votchState.humid = humid;
        //        }
        //        iRamp++;
        //        WriteValues();
        //        ReadValues();
        //        CheckErrors();

        //        //string time = (sw.ElapsedMilliseconds / 1000).ToString();

        //        //aTimer.Enabled = true;

        //    }
        //    else
        //    {
        //        iRamp = 0;
        //        //aTimer.Enabled = false;
        //        aTimer.Stop();
        //        rampIsRunning = false;
        //        if (wait != 0)
        //        {
        //            System.Threading.Thread.Sleep(wait);
        //        }
        //    }

        //}
        #endregion
    }
}
