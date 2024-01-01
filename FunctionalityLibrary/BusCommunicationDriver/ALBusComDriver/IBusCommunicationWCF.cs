using System.Collections.Generic;
using System.ServiceModel;

namespace ALBusServer
{
    public enum StatusEnumInternal { Configured, Initialized, Connected, Released, NotDefined }
    [ServiceContract]
    public interface IBusCommunicationWCF
    {
        [OperationContract]
        void Initialize();

        [OperationContract]
        void Dispose();

        //[OperationContract]
        //void LogMessage(string message, bool logTimeStamp);

        [OperationContract]
        void SetOffBoard(ALBusComDriver.OffBoardConfig offBoardConfig);

        [OperationContract]
        void SetOffBoardEdiabas(ALBusComDriver.OffBoardConfigEdiabas offBoardEdiabas);

        [OperationContract]
        List<ALBusComDriver.OffBoardLogicalLinkParallelServices> OffBoardExecuteServicesInParallel(List<ALBusComDriver.OffBoardLogicalLinkParallelServices> offBoardParallelServicesSetting);

        [OperationContract]
        ALBusComDriver.OffBoardResponse OffBoardExecuteService(string logicalLinkName, ALBusComDriver.OffBoardService serviceSetting);

        [OperationContract]
        void OffBoardExecuteServiceCyclically(string logicalLinkName, ALBusComDriver.OffBoardService serviceSetting, int cycleTime);

        [OperationContract]
        void StopOffBoardExecuteServiceCyclically();

        [OperationContract]
        List<ALBusComDriver.OffBoardFlashJobResult> OffBoardFlashJob(List<ALBusComDriver.OffBoardFlashJobSetting> flashJobSetttings);

        [OperationContract]
        string OffBoardVariantIdentificationAndSelection(string logicalLinkName);

        [OperationContract]
        void OnBoardActiveScheduleTable(string deviceName, string busType, string channel, string scheduleTable);

        [OperationContract]
        void OnBoardActiveScheduleTableByName(string scheduleTable);

        [OperationContract]
        string OnBoardGetGlobalVariable(string deviceName, string busType, string channel, string variableName);

        [OperationContract]
        string OnBoardGetGlobalVariableByName(string variableName);

        [OperationContract]
        byte[] OnBoardGetGlobalVariableRaw(string deviceName, string busType, string channel, string variableName);

        [OperationContract]
        byte[] OnBoardGetGlobalVariableByNameRaw(string variableName);

        [OperationContract]
        ALBusComDriver.OnBoardSignal OnBoardGetSignal(string deviceName, string busType, string channel, string frameName, string signalName);

        [OperationContract]
        ALBusComDriver.OnBoardSignal OnBoardGetSignal2(string frameName, string signalName);

        [OperationContract]
        byte[] OnBoardReceiveFrameOnce(string deviceName, string busType, string channel, uint frameID, uint resFrameID, byte[] data, uint timeout);

        [OperationContract]
        byte[] OnBoardReceiveFrameOnce2(uint frameID, uint resFrameID, byte[] data, uint timeout);

        [OperationContract]
        void SetOnBoards(List<ALBusComDriver.OnBoardConfig> onBoards);

        [OperationContract]
        List<ALBusComDriver.OnBoardConfig> GetOnBoards();

        [OperationContract]
        void OnBoardSendFrameOnce(string deviceName, string busType, string channel, uint frameID, byte[] data);

        [OperationContract]
        void OnBoardSendFrameOnce2(uint frameID, byte[] data);

        [OperationContract]
        void OnBoardSetGlobalVariable(string deviceName, string busType, string channel, string variableName, string variableValue);

        [OperationContract]
        void OnBoardSetGlobalVariable2(string variableName, string variableValue);

        [OperationContract]
        void OnBoardSetSignal(string deviceName, string busType, string channel, string frameName, string signalName, string signalValue);

        [OperationContract]
        void OnBoardSetSignal2(string frameName, string signalName, string signalValue);

        [OperationContract]
        void OnBoardsCheckRunning();

        [OperationContract]
        List<string> OnBoardGetEcus(string netConfigFile);

        [OperationContract]
        List<string> OnBoardGetInputSignals(string networkCfgFile, string ecus);

        [OperationContract]
        List<string> OnBoardGetOutputSignals(string networkCfgFile, string ecus);

        //[OperationContract]
        //void SetLogging(bool enable, string logPath, int maxLogFilesCount);

        [OperationContract]
        void OnBoardStart();

        [OperationContract]
        void OnBoardStop();
        
        [OperationContract]
        void OffBoardStart();

        [OperationContract]
        void OffBoardStop();

        [OperationContract]
        void Start();

        [OperationContract]
        void Stop();

        [OperationContract]
        ALBusComDriver.OffBoardResponse OffBoardExecuteOTX(ALBusComDriver.OffBoardService serviceSetting);

        [OperationContract]
        StatusEnumInternal GetStatus();

        [OperationContract]
        void OnBoardSendCANFDFrameCyclic(string deviceName, string busType, string channel, uint frameId, string data, uint timeOutFrame);

        [OperationContract]
        void OnBoardSendCANFDFrameCyclic1(string deviceName, string busType, string channel, uint frameId, IEnumerable<string> data, uint timeOutFrame, uint timeOutSubList);

        [OperationContract]
        void OnBoardSendCANFDFrameCyclic2(string deviceName, string busType, string channel, Dictionary<uint, IEnumerable<string>> frameData, uint timeOutFrame, uint timeOutSubList);

        [OperationContract]
        void OnBoardSendFrameCyclic(string deviceName, string busType, string channel, uint frameId, string data, uint timeOutFrame);

        [OperationContract]
        void OnBoardSendFrameCyclic1(string deviceName, string busType, string channel, uint frameId, IEnumerable<string> data, uint timeOutFrame, uint timeOutSubList);

        [OperationContract]
        void OnBoardSendFrameCyclic2(string deviceName, string busType, string channel, Dictionary<uint, IEnumerable<string>> frameData, uint timeOutFrame, uint timeOutSubList);

        [OperationContract]
        void SetGlobalAdapterProjectVariable(string deviceName, string busType, string channel, string variableName, string value);

        [OperationContract]
        string GetGlobalAdapterProjectVariable(string deviceName, string busType, string channel, string variableName);
    }
}
