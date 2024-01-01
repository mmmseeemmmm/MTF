using ALBusServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;

namespace ALBusComDriver
{
    //HKEY_CURRENT_USER\Software\Microsoft\Windows\Windows Error Reporting DontShowUI =1
    public class BusCommunicationDriver : IDisposable
    {
        public enum StatusEnum { Configured, Initialized, Connected, Released, NotDefined }
        private Process alBusServerProcess;
        private IBusCommunicationWCF remoteBusCommunication = null;
        private bool isInitialized = false;
        private bool isStarted = false;
        private List<OnBoardConfig> onBoards = null;
        private OffBoardConfig offBoard = null;
        private OffBoardConfigEdiabas offBoardEdiabas = null;
        private LoggingInfo loggingInfo = null;
        private readonly string basePath = string.Empty;

        public BusCommunicationDriver() : this(string.Empty)
        { }

        public BusCommunicationDriver(string basePath) : this(basePath, true)
        { }

        public BusCommunicationDriver(string basePath, bool startServer)
        {
            this.basePath = string.IsNullOrEmpty(basePath) ? Environment.CurrentDirectory : basePath;
            if (startServer)
            {
                var alBusServers = Process.GetProcesses().Where(p => p.ProcessName.StartsWith("ALBusServer"));
                foreach (var server in alBusServers)
                {
                    logMessage("Kill old process PID=" + server.Id, true);
                    server.Kill();
                }
                startProcess();
            }

            connectToWCF();
        }

        private void startProcess()
        {
            var fileName = Path.Combine(basePath, "ALServer", "ALBusServer.exe");
            if (!File.Exists(fileName))
            {
                logMessage(string.Format("Init failed, file {0} not found. Check ALBusServer.exe location, or use constructor with base path specification.", fileName), true);
                throw new Exception(string.Format("File {0} not found. Check ALBusServer.exe location, or use constructor with base path specification.", fileName));
            }
            callAndLog(() =>
            {
                alBusServerProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = fileName,
                    WorkingDirectory = Path.Combine(basePath, "ALServer"),
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                });
                alBusServerProcess.EnableRaisingEvents = true;
            }
            , "Starting server application", "Server application started", false);

