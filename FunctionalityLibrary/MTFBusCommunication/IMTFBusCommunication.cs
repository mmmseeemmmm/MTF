using System;
using MTFBusCommunication.Structures;
using System.Collections.Generic;
namespace MTFBusCommunication
{
    public interface IMTFBusCommunication : IDisposable
    {
        MTFOffBoardFlashJobResult GetFlashJobResult(int index);
        string GetKey(int resultIndex, int valueIndex);
        OnBoardConfiguration GetOnBoardItem(int index);
        string GetValue(int resultIndex, int valueIndex);
        void Initialize();
        MTFOffBoardConfig OffBoard { set; }
        MTFOffBoardResponse OffBoardExecuteService(string logicalLinkName, MTFOffBoardService serviceSetting);
        List<MTFOffBoardLogicalLinkParallelResponses> OffBoardExecuteServicesInParallel(List<MTFOffBoardLogicalLinkParallelServices> offBoardParallelServicesSetting);
        void OffBoardExecuteServiceCyclically(string logicalLinkName, MTFOffBoardService serviceSetting,int cycleTime);
        void StopOffBoardExecuteServiceCyclically();
        List<MTFOffBoardFlashJobResult> OffBoardFlashJob(List<MTFOffBoardFlashJobSetting> flashJobSetttings);
        string OffBoardVariantIdentificationAndSelection(string logicalLinkName);
        void OnBoardActiveScheduleTable(int virtualChannel, string scheduleTable);
        string OnBoardGetGlobalVariable(int virtualChannel, string variableName);
        MTFOnBoardSignal OnBoardGetSignal(int virtualChannel, string frameName, string signalName);
        byte[] OnBoardReceiveFrameOnce(int virtualChannel, uint frameId, uint resFrameId, byte[] data, uint timeout);
        List<OnBoardConfiguration> OnBoards { set; }
        void OnBoardSendFrameOnce(int virtualChannel, uint frameId, byte[] data);
        void OnBoardSetGlobalVariable(int virtualChannel, string variableName, string variableValue);
        void OnBoardSetSignal(int virtualChannel, string frameName, string signalName, string signalValue);
        void Start();
        MTFBusComDriverStatusEnum Status { get; }
        void Stop();
        MTFOffBoardResponse OffBoardExecuteOTX(MTFOffBoardService serviceSetting);
        void SetLogging(bool enable, string logPath, int maxLogFilesCount);
    }
}
