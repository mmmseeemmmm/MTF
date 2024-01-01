using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PLC.Helpers;
using System.Threading;

namespace PLC.AssemblyLineCom
{
    public class AssemblyLineComProtocol
    {
        private bool errorRecLenData = false;
        private bool errorWatchDog = false;
        private bool errorTimeOutOnData = true;
        private bool firstPackedReceived = false;

        private string ipPC;
        private string portPC;
        private string ipPLC;
        private string portPLC;
        private string portSendingPLC;
        private PlcEnums.PLCType plcType;
        private UInt16 cobIdToPLC = 0;
        private UInt16 cobIdFromPLC = 0;

        private List<Signal> dataToPLC = null;
        private List<Signal> dataFromPLC = null;

        public byte[] RawDataToPLC = null;
        public byte[] RawDataFromPLC = null;

        private IPLCCommunication plcCommunication = null;

        internal UInt16 frameCounter = 0;
        public const int headerLenght = 20;
        public const int completeLenght = 52;
        public const bool littleEndian = true;
        private const UInt16 cobIdToPLCdefault = 10026;
        private const UInt16 cobIdFromPLCdefault = 20026;
        private UInt16 previousOdLinkyCitac = 0;
        private DateTime lastUpdateOdLinkyCitac = DateTime.Now;

        public AssemblyLineComProtocol(string IpPC, string PortPC, string IpPLC, string PortPLC, PlcEnums.PLCType plcType, string PortSendingPLC)
            :this(IpPC,PortPC,IpPLC,PortPLC,plcType,PortSendingPLC, cobIdToPLCdefault, cobIdFromPLCdefault)
        {
        }

        public AssemblyLineComProtocol(string IpPC, string PortPC, string IpPLC, string PortPLC, PlcEnums.PLCType plcType, string PortSendingPLC, UInt16 CobIdToPLC, UInt16 CobIdFromPLC)
        {
            this.ipPC = IpPC;
            this.portPC = PortPC;
            this.ipPLC = IpPLC;
            this.portPLC = PortPLC;
            this.plcType = plcType;
            this.portSendingPLC = PortSendingPLC;
            this.cobIdToPLC = CobIdToPLC;
            this.cobIdFromPLC = CobIdFromPLC;

            try
            {
                CreatePlcCommunication();

                waitForFirstReceive();

                checkCobID();
            }
            catch (Exception ex)
            {
                //Logging.AddTraceLine(ex.Message);
                Close();
                throw ex;
            }
        }

        public bool ProccesHL
        {
            get { return (bool)getSignal("Zpracuj"); }
        }

        public bool NotproccesHL
        {
            get { return (bool)getSignal("Nezpracuj"); }
        }

        public bool WtRdy
        {
            set { setSignal("WT_RDY", value); }
        }

        public bool OK
        {
            set { setSignal("OK", value); }
        }

        public bool NOK
        {
            set { setSignal("NOK", value); }
        }

        public string PalleteID
        {
            get { return getSignal("wOdLinkyCisloWT").ToString(); }
        }

        public void SendData()
        {
            plcCommunication.SendData(RawDataToPLC);
        }

        public void setSignal(string name, object value)
        {
            setSignal(name, value, true);
        }

        public void setSignal(string name, object value, bool throwException)
        {
            int index = GetSignalIndex(name, true);
            dataToPLC[index].Value = value;
            CommunicationBase.SignalToRAW(dataToPLC[index], ref this.RawDataToPLC, littleEndian);
        }

        public object getSignal(string name)
        {
            checkCommunicationRunning();
            int index = GetSignalIndex(name, false);
            Signal resultSignal = CommunicationBase.RAWtoSignal(dataFromPLC[index], this.RawDataFromPLC, littleEndian);
            if (resultSignal == null)
                return null;
            dataFromPLC[index].Value = resultSignal.Value;

            return dataFromPLC[index].Value;
        }

        public void Close()
        {
            if (plcCommunication != null)
            {
                plcCommunication.Close();
            }
        }

