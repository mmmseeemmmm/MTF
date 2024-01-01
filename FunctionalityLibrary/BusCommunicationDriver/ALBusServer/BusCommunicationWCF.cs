using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ALBusServer
{
    class BusCommunicationWCF : IBusCommunicationWCF
    {
        private ALBusComDriverServer.BusCommunicationDriver busCommunicationDriver;

        public BusCommunicationWCF()
        {
            try
            {
                busCommunicationDriver = new ALBusComDriverServer.BusCommunicationDriver();
            }
            catch(Exception e)
            {
                throw new System.ServiceModel.FaultException(e.Message);
            }
        }

        public void Initialize()
        {
            callAndLog(() => busCommunicationDriver.Initialize(), "Call Initialize", "Initialize finished");
        }

        public void Dispose()
        {
            logMessage("Dispose called", true);
            if (this.busCommunicationDriver != null)
            {
                this.busCommunicationDriver.Dispose();
            }
        }

        private void logMessage(string message, bool logTimeStamp)
        {
            try
            {
                busCommunicationDriver.LogMessage(message, logTimeStamp);
            }
            catch (Exception e)
            {
                throw new System.ServiceModel.FaultException(e.Message);
            }
        }

        public List<string> OnBoardGetEcus(string netConfigFile)
        {
            return callAndLog(() => busCommunicationDriver.OnBoardGetEcus(netConfigFile), "Call OnBoardGetEcus", "OnBoardGetEcus finished");
        }

        public List<string> OnBoardGetInputSignals(string networkCfgFile, string ecus)
        {
            return callAndLog(() => busCommunicationDriver.OnBoardGetInputSignals(networkCfgFile, ecus), "Call OnBoardGetInputSignals", "OnBoardGetInputSignals finished");
        }

        public List<string> OnBoardGetOutputSignals(string networkCfgFile, string ecus)
        {
            return callAndLog(() => busCommunicationDriver.OnBoardGetOutputSignals(networkCfgFile, ecus), "Call OnBoardGetOutputSignals", "OnBoardGetOutputSignals finished");
        }

        public void SetOffBoard(ALBusComDriver.OffBoardConfig offBoardConfig)
        {
            callAndLog(() => busCommunicationDriver.OffBoard = offBoardConfig, "Call SetOffBoard", "SetOffBoard finished");
        }

        public void SetOffBoardEdiabas(ALBusComDriver.OffBoardConfigEdiabas offBoardEdiabas)
        {
            callAndLog(() => busCommunicationDriver.OffBoardEdiabas = offBoardEdiabas, "Call SetOffBoardEdiabas", "SetOffBoardEdiabas finished");
        }

        public List<ALBusComDriver.OffBoardLogicalLinkParallelServices> OffBoardExecuteServicesInParallel(List<ALBusComDriver.OffBoardLogicalLinkParallelServices> offBoardParallelServicesSetting)
        {
            return callAndLog(() => busCommunicationDriver.OffBoardExecuteServicesInParallel(offBoardParallelServicesSetting), "Call OffBoardExecuteServicesInParallel", "OffBoardExecuteService finished");
        }

        public ALBusComDriver.OffBoardResponse OffBoardExecuteService(string logicalLinkName, ALBusComDriver.OffBoardService serviceSetting)
        {
            return callAndLog(() => busCommunicationDriver.OffBoardExecuteService(logicalLinkName, serviceSetting), "Call OffBoardExecuteService", "OffBoardExecuteService finished");
        }

        public void OffBoardExecuteServiceCyclically(string logicalLinkName, ALBusComDriver.OffBoardService serviceSetting, int cycleTime)
        {
            callAndLog(() => busCommunicationDriver.OffBoardExecuteServiceCyclically(logicalLinkName, serviceSetting, cycleTime), "Call OffBoardExecuteServiceCyclically", "OffBoardExecuteServiceCyclically finished");
        }

        public void StopOffBoardExecuteServiceCyclically()
        {
            callAndLog(() => busCommunicationDriver.StopOffBoardExecuteServiceCyclically(), "Call StopOffBoardExecuteServiceCyclically", "StopOffBoardExecuteServiceCyclically finished");
        }

        public List<ALBusComDriver.OffBoardFlashJobResult> OffBoardFlashJob(List<ALBusComDriver.OffBoardFlashJobSetting> flashJobSetttings)
        {
            return callAndLog(() => busCommunicationDriver.OffBoardFlashJob(flashJobSetttings), "Call OffBoardFlashJob", "OffBoardFlashJob finished");
        }

        public string OffBoardVariantIdentificationAndSelection(string logicalLinkName)
        {
            return callAndLog(() => busCommunicationDriver.OffBoardVariantIdentificationAndSelection(logicalLinkName), "Call OffBoardVariantIdentificationAndSelection", "OffBoardVariantIdentificationAndSelection finished");
        }

        public void OnBoardActiveScheduleTable(string deviceName, string busType, string channel, string scheduleTable)
        {
            callAndLog(() => busCommunicationDriver.OnBoardActiveScheduleTable(deviceName, busType, channel, scheduleTable), "Call OnBoardActiveScheduleTable", "OnBoardActiveScheduleTable finished");
        }

        public void OnBoardActiveScheduleTableByName(string scheduleTable)
        {
            callAndLog(() => busCommunicationDriver.OnBoardActiveScheduleTable(scheduleTable), "Call OnBoardActiveScheduleTableByName", "OnBoardActiveScheduleTableByName finished");
        }

        public string OnBoardGetGlobalVariable(string deviceName, string busType, string channel, string variableName)
        {
            return callAndLog(() => busCommunicationDriver.OnBoardGetGlobalVariable(deviceName, busType, channel, variableName), "Call OnBoardGetGlobalVariable", "OnBoardGetGlobalVariable finished");
        }

        public string OnBoardGetGlobalVariableByName(string variableName)
        {
            return callAndLog(() => busCommunicationDriver.OnBoardGetGlobalVariable(variableName), "Call OnBoardGetGlobalVariableByName", "OnBoardGetGlobalVariableByName finished");
        }

        public byte[] OnBoardGetGlobalVariableRaw(string deviceName, string busType, string channel, string variableName)
        {
            return callAndLog(() => busCommunicationDriver.OnBoardGetGlobalVariableRaw(deviceName, busType, channel, variableName), "Call OnBoardGetGlobalVariableRaw", "OnBoardGetGlobalVariableRaw finished");
        }

        public byte[] OnBoardGetGlobalVariableByNameRaw(string variableName)
        {
            return callAndLog(() => busCommunicationDriver.OnBoardGetGlobalVariableByNameRaw(variableName), "Call OnBoardGetGlobalVariableByNameRaw", "OnBoardGetGlobalVariableByNameRaw finished");
        }

        public ALBusComDriver.OnBoardSignal OnBoardGetSignal(string deviceName, string busType, string channel, string frameName, string signalName)
        {
            return callAndLog(() => busCommunicationDriver.OnBoardGetSignal(deviceName, busType, channel, frameName, signalName), "Call OnBoardGetSignal", "OnBoardGetSignal finished");
        }

        public ALBusComDriver.OnBoardSignal OnBoardGetSignal2(string frameName, string signalName)
        {
            return callAndLog(() => busCommunicationDriver.OnBoardGetSignal(frameName, signalName), "Call OnBoardGetSignal2", "OnBoardGetSignal2 finished");
        }

        public byte[] OnBoardReceiveFrameOnce(string deviceName, string busType, string channel, uint frameID, uint resFrameID, byte[] data, uint timeout)
        {
            return callAndLog(() => busCommunicationDriver.OnBoardReceiveFrameOnce(deviceName, busType, channel, frameID, resFrameID, data, timeout), "Call OnBoardReceiveFrameOnce", "OnBoardReceiveFrameOnce finished");
        }

        public byte[] OnBoardReceiveFrameOnce2(uint frameID, uint resFrameID, byte[] data, uint timeout)
        {
            return callAndLog(() => busCommunicationDriver.OnBoardReceiveFrameOnce(frameID, resFrameID, data, timeout), "Call OnBoardReceiveFrameOnce2", "OnBoardReceiveFrameOnce2 finished");
        }

        public void SetOnBoards(List<ALBusComDriver.OnBoardConfig> onBoards)
        {
            callAndLog(() => busCommunicationDriver.OnBoards = onBoards, "Call SetOnBoards", "SetOnBoards finished");
        }

        public List<ALBusComDriver.OnBoardConfig> GetOnBoards()
        {
            return callAndLog(() => busCommunicationDriver.OnBoards, "Call GetOnBoards", "GetOnBoards finished");
        }

        public void OnBoardSendFrameOnce(string deviceName, string busType, string channel, uint frameID, byte[] data)
        {
            callAndLog(() => busCommunicationDriver.OnBoardSendFrameOnce(deviceName, busType, channel, frameID, data), "Call OnBoardSendFrameOnce", "OnBoardSendFrameOnce finished");
        }

        public void OnBoardSendFrameOnce2(uint frameID, byte[] data)
        {
            callAndLog(() => busCommunicationDriver.OnBoardSendFrameOnce(frameID, data), "Call OnBoardSendFrameOnce2", "OnBoardSendFrameOnce2 finished");
        }

        public void OnBoardSetGlobalVariable(string deviceName, string busType, string channel, string variableName, string variableValue)
        {
            callAndLog(() => busCommunicationDriver.OnBoardSetGlobalVariable(deviceName, busType, channel, variableName, variableValue), "Call OnBoardSetGlobalVariable", "OnBoardSetGlobalVariable finished");
        }

        public void OnBoardSetGlobalVariable2(string variableName, string variableValue)
        {
            callAndLog(() => busCommunicationDriver.OnBoardSetGlobalVariable(variableName, variableValue), "Call OnBoardSetGlobalVariable2", "OnBoardSetGlobalVariable2 finished");
        }

        public void OnBoardSetSignal(string deviceName, string busType, string channel, string frameName, string signalName, string signalValue)
        {
            callAndLog(() => busCommunicationDriver.OnBoardSetSignal(deviceName, busType, channel, frameName, signalName, signalValue), "Call OnBoardSetSignal", "OnBoardSetSignal finished");
        }

        public void OnBoardSetSignal2(string frameName, string signalName, string signalValue)
        {
            callAndLog(() => busCommunicationDriver.OnBoardSetSignal(frameName, signalName, signalValue), "Call OnBoardSetSignal2", "OnBoardSetSignal2 finished");
        }

        public void OnBoardsCheckRunning()
        {
            callAndLog(() => busCommunicationDriver.OnBoardsCheckRunning(), "Call OnBoardsCheckRunning", "OnBoardsCheckRunning finished");
        }

        //public void SetLogging(bool enable, string logPath, int maxLogFilesCount)
        //{
        //    callAndLog(() => busCommunicationDriver.SetLogging(enable, logPath, maxLogFilesCount), "Call SetLogging", "SetLogging finished");
        //}

        public void OnBoardStart()
        {
            callAndLog(() => busCommunicationDriver.OnBoardStart(), "Call OnBoardStart", "OnBoardStart finished");
        }

        public void OnBoardStop()
        {
            callAndLog(() => busCommunicationDriver.OnBoardStop(), "Call OnBoardStop", "OnBoardStop finished");
        }

        public void OffBoardStart()
        {
            callAndLog(() => busCommunicationDriver.OffBoardStart(), "Call OffBoardStart", "OffBoardStart finished");
        }

        public void OffBoardStop()
        {
            callAndLog(() => busCommunicationDriver.OffBoardStop(), "Call OffBoardStop", "OffBoardStop finished");
        }

        public void Start()
        {
            callAndLog(() => busCommunicationDriver.Start(), "Call Start", "Start finished");
        }

        public void Stop()
        {
            callAndLog(() => busCommunicationDriver.Stop(), "Call Stop", "Stop finished");
        }

        public ALBusComDriver.OffBoardResponse OffBoardExecuteOTX(ALBusComDriver.OffBoardService serviceSetting)
        {
            return callAndLog(() => busCommunicationDriver.OffBoardExecuteOTX(serviceSetting), "Call OffBoardExecuteOTX", "OffBoardExecuteOTX finished");
        }

        public StatusEnumInternal GetStatus()
        {
            return callAndLog(() => busCommunicationDriver.Status, "Call GetStatus", "GetStatus finished");
        }

        public void OnBoardSendCANFDFrameCyclic(string deviceName, string busType, string channel, uint frameId, string data, uint timeOutFrame)
        {
            callAndLog(() => busCommunicationDriver.OnBoardSendCANFDFrameCyclic(deviceName, busType, channel, frameId, data, timeOutFrame), "Call SendCANFDFrameCyclic", "SendCANFDFrameCyclic finished");
        }

        public void OnBoardSendCANFDFrameCyclic1(string deviceName, string busType, string channel, uint frameId, IEnumerable<string> data, uint timeOutFrame, uint timeOutSubList)
        {
            callAndLog(() => busCommunicationDriver.OnBoardSendCANFDFrameCyclic(deviceName, busType, channel, frameId, data, timeOutFrame, timeOutSubList), "Call SendCANFDFrameCyclic", "SendCANFDFrameCyclic finished");
        }

        public void OnBoardSendCANFDFrameCyclic2(string deviceName, string busType, string channel, Dictionary<uint, IEnumerable<string>> frameData, uint timeOutFrame, uint timeOutSubList)
        {
            callAndLog(() => busCommunicationDriver.OnBoardSendCANFDFrameCyclic(deviceName, busType, channel, frameData, timeOutFrame, timeOutSubList), "Call SendCANFDFrameCyclic", "SendCANFDFrameCyclic finished");
        }

        public void OnBoardSendFrameCyclic(string deviceName, string busType, string channel, uint frameId, string data, uint timeOutFrame)
        {
            callAndLog(() => busCommunicationDriver.OnBoardSendFrameCyclic(deviceName, busType, channel, frameId, data, timeOutFrame), "Call SendFrameCyclic", "SendFrameCyclic finished");
        }

        public void OnBoardSendFrameCyclic1(string deviceName, string busType, string channel, uint frameId, IEnumerable<string> data, uint timeOutFrame, uint timeOutSubList)
        {
            callAndLog(() => busCommunicationDriver.OnBoardSendFrameCyclic(deviceName, busType, channel, frameId, data, timeOutFrame, timeOutSubList), "Call SendFrameCyclic", "SendFrameCyclic finished");
        }

        public void OnBoardSendFrameCyclic2(string deviceName, string busType, string channel, Dictionary<uint, IEnumerable<string>> frameData, uint timeOutFrame, uint timeOutSubList)
        {
            callAndLog(() => busCommunicationDriver.OnBoardSendFrameCyclic(deviceName, busType, channel, frameData, timeOutFrame, timeOutSubList), "Call SendFrameCyclic", "SendFrameCyclic finished");
        }
        
        public void SetGlobalAdapterProjectVariable(string deviceName, string busType, string channel, string variableName, string value)
        {
            callAndLog(() => busCommunicationDriver.SetGlobalAdapterProjectVariable(deviceName, busType, channel, variableName, value), "Call SetGlobalAdapterProjectVariable", "SetGlobalAdapterProjectVariable finished");
        }

        public string GetGlobalAdapterProjectVariable(string deviceName, string busType, string channel, string variableName)
        {
            return callAndLog(() => busCommunicationDriver.GetGlobalAdapterProjectVariable(deviceName, busType, channel, variableName), "Call GetGlobalAdapterProjectVariable", "GetGlobalAdapterProjectVariable finished");
        }

        private void callAndLog(Action f, string startText, string finishText)
        {
            if (!string.IsNullOrEmpty(startText))
            {
                logMessage(startText, true);
            }
            try
            {
                f();
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(startText))
                {
                    logMessage(startText, true);
                }

                logMessage(string.Format("Call failed with error: {0}", e.Message), true);

                if (!string.IsNullOrEmpty(finishText))
                {
                    logMessage(finishText, true);
                }

                throw new System.ServiceModel.FaultException(e.Message);
            }
            finally
            {
                if (!string.IsNullOrEmpty(finishText))
                {
                    logMessage(finishText, true);
                }
            }
        }

        private T callAndLog<T>(Func<T> f, string startText, string finishText)
        {
            if (!string.IsNullOrEmpty(startText))
            {
                logMessage(startText, true);
            }
            try
            {
                return f();
            }
            catch (Exception e)
            {
                if (string.IsNullOrEmpty(startText))
                {
                    logMessage(startText, true);
                }

                logMessage(string.Format("Call failed with error: {0}", e.Message), true);

                if (!string.IsNullOrEmpty(finishText))
                {
                    logMessage(finishText, true);
                }

                throw new System.ServiceModel.FaultException(e.Message);
            }
            finally
            {
                if (!string.IsNullOrEmpty(finishText))
                {
                    logMessage(finishText, true);
                }
            }
        }
    }
}
