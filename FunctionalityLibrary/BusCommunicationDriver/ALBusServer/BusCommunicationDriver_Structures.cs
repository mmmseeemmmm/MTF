using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AUTOMATIONAPILib;

namespace ALBusComDriver
{
    //[Serializable]
    //public class OffBoardConfig
    //{
    //    //OffBoard
    //    private string projectName = string.Empty;
    //    private bool enableByteTrace = false;
    //    private string byteTraceInterfaceName = string.Empty;
    //    private List<string> logicalLinks = new List<string>();


    //    public string ProjectName
    //    {
    //        get { return projectName; }
    //        set { projectName = value; }
    //    }
    //    private string vit = string.Empty;

    //    public string VIT
    //    {
    //        get { return vit; }
    //        set { vit = value; }
    //    }


    //    public bool EnableByteTrace
    //    {
    //        get { return enableByteTrace; }
    //        set { enableByteTrace = value; }
    //    }


    //    public string ByteTraceInterfaceName
    //    {
    //        get { return byteTraceInterfaceName; }
    //        set { byteTraceInterfaceName = value; }
    //    }


    //    public List<string> LogicalLinks
    //    {
    //        get { return logicalLinks; }
    //        set { logicalLinks = value; }
    //    }
    //}

    //[Serializable]
    //public class OnBoardConfig
    //{
    //    //OnBoard
    //    private string deviceName = string.Empty;
    //    private string busType = string.Empty;
    //    private string busChannel = string.Empty;
    //    private string netcCfgFile = string.Empty;
    //    private string customCgfFile = string.Empty;
    //    private string busNodes = string.Empty;
    //    private string linScheduleTable = string.Empty;
    //    private string linMasterNode = string.Empty;

    //    public hwType HWType { get; set; }

    //    public string DeviceName
    //    {
    //        get { return deviceName; }
    //        set { deviceName = value; }
    //    }

    //    public string BusType
    //    {
    //        get { return busType; }
    //        set { busType = value; }
    //    }

    //    public string BusChannel
    //    {
    //        get { return busChannel; }
    //        set { busChannel = value; }
    //    }

    //    public string NetcCfgFile
    //    {
    //        get { return netcCfgFile; }
    //        set { netcCfgFile = value; }
    //    }

    //    public string CustomCgfFile
    //    {
    //        get { return customCgfFile; }
    //        set { customCgfFile = value; }
    //    }

    //    public string BusNodes
    //    {
    //        get { return busNodes; }
    //        set { busNodes = value; }
    //    }


    //    public string LINScheduleTable
    //    {
    //        get { return linScheduleTable; }
    //        set { linScheduleTable = value; }
    //    }

    //    public string LINMasterNode
    //    {
    //        get { return linMasterNode; }
    //        set { linMasterNode = value; }
    //    }

    //}

    //[Serializable]
    //public class OnBoardSignal
    //{
    //    private string value = string.Empty;
    //    private string unit = string.Empty;
    //    public string Value
    //    {
    //        get { return this.value; }
    //        set { this.value = value; }
    //    }

    //    public string Unit
    //    {
    //        get { return unit; }
    //        set { unit = value; }
    //    }
    //}

    public class VCFObj
    {
        public OnBoardConfig OnBoardSetting { get; set; }
        public ALRestbusVCF.ICommunicationObject ComObj { get; set; }
    }

    internal class LogicalLink
    {
        public string Name { get; set; }
        public dtsPcAccessPath Value { get; set; }
    }

    //[Serializable]
    //public class OffBoardService
    //{
    //    private List<OffBoardRequestParameter> requestParameters = new List<OffBoardRequestParameter>();
    //    private List<OffBoardResponseSetting> responses = new List<OffBoardResponseSetting>();


    //    public string ServiceName { get; set; }

    //    public List<OffBoardRequestParameter> RequestParameters
    //    {
    //        get { return requestParameters; }
    //        set { requestParameters = value; }
    //    }

    //    public List<OffBoardResponseSetting> Responses
    //    {
    //        get { return responses; }
    //        set { responses = value; }
    //    }
    //}

    //[Serializable]
    //public class OffBoardRequestParameter
    //{
    //    public string Name { get; set; }
    //    public string Value { get; set; }
    //}

    //[Serializable]
    //public class OffBoardResponse
    //{
    //    private List<OffBoardServiceResult> results = new List<OffBoardServiceResult>();

