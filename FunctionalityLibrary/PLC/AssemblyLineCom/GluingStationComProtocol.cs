
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PLC.Helpers;
using System.Threading;
using System.IO;

namespace PLC.AssemblyLineCom
{
    public class GluingStationComProtocol
    {
        private const string LogPrefix = "AssemblyLineCom";

        private string ipPC;
        private string portPC;
        private string ipPLC;
        private string portPLC;
        private UInt16 cobIdToPLC;
        private PlcEnums.PLCType plcType;

        private List<Signal> dataToPLC = null;
        private List<Signal> dataFromPLC = null;

        // Signals
        private List<Signal> signalsToPlc1 = null;
        private List<Signal> signalsToPlc2 = null;
        private List<Signal> signalsToPlc3 = null;

        // MessageSize
        private const int MessageListSize1 = 11; //11
        private const int MessageListSize2 = 11; //11
        private const int MessageListSize3 = 8; //9

        private const int MessageLength1 = headerLenght + MessageListSize1 * (headlampCodeLenght + 1);
        private const int MessageLength2 = headerLenght + MessageListSize2 * (headlampCodeLenght + 1);
        private const int MessageLength3 = headerLenght + MessageListSize3 * (headlampCodeLenght + 1) + 2;

        private byte[] rawDataToPlc1 = null;
        private byte[] rawDataToPlc2 = null;
        private byte[] rawDataToPlc3 = null;

        public byte[] RawDataToPLC = null;
        public byte[] RawDataFromPLC = null;

        private IPLCCommunication plcCommunication = null;

        private const int listSize = 30;
        private const int headlampCodeLenght = 22;

        public const int headerLenght = 20;
        public const int completeLenght = 20 + listSize * (headlampCodeLenght + 1) + 2;
        public const bool littleEndian = true;
        
        private List<string> checkedHeadlamps = new List<string>(listSize);
        int actualPossitionToSafe = 0;
        string initString = "0000000000000000000000";

