using System;
using System.Text;
using AutomotiveLighting.MTFCommon;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Utils;
using System.Net;
using System.Xml;


namespace BarcodeScanner
{
    public delegate void DataReceivedEventHandler(string data, EventArgs e);

    [MTFClass(Name = "Barcode Scanner", Icon = MTFIcons.BarcodeScanner)]
    [MTFClassCategory("Identification")]
    public class Scanner : IDisposable, ICanStop
    {
        private readonly string comPortName;
        private readonly int baudRate;
        private readonly bool rtsEnabled;
        private readonly string turnOnCommand;
        private readonly string ip;
        private SerialPort scanerPort;
        private string barcode = string.Empty;

        private ISystemConnector cognexCon = null;
        private DataManSystem cognexSystem = null;
        private ResultCollector cognexResults;
        private bool stop;

        public event DataReceivedEventHandler DataReceived;

        IMTFSequenceRuntimeContext RuntimeContext;

        [MTFConstructor(Description = "Create Barcode Scanner - with COM Port")]
        public Scanner(string port, int baudRate) : this(port, baudRate, false, string.Empty)
        { }

        [MTFConstructor(Description = "Create Barcode Scanner - with COM Port")]
        [MTFAdditionalParameterInfo(ParameterName = "turnOnCommand", DefaultValue = "04 E4 04 00 FF 14", Description = "Hex value of command which is send before each read command, if empty, nothing is send.")]
        public Scanner(string port, int baudRate, bool rtsEnabled, string turnOnCommand)
        {
            this.comPortName = port;
            this.baudRate = baudRate;
            this.rtsEnabled = rtsEnabled;
            this.turnOnCommand = turnOnCommand;
            Init();//TODO Don't call init in constructor
        }

        [MTFConstructor(Description = "Create Barcode Scanner - Cognex with Ethernet, userName: admin - psw: string.empty")]
        public Scanner(string IP)
        {
            ip = IP;
            InitCodex();//TODO Don't call init in constructor
        }

        private void InitCodex()
        {
            IPAddress IP = IPAddress.Parse(ip);                        
            EthSystemConnector conn = new EthSystemConnector(IP, 23);
            conn.UserName = "admin";
            conn.Password = string.Empty;
            cognexCon = conn;
            cognexSystem = new DataManSystem(cognexCon);
            ResultTypes requested_result_types = ResultTypes.ReadXml;
            cognexResults = new ResultCollector(cognexSystem, requested_result_types);
            cognexResults.ComplexResultArrived += Results_ComplexResultArrived;

            cognexSystem.Connect();
            cognexSystem.SetResultTypes(requested_result_types);
        }

        public void Init()
        {
            ConnectCom();
        }

        private void ConnectCom()
        {
            scanerPort = new SerialPort(comPortName, baudRate);
            scanerPort.DataReceived += scanerPort_DataReceived;
            scanerPort.Encoding = Encoding.UTF8;
            scanerPort.RtsEnable = rtsEnabled;
            scanerPort.Open();
        }

        private void DisconnectCom()
        {
            if (scanerPort != null && scanerPort.IsOpen)
            {
                scanerPort.DataReceived -= scanerPort_DataReceived;
                scanerPort.Close();
                scanerPort.Dispose();
            }            
        }

        public void Dispose()
        {
            DisconnectCom();

            if (null != cognexSystem)
            {
                cognexResults.ComplexResultArrived -= Results_ComplexResultArrived;
                try
                {
                    cognexSystem.Disconnect();
                }
                catch { };
            }

            cognexCon = null;
            cognexSystem = null;

            GC.SuppressFinalize(this);
        }

        ~Scanner()
        {
            this.Dispose();
        }

        void Results_ComplexResultArrived(object sender, ResultInfo e)
        {
            barcode = !string.IsNullOrEmpty(e.ReadString) ? e.ReadString : GetReadStringFromResultXml(e.XmlResult);
           
        }

        private string GetReadStringFromResultXml(string resultXml)
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                doc.LoadXml(resultXml);

                XmlNode full_string_node = doc.SelectSingleNode("result/general/full_string");

                if (full_string_node != null)
                {
                    XmlAttribute encoding = full_string_node.Attributes["encoding"];
                    if (encoding != null && encoding.InnerText == "base64")
                    {
                        byte[] code = Convert.FromBase64String(full_string_node.InnerText);
                        return cognexSystem.Encoding.GetString(code, 0, code.Length);
                    }

                    return full_string_node.InnerText;
                }
            }
            catch
            {
            }

            return "";
        }


        private void Waiting()
        {
            barcode = string.Empty;
            RuntimeContext.TextNotification("Waiting for barcode..."); 

            while (string.IsNullOrEmpty(barcode) && !stop)
            {
                Thread.Sleep(500);
            }
        }



        void scanerPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = sender as SerialPort;
            string stringData = string.Empty;
            do
            {
                stringData += port.ReadExisting();
                Thread.Sleep(100);
            } while (port.BytesToRead > 0);
            barcode = stringData.Trim();
            if (DataReceived!=null)
            {
                DataReceived(barcode, EventArgs.Empty);
            }
        }

        private void TurnOnScanner()
        {
            if (string.IsNullOrEmpty(turnOnCommand))
            {
                return;
            }

            //scanerPort.DiscardInBuffer();
            //scanerPort.DiscardOutBuffer();
            var valore = turnOnCommand.Split(' ').Select(v => Convert.ToByte(v, 16)).ToArray();
            scanerPort.Write(valore, 0, valore.Length);
            //scanerPort.DiscardOutBuffer();
            //scanerPort.DiscardInBuffer();
        }
        
        [MTFProperty]
        public string Data
        {
            get
            {
                TurnOnScanner();
                Waiting();
                return barcode;
            }
        }

        [MTFMethod(DisplayName = "Get Data by index")]
        public string GetDataByIndex(IndexesPair toParse)
        {
            TurnOnScanner();
            Waiting();
            return !string.IsNullOrEmpty(barcode)
                ? (toParse != null ? barcode.Substring(toParse.StartIndex, toParse.Length) : barcode)
                : null;
        }

        public bool Stop
        {
            set { stop = value; }
        }
    }
}