        private void CreatePlcCommunication()
        {
            Close();

            RawDataToPLC = new byte[completeLenght];
            RawDataFromPLC = new byte[completeLenght];

            dataToPLC = new List<Signal>();
            dataFromPLC = new List<Signal>();

            frameCounter = 0;

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
            setSignal("nItems", 16, false);

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
            // to trasfer system
            dataToPLC.Add(new Signal(0 + headerLenght * 8, "OK", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(1 + headerLenght * 8, "NOK", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(2 + headerLenght * 8, "WT_RDY", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(3 + headerLenght * 8, "DoLinkyBoolRes1", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(4 + headerLenght * 8, "DoLinkyBoolRes2", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(5 + headerLenght * 8, "DoLinkyBoolRes3", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(6 + headerLenght * 8, "DoLinkyBoolRes4", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(7 + headerLenght * 8, "DoLinkyBoolRes5", PlcEnums.DataType.BOOL));
            dataToPLC.Add(new Signal(8 + headerLenght * 8, "DoLinkyByteRes6", PlcEnums.DataType.Byte));

            dataToPLC.Add(new Signal(16 + headerLenght * 8, "DoLinkyCitac", PlcEnums.DataType.UINT16));

            dataToPLC.Add(new Signal(32 + headerLenght * 8, "DoLinkyStringRes", PlcEnums.DataType.String, 28));

            //from transfer system
            dataFromPLC.Add(new Signal(64, "COB-ID", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(0 + headerLenght * 8, "wOdLinkyCisloVoziku", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(16 + headerLenght * 8, "wOdLinkyCisloWT", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(32 + headerLenght * 8, "wOdLinkyCipPredniA", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(48 + headerLenght * 8, "wOdLinkyCipPredniB", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(64 + headerLenght * 8, "wOdLinkyCipZadniA", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(80 + headerLenght * 8, "wOdLinkyCipZadniB", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(96 + headerLenght * 8, "wOdLinkyAktualniStanice", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(112 + headerLenght * 8, "wOdLinkyAktualniCil", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(128 + headerLenght * 8, "wOdLinkyPrechoziCil", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(144 + headerLenght * 8, "wOdLinkyCitac", PlcEnums.DataType.UINT16));
            //dataFromPLC.Add(new Signal(160, "wOdLinkyAkce", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(160 + headerLenght * 8, "Nezpracuj", PlcEnums.DataType.BOOL));
            dataFromPLC.Add(new Signal(161 + headerLenght * 8, "Zpracuj", PlcEnums.DataType.BOOL));
            dataFromPLC.Add(new Signal(162 + headerLenght * 8, "OdLinkyBoolRes1", PlcEnums.DataType.BOOL));
            dataFromPLC.Add(new Signal(163 + headerLenght * 8, "OdLinkyBoolRes2", PlcEnums.DataType.BOOL));
            dataFromPLC.Add(new Signal(164 + headerLenght * 8, "OdLinkyBoolRes3", PlcEnums.DataType.BOOL));
            dataFromPLC.Add(new Signal(165 + headerLenght * 8, "OdLinkyBoolRes4", PlcEnums.DataType.BOOL));
            dataFromPLC.Add(new Signal(166 + headerLenght * 8, "OdLinkyBoolRes5", PlcEnums.DataType.BOOL));
            dataFromPLC.Add(new Signal(167 + headerLenght * 8, "OdLinkyBoolRes6", PlcEnums.DataType.BOOL));
            dataFromPLC.Add(new Signal(168 + headerLenght * 8, "OdLinkyByteRes7", PlcEnums.DataType.Byte));

            dataFromPLC.Add(new Signal(176 + headerLenght * 8, "wOdlinkyRezerva1", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(192 + headerLenght * 8, "wOdlinkyRezerva2", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(208 + headerLenght * 8, "wOdlinkyRezerva3", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(224 + headerLenght * 8, "wOdlinkyRezerva4", PlcEnums.DataType.UINT16));
            dataFromPLC.Add(new Signal(240 + headerLenght * 8, "wOdlinkyRezerva5", PlcEnums.DataType.UINT16));

            plcCommunication = CommunicationBase.GetInstance(this.plcType);// new Bosch_UDP();
            plcCommunication.OnData += plcCommunication_OnData;

            plcCommunication.Start(this.ipPC, this.portPC, this.ipPLC, this.portPLC, false, this.portSendingPLC);

        }

        private void plcCommunication_OnData(object sender, byte[] data)
        {
            try
            {
                if (RawDataFromPLC.Length != data.Length)
                {
                    errorRecLenData = true;
                    return;
                }
                else
                {
                    errorRecLenData = false;
                }

                RawDataFromPLC = data;
                UInt16 citac = (UInt16)getSignal("wOdLinkyCitac");

                setSignal("DoLinkyCitac", citac);

                if (citac != previousOdLinkyCitac) 
                {
                    lastUpdateOdLinkyCitac = DateTime.Now;
                }

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

        private void waitForFirstReceive()
        {
            int counter = 0;

            while (counter < 30)
            {
                if (plcCommunication.LastRxTimeStamp() != DateTime.MinValue)
                {
                    break;
                }
                counter++;
                Thread.Sleep(100);
            }

            if (plcCommunication.LastRxTimeStamp() == DateTime.MinValue)
            {
                var errorMessage = "No data received. Please check network cable, IP addresses and ports setting";
                //Logging.AddTraceLine(errorMessage);
                throw new Exception(errorMessage);
            }
            firstPackedReceived = true;
        }

        private void checkCommunicationRunning()
        {
            if (firstPackedReceived)
            {
                if (DateTime.Now > plcCommunication.LastRxTimeStamp().AddSeconds(10))
                {
                    var errorMessage = "Communication with transfer system stopped working. No data received for more than 10 seconds";
                    //Logging.AddTraceLine(errorMessage);
                    throw new Exception(errorMessage);
                }
                if (DateTime.Now > lastUpdateOdLinkyCitac.AddSeconds(10))
                {
                    var errorMessage = "Communication with transfer system stopped working. OdLinkyCitac was not changed for more than 10 seconds";
                    //Logging.AddTraceLine(errorMessage);
                    throw new Exception(errorMessage);
                }
            }
        }

        private void checkCobID()
        {
            if (cobIdFromPLC != (UInt16)getSignal("COB-ID"))
            {
                var errorMessage = "Invalid COB-ID from PLC";
                //Logging.AddTraceLine(errorMessage);
                throw new Exception(errorMessage);
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