            connectToWCF();
        }

        private void connectToWCF()
        {
            callAndLog(() => { repeateCall(connectToServer, 4); }, "Connecting to server", "Server connected", false);
        }

        private void restart()
        {
            logMessage("------------ ALBusServer.exe crashed - restart process", true);
            if (alBusServerProcess != null && !alBusServerProcess.HasExited)
            {
                alBusServerProcess.Kill();
            }
            startProcess();

            connectToWCF();

            reinitInternal();
        }

        private void reinitInternal()
        {
            if (loggingInfo != null)
            {
                logMessage("Reinit logging", true);
                //SetLogging(loggingInfo.IsEnabled, loggingInfo.LogPath, loggingInfo.MaxLogFilesCount);
            }
            if (onBoards != null)
            {
                logMessage("Reinit onBoard", true);
                OnBoards = onBoards;
            }
            if (offBoard != null)
            {
                logMessage("Reint offBoard", true);
                OffBoard = offBoard;
            }
            if (offBoardEdiabas != null)
            {
                logMessage("Reinti offBoardEdiabas", true);
                OffBoardEdiabas = offBoardEdiabas;
            }

            if (isInitialized)
            {
                logMessage("Call Initialize", true);
                Initialize();

                if (isStarted)
                {
                    logMessage("Call start", true);
                    Start();
                }
            }
            logMessage("----------------------------------------- Reinit done", true);
        }

        private StatusEnum status;

        private void callAndLog(Action f)
        {
            callAndLog(f, string.Empty, string.Empty, false);
        }

        private void callAndLog(Action f, string startText)
        {
            callAndLog(f, startText, string.Empty, false);
        }
        
        private void callAndLog(Action f, string startText, string finishText)
        {
            callAndLog(f, startText, finishText, false);
        }

        private void callAndLog(Action f, string startText, string finishText, bool logOnlyError)
        {
            if (!logOnlyError && !string.IsNullOrEmpty(startText))
            {
                logMessage(startText, true);
            }
            try
            {
                try
                {
                    f();
                }
                catch (FaultException e)
                {
                    throw new Exception(e.Message);
                }
                catch(CommunicationException)
                {
                    restart();
                    f();
                }
            }
            catch (Exception e)
            {
                if (logOnlyError && !string.IsNullOrEmpty(startText))
                {
                    logMessage(startText, true);
                }

                logMessage(string.Format("Call failed with error: {0}", e.Message), true);
                
                if (logOnlyError && !string.IsNullOrEmpty(finishText))
                {
                    logMessage(finishText, true);
                }
                throw e;
            }
            finally
            {
                if (!logOnlyError && !string.IsNullOrEmpty(finishText))
                {
                    logMessage(finishText, true);
                }
            }
        }



        private T callAndLog<T>(Func<T> f)
        {
            return callAndLog(f, string.Empty, string.Empty, true);
        }

        private T callAndLog<T>(Func<T> f, string startText)
        {
            return callAndLog(f, startText, string.Empty, true);
        }

        private T callAndLog<T>(Func<T> f, string startText, string finishText)
        {
            return callAndLog(f, startText, finishText, true);
        }

        private T callAndLog<T>(Func<T> f, string startText, string finishText, bool logOnlyError)
        {
            if (!logOnlyError && !string.IsNullOrEmpty(startText))
            {
                logMessage(startText, true);
            }
            try
            {
                try
                {
                    return f();
                }
                catch (FaultException e)
                {
                    throw new Exception(e.Message);
                }
                catch (CommunicationException)
                {
                    restart();
                    return f();
                }
            }
            catch (Exception e)
            {
                if (logOnlyError && !string.IsNullOrEmpty(startText))
                {
                    logMessage(startText, true);
                }

                logMessage(string.Format("Call failed with error: {0}", e.Message), true);

                if (logOnlyError && !string.IsNullOrEmpty(finishText))
                {
                    logMessage(finishText, true);
                }
                throw;
            }
            finally
            {
                if (!logOnlyError && !string.IsNullOrEmpty(finishText))
                {
                    logMessage(finishText, true);
                }
            }
        }

        private void repeateCall(Action f, int repeatCount)
        {
            try
            {
                f();
            }
            catch(Exception e)
            {
                if (repeatCount > 0)
                {
                    repeateCall(f, repeatCount--);
                }
                else
                {
                    throw;
                }
            }
        }

        private void connectToServer()
        {
            var myBinding = new NetTcpBinding
            {
                ReceiveTimeout = new TimeSpan(0, 20, 0),
                SendTimeout = new TimeSpan(0, 20, 0),
                MaxReceivedMessageSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                Security = {Mode = SecurityMode.None}
            };

            var myEndpoint = new EndpointAddress("net.tcp://localhost:3553/BusCommunication");
            var myChannelFactory = new ChannelFactory<IBusCommunicationWCF>(myBinding, myEndpoint);// DuplexChannelFactory<IBusCommunicationWCF>( (callbackHandler, myBinding, myEndpoint);

            remoteBusCommunication = myChannelFactory.CreateChannel();
        }

        public void Dispose()
        {
            if (remoteBusCommunication != null)
            {
                callAndLog(() => remoteBusCommunication.Dispose(), "Call Dispose", "Dispose finished");
                remoteBusCommunication = null;
                alBusServerProcess.Kill();
            }
        }

        public void Initialize()
        {
            callAndLog(() => remoteBusCommunication.Initialize(), "Call Initialize", "Initialize finished");

            status = StatusEnum.Initialized;
            isInitialized = true;
        }

        public void ForceReinitialize()
        {
            logMessage("Force reinitialization", true);
            if (alBusServerProcess != null && !alBusServerProcess.HasExited && remoteBusCommunication != null)
            {
                try
                {
                    remoteBusCommunication.Dispose();
                }
                catch
                {
                    logMessage("Dispose of remote object crashed -> AL Server will be restarted", true);
                }
            }

            if (alBusServerProcess != null && !alBusServerProcess.HasExited)
            {
                alBusServerProcess.Kill();
            }
            logMessage("Force reinitialization - start ALServer", true);
            startProcess();
            connectToWCF();
            logMessage("Force reinitialization - reinit by previous settings", true);
            reinitInternal();
            logMessage("Force reinitialization DONE", true);
        }

        //public void LogMessage(string message, bool logTimeStamp)
        //{
        //    callAndLog(() => remoteBusCommunication.LogMessage(message, logTimeStamp), "Call logMessage", "logMessage finished");
        //}

        private void logMessage(string message, bool logTimeStamp)
        {
            Logging.LogMessage(message, Logging.LogFilePrefixBusComClient, logTimeStamp);
        }

        public List<string> OnBoardGetEcus(string netConfigFile)
        {
            return callAndLog(() => remoteBusCommunication.OnBoardGetEcus(netConfigFile), "Call GetEcus", "GetEcus finished");
        }

        public List<string> OnBoardGetInputSignals(string networkCfgFile, string ecus)
        {
            return callAndLog(() => remoteBusCommunication.OnBoardGetInputSignals(networkCfgFile, ecus), "Call get input signal", "Get input signal finished");
        }

        public List<string> OnBoardGetOutputSignals(string networkCfgFile, string ecus)
        {
            return callAndLog(() => remoteBusCommunication.OnBoardGetOutputSignals(networkCfgFile, ecus), "Call get output signal", "Get output signal finished");
        }

        public OffBoardConfig OffBoard {
            set
            {
                offBoard = value;
                callAndLog(() => remoteBusCommunication.SetOffBoard(value), "Call SetOffBoard : " + value.TextValue, "SetOffBoard finished");
                status = StatusEnum.Configured;
            }
        }

        public OffBoardConfigEdiabas OffBoardEdiabas {
            set 
            {
                offBoardEdiabas = value;
                callAndLog(() => remoteBusCommunication.SetOffBoardEdiabas(value), "Call SetOffBoardEdiabas", "SetOffBoardEdiabas finished");
                status = StatusEnum.Configured;
            } 
        }

        public List<OffBoardLogicalLinkParallelServices> OffBoardExecuteServicesInParallel(List<OffBoardLogicalLinkParallelServices> offBoardParallelServicesSetting)
        {
            return callAndLog(() => remoteBusCommunication.OffBoardExecuteServicesInParallel(offBoardParallelServicesSetting), "Call OffBoardExecuteServicesInParallel", "OffBoardExecuteService finished");
        }

        public OffBoardResponse OffBoardExecuteService(string logicalLinkName, OffBoardService serviceSetting)
        {
            return callAndLog(() => remoteBusCommunication.OffBoardExecuteService(logicalLinkName, serviceSetting), "Call OffBoardExecuteService", "OffBoardExecuteService finished");
        }

        public void OffBoardExecuteServiceCyclically(string logicalLinkName, OffBoardService serviceSetting, int cycleTime)
        {
            callAndLog(() => remoteBusCommunication.OffBoardExecuteServiceCyclically(logicalLinkName, serviceSetting, cycleTime), "Call OffBoardExecuteServiceCyclically", "OffBoardExecuteServiceCyclically finished");
        }

        public void StopOffBoardExecuteServiceCyclically()
        {
            callAndLog(() => remoteBusCommunication.StopOffBoardExecuteServiceCyclically(), "Call StopOffBoardExecuteServiceCyclically", "StopOffBoardExecuteServiceCyclically finished");
        }

        public List<OffBoardFlashJobResult> OffBoardFlashJob(List<OffBoardFlashJobSetting> flashJobSetttings)
        {
            return callAndLog(() => remoteBusCommunication.OffBoardFlashJob(flashJobSetttings), "Call OffBoardFlashJob", "OffBoardFlashJob finished");
        }

        public string OffBoardVariantIdentificationAndSelection(string logicalLinkName)
        {
            return callAndLog(() => remoteBusCommunication.OffBoardVariantIdentificationAndSelection(logicalLinkName), "Call OffBoardVariantIdentificationAndSelection", "OffBoardVariantIdentificationAndSelection finished");
        }

        public void OnBoardActiveScheduleTable(string deviceName, string busType, string channel, string scheduleTable)
        {
            callAndLog(() => remoteBusCommunication.OnBoardActiveScheduleTable(deviceName, busType, channel, scheduleTable), "Call OnBoardActiveScheduleTable", "OnBoardActiveScheduleTable finished");
        }

        public void OnBoardActiveScheduleTable(string scheduleTable)
        {
            callAndLog(() => remoteBusCommunication.OnBoardActiveScheduleTableByName(scheduleTable), "Call OnBoardActiveScheduleTable by name", "OnBoardActiveScheduleTable finished");
        }

        public string OnBoardGetGlobalVariable(string deviceName, string busType, string channel, string variableName)
        {
            return callAndLog(() => remoteBusCommunication.OnBoardGetGlobalVariable(deviceName, busType, channel, variableName), "Call OnBoardGetGlobalVariable", "OnBoardGetGlobalVariable finished");
        }

        public string OnBoardGetGlobalVariable(string variableName)
        {
            return callAndLog(() => remoteBusCommunication.OnBoardGetGlobalVariableByName(variableName), "Call OnBoardGetGlobalVariable by name", "OnBoardGetGlobalVariable finished");
        }

        public byte[] OnBoardGetGlobalVariableRaw(string deviceName, string busType, string channel, string variableName)
        {
            return callAndLog(() => remoteBusCommunication.OnBoardGetGlobalVariableRaw(deviceName, busType, channel, variableName), "Call OnBoardGetGlobalVariable", "OnBoardGetGlobalVariable finished");
        }

        public byte[] OnBoardGetGlobalVariableRaw(string variableName)
        {
            return callAndLog(() => remoteBusCommunication.OnBoardGetGlobalVariableByNameRaw(variableName), "Call OnBoardGetGlobalVariable by name", "OnBoardGetGlobalVariable finished");
        }

        public OnBoardSignal OnBoardGetSignal(string deviceName, string busType, string channel, string frameName, string signalName)
        {
            return callAndLog(() => remoteBusCommunication.OnBoardGetSignal(deviceName, busType, channel, frameName, signalName), "Call OnBoardGetSignal", "OnBoardGetSignal finished");
        }

        public OnBoardSignal OnBoardGetSignal(string frameName, string signalName)
        {
            return callAndLog(() => remoteBusCommunication.OnBoardGetSignal2(frameName, signalName), "Call OnBoardGetSigna2", "OnBoardGetSignal2 finished");
        }

        public byte[] OnBoardReceiveFrameOnce(string deviceName, string busType, string channel, uint frameID, uint resFrameID, byte[] data, uint timeout)
        {
            return callAndLog(() => remoteBusCommunication.OnBoardReceiveFrameOnce(deviceName, busType, channel, frameID, resFrameID, data, timeout), "Call OnBoardReceiveFrameOnce", "OnBoardReceiveFrameOnce finished");
        }

        public byte[] OnBoardReceiveFrameOnce(uint frameID, uint resFrameID, byte[] data, uint timeout)
        {
            return callAndLog(() => remoteBusCommunication.OnBoardReceiveFrameOnce2(frameID, resFrameID, data, timeout), "Call OnBoardReceiveFrameOnce", "OnBoardReceiveFrameOnce finished");
        }

        public List<OnBoardConfig> OnBoards 
        {
            get { return callAndLog(() => remoteBusCommunication.GetOnBoards(), "Call GetOnBoards", "GetOnBoards finished"); }
            set 
            {
                onBoards = value;
                callAndLog(() => remoteBusCommunication.SetOnBoards(value), "Call SetOnBoards : " + string.Join(Environment.NewLine, value.Select(ob => ob.TextValue)), "SetOnBoards finished");
                status = StatusEnum.Configured;
            }
        }

        public void OnBoardSendFrameOnce(string deviceName, string busType, string channel, uint frameID, byte[] data)
        {
            callAndLog(() => remoteBusCommunication.OnBoardSendFrameOnce(deviceName, busType, channel, frameID, data), "Call OnBoardSendFrameOnce", "OnBoardSendFrameOnce finished");
        }
        
        public void OnBoardSendFrameOnce(uint frameID, byte[] data)
        {
            callAndLog(() => remoteBusCommunication.OnBoardSendFrameOnce2(frameID, data), "Call OnBoardSendFrameOnce2", "OnBoardSendFrameOnce2 finished");
        }

        public void OnBoardSetGlobalVariable(string deviceName, string busType, string channel, string variableName, string variableValue)
        {
            callAndLog(() => remoteBusCommunication.OnBoardSetGlobalVariable(deviceName, busType, channel, variableName, variableValue), "Call OnBoardSetGlobalVariable", "OnBoardSetGlobalVariable finished");
        }

        public void OnBoardSetGlobalVariable(string variableName, string variableValue)
        {
            callAndLog(() => remoteBusCommunication.OnBoardSetGlobalVariable2(variableName, variableValue), "Call OnBoardSetGlobalVariable2", "OnBoardSetGlobalVariable2 finished");
        }

        public void OnBoardSetSignal(string deviceName, string busType, string channel, string frameName, string signalName, string signalValue)
        {
            callAndLog(() => remoteBusCommunication.OnBoardSetSignal(deviceName, busType, channel, frameName, signalName, signalValue), "Call OnBoardSetSignal", "OnBoardSetSignal finished");
        }

        public void OnBoardSetSignal(string frameName, string signalName, string signalValue)
        {
            callAndLog(() => remoteBusCommunication.OnBoardSetSignal2(frameName, signalName, signalValue), "Call OnBoardSetSignal2", "OnBoardSetSignal2 finished");
        }

        public void OnBoardsCheckRunning()
        {
            callAndLog(() => remoteBusCommunication.OnBoardsCheckRunning(), "Call OnBoardsCheckRunning", "OnBoardsCheckRunning finished");
        }

        //public void SetLogging(bool enable, string logPath, int maxLogFilesCount)
        //{
        //    callAndLog(() => remoteBusCommunication.SetLogging(enable, logPath, maxLogFilesCount), "Call SetLogging", "SetLogging finished");
        //    loggingInfo = new LoggingInfo { IsEnabled = enable, LogPath = logPath, MaxLogFilesCount = maxLogFilesCount };
        //}

        public void OnBoardStart()
        {
            callAndLog(() => remoteBusCommunication.OnBoardStart(), "Call OnBoardStart", "OnBoardStart finished");
            status = StatusEnum.Connected;
            isStarted = true;           
        }

        public void OnBoardStop()
        {
            callAndLog(() => remoteBusCommunication.OnBoardStop(), "Call OnBoardStop", "OnBoardStop finished");
            status = StatusEnum.Connected;
            isStarted = true;
        }

        public void OffBoardStart()
        {
            callAndLog(() => remoteBusCommunication.OffBoardStart(), "Call OffBoardStart", "OnBoardStart finished");
            status = StatusEnum.Connected;
            isStarted = true;           
        }

        public void OffBoardStop()
        {
            callAndLog(() => remoteBusCommunication.OffBoardStop(), "Call OffBoardStop", "OnBoardStop finished");
            status = StatusEnum.Connected;
            isStarted = true;
        }

        public void Start()
        {
            callAndLog(() => remoteBusCommunication.Start(), "Call Start", "Start finished");
            status = StatusEnum.Released;
            isStarted = false;
        }

        public StatusEnum Status 
        {
            get { 
                //var x = callAndLog<StatusEnumInternal>(() => remoteBusCommunication.GetStatus(), "Call GetStatus", "GetStatus finished");
                //return (StatusEnum)Enum.Parse(typeof(StatusEnum), x.ToString());

                return status;
            } 
        }
        
        public void Stop()
        {
            callAndLog(() => remoteBusCommunication.Stop(), "Call Stop", "Stop finished");
            status = StatusEnum.Released;
            isStarted = false;
        }

        public OffBoardResponse OffBoardExecuteOTX(OffBoardService serviceSetting)
        {
            return callAndLog(() => remoteBusCommunication.OffBoardExecuteOTX(serviceSetting), "Call OffBoardExecuteOTX", "OffBoardExecuteOTX finished");
        }

        public void OnBoardSendCANFDFrameCyclic(string deviceName, string busType, string channel, uint frameId, string data, uint timeOutFrame)
        {
            callAndLog(() => remoteBusCommunication.OnBoardSendCANFDFrameCyclic(deviceName, busType, channel, frameId, data, timeOutFrame), "Call SendCANFDFrameCyclic", "SendCANFDFrameCyclic finished");
        }

        public void OnBoardSendCANFDFrameCyclic(string deviceName, string busType, string channel, uint frameId, IEnumerable<string> data, uint timeOutFrame, uint timeOutSubList)
        {
            callAndLog(() => remoteBusCommunication.OnBoardSendCANFDFrameCyclic1(deviceName, busType, channel, frameId, data, timeOutFrame, timeOutSubList), "Call SendCANFDFrameCyclic", "SendCANFDFrameCyclic finished");
        }

        public void OnBoardSendCANFDFrameCyclic(string deviceName, string busType, string channel, Dictionary<uint, IEnumerable<string>> frameData, uint timeOutFrame, uint timeOutSubList)
        {
            callAndLog(() => remoteBusCommunication.OnBoardSendCANFDFrameCyclic2(deviceName, busType, channel, frameData, timeOutFrame, timeOutSubList), "Call SendCANFDFrameCyclic", "SendCANFDFrameCyclic finished");
        }

        public void OnBoardSendFrameCyclic(string deviceName, string busType, string channel, uint frameId, string data, uint timeOutFrame)
        {
            callAndLog(() => remoteBusCommunication.OnBoardSendFrameCyclic(deviceName, busType, channel, frameId, data, timeOutFrame), "Call SendFrameCyclic", "SendFrameCyclic finished");
        }

        public void OnBoardSendFrameCyclic(string deviceName, string busType, string channel, uint frameId, IEnumerable<string> data, uint timeOutFrame, uint timeOutSubList)
        {
            callAndLog(() => remoteBusCommunication.OnBoardSendFrameCyclic1(deviceName, busType, channel, frameId, data, timeOutFrame, timeOutSubList), "Call SendFrameCyclic", "SendFrameCyclic finished");
        }

        public void OnBoardSendFrameCyclic(string deviceName, string busType, string channel, Dictionary<uint, IEnumerable<string>> frameData, uint timeOutFrame, uint timeOutSubList)
        {
            callAndLog(() => remoteBusCommunication.OnBoardSendFrameCyclic2(deviceName, busType, channel, frameData, timeOutFrame, timeOutSubList), "Call SendFrameCyclic", "SendFrameCyclic finished");
        }

        public void SetGlobalAdapterProjectVariable(string deviceName, string busType, string channel, string variableName, string value)
        {
            callAndLog(() => remoteBusCommunication.SetGlobalAdapterProjectVariable(deviceName, busType, channel, variableName, value), "Call SetGlobalAdapterProjectVariable", "SetGlobalAdapterProjectVariable finished");
        }

        public string GetGlobalAdapterProjectVariable(string deviceName, string busType, string channel, string variableName)
        {
            return callAndLog(() => remoteBusCommunication.GetGlobalAdapterProjectVariable(deviceName, busType, channel, variableName), "Call GetGlobalAdapterProjectVariable", "GetGlobalAdapterProjectVariable finished");
        }
    }

    public static class LogObject
    {
        public static void LogMessage(string message)
        {
            if (logMethodThroughMTF == null)
            {
                LogToFile(message);
            }
            else
            {
                logMethodThroughMTF(message);
            }

        }

        private static void LogToFile(string message)
        {
        }

        private static Action<string> logMethodThroughMTF;

        public static void SetLogMethod(Action<string> logMethod)
        {
            logMethodThroughMTF = logMethod;
        }
    }

    class LoggingInfo
    {
        public bool IsEnabled { get; set; }
        public string LogPath { get; set; }
        public int MaxLogFilesCount { get; set; }
    }
}