    //    public List<OffBoardServiceResult> Results
    //    {
    //        get { return results; }
    //        set { results = value; }
    //    }
    //    public type_dtsPcExecutionStates ExecState { get; set; }
    //    public Byte[] RawRequest { get; set; }
    //    public Byte[] RawResponse { get; set; }
    //}

    //[Serializable]
    //public class OffBoardServiceResult : OffBoardResponseSetting
    //{
    //    private List<string> values = new List<string>();

    //    public List<string> Values
    //    {
    //        get { return values; }
    //        set { values = value; }
    //    }
    //    private bool checkResult = false;

    //    public bool CheckResult
    //    {
    //        get { return checkResult; }
    //        set { checkResult = value; }
    //    }
    //}

    //[Serializable]
    //public class OffBoardResponseSetting
    //{
    //    private List<string> keys = new List<string>();

    //    public List<string> Keys
    //    {
    //        get { return keys; }
    //        set { keys = value; }
    //    }

    //    private List<string> ignoredValues = new List<string>();

    //    public List<string> IgnoredValues
    //    {
    //        get { return ignoredValues; }
    //        set { ignoredValues = value; }
    //    }

    //    private bool doCheckResponse = false;

    //    public bool DoCheckResponse
    //    {
    //        get { return doCheckResponse; }
    //        set { doCheckResponse = value; }
    //    }
    //    private Limit limit = new Limit();

    //    public Limit Limit
    //    {
    //        get { return limit; }
    //        set { limit = value; }
    //    }
    //}

    //[Serializable]
    //public class OffBoardFlashJobSetting
    //{
    //    private string logicalLink = string.Empty;

    //    public string LogicalLink
    //    {
    //        get { return logicalLink; }
    //        set { logicalLink = value; }
    //    }
    //    private string flashJobName = string.Empty;

    //    public string FlashJobName
    //    {
    //        get { return flashJobName; }
    //        set { flashJobName = value; }
    //    }
    //    private string sessionName = string.Empty;

    //    public string SessionName
    //    {
    //        get { return sessionName; }
    //        set { sessionName = value; }
    //    }
    //}

    //[Serializable]
    //public class OffBoardFlashJobSettings
    //{
    //    private List<OffBoardFlashJobSetting> offBoardFlashJobSettingList = new List<OffBoardFlashJobSetting>();

    //    public List<OffBoardFlashJobSetting> OffBoardFlashJobSettingList
    //    {
    //        get { return offBoardFlashJobSettingList; }
    //        set { offBoardFlashJobSettingList = value; }
    //    }
    //}

    //[Serializable]
    //public class OffBoardFlashJobResult : OffBoardFlashJobSetting
    //{
    //    private string executionState = string.Empty;

    //    public string ExecutionState
    //    {
    //        get { return executionState; }
    //        set { executionState = value; }
    //    }
    //    private string jobResult = string.Empty;

    //    public string JobResult
    //    {
    //        get { return jobResult; }
    //        set { jobResult = value; }
    //    }
    //}

    //[Serializable]
    //public class Limit
    //{
    //    private double min = -999.99;

    //    public double Min
    //    {
    //        get { return min; }
    //        set { min = value; }
    //    }
    //    private double max = 999.99;

    //    public double Max
    //    {
    //        get { return max; }
    //        set { max = value; }
    //    }
    //    private List<string> exactly = new List<string>();

    //    public List<string> Exactly
    //    {
    //        get { return exactly; }
    //        set { exactly = value; }
    //    }
    //}

    public class Parameter
    {
        private string name = string.Empty;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private object value = string.Empty;

        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public Parameter(string name, object value)
        {
            this.name = name;
            this.value = value;
        }
    }

    //[Serializable]
    //public class ReceivedResponse
    //{
    //    public string setName;
    //    public string resulNo;
    //    public string resultName;
    //    public string resultValue;
    //}

    //public enum hwType
    //{
    //    HSX = 0,
    //    Vector = 1
    //}

    //[Serializable]
    //public class OffBoardConfigEdiabas
    //{
    //    //OffBoard
    //    private string serialNumber = string.Empty;
    //    private string busChannel = string.Empty;
    //    private string sgbdFolder = string.Empty;


    //    public string SerialNumber
    //    {
    //        get { return serialNumber; }
    //        set { serialNumber = value; }
    //    }

    //    public string BusChannel
    //    {
    //        get { return busChannel; }
    //        set { busChannel = value; }
    //    }

    //    public string SgbdFolder
    //    {
    //        get { return sgbdFolder; }
    //        set { sgbdFolder = value; }
    //    }
    //}
}
