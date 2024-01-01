using System;
using System.Collections.Generic;
using PLC.Helpers;


namespace PLC
{
    public class PLC_Protocol
    {
        private bool errorRecLenData = false;
        private bool errorWatchDog = false;
        private bool errorTimeOutOnData = true;

        //Header lenght 20, data lenght 256
        public const int headerLenght = 20;
        public const int dataLenght = 276;
        public const bool littleEndian = true;
        public byte[] RawDataToPLC = null;
        public byte[] RawDataFromPLC = null;
        internal UInt16 frameCounter = 0;
        internal UInt16 lastWatchDog = 0xFFFF;

        private int cycleTime = 200; //in ms

        private List<Signal> dataToPLC = null;
        private List<Signal> dataFromPLC = null;

        private IPLCCommunication plcCommunication = null;

        public bool sending = false;
        //private Thread mainControlThread;
        //private bool close = false;
        //private bool threadIsRunning = false;
        //private bool userIsSettingSignal = false;

        private string ipPC;
        private string portPC;
        private string ipPLC;
        private string portPLC;
        private PlcEnums.PLCType plcType;
   
        private const string timeoutErrorText = "errorTimeOutOnData - NO data from PLC more than ";
        private int countOfRestarts = 0;
        
        public string errorStatus
        {
            get
            {
                if ((DateTime.Now - LastRxTimeStamp).TotalMilliseconds > cycleTime * 10)
                    errorTimeOutOnData = true;
                else
                    errorTimeOutOnData = false;

                string text = string.Empty;
                if (errorRecLenData)
                    text = ("errorRecLenData - Received data lenght not correct, ");
                if (errorWatchDog)
                    text = text + ("errorWatchDog - Watch Dog from PLC Error, ");
                if (errorTimeOutOnData)
                    text = text + (timeoutErrorText + cycleTime * 10 + " ms");
                return text;
            }
        }

        public PLC_Protocol(string IpPC, string PortPC, string IpPLC, string PortPLC, PlcEnums.PLCType plcType)
        {
            this.ipPC = IpPC;
            this.portPC = PortPC;
            this.ipPLC = IpPLC;
            this.portPLC = PortPLC;
            this.plcType = plcType;
   
            Logging.AddTraceLine($"PLC Driver - ver.{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}", false);
            CreatePlcCommunication();
        }