        private System.Timers.Timer timerForCyclicSending;
        private int timerInterval = 500;
        private long taskCounter;
        private System.Diagnostics.Stopwatch auxTimer;
        static object locker = new object();
        string timerExceptionText = string.Empty;
        string pathToFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),"SavedList.txt");
        UInt16 counter = 0;

        public GluingStationComProtocol(string IpPC, string PortPC, string IpPLC, string PortPLC, PlcEnums.PLCType plcType, UInt16 cobIdToPLC)
        {
            this.ipPC = IpPC;
            this.portPC = PortPC;
            this.ipPLC = IpPLC;
            this.portPLC = PortPLC;
            this.plcType = plcType;
            this.cobIdToPLC = cobIdToPLC;

            for (int i = 0; i < listSize; i++)
			{
                checkedHeadlamps.Add(initString);
			}

            loadHlListFromFile();

            try
            {
                CreatePlcCommunication();
            }
            catch (Exception ex)
            {
                AL.Utils.Logging.Log.LogMessage(ex, LogPrefix, true);
                //Logging.AddTraceLine(ex.Message);
                Close();
                throw ex;
            }
        }

        public void Close()
        {
            if (timerForCyclicSending != null)
            {
                timerForCyclicSending.Stop();
                timerForCyclicSending.Dispose();
            }
            if (plcCommunication != null)
            {
                plcCommunication.Close();
            }
        }

        public void AddHeadlamp(string headlampCode)
        {
            if (!string.IsNullOrEmpty(timerExceptionText))
            {
                //Logging.AddTraceLine(timerExceptionText);
                throw new Exception(timerExceptionText);
            }

            headlampCode = make22CharString(headlampCode);

            if (actualPossitionToSafe == listSize)
            {
                actualPossitionToSafe = 0;
            }
            checkedHeadlamps[actualPossitionToSafe++] = headlampCode;
            convertListToStringAndSetSignal();
        }

        private void CreatePlcCommunication()
        {
            Close();

            RawDataToPLC = new byte[completeLenght];
            RawDataFromPLC = new byte[completeLenght];

            dataToPLC = new List<Signal>();
            dataFromPLC = new List<Signal>();

            rawDataToPlc1 = new byte[MessageLength1];
            rawDataToPlc2 = new byte[MessageLength2];
            rawDataToPlc3 = new byte[MessageLength3];

            this.signalsToPlc1 = this.CreateSignals(11,0, MessageLength1, (headlampCodeLenght + 1) * MessageListSize1, ref rawDataToPlc1, false);
            this.signalsToPlc2 = this.CreateSignals(11,1, MessageLength2, (headlampCodeLenght + 1) * MessageListSize2, ref rawDataToPlc2, false);
            this.signalsToPlc3 = this.CreateSignals(9,2, MessageLength3, (headlampCodeLenght + 1) * MessageListSize3, ref rawDataToPlc3, true);

            //Bosch Header            
            dataToPLC.Add(new Signal(0, "TelegramIdent", PlcEnums.DataType.UInt32));
            setSignal("TelegramIdent", 0x33532D00, false);

            //Type of mes. - use 0
            dataToPLC.Add(new Signal(32, "dwID", PlcEnums.DataType.UInt32));

            //List identifiear COB-ID - use = 1
            dataToPLC.Add(new Signal(64, "nIndex", PlcEnums.DataType.UINT16));
            setSignal("nIndex", cobIdToPLC, false);

            //ID of data Block - use 0
            dataToPLC.Add(new Signal(80, "nSubIndex", PlcEnums.DataType.UINT16));

            //nr. of varaibles in data block - use = 1
            dataToPLC.Add(new Signal(96, "nItems", PlcEnums.DataType.UINT16));
            //setSignal("nItems", 1);
            setSignal("nItems", 11, false);

            //Lenght - header + data
            dataToPLC.Add(new Signal(112, "nLen", PlcEnums.DataType.UINT16));
            //setSignal("nLen", dataLenght);
            setSignal("nLen", completeLenght, false);

            //Package counter - use static 0?
            dataToPLC.Add(new Signal(128, "nCounter", PlcEnums.DataType.UINT16));

            //use 0
            dataToPLC.Add(new Signal(144, "byFlags", PlcEnums.DataType.Byte));

            //CRC of header without nCounter
            dataToPLC.Add(new Signal(152, "byCheckSum", PlcEnums.DataType.Byte));
            
            // to gluing station

            dataToPLC.Add(new Signal(headerLenght * 8, "SeznamVyrobku", PlcEnums.DataType.ByteArray, (headlampCodeLenght+1)* listSize));

            dataToPLC.Add(new Signal(headerLenght * 8 + (headlampCodeLenght + 1) * listSize * 8, "wCitacLWR", PlcEnums.DataType.UINT16));

            plcCommunication = CommunicationBase.GetInstance(this.plcType);// new Bosch_UDP();

            plcCommunication.Start(this.ipPC, this.portPC, this.ipPLC, this.portPLC, true, string.Empty);

            convertListToStringAndSetSignal();

            auxTimer = new System.Diagnostics.Stopwatch();
            auxTimer.Start();
            taskCounter = 0;
            timerForCyclicSending = new System.Timers.Timer(timerInterval);
            timerForCyclicSending.Enabled = true;
            timerForCyclicSending.Elapsed += timerForCyclicSending_Elapsed;
        }

        void timerForCyclicSending_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                System.Timers.Timer timer = sender as System.Timers.Timer;
                taskCounter++;
                double fixedInterval = timerInterval + timerInterval * taskCounter - auxTimer.ElapsedMilliseconds;

                if (fixedInterval < 1)
                {
                    timer.Interval = 1;
                }
                else
                {
                    timer.Interval = fixedInterval;
                }

                int currentCounter = counter;
                setSignal("wCitacLWR", counter++);

                if (counter > 65000)
                {
                    counter = 0;
                    currentCounter = 0;
                }

                lock (locker)
                {
                    //RawDataToPLC[42] = 0;
                    //RawDataToPLC[65] = 0;
                    //RawDataToPLC[88] = 0;
                    //RawDataToPLC[111] = 0;
                    //RawDataToPLC[134] = 0;
                    //RawDataToPLC[157] = 0;
                    //RawDataToPLC[180] = 0;
                    //RawDataToPLC[203] = 0;
                    //RawDataToPLC[226] = 0;
                    //RawDataToPLC[249] = 0;

                    const int startIndex = 42;
                    const int typeSize = headlampCodeLenght + 1;

                    for (int i = 0; i < listSize; i++)
                    {
                        int index = (i * typeSize) + startIndex;

                        RawDataToPLC[index] = 0;
                    }

                    //plcCommunication.SendData(RawDataToPLC);
                    this.SendLargeData(currentCounter);
                }
            }
            catch(Exception ex)
            {
                AL.Utils.Logging.Log.LogMessage(ex, LogPrefix, true);

                timerExceptionText = ex.Message;
            }
        }

        private void SendLargeData(int citac)
        {
            int index = GetSignalIndex("SeznamVyrobku", true, this.dataToPLC);
            var value = (byte[])dataToPLC[index].Value;

            //Message 1
            int message1ValuesLength = (headlampCodeLenght + 1) * (MessageListSize1);
            byte[] message1Values = new byte[message1ValuesLength];

            for (int i = 0; i < message1Values.Length; i++)
            {
                message1Values[i] = value[i];
            }
            
            setSignal2("SeznamVyrobku", message1Values, this.signalsToPlc1, ref this.rawDataToPlc1);
           // setSignal2("wCitacLWR", citac, this.signalsToPlc1, ref this.rawDataToPlc1);
   
            plcCommunication.SendData(rawDataToPlc1);
            
            //Message 2
            int message2ValuesLength = (headlampCodeLenght + 1) * (MessageListSize2);
            byte[] message2Values = new byte[message2ValuesLength];

            for (int i = 0; i < message2Values.Length; i++)
            {
                message2Values[i] = value[i + message1Values.Length];
            }

            setSignal2("SeznamVyrobku", message2Values, this.signalsToPlc2, ref this.rawDataToPlc2);
            //setSignal2("wCitacLWR", citac, this.signalsToPlc2, ref this.rawDataToPlc2);

            plcCommunication.SendData(rawDataToPlc2);

            //Message 3
            int message3ValuesLength = (headlampCodeLenght + 1) * (MessageListSize3);
            byte[] message3Values = new byte[message3ValuesLength];

            for (int i = 0; i < message3Values.Length; i++)
            {
                message3Values[i] = value[i + message1Values.Length + message2Values.Length];
            }

            setSignal2("SeznamVyrobku", message3Values, this.signalsToPlc3, ref this.rawDataToPlc3);
            setSignal2("wCitacLWR", citac, this.signalsToPlc3, ref this.rawDataToPlc3);

            plcCommunication.SendData(this.rawDataToPlc3);
        }

        private List<Signal> CreateSignals(int itemsCount, int subIndex, int messageLength, int dataLength, ref byte[] rawData, bool isIncludedCounter)
        {
            var signals = new List<Signal>();

            //Bosch Header            
            signals.Add(new Signal(0, "TelegramIdent", PlcEnums.DataType.UInt32));
            setSignal2("TelegramIdent", 0x33532D00, signals, ref rawData);

            //Type of mes. - use 0
            signals.Add(new Signal(32, "dwID", PlcEnums.DataType.UInt32));

            //List identifiear COB-ID - use = 1
            signals.Add(new Signal(64, "nIndex", PlcEnums.DataType.UINT16));
            setSignal2("nIndex", cobIdToPLC, signals, ref rawData);

            //ID of data Block - use 0
            signals.Add(new Signal(80, "nSubIndex", PlcEnums.DataType.UINT16));
            setSignal2("nSubIndex", subIndex, signals, ref rawData);

            //nr. of varaibles in data block - use = 1
            signals.Add(new Signal(96, "nItems", PlcEnums.DataType.UINT16));
            setSignal2("nItems", itemsCount, signals, ref rawData);

            //Lenght - header + data
            signals.Add(new Signal(112, "nLen", PlcEnums.DataType.UINT16));
            setSignal2("nLen", messageLength, signals, ref rawData);

            //Package counter - use static 0?
            signals.Add(new Signal(128, "nCounter", PlcEnums.DataType.UINT16));

            //use 0
            signals.Add(new Signal(144, "byFlags", PlcEnums.DataType.Byte));

            //CRC of header without nCounter
            signals.Add(new Signal(152, "byCheckSum", PlcEnums.DataType.Byte));

            // to gluing station

            signals.Add(new Signal(headerLenght * 8, "SeznamVyrobku", PlcEnums.DataType.ByteArray, dataLength));

            if (isIncludedCounter)
            {
                signals.Add(new Signal(headerLenght * 8 + dataLength * 8, "wCitacLWR", PlcEnums.DataType.UINT16));
            }

            return signals;
        }

        private void convertListToStringAndSetSignal()
        {
            byte[] byteArray = new byte[(headlampCodeLenght+1)* listSize];
            
            List<byte> byteList = new List<byte>();
            foreach (string hl in checkedHeadlamps)
            {
                if (hl.Length != headlampCodeLenght)
                {
                    var errorMessage = "HL code does not have " + headlampCodeLenght + " characters.";
                    //Logging.AddTraceLine(errorMessage);
                    throw new Exception(errorMessage);
                }
                else
                {
                    byteList.AddRange(ASCIIEncoding.ASCII.GetBytes(hl));
                    byteList.Add(0);
                }
            }

            byteArray = byteList.ToArray();

            setSignal("SeznamVyrobku", byteArray);

            safeHlListToFile();
        }

        private void safeHlListToFile()
        {
            using (TextWriter tw = new StreamWriter(pathToFile))
            {
                foreach (String s in checkedHeadlamps)
                {
                    tw.WriteLine(s);
                }
                tw.WriteLine("#" + actualPossitionToSafe);   
            }
        }

        private void loadHlListFromFile()
        {
            try
            {
                using (TextReader tr = new StreamReader(pathToFile))
                {
                    string line;
                    int j = 0;

                    while ((line = tr.ReadLine()) != null)
                    {
                        if (line.StartsWith("#"))
                        {
                            actualPossitionToSafe = int.Parse(line.Substring(1));
                            continue;
                        }
                        if (line.Length != headlampCodeLenght)
                        {
                            var errorMessage = "File " + pathToFile + " contains not valid HL codes.";
                            //Logging.AddTraceLine(errorMessage);
                            throw new Exception(errorMessage);
                        }
                        if (j >= listSize+1)
                        {
                            var errorMessage = "File " + pathToFile + " contains more than " + listSize + " HL codes.";
                            //Logging.AddTraceLine(errorMessage);
                            throw new Exception(errorMessage);
                        }
                        checkedHeadlamps[j++] = line;
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                //if file not exist continue without loading
                AL.Utils.Logging.Log.LogMessage(ex, LogPrefix, true);
            }
            
        }

        private void setSignal(string name, object value)
        {
            setSignal(name, value, true);
        }

        private void setSignal(string name, object value, bool throwException)
        {
            lock (locker)
            {
                int index = GetSignalIndex(name, true, this.dataToPLC);
                dataToPLC[index].Value = value;
                CommunicationBase.SignalToRAW(dataToPLC[index], ref this.RawDataToPLC, littleEndian);
            }
        }


        private void setSignal2(string name, object value, List<Signal> signals, ref byte[] rawData)
        {
            lock (locker)
            {
                int index = GetSignalIndex(name, true, signals);
                signals[index].Value = value;
                CommunicationBase.SignalToRAW(signals[index], ref rawData, littleEndian);
            }
        }

        private int GetSignalIndex(string name, bool toPLC, List<Signal> signals)
        {
            bool done = false;
            if (toPLC)
            {
                for (int i = 0; i < signals.Count; i++)
                {
                    if (signals[i].Name == name)
                    {
                        done = true;
                        return i;
                    }
                }
            }
            else
            {
                for (int i = 0; i < dataFromPLC.Count; i++)
                {
                    if (dataFromPLC[i].Name == name)
                    {
                        done = true;
                        return i;
                    }
                }
            }

            if (!done)
            {
                string message = "GetSignalIndex error - setting of signal was not found - " + name;

                AL.Utils.Logging.Log.LogMessage(message, LogPrefix, true);
                //Logging.AddTraceLine(message);
                throw new Exception(message);
            }

            return -1;
        }

        private string make22CharString(string input)
        {

            if (input.Length < headlampCodeLenght)
            {
                while (input.Length < headlampCodeLenght)
                {
                    input = input + "0";
                }
            }
            if (input.Length > headlampCodeLenght)
            {
                return input.Substring(0, headlampCodeLenght);
            }

            return input;
        }
    }
}