        private void CreatePlcCommunication()
        {
            this.plcCommunication?.Close();

            RawDataToPLC = new byte[dataLenght];
            RawDataFromPLC = new byte[dataLenght];
            frameCounter = 0;
            dataToPLC = new List<Signal>();
            dataFromPLC = new List<Signal>();

            //Bosch Header            
            dataToPLC.Add(new Signal(0, "TelegramIdent", PlcEnums.DataType.UInt32));
            setSignal("TelegramIdent", 0x33532d00, false);

            //Type of mes. - use 0
            dataToPLC.Add(new Signal(32, "dwID", PlcEnums.DataType.UInt32));

            //List identifiear COB-ID - use = 1
            dataToPLC.Add(new Signal(64, "nIndex", PlcEnums.DataType.UINT16));
            setSignal("nIndex", 5, false);

            //ID of data Block - use 0
            dataToPLC.Add(new Signal(80, "nSubIndex", PlcEnums.DataType.UINT16));

            //nr. of varaibles in data block - use = 1
            dataToPLC.Add(new Signal(96, "nItems", PlcEnums.DataType.UINT16));
            //setSignal("nItems", 1);
            setSignal("nItems", 256, false);

            //Lenght - header + data
            dataToPLC.Add(new Signal(112, "nLen", PlcEnums.DataType.UINT16));
            //setSignal("nLen", dataLenght);
            setSignal("nLen", dataLenght, false);

            //Package counter - use static 0?
            dataToPLC.Add(new Signal(128, "nCounter", PlcEnums.DataType.UINT16));

            //use 0
            dataToPLC.Add(new Signal(144, "byFlags", PlcEnums.DataType.Byte));

            //CRC of header without nCounter
            dataToPLC.Add(new Signal(152, "byCheckSum", PlcEnums.DataType.Byte));

            //End Of Header
            dataToPLC.Add(new Signal(0 + headerLenght * 8, "TestPcDoneOK", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(1 + headerLenght * 8, "TestPcDoneNOK", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(2 + headerLenght * 8, "PcReady", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(3 + headerLenght * 8, "PcError", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(4 + headerLenght * 8, "C15", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(5 + headerLenght * 8, "LowBeam", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(6 + headerLenght * 8, "HiBeam", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(7 + headerLenght * 8, "TurnIndicator", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(8 + headerLenght * 8, "HiBeamSpot", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(9 + headerLenght * 8, "CornerLight", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(10 + headerLenght * 8, "PosLight", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(11 + headerLenght * 8, "DaytimeRunningLight", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(12 + headerLenght * 8, "AdditionalECU", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(13 + headerLenght * 8, "SideMarker", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(14 + headerLenght * 8, "Collective", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(15 + headerLenght * 8, "VariantCANSwitch", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(16 + headerLenght * 8, "WatchDog", PlcEnums.DataType.UINT16));
            dataToPLC.Add(new Signal(40 + headerLenght * 8, "PwmOut", PlcEnums.DataType.UINT16));
            dataToPLC.Add(new Signal(56 + headerLenght * 8, "AnalogOut", PlcEnums.DataType.UINT16));


            dataFromPLC.Add(new Signal(0 + headerLenght * 8, "PlcToPcStart", PlcEnums.DataType.BOOL));
            dataFromPLC.Add(new Signal(1 + headerLenght * 8, "PlcError", PlcEnums.DataType.BOOL));
            dataFromPLC.Add(new Signal(2 + headerLenght * 8, "LeftDUT", PlcEnums.DataType.BOOL));
            dataFromPLC.Add(new Signal(3 + headerLenght * 8, "RightDUT", PlcEnums.DataType.BOOL));
            dataFromPLC.Add(new Signal(2 + headerLenght * 8, "PlcTestOK", PlcEnums.DataType.BOOL));
            dataFromPLC.Add(new Signal(3 + headerLenght * 8, "PlcTestNOK", PlcEnums.DataType.BOOL));
            dataFromPLC.Add(new Signal(16 + headerLenght * 8, "WatchDog", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(40 + headerLenght * 8, "VoltageUBat", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(56 + headerLenght * 8, "VoltageAnalogIn", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(72 + headerLenght * 8, "CurrentC15", PlcEnums.DataType.UINT16, 0.03125));
            dataFromPLC.Add(new Signal(88 + headerLenght * 8, "CurrentLowBeam", PlcEnums.DataType.UINT16, 0.15625));
            dataFromPLC.Add(new Signal(104 + headerLenght * 8, "CurrentHiBeam", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(120 + headerLenght * 8, "CurrentTurnIndicator", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(136 + headerLenght * 8, "CurrentHiBeamSpot", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(152 + headerLenght * 8, "CurrentCornerLight", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(136 + headerLenght * 8, "CurrentPosLight", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(168 + headerLenght * 8, "CurrentDaytimeRunningLight", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(200 + headerLenght * 8, "CurrentAdditionalECU", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(216 + headerLenght * 8, "CurrentSideMarker", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(312 + headerLenght * 8, "FixtureCoding", PlcEnums.DataType.Byte));

            dataFromPLC.Add(new Signal(320 + headerLenght * 8, "BarCode", PlcEnums.DataType.String, 40));
            dataFromPLC.Add(new Signal(640 + headerLenght * 8, "BarCode2", PlcEnums.DataType.String, 60));
            dataFromPLC.Add(new Signal(1120 + headerLenght * 8, "BarCode3", PlcEnums.DataType.String, 60));

            dataFromPLC.Add(new Signal(1600 + headerLenght * 8, "UserChip", PlcEnums.DataType.String, 10));
            dataFromPLC.Add(new Signal(1680 + headerLenght * 8, "BarCode4", PlcEnums.DataType.String, 40));

            plcCommunication = CommunicationBase.GetInstance(this.plcType);
            plcCommunication.OnData += plcCommunication_OnData;

            plcCommunication.Start(this.ipPC, this.portPC, this.ipPLC, this.portPLC, false, string.Empty);
        }

        ~PLC_Protocol()
        {
            Close();
        }
        
        public DateTime LastRxTimeStamp
        {
            get
            {
                if (plcCommunication != null)
                {
                    return plcCommunication.LastRxTimeStamp();
                }

                return DateTime.MinValue;
            }
        }

        public void Close()
        {
            //if (threadIsRunning)
            //{
            //    close = true;
            //    int counter = 0;
            //    while (threadIsRunning && counter < 3)
            //    {
            //        Thread.Sleep(100);
            //        counter++;
            //    }
            //    if (counter >= 3)
            //        throw new Exception("Internal error with closing of mainControlThread!!!");
            //}

            if (plcCommunication != null)
            {
                plcCommunication.Close();
            }
        }



        internal void plcCommunication_OnData(object sender, byte[] data)
        {
            try
            {
                if (RawDataFromPLC.Length != data.Length)
                {
                    errorRecLenData = true;
                    return;
                }
                else
                    errorRecLenData = false;


                RawDataFromPLC = data;

                //get WatchDog increment it and send back to PLC

                UInt16 watchDog = (UInt16)((UInt16)getSignal("WatchDog", false) + 1);
                if (watchDog != lastWatchDog)
                {
                    errorWatchDog = false;
                    lastWatchDog = watchDog;
                }
                else
                {
                    lastWatchDog = watchDog;
                    //to strict set ErrorWatchDog when received same packet twice - TODO - add some timeout instead
                    //errorWatchDog = true;

                    //if (reinitIfNoDataFromPLC)
                    //{
                    //    Logging.AddTraceLine("Restart PLC communication");
                    //    CreatePlcCommunication();
                    //    Logging.AddTraceLine("Communication has been successfully restarted");
                    //}

                    return;
                }
                
                setSignal("WatchDog", watchDog, false);
     
                //Increment Counter of block data
                //setSignal("nSubIndex", frameCounter);
                setSignal("nCounter", frameCounter, false);
                frameCounter = (UInt16)(frameCounter + 1);

                plcCommunication.SendData(RawDataToPLC);
            }
            catch (Exception ex)
            {
                //Logging.AddTraceLine(ex.Message);
                throw ex;
            }
        }


        //private void CyclicCommunication()
        //{
        //    threadIsRunning = true;
        //    while (!close)
        //    {
        //        if (plcCommunication != null)
        //        {
        //            int counter = 0;
        //            while (userIsSettingSignal && counter < 5)
        //            {
        //                Thread.Sleep(5);
        //                counter++;
        //            }
        //            if (counter >= 5)
        //                throw new Exception("CyclicCommunication could not send data - userIsSettingSignal timeout");
        //            plcCommunication.SendData(RawDataToPLC);
        //        }
        //        Thread.Sleep(cycleTime);
        //    }
        //    threadIsRunning = false;
        //}


        public void setSignal(string name, object value)
        {
            setSignal(name, value, true);
        }

        public void setSignal(string name, object value, bool throwException)
        {
            if (throwException)
            {
                checkPLCStatus();
            }
            int index = GetSignalIndex(name, true);
            dataToPLC[index].Value = value;
            CommunicationBase.SignalToRAW(dataToPLC[index], ref this.RawDataToPLC, littleEndian);

            //Logging.AddTraceLineCounterSet($"setSignal: {name}");
        }

        public object getSignal(string name)
        {
            return getSignal(name, true);
        }

        public object getSignal(string name, bool throwException)
        {
            if (throwException)
            {
                checkPLCStatus();
            }
            int index = GetSignalIndex(name, false);
            Signal resultSignal = CommunicationBase.RAWtoSignal(dataFromPLC[index], this.RawDataFromPLC, littleEndian);
            if (resultSignal == null)
                return null;
            dataFromPLC[index].Value = resultSignal.Value;

            //Logging.AddTraceLineCounterGet($"getSignal: {name}");
            return dataFromPLC[index].Value;
        }

        private void checkPLCStatus()
        {
            //if (errorStatus.Contains(timeoutErrorText))
            //{
            //    Log.LogMessage("Restarting plc communication", logFilePrefix, true);
            //    CreatePlcCommunication();
            //    Log.LogMessage("Communication has been successfully restarted", logFilePrefix, true);
            //    countOfRestarts++;
            //}
            if(errorStatus != string.Empty)
            {
                //Logging.AddTraceLine(errorStatus);
                throw new Exception("PLC Error: " + errorStatus);
            }  
        }

        private int GetSignalIndex(string name, bool toPLC)
        {
            bool done = false;
            if (toPLC)
            {
                for (int i = 0; i < dataToPLC.Count; i++)
                {
                    if (dataToPLC[i].Name == name)
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
                var errorMessage = "GetSignalIndex error - setting of signal was not found - " + name;
                //Logging.AddTraceLine(errorMessage);
                throw new Exception(errorMessage);
            }

            return -1;
        }
    }
}
