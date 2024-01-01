using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AUTOMATIONAPILib;
using ALRestbusVCF;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;
using System.IO;
using ALBusComDriver;
using ALBusServer;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ALBusComDriverServer
{
    public class BusCommunicationDriver : IDisposable
    {
        private StatusEnumInternal status = StatusEnumInternal.NotDefined;
        private dtsPcSystem dtsSystem;
        private dtsPcAPI dtsApi;
        private List<LogicalLink> dtsLogLinks;
        private List<VCFObj> vcfObjs;
        private OffBoardConfig offBoard = new OffBoardConfig();
        private OffBoardConfigEdiabas offBoardEdiabas = new OffBoardConfigEdiabas();
        private List<OnBoardConfig> onBoards = new List<OnBoardConfig>();
        private ALBusComDriver.Ediabas ediabas;
        private List<CyclicService> cyclicServices = new List<CyclicService>();
        private object communicationLock = new object();

        public BusCommunicationDriver()
        {
        }

        public OffBoardConfig OffBoard
        {
            set
            {
                offBoard = value;
                status = StatusEnumInternal.Configured;
            }
        }

        public OffBoardConfigEdiabas OffBoardEdiabas
        {
            set
            {
                offBoardEdiabas = value;
                status = StatusEnumInternal.Configured;
            }
        }

        public List<OnBoardConfig> OnBoards
        {
            set
            {
                //onBoards = value;
                foreach (var onBoardCfg in value)
                {
                    var existingCfg = onBoards.FirstOrDefault(i => i.DeviceName == onBoardCfg.DeviceName && i.BusType == onBoardCfg.BusType && i.BusChannel == onBoardCfg.BusChannel);
                    if (existingCfg == null) //add new
                    {
                        onBoards.Add(onBoardCfg);
                    }
                    else//reconfigure existing
                    {
                        AssignConfiguration(existingCfg, onBoardCfg);
                    }
                }
                status = StatusEnumInternal.Configured;
            }
            get
            {
                return onBoards;
            }
        }

        public StatusEnumInternal Status
        {
            get
            {
                return status;
            }
        }

        ~BusCommunicationDriver()
        {
            Dispose();
        }

        public void Dispose()
        {
            LogMessage("Dispose", true);

            try
            {

                StopOffBoardExecuteServiceCyclically();
                DTSClose();
                VCFClose();
                if (ediabas != null)
                {
                    ediabas.Close();
                    ediabas = null;
                }
            }
            catch (Exception e)
            {
                LogMessage("Dispose crahed with error: " + e.Message, true);
            }
            //LogMessage("Dispose end", true);
            GC.SuppressFinalize(this);
        }

        //Init VCF, INIT DTS, Start VCF communication
        public void Initialize()
        {
            try
            {
                if (status == StatusEnumInternal.NotDefined)
                    throw new Exception("CommunicationDriver is not configured, Initialize()");

                Logging.SortFolder();
                DTSClose();
                //VCFClose();

                try
                {
                    VCFInit();
                }
                catch
                {
                    VCFInit();
                }
                DTSInit();
                InitializeEdiabas();
                //?
                //VCFStartCommunication();

                status = StatusEnumInternal.Initialized;
            }
            catch (Exception ex)
            {
                CheckError(ex, "Initialize()", false);
            }
        }

        //public void SetLogging(bool enable, string logPath, int maxLogFilesCount)
        //{
        //    logging.SetLogging(enable, logPath, maxLogFilesCount);
        //}

        private void InitializeEdiabas()
        {
            if (!string.IsNullOrEmpty(offBoardEdiabas.SerialNumber) && !string.IsNullOrEmpty(offBoardEdiabas.BusChannel) && !string.IsNullOrEmpty(offBoardEdiabas.SgbdFolder))
            {
                ediabas = new ALBusComDriver.Ediabas(offBoardEdiabas.SerialNumber, offBoardEdiabas.BusChannel, offBoardEdiabas.SgbdFolder);
            }
        }

        public void OnBoardStart()
        {
            foreach (var vcfObj in vcfObjs)
            {
                if (!vcfObj.ComObj.IsBusCommunication())
                {
                    vcfObj.ComObj.Start();
                }
            }
        }

        public void OnBoardStop()
        {
            foreach (var vcfObj in vcfObjs)
            {
                vcfObj.ComObj.Stop();
            }
        }

        public void OffBoardStart()
        {
            try
            {
                if (status == StatusEnumInternal.Connected)
                {
                    LogMessage("Connected -> start ignored.", true);
                    return;
                }
                LogMessage("Start()", true);
                if (status != StatusEnumInternal.Initialized && status != StatusEnumInternal.Released && status != StatusEnumInternal.Configured)
                    throw new Exception("CommunicationDriver must be Initialized and not in Headlamp Connected Mode, HeadlampsBeenConnected()");
                DTSOpenLogicalLinks();
                DTSStartCommunication();
                status = StatusEnumInternal.Connected;
            }
            catch (Exception ex)
            {
                CheckError(ex, "Start()", false);
            }
        }

        public void OffBoardStop()
        {
            try
            {
                LogMessage("Stop()", true);
                //if (status != StatusEnumInternal.Connected)
                //    throw new Exception("CommunicationDriver must be Connected for Release Function, HeadlampsBeReleased()");
                DTSCloseLogicalLinks();
                status = StatusEnumInternal.Released;
                LogMessage("Stop() end", true);
            }
            catch (Exception ex)
            {
                CheckError(ex, "Stop()", false);
            }
        }

        //Open LLs, Start Diagnostic communication
        public void Start()
        {
            try
            {
                if (status == StatusEnumInternal.Connected)
                {
                    LogMessage("Connected -> start ignored.", true);
                    return;
                }
                LogMessage("Start()", true);
                if (status != StatusEnumInternal.Initialized && status != StatusEnumInternal.Released && status != StatusEnumInternal.Configured)
                    throw new Exception("CommunicationDriver must be Initialized and not in Headlamp Connected Mode, HeadlampsBeenConnected()");
                DTSOpenLogicalLinks();
                DTSStartCommunication();
                status = StatusEnumInternal.Connected;
                
            }
            catch (Exception ex)
            {
                CheckError(ex, "Start()", false);
            }
        }

        //Stop Diagnostic communication, close LLs
        public void Stop()
        {
            try
            {
                LogMessage("Stop()", true);
                //if (status != StatusEnumInternal.Connected)
                //    throw new Exception("CommunicationDriver must be Connected for Release Function, HeadlampsBeReleased()");
                DTSCloseLogicalLinks();
                status = StatusEnumInternal.Released;
                LogMessage("Stop() end", true);
            }
            catch (Exception ex)
            {
                CheckError(ex, "Stop()", false);
            }
        }

        public OffBoardResponse OffBoardExecuteOTX(OffBoardService serviceSetting)
        {
            OffBoardResponse serviceResult = new OffBoardResponse();

             string otxScriptOrProject = serviceSetting.ServiceName;
            try
            {
                if (dtsSystem == null)
                {
                    throw new Exception("DTS System was not initialized");
                }

                LogMessage(string.Format("OffBoardExecuteOTX({0})", otxScriptOrProject), true);

                dtsPcRoutineUC routineUc = dtsSystem.createOtx(otxScriptOrProject);

                SetRequestParameters(routineUc, serviceSetting);

                routineUc.execute();
                routineUc.waitUntilFinished(0);

                GetResponses(routineUc, serviceSetting, serviceResult);

                routineUc = null;
            }
            catch (Exception ex)
            {
                CheckError(ex, string.Format("OffBoardExecuteOTX({0})", otxScriptOrProject), false);
            }

            return serviceResult;
        }

        #region private DTS
        private void DTSInit()
        {
            if (offBoard == null || offBoard.ProjectName == string.Empty || offBoard.VIT == string.Empty)
                return;

            try
            {
                //Open DTS Project
                dtsApi = new dtsPcAPI();
                if (dtsApi == null)
                    throw new Exception("AUTOMATIONAPILib.dtsPcAPI-object not created, DTSInit()");
                dtsApi.setLogFileSize(5000000);
                dtsApi.setTraceLevel(type_dtsPcErrorLevel.ELEVEL_ERROR);

                dtsSystem = dtsApi.dtsInit_UC(offBoard.ProjectName, offBoard.VIT);
                if (dtsSystem == null)
                    throw new Exception("Initialization of DTS failed, DTSInit()");

                //Start ByteTrace
                if (offBoard.EnableByteTrace)
                {
                    dtsSystem.setByteTraceInterface(offBoard.ByteTraceInterfaceName);
                    dtsSystem.setByteTraceFileLimit(10000000);
                    dtsSystem.startByteTrace("OffBoardByteTrace");
                }

            }
            catch (Exception ex)
            {
                try
                {
                    CheckError(ex, "DTSInit() - " + offBoard.ProjectName + ", " + offBoard.VIT, true);
                }
                finally
                {
                    FreeDTSObj();
                }
            }
        }

        private void DTSOpenLogicalLinks()
        {
            if (offBoard == null || offBoard.LogicalLinks == null)
                return;
            try
            {
                dtsLogLinks = new List<LogicalLink>();
                for (int i = 0; i < offBoard.LogicalLinks.Count; i++)
                {

                    LogicalLink ll = new LogicalLink();
                    ll.Name = offBoard.LogicalLinks[i];
                    LogMessage("DTSOpenLogicalLinks - try" + ll.Name, true);
                    ll.Value = dtsSystem.loadAccessPath(ll.Name);
                    dtsLogLinks.Add(ll);

                    LogMessage("DTSOpenLogicalLinks - done " + ll.Name, true);
                }
            }
            catch (Exception ex)
            {
                CheckError(ex, "DTSOpenLogicalLinks()", true);
            }
        }

        private void DTSClose()
        {
            try
            {
                DTSCloseLogicalLinks();
            }
            catch (Exception e)
            {
                LogMessage("DTSClose -> DTSCloseLogicalLinks error: " + e.ToString(), true);
            }

            if (offBoard.EnableByteTrace && dtsSystem != null)
            {
                try
                {
                    LogMessage("DTSClose -> dtsSystem.stopByteTrace", true);
                    dtsSystem.stopByteTrace();
                    LogMessage("DTSClose -> dtsSystem.stopByteTrace DONE", true);
                }
                catch (Exception e)
                {
                    LogMessage("DTSClose -> dtsSystem.stopByteTrace error: " + e.ToString(), true);
                }
            }

            try
            {
                if (dtsSystem != null)
                {
                    LogMessage("DTSClose -> dtsApi.dtsEnd", true);
                    dtsApi?.dtsEnd(dtsSystem);
                    LogMessage("DTSClose -> dtsApi.dtsEnd DONE", true);
                }
            }
            catch (Exception e)
            {
                LogMessage("DTSClose -> dtsApi.dtsEnd error: " + e.ToString(), true);
            }

            FreeDTSObj();
        }

        private void FreeDTSObj()
        {
            try
            {
                if (dtsApi != null)
                {

                    LogMessage("DTSClose -> Marshal.ReleaseComObject(dtsApi)", true);
                    Marshal.ReleaseComObject(dtsApi);
                    LogMessage("DTSClose -> Marshal.ReleaseComObject(dtsApi) DONE", true);
                }
            }
            catch (Exception e)
            {
                LogMessage("DTSClose -> Marshal.ReleaseComObject(dtsApi) error: " + e.ToString(), true);
            }

            try
            {
                if (dtsSystem != null)
                {
                    LogMessage("DTSClose -> Marshal.ReleaseComObject(dtsSystem)", true);
                    Marshal.ReleaseComObject(dtsSystem);
                    LogMessage("DTSClose -> Marshal.ReleaseComObject(dtsSystem) DONE", true);
                }
            }
            catch (Exception e)
            {
                LogMessage("DTSClose ->  Marshal.ReleaseComObject(dtsSystem) error: " + e.ToString(), true);
            }
        }

        private void DTSCloseLogicalLinks()
        {
            LogMessage("DTSCloseLogicalLinks()", true);
            if (dtsLogLinks == null)
            {
                LogMessage("DTSCloseLogicalLinks() end - No logical links to close", true);
                return;
            }

            for (int i = 0; i < dtsLogLinks.Count; i++)
            {
                try
                {
                    if (dtsLogLinks[i] != null)
                    {
                        LogMessage("stopCommunication() " + dtsLogLinks[i].Name, true);
                        dtsLogLinks[i].Value.stopCommunication();
                        LogMessage("closeLogicalLink() " + dtsLogLinks[i].Name, true);
                        dtsLogLinks[i].Value.closeLogicalLink();
                    }
                    dtsSystem.releaseAccessPath(dtsLogLinks[i].Value);
                    Marshal.ReleaseComObject(dtsLogLinks[i].Value);
                    LogMessage("ReleaseComObject done " + dtsLogLinks[i].Name, true);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            dtsLogLinks = null;
            LogMessage("DTSCloseLogicalLinks() end", true);
        }

        private void DTSStartCommunication()
        {
            if (dtsLogLinks == null)
            {
                return;
            }

            List<Task> tasks = new List<Task>();
            StringBuilder exceptionMessages = new StringBuilder();

            foreach (var dtsLogLink in dtsLogLinks)            
            {
                var localDtsLogLink = dtsLogLink;

                Task newTask = new Task(() =>
                {
                    bool done = false;
                    int counter = 0;

                LogMessage("DTSStartCommunication - try" + dtsLogLink.Name, true);
                while (!done)
                {
                    try
                    {
                        counter++;
                        dtsLogLink.Value.startCommunication();

                            done = true;
                        }
                        catch (Exception ex)
                        {
                            if (counter > 3)
                            {
                                try
                                {
                                    CheckError(ex, "DTSStartCommunication()", true);
                                }
                                catch (Exception innerEx)
                                {
                                    exceptionMessages.AppendLine(innerEx.Message);
                                }
                            }
                        }
                    }

                LogMessage("DTSStartCommunication - done" + localDtsLogLink.Name, true);                    
                });

                tasks.Add(newTask);
                newTask.Start();
                System.Threading.Thread.Sleep(16);
            }
                        
            Task.WaitAll(tasks.ToArray(), 10000);

            if (exceptionMessages.Length > 0)
            {
                throw new Exception(exceptionMessages.ToString());
            }
        }

        private int GetLLIndex(string logicalLinkName)
        {
            int result = -1;
            if (dtsLogLinks == null || dtsLogLinks.Count == 0)
                throw new Exception("No OffBoard Logical Link been initialized!, GetLLIndex(string logicalLinkName)");
            string availableLL = string.Empty;
            for (int i = 0; i < dtsLogLinks.Count; i++)
            {
                if (availableLL != string.Empty)
                {
                    availableLL = availableLL + ", ";
                }
                availableLL = availableLL + dtsLogLinks[i].Name;

                if (dtsLogLinks[i].Name == logicalLinkName)
                    result = i;
            }
            if (result == -1)
                throw new Exception(logicalLinkName + " Is not configured, available only: " + availableLL + ", GetLLIndex(string logicalLinkName)");
            return result;
        }

        private string VariantIdentification(int counter, int linkIndex)
        {
            try
            {
                return dtsLogLinks[linkIndex].Value.variantIdentificationAndSelection();
            }
            catch (Exception ex)
            {
                //When exception occured more than 5 times, throw new exception.
                if (counter >= 5) throw new Exception("VariantIdentificationAndSelection failed", ex);
                Thread.Sleep(100);
                return VariantIdentification(counter + 1, linkIndex);
            }
        }

        internal OffBoardResponse DTSExecuteRoutine(dtsPcAccessPath ll, OffBoardService serviceSetting)
        {
            OffBoardResponse serviceResult = new OffBoardResponse();

            dtsPcRoutineUC routineUC = ll.getRoutineUC();
            routineUC.setService(serviceSetting.ServiceName);

            LogMessage("Service name: " + serviceSetting.ServiceName, false);

            SetRequestParameters(routineUC, serviceSetting);

            Execute(routineUC, serviceResult);

            GetResponses(routineUC, serviceSetting, serviceResult);

            #region CheckValueResponses

            foreach (OffBoardServiceResult result in serviceResult.Results)
            {
                if (result.DoCheckResponse)
                {
                    int foundCounter = 0;
                    double valDouble;
                    //int valInt;
                    if (result.Limit.Min != -999.99 && result.Limit.Max != 999.99)
                    {
                        foreach (string item in result.Values)
                        {
                            //if (int.TryParse(item, out valInt))
                            //{
                            //    if (result.Limit.Min <= valInt && valInt <= result.Limit.Max)
                            //    {
                            //        foundCounter++;
                            //    }
                            //}
                            if (double.TryParse(item, out valDouble))
                            {
                                if (result.Limit.Min <= valDouble && valDouble <= result.Limit.Max)
                                {
                                    foundCounter++;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (string item in result.Values)
                        {
                            if (result.Limit.Exactly.Contains(item))
                            {
                                foundCounter++;
                            }
                        }
                    }
                    if (foundCounter == result.Values.Count)
                    {
                        result.CheckResult = true;
                    }
                }
            }
            #endregion
            return serviceResult;

        }

        private void SetRequestParameters(dtsPcRoutineUC routineUC, OffBoardService serviceSetting)
        {
            LogMessage("Request parameters:", false);
            if (serviceSetting.RequestParameters != null)
            {
                foreach (OffBoardRequestParameter param in serviceSetting.RequestParameters)
                {
                    try
                    {
                        string toParse = param.Value;

                        switch (toParse.Substring(0, Math.Min(3, toParse.Length)).ToUpper())
                        {
                            case "\\%L":
                                if (toParse.Substring(3).ToLower().StartsWith("0x"))
                                {
                                    routineUC.setParamLong(param.Name, int.Parse(toParse.Substring(5), NumberStyles.AllowHexSpecifier));
                                }
                                else
                                {
                                    routineUC.setParamLong(param.Name, int.Parse(toParse.Substring(3)));
                                }
                                break;
                            case "\\%D":
                                routineUC.setParamDouble(param.Name, double.Parse(toParse.Substring(3).Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture));
                                break;
                            case "\\%S":
                                routineUC.setParamString(param.Name, toParse.Substring(3));
                                break;
                            case "\\%[":
                                Array byteArray = ConvertStringToByteArray(toParse.Substring(3));
                                routineUC.setParamByteString(param.Name, byteArray);
                                break;
                            default:
                                TryParsingInOrderLongDoubleString(routineUC, param);
                                break;
                        }

                        LogMessage(param.Name + ": " + param.Value, false);
                    }
                    catch
                    {
                        throw new Exception("RequestParameters values could not be parsed - supported are just numbers (long, double) string and byte arrays");
                    }
                }
            }
        }

        private void TryParsingInOrderLongDoubleString(dtsPcRoutineUC routineUC, OffBoardRequestParameter param)
        {
            int valInt = 0;
            double valDouble;
            string toParse = param.Value;

            if (toParse.ToLower().StartsWith("0x"))
            {
                try
                {
                    valInt = int.Parse(toParse.Substring(2), NumberStyles.AllowHexSpecifier);
                    routineUC.setParamLong(param.Name, valInt);
                }
                catch
                {
                    try
                    {
                        routineUC.setParamDouble(param.Name, valInt);
                    }
                    catch
                    {
                        routineUC.setParamString(param.Name, toParse);
                    }
                }
            }
            else
            {
                if (int.TryParse(toParse, out valInt))
                {
                    try
                    {
                        routineUC.setParamLong(param.Name, valInt);
                    }
                    catch
                    {
                        try
                        {
                            routineUC.setParamDouble(param.Name, double.Parse(toParse.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture));
                        }
                        catch
                        {
                            routineUC.setParamString(param.Name, toParse);
                        }
                    }
                }
                else if (double.TryParse(toParse.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out valDouble))
                {
                    try
                    {
                        routineUC.setParamDouble(param.Name, valDouble);
                    }
                    catch
                    {
                        routineUC.setParamString(param.Name, toParse);
                    }
                }
                else
                {
                    routineUC.setParamString(param.Name, toParse);
                }
            }

        }

        private Array ConvertStringToByteArray(string inputString)
        {
            string[] allStrings = inputString.Split(',');

            byte[] result = new byte[allStrings.Length];

            for (int i = 0; i < allStrings.Length; i++)
            {
                if (allStrings[i].ToLower().StartsWith("0x"))
                {
                    result[i] = byte.Parse(allStrings[i].Substring(2), NumberStyles.AllowHexSpecifier);
                }
                else
                {
                    result[i] = byte.Parse(allStrings[i]);
                }
            }
            return result;
        }

        private ExecutionState convertExecutionState(type_dtsPcExecutionStates execState)
        {
            switch (execState)
            {
                case type_dtsPcExecutionStates.EXECUTION_CANCELED:
                    return ExecutionState.EXECUTION_CANCELED;
                case type_dtsPcExecutionStates.EXECUTION_FAILED:
                    return ExecutionState.EXECUTION_FAILED;
                case type_dtsPcExecutionStates.EXECUTION_NEGATIVE_RESPONSE:
                    return ExecutionState.EXECUTION_NEGATIVE_RESPONSE;
                case type_dtsPcExecutionStates.EXECUTION_NOT_STARTED:
                    return ExecutionState.EXECUTION_NOT_STARTED;
                case type_dtsPcExecutionStates.EXECUTION_OK:
                    return ExecutionState.EXECUTION_OK;
                case type_dtsPcExecutionStates.EXECUTION_PENDING:
                    return ExecutionState.EXECUTION_PENDING;
                case type_dtsPcExecutionStates.EXECUTION_PENDING_INTERMEDIATE_RESULT:
                    return ExecutionState.EXECUTION_PENDING_INTERMEDIATE_RESULT;
            }

            return ExecutionState.EXECUTION_NEGATIVE_RESPONSE;
        }

        private void Execute(dtsPcRoutineUC routineUC, OffBoardResponse serviceResult)
        {
           

            routineUC.execute();
            serviceResult.ExecState = convertExecutionState(routineUC.getExecState());
            serviceResult.RawRequest = (Byte[])routineUC.getRequestBuffer();
            serviceResult.RawResponse = (Byte[])routineUC.getResultBuffer();
            if (serviceResult.RawResponse.Length == 0)
                throw new Exception("OffBoard Service execution not succesful - no RAW response come from ECU, please check if ECU is connected");
        }

        private void GetResponses(dtsPcRoutineUC routineUC, OffBoardService serviceSetting, OffBoardResponse serviceResult)
        {
            Array names;
            Array values;
            routineUC.getResults(out names, out values);
            serviceResult.ExecState = convertExecutionState(routineUC.getExecState());

            LogMessage("ExecState: " + serviceResult.ExecState, false);
            if (serviceResult.ExecState != ExecutionState.EXECUTION_OK)
                throw new Exception("OffBoard Service execution not succesful - " + serviceSetting.ServiceName + ": " + serviceResult.ExecState.ToString());

            LogMessage("Response: ", false);
            List<Parameter> responseParameters = new List<Parameter>();
            for (int i = 0; i < names.Length; i++)
            {
                responseParameters.Add(new Parameter(names.GetValue(i).ToString(), values.GetValue(i)));
                LogMessage(names.GetValue(i).ToString() + ":\t" + convertObjectToString(values.GetValue(i)), false);
            }

            if (serviceSetting.Responses != null)
            {
                foreach (OffBoardResponseSetting resSetting in serviceSetting.Responses)
                {
                    int keyLenght = resSetting.Keys.Count;
                    int keyCounter = 0;

                    OffBoardServiceResult res = new OffBoardServiceResult();
                    foreach (string key in resSetting.Keys)
                    {
                        res.Keys.Add(key);
                    }

                    if (resSetting.IgnoredValues != null)
                    {
                        foreach (string ignoredValue in resSetting.IgnoredValues)
                        {
                            res.IgnoredValues.Add(ignoredValue);
                        }
                    }
                    res.DoCheckResponse = resSetting.DoCheckResponse;
                    res.Limit = resSetting.Limit;

                    foreach (Parameter param in responseParameters)
                    {
                        if (param.Name == resSetting.Keys[keyCounter])
                        {
                            keyCounter++;
                            if (keyCounter == keyLenght)
                            {
                                if (param.Value != null)
                                {
                                    string responseValueAsString = convertObjectToString(param.Value);
                                    #region ignored //is it ignored?
                                    bool ignored = false;
                                    if (resSetting.IgnoredValues != null)
                                    {
                                        foreach (string ignoredValue in resSetting.IgnoredValues)
                                        {
                                            if (ignoredValue != null)
                                            {
                                                if (responseValueAsString.ToString().ToLower().Contains(ignoredValue.ToLower()))
                                                {
                                                    ignored = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    if (!ignored)
                                        res.Values.Add(responseValueAsString.ToString());
                                }
                                else
                                {
                                    res.Values.Add(string.Empty);
                                }

                                keyCounter = 0;
                            }
                        }
                    }
                    serviceResult.Results.Add(res);
                }
            }
        }

        internal List<OffBoardFlashJobResult> DTSFlashJob(List<dtsPcAccessPath> LLs, List<OffBoardFlashJobSetting> flashJobSetttings)
        {
            List<OffBoardFlashJobResult> flashResult = new List<OffBoardFlashJobResult>();
            List<dtsPcRoutineUC> routines = new List<dtsPcRoutineUC>();
            //routineUC.setService(serviceSetting.ServiceName);

            for (int i = 0; i < flashJobSetttings.Count; i++)
            {
                dtsPcRoutineUC routine = LLs[i].getRoutineUC();
                OffBoardFlashJobResult result = new OffBoardFlashJobResult();
                result.LogicalLink = flashJobSetttings[i].LogicalLink;
                result.FlashJobName = flashJobSetttings[i].FlashJobName;
                result.SessionName = flashJobSetttings[i].SessionName;
                flashResult.Add(result);
                routine.setFlashJob(result.FlashJobName, result.SessionName);
                routine.execute();
                routines.Add(routine);
            }
            int doneCounter = 0;
            while (routines.Count != doneCounter)
            {
                Thread.Sleep(250);
                doneCounter = 0;
                for (int i = 0; i < routines.Count; i++)
                {
                    type_dtsPcExecutionStates state = routines[i].getExecState();
                    if (state != type_dtsPcExecutionStates.EXECUTION_NOT_STARTED && state != type_dtsPcExecutionStates.EXECUTION_PENDING && state != type_dtsPcExecutionStates.EXECUTION_PENDING_INTERMEDIATE_RESULT)
                    {
                        doneCounter++;
                    }
                }
            }
            string errorText = string.Empty;
            for (int i = 0; i < routines.Count; i++)
            {
                flashResult[i].ExecutionState = routines[i].getExecState().ToString();
                flashResult[i].JobResult = routines[i].getResultValueString("OPA_JobResul");

                LogMessage("FlashJob Nr. " + i.ToString() + " LogicalLink: " + flashResult[i].LogicalLink, false);
                LogMessage("FlashJob Nr. " + i.ToString() + " FlashJobName: " + flashResult[i].FlashJobName, false);
                LogMessage("FlashJob Nr. " + i.ToString() + " SessionName: " + flashResult[i].SessionName, false);
                LogMessage("FlashJob Nr. " + i.ToString() + " JobResult: " + flashResult[i].JobResult, false);

                if (flashResult[i].JobResult != "Programming successful finished")
                    errorText = errorText + ", " + flashResult[i].LogicalLink + ", " + flashResult[i].FlashJobName + ", " + flashResult[i].SessionName + flashResult[i].JobResult;
            }

            if (errorText != string.Empty)
                throw new Exception("Flash job faild!!!" + errorText);

            return flashResult;
        }

        //Check if exception is from DTS or not
        private void CheckError(Exception ex, string additional, bool getErrorsFromDTS)
        {
            string result = string.Empty;
            int ErrorCount;
            if (getErrorsFromDTS)
            {
                try
                {
                    if (dtsApi != null)
                    {

                        ErrorCount = dtsApi.getNrOfErrors();
                        if (ErrorCount > 0)
                        {
                            for (int i = 0; i < ErrorCount; i++)
                            {
                                string error;
                                type_dtsPcErrorLevel errorLevel = type_dtsPcErrorLevel.ELEVEL_ERROR;
                                dtsApi.getError(i, out errorLevel, out error);
                                result = result + "---" + error;
                            }
                        }
                    }
                }
                catch { }
            }
            if (result != string.Empty)
            {
                LogMessage(result + ", " + additional, true);
                throw new Exception(result + ", " + additional);
            }
            else
            {
                LogMessage(ex.Message + ", " + additional, true);
                throw new Exception(ex.Message + ", " + additional);
            }
        }


        #endregion

        #region private VCF
        private void VCFInit()
        {
            if (onBoards == null)
                return;
            try
            {
                if (Logging.IsLoggingEnable())
                {
                    ALRestbusVCF.ALRestbusVCF.EnableLogging();
                }
                else
                {
                    ALRestbusVCF.ALRestbusVCF.DisableLogging();
                }

                ALRestbusVCF.ALRestbusVCF.LogFilePath = Logging.LogPath;

                if (vcfObjs == null)
                {
                    vcfObjs = new List<VCFObj>();
                }

                for (int i = 0; i < onBoards.Count; i++)
                {
                    if (vcfObjs.Count > 0)//Some already running
                    {
                        var sameIdObj = vcfObjs.FirstOrDefault(runningObj => runningObj.OnBoardSetting.DeviceName == onBoards[i].DeviceName && runningObj.OnBoardSetting.BusType == onBoards[i].BusType && runningObj.OnBoardSetting.BusChannel == onBoards[i].BusChannel);
                        if (sameIdObj != null)//if already exist comObj on the same device,bus type and channel 
                        {
                            if (AreTotallySame(sameIdObj.OnBoardSetting, onBoards[i]))
                            {
                                continue;
                            }
                            else
                            {
                                ReInitComObj(sameIdObj, onBoards[i]);
                                continue;
                            }
                        }
                    }
                    //create new
                    VCFObj vcfObj = new VCFObj();
                    vcfObj.OnBoardSetting = new OnBoardConfig();

                    AssignConfiguration(vcfObj.OnBoardSetting, onBoards[i]);
                    if (onBoards[i].HWType == hwType.HSX)
                    {
                        vcfObj.ComObj = ALRestbusVCF.ALRestbusVCF.GetCommunicationObject(onBoards[i].DeviceName, onBoards[i].BusType, onBoards[i].BusChannel, onBoards[i].NetcCfgFile, onBoards[i].BusNodes, onBoards[i].CustomCgfFile, onBoards[i].LINScheduleTable, onBoards[i].LINMasterNode, onBoards[i].ClusterName, onBoards[i].ContinueOnError);
                    }
                    if (onBoards[i].HWType == hwType.Vector)
                    {
                        vcfObj.ComObj = new Restbus3Wrapper(onBoards[i].DeviceName, onBoards[i].BusType, onBoards[i].BusChannel, onBoards[i].NetcCfgFile, onBoards[i].BusNodes, onBoards[i].CustomCgfFile, onBoards[i].LINScheduleTable, onBoards[i].LINMasterNode, onBoards[i].ClusterName, onBoards[i].ContinueOnError);
                    }
                    vcfObj.ComObj.Start();
                    vcfObjs.Add(vcfObj);
                }
            }
            catch (Exception ex)
            {
                CheckError(ex, "VCFInit()", true);
            }
        }

        private void ReInitComObj(VCFObj ComObjToReinit, OnBoardConfig onBoardConfig)
        {
            ComObjToReinit.ComObj.Stop();
            ComObjToReinit.ComObj.Dispose();

            AssignConfiguration(ComObjToReinit.OnBoardSetting, onBoardConfig);

            if (onBoardConfig.HWType == hwType.HSX)
            {
                ComObjToReinit.ComObj = ALRestbusVCF.ALRestbusVCF.GetCommunicationObject(onBoardConfig.DeviceName,
                    onBoardConfig.BusType, onBoardConfig.BusChannel, onBoardConfig.NetcCfgFile, onBoardConfig.BusNodes,
                    onBoardConfig.CustomCgfFile, onBoardConfig.LINScheduleTable, onBoardConfig.LINMasterNode,
                    onBoardConfig.ClusterName, onBoardConfig.ContinueOnError);
            }
            else if (onBoardConfig.HWType == hwType.Vector)
            {
                ComObjToReinit.ComObj = new Restbus3Wrapper(onBoardConfig.DeviceName,
                    onBoardConfig.BusType, onBoardConfig.BusChannel, onBoardConfig.NetcCfgFile, onBoardConfig.BusNodes,
                    onBoardConfig.CustomCgfFile, onBoardConfig.LINScheduleTable, onBoardConfig.LINMasterNode,
                    onBoardConfig.ClusterName, onBoardConfig.ContinueOnError);
            }

            ComObjToReinit.ComObj.Start();
        }

        private bool AreTotallySame(OnBoardConfig onBoardConfig1, OnBoardConfig onBoardConfig2)
        {
            if (onBoardConfig1.BusChannel == onBoardConfig2.BusChannel &&
                onBoardConfig1.BusNodes == onBoardConfig2.BusNodes &&
                onBoardConfig1.BusType == onBoardConfig2.BusType &&
                onBoardConfig1.CustomCgfFile == onBoardConfig2.CustomCgfFile &&
                onBoardConfig1.DeviceName == onBoardConfig2.DeviceName &&
                onBoardConfig1.LINMasterNode == onBoardConfig2.LINMasterNode &&
                onBoardConfig1.LINScheduleTable == onBoardConfig2.LINScheduleTable &&
                onBoardConfig1.NetcCfgFile == onBoardConfig2.NetcCfgFile)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AssignConfiguration(OnBoardConfig leftSide, OnBoardConfig rightSide)
        {
            leftSide.BusChannel = rightSide.BusChannel;
            leftSide.BusNodes = rightSide.BusNodes;
            leftSide.BusType = rightSide.BusType;
            leftSide.CustomCgfFile = rightSide.CustomCgfFile;
            leftSide.DeviceName = rightSide.DeviceName;
            leftSide.LINMasterNode = rightSide.LINMasterNode;
            leftSide.LINScheduleTable = rightSide.LINScheduleTable;
            leftSide.NetcCfgFile = rightSide.NetcCfgFile;
        }

        private void VCFClose()
        {
            if (vcfObjs != null)
            {
                VCFStopCommunication();

                for (int i = 0; i < vcfObjs.Count; i++)
                {
                    try
                    {
                        vcfObjs[i].ComObj.Dispose();
                    }
                    catch
                    { }
                }
                if (vcfObjs.Count > 0)
                    vcfObjs.Clear();
            }
        }

        private void VCFStopCommunication()
        {
            if (vcfObjs == null)
                return;
            try
            {
                for (int i = 0; i < vcfObjs.Count; i++)
                {
                    vcfObjs[i].ComObj.Stop();
                }
            }
            catch (Exception ex)
            {
                CheckError(ex, "VCFStopCommunication()", true);
            }
        }

        private void VCFStartCommunication()
        {
            if (vcfObjs == null)
                return;
            try
            {
                for (int i = 0; i < vcfObjs.Count; i++)
                {
                    vcfObjs[i].ComObj.Start();
                }
            }
            catch (Exception ex)
            {
                CheckError(ex, "VCFStartCommunication()", true);
            }
        }

        private int GetOnBoardDeviceID(string deviceName, string busType, string channel)
        {
            int result = -1;
            if (vcfObjs == null || vcfObjs.Count == 0)
                throw new Exception("No OnBoard Device been initialized!, GetOnBoardDeviceID(string deviceName, string busType ,string channel)");


            string availableDevices = string.Empty;
            for (int i = 0; i < vcfObjs.Count; i++)
            {
                if (availableDevices != string.Empty)
                {
                    availableDevices = availableDevices + ", ";
                }
                availableDevices = availableDevices + vcfObjs[i].OnBoardSetting.DeviceName + vcfObjs[i].OnBoardSetting.BusType + vcfObjs[i].OnBoardSetting.BusChannel;
            }

            if ("UseDefault" == deviceName && "UseDefault" == busType && "UseDefault" == channel)
            {
                if (vcfObjs.Count == 1)
                    return 0;
                else
                {
                    throw new Exception("Please specify deviceName, busType and channel! Configured: " + availableDevices + ", GetOnBoardDeviceID(string deviceName, string busType ,string channel)");
                }
            }


            for (int i = 0; i < vcfObjs.Count; i++)
            {
                if (vcfObjs[i].OnBoardSetting.DeviceName == deviceName && vcfObjs[i].OnBoardSetting.BusType == busType && vcfObjs[i].OnBoardSetting.BusChannel == channel)
                {
                    result = i;
                }
            }
            if (result == -1)
                throw new Exception(deviceName + " " + busType + channel + " Is not configured, available only: " + availableDevices + ", GetOnBoardDeviceID(string deviceName, string busType ,string channel)");


            return result;
        }

        private void CheckIsBusCommunication(int index)
        {
            if (vcfObjs[index].ComObj.IsBusCommunication() != true)
                throw new Exception("Bus Communication is not working on " + vcfObjs[index].OnBoardSetting.DeviceName + " " + vcfObjs[index].OnBoardSetting.BusType + vcfObjs[index].OnBoardSetting.BusChannel + ", CheckIsBusCommunication(int index)");
        }

        #endregion

        #region Public DTS

        public string OffBoardVariantIdentificationAndSelection(string logicalLinkName)
        {
            int index = GetLLIndex(logicalLinkName);
            string result = string.Empty;
            try
            {
                LogMessage("VariantIdentificationAndSelection by Logical Link: " + logicalLinkName, true);
                result = dtsLogLinks[index].Value.variantIdentificationAndSelection();
            }
            catch (Exception ex)
            {
                CheckError(ex, "OffBoardVariantIdentificationAndSelection(string logicalLinkName)" + ", " + logicalLinkName, true);
            }

            LogMessage(Environment.NewLine, false);

            return result;
        }

        public List<OffBoardLogicalLinkParallelServices> OffBoardExecuteServicesInParallel(List<OffBoardLogicalLinkParallelServices> listOfOffBoardLogicalLinkParallelServices)
        {
            List<Task> tasks = new List<Task>();
            StringBuilder exceptionMessages = new StringBuilder();
            
            // DEBUG ONLY
            foreach (var offBoardLogicalLinkParallelServices in listOfOffBoardLogicalLinkParallelServices)
            {
                var offBoardParallelServices = offBoardLogicalLinkParallelServices.OffBoardParallelServices;
                string logicalLink = offBoardLogicalLinkParallelServices.LogicalLink;
                Task logicalLinkTask = new Task(() =>
                                                {
                                                    foreach (var offBoardParallelService in offBoardParallelServices)
                                                    {
                                                        try
                                                        {
                                                            offBoardParallelService.OffBoardResponse = this.OffBoardExecuteService(logicalLink, offBoardParallelService.OffBoardService);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            try
                                                            {
                                                                CheckError(ex,
                                                                    "OffBoardExecuteService(string logicalLinkName, OffBoardService serviceSetting)" +
                                                                    ", " + logicalLink + ", " +
                                                                    offBoardParallelService.OffBoardService.ServiceName, true);
                                                            }
                                                            catch (Exception checkedEx)
                                                            {
                                                                exceptionMessages.AppendLine(checkedEx.Message);
                                                            }
                                                        }
                                                    }
                                                });
                tasks.Add(logicalLinkTask);
                logicalLinkTask.Start();
            }

            Task.WaitAll(tasks.ToArray());
            
            if (exceptionMessages.Length > 0)
            {
                throw new Exception(exceptionMessages.ToString());
            }

            return listOfOffBoardLogicalLinkParallelServices;
        }

        public OffBoardResponse OffBoardExecuteService(string logicalLinkName, OffBoardService serviceSetting)
        {
            OffBoardResponse result = null;

            if (offBoard.LogicalLinks.Contains(logicalLinkName))
            {
                int index = GetLLIndex(logicalLinkName);

                try
                {
                    LogMessage("ExecuteService by Logical Link: " + dtsLogLinks[index].Name, true);
                    System.Diagnostics.Stopwatch durationStopWatch = new System.Diagnostics.Stopwatch();
                    durationStopWatch.Start();
                    result = DTSExecuteRoutine(dtsLogLinks[index].Value, serviceSetting);
                    durationStopWatch.Stop();
                    LogMessage("Duration of " + logicalLinkName + " - " + serviceSetting.ServiceName + " was " + durationStopWatch.ElapsedMilliseconds + "ms" , true);
                }
                catch (Exception ex)
                {
                    CheckError(ex, "OffBoardExecuteService(string logicalLinkName, OffBoardService serviceSetting)" + ", " + logicalLinkName + ", " + serviceSetting.ServiceName, true);
                }

                LogMessage(Environment.NewLine, false);
            }
            else if (ediabas != null) //ediabas started
            {
                lock (this.communicationLock)
                {
                    result = ediabas.OffBoardExecuteService(logicalLinkName, serviceSetting);
                }
            }
            else
            {
                throw new Exception("Off Board settings does not contain this logical link or Edibas has not been initialized yet.");
            }

            return result;
        }
        
        public void OffBoardExecuteServiceCyclically(string logicalLinkName, OffBoardService serviceSetting, int cycleTime)
        {
            this.cyclicServices.Add(new CyclicService(logicalLinkName, serviceSetting, cycleTime, this.taskForCyclicService_Elapsed));
        }

        void taskForCyclicService_Elapsed(object sender, EventArgs e)
        {
            LogMessage("Executing cyclic service on timer thread.", true);

            var thisCyclicService = (CyclicService)sender;
            
            thisCyclicService.TaskCounter++;
            double fixedInterval = thisCyclicService.TimerInterval + thisCyclicService.TimerInterval * thisCyclicService.TaskCounter - thisCyclicService.AuxTimer.ElapsedMilliseconds;

            if (fixedInterval < 1)
            {
                thisCyclicService.TaskForCyclicService.Interval = 1;
            }
            else
            {
                thisCyclicService.TaskForCyclicService.Interval = fixedInterval;
            }

            try
            {
                OffBoardExecuteService(thisCyclicService.LogicalLinkForCyclicService, thisCyclicService.ServiceSettingForCyclicService);
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message, true);
            }
        }

        public void StopOffBoardExecuteServiceCyclically()
        {
            foreach (var cyclicService in this.cyclicServices)
            {
                cyclicService.Stop();
            }
        
            this.cyclicServices.Clear();
        }

        public List<OffBoardFlashJobResult> OffBoardFlashJob(List<OffBoardFlashJobSetting> flashJobSetttings)
        {
            List<OffBoardFlashJobResult> result = null;
            string logicalLinkNames = string.Empty;
            string sessionNames = string.Empty;
            List<dtsPcAccessPath> LLs = new List<dtsPcAccessPath>();

            foreach (OffBoardFlashJobSetting flashJob in flashJobSetttings)
            {
                int index = GetLLIndex(flashJob.LogicalLink);
                LLs.Add(dtsLogLinks[index].Value);
                logicalLinkNames = ", " + logicalLinkNames + flashJob.LogicalLink;
                sessionNames = ", " + sessionNames + flashJob.SessionName;
            }

            LogMessage("Flash job by Logical Links: " + logicalLinkNames, true);

            try
            {
                result = DTSFlashJob(LLs, flashJobSetttings);
            }
            catch (Exception ex)
            {
                CheckError(ex, "OffBoardFlashJob(List<OffBoardFlashJobSetting> flashJobSetttings)" + logicalLinkNames + sessionNames, true);
            }

            LogMessage(Environment.NewLine, false);

            return result;
        }

        #endregion

        #region Public VCF

        public List<string> OnBoardGetEcus(string networkCfgFile)
        {
            return ALRestbusVCF.ALRestbusVCF.GetEcus(networkCfgFile);
        }

        public List<string> OnBoardGetInputSignals(string networkCfgFile, string ecus)
        {
            return ALRestbusVCF.ALRestbusVCF.GetInputSignals(networkCfgFile, ecus);
        }

        public List<string> OnBoardGetOutputSignals(string networkCfgFile, string ecus)
        {
            return ALRestbusVCF.ALRestbusVCF.GetOutputSignals(networkCfgFile, ecus);
        }

        public OnBoardSignal OnBoardGetSignal(string deviceName, string busType, string channel, string frameName, string signalName)
        {
            string signalValue = string.Empty;
            string signalUnit = string.Empty;
            OnBoardSignal result = null;
            try
            {
                int index = GetOnBoardDeviceID(deviceName, busType, channel);
                CheckIsBusCommunication(index);
                vcfObjs[index].ComObj.GetSignal(frameName, signalName, ref signalValue, ref signalUnit);
                result = new OnBoardSignal() { Value = signalValue, Unit = signalUnit };

            }
            catch (Exception ex)
            {
                CheckError(ex, "OnBoardGetSignal: " + deviceName + ", " + busType + ", " + channel + ", " + frameName + ", " + signalName, false);
            }
            return result;
        }

        public OnBoardSignal OnBoardGetSignal(string frameName, string signalName)
        {
            return OnBoardGetSignal("UseDefault", "UseDefault", "UseDefault", frameName, signalName);
        }

        public void OnBoardSetSignal(string deviceName, string busType, string channel, string frameName, string signalName, string signalValue)
        {
            try
            {
                int index = GetOnBoardDeviceID(deviceName, busType, channel);
                CheckIsBusCommunication(index);
                vcfObjs[index].ComObj.SetSignal(frameName, signalName, signalValue);
            }
            catch (Exception ex)
            {
                CheckError(ex, "OnBoardSetSignal: " + deviceName + ", " + busType + ", " + channel + ", " + frameName + ", " + signalName + ", " + signalValue, false);
            }
        }

        public void OnBoardSetSignal(string frameName, string signalName, string signalValue)
        {
            OnBoardSetSignal("UseDefault", "UseDefault", "UseDefault", frameName, signalName, signalValue);
        }

        public void OnBoardSetGlobalVariable(string deviceName, string busType, string channel, string variableName, string variableValue)
        {
            try
            {
                int index = GetOnBoardDeviceID(deviceName, busType, channel);
                int variableValueInt = 0;
                if (!int.TryParse(variableValue, out variableValueInt))
                {
                    throw new Exception("OnBoardSetGlobalVariable - For variableValue is supported just 'int' value passed as string");
                }
                CheckIsBusCommunication(index);
                vcfObjs[index].ComObj.SetGlobalVariable(variableName, variableValueInt);
            }
            catch (Exception ex)
            {
                CheckError(ex, "OnBoardSetGlobalVariable: " + deviceName + ", " + busType + ", " + channel + ", " + variableName + ", " + variableValue, false);
            }
        }

        public void OnBoardSetGlobalVariable(string variableName, string variableValue)
        {
            OnBoardSetGlobalVariable("UseDefault", "UseDefault", "UseDefault", variableName, variableValue);
        }

        public string OnBoardGetGlobalVariable(string deviceName, string busType, string channel, string variableName)
        {
            byte[] variableValue = null;
            string result = string.Empty;
            try
            {
                int index = GetOnBoardDeviceID(deviceName, busType, channel);
                CheckIsBusCommunication(index);
                if (vcfObjs[index].OnBoardSetting.HWType == hwType.Vector)
                {
                    string varVal = string.Empty;
                    ((Restbus3Wrapper)vcfObjs[index].ComObj).GetGlobalVariableString(variableName, ref varVal);
                    return varVal;
                }
                vcfObjs[index].ComObj.GetGlobalVariable(variableName, ref variableValue);
                byte[] variableValueSwitched = new byte[variableValue.Length];
                for (int i = 0; i < variableValueSwitched.Length; i++)
                {
                    variableValueSwitched[variableValueSwitched.Length - 1 - i] = variableValue[i];
                }
                if (variableValue != null && variableValue.Length > 0)
                    result = ByteToInt32(variableValue);
            }
            catch (Exception ex)
            {
                CheckError(ex, "OnBoardGetGlobalVariable: " + deviceName + ", " + deviceName + ", " + busType + ", " + channel + ", " + variableName + ", " + variableValue, false);
            }

            return result;
        }

        public string OnBoardGetGlobalVariable(string variableName)
        {
            return OnBoardGetGlobalVariable("UseDefault", "UseDefault", "UseDefault", variableName);
        }


        public byte[] OnBoardGetGlobalVariableRaw(string deviceName, string busType, string channel, string variableName)
        {
            byte[] variableValue = null;
            try
            {
                int index = GetOnBoardDeviceID(deviceName, busType, channel);
                CheckIsBusCommunication(index);
                vcfObjs[index].ComObj.GetGlobalVariable(variableName, ref variableValue);
            }
            catch (Exception ex)
            {
                CheckError(ex, "OnBoardGetGlobalVariable: " + deviceName + ", " + deviceName + ", " + busType + ", " + channel + ", " + variableName + ", " + variableValue, false);
            }

            return variableValue;
        }

        
        public byte[] OnBoardGetGlobalVariableByNameRaw(string variableName)
        {
            return OnBoardGetGlobalVariableRaw("UseDefault", "UseDefault", "UseDefault", variableName);
        }

        private string ByteToInt32(byte[] data)
        {
            byte[] variableValueSwitched = new byte[data.Length];
            for (int i = 0; i < variableValueSwitched.Length; i++)
            {
                variableValueSwitched[variableValueSwitched.Length - 1 - i] = data[i];
            }
            return BitConverter.ToInt32(variableValueSwitched, 0).ToString();
        }



        public void OnBoardActiveScheduleTable(string deviceName, string busType, string channel, string scheduleTable)
        {
            try
            {
                int index = GetOnBoardDeviceID(deviceName, busType, channel);
                CheckIsBusCommunication(index);
                vcfObjs[index].ComObj.ActivateTable(scheduleTable);
            }
            catch (Exception ex)
            {
                CheckError(ex, "OnBoardActiveScheduleTable: " + deviceName + ", " + busType + ", " + channel + ", " + scheduleTable, false);
            }
        }

        public void OnBoardActiveScheduleTable(string scheduleTable)
        {
            OnBoardActiveScheduleTable("UseDefault", "UseDefault", "UseDefault", scheduleTable);
        }

        public void OnBoardSendFrameOnce(string deviceName, string busType, string channel, uint frameID, byte[] data)
        {
            try
            {
                int index = GetOnBoardDeviceID(deviceName, busType, channel);
                CheckIsBusCommunication(index);
                vcfObjs[index].ComObj.SendFrameOnce(frameID, data, (uint)data.Length);
            }
            catch (Exception ex)
            {
                CheckError(ex, "OnBoardSendFrameOnce: " + deviceName + ", " + busType + ", " + channel + ", " + frameID, false);
            }
        }

        public void OnBoardSendFrameOnce(uint frameID, byte[] data)
        {
            OnBoardSendFrameOnce("UseDefault", "UseDefault", "UseDefault", frameID, data);
        }

        public byte[] OnBoardReceiveFrameOnce(string deviceName, string busType, string channel, uint frameID, uint resFrameID, byte[] data, uint timeout)
        {
            byte[] result = null;
            try
            {
                int index = GetOnBoardDeviceID(deviceName, busType, channel);
                CheckIsBusCommunication(index);
                vcfObjs[index].ComObj.ReceiveFrameOnce(frameID, resFrameID, data, ref result, timeout);
            }
            catch (Exception ex)
            {
                CheckError(ex, "OnBoardReceiveFrameOnce: " + deviceName + ", " + busType + ", " + channel + ", " + frameID + ", " + resFrameID + "," + timeout, false);
            }
            return result;
        }

        public byte[] OnBoardReceiveFrameOnce(uint frameID, uint resFrameID, byte[] data, uint timeout)
        {
            return OnBoardReceiveFrameOnce("UseDefault", "UseDefault", "UseDefault", frameID, resFrameID, data, timeout);
        }

        public void OnBoardsCheckRunning()
        {
            try
            {
                if (vcfObjs == null || vcfObjs.Count == 0)
                    throw new Exception("No OnBoard Device been initialized!, GetOnBoardDeviceID(string deviceName, string busType ,string channel)");


                string notRunning = string.Empty;



                for (int i = 0; i < vcfObjs.Count; i++)
                {
                    if (!vcfObjs[i].ComObj.IsBusCommunication())
                    {
                        notRunning = notRunning + vcfObjs[i].OnBoardSetting.DeviceName + " - " + vcfObjs[i].OnBoardSetting.BusType + " - " + vcfObjs[i].OnBoardSetting.BusChannel + ", ";
                    }
                }
                if (notRunning != string.Empty)
                    throw new Exception("OnBoardsCheckRunning - These onboards are not running: " + notRunning);
            }
            catch (Exception ex)
            {
                CheckError(ex, "OnBoardsCheckRunning:", false);
            }

        }

        #endregion

        #region Public ALRestbus3

        public void OnBoardSendCANFDFrameCyclic(string deviceName, string busType, string channel, uint frameId, string data, uint timeOutFrame)
        {
            try
            {
                int index = GetOnBoardDeviceID(deviceName, busType, channel);
                CheckIsBusCommunication(index);
                if (!(vcfObjs[index].ComObj is Restbus3Wrapper))
                {
                    throw new Exception("Mehtod isn't implemented on this kind of communication object.");
                }
                ((Restbus3Wrapper) vcfObjs[index].ComObj).SendCANFDFrameCyclic(frameId, StrToInt(data), timeOutFrame);
            }
            catch (Exception ex)
            {
                CheckError(ex, "SendCANFDFrameCyclic: " + deviceName + ", " + busType + ", " + channel, false);
            }            
        }

        public void OnBoardSendCANFDFrameCyclic(string deviceName, string busType, string channel, uint frameId, IEnumerable<string> data, uint timeOutFrame, uint timeOutSubList)
        {
            try
            {
                int index = GetOnBoardDeviceID(deviceName, busType, channel);
                CheckIsBusCommunication(index);
                if (!(vcfObjs[index].ComObj is Restbus3Wrapper))
                {
                    throw new Exception("Mehtod isn't implemented on this kind of communication object.");
                }
                ((Restbus3Wrapper)vcfObjs[index].ComObj).SendCANFDFrameCyclic(frameId, data.Select(StrToInt).ToList(), timeOutFrame, timeOutSubList);
            }
            catch (Exception ex)
            {
                CheckError(ex, "SendCANFDFrameCyclic: " + deviceName + ", " + busType + ", " + channel, false);
            }
        }

        public void OnBoardSendCANFDFrameCyclic(string deviceName, string busType, string channel, Dictionary<uint, IEnumerable<string>> frameData, uint timeOutFrame, uint timeOutSubList)
        {
            foreach (var frameId in frameData.Keys)
            {
                OnBoardSendCANFDFrameCyclic(deviceName, busType, channel, frameId, frameData[frameId], timeOutFrame, timeOutSubList);
            }
        }

        public void OnBoardSendFrameCyclic(string deviceName, string busType, string channel, uint frameId, string data, uint timeOutFrame)
        {
            try
            {
                int index = GetOnBoardDeviceID(deviceName, busType, channel);
                CheckIsBusCommunication(index);
                if (!(vcfObjs[index].ComObj is Restbus3Wrapper))
                {
                    throw new Exception("Mehtod isn't implemented on this kind of communication object.");
                }
                ((Restbus3Wrapper)vcfObjs[index].ComObj).SendFrameCyclic(frameId, StrToInt(data), timeOutFrame);
            }
            catch (Exception ex)
            {
                CheckError(ex, "SendCANFDFrameCyclic: " + deviceName + ", " + busType + ", " + channel, false);
            }
        }

        public void OnBoardSendFrameCyclic(string deviceName, string busType, string channel, uint frameId, IEnumerable<string> data, uint timeOutFrame, uint timeOutSubList)
        {
            try
            {
                int index = GetOnBoardDeviceID(deviceName, busType, channel);
                CheckIsBusCommunication(index);
                if (!(vcfObjs[index].ComObj is Restbus3Wrapper))
                {
                    throw new Exception("Mehtod isn't implemented on this kind of communication object.");
                }
                ((Restbus3Wrapper)vcfObjs[index].ComObj).SendFrameCyclic(frameId, data.Select(StrToInt).ToList(), timeOutFrame, timeOutSubList);
            }
            catch (Exception ex)
            {
                CheckError(ex, "SendCANFDFrameCyclic: " + deviceName + ", " + busType + ", " + channel, false);
            }
        }

        public void OnBoardSendFrameCyclic(string deviceName, string busType, string channel, Dictionary<uint, IEnumerable<string>> frameData, uint timeOutFrame, uint timeOutSubList)
        {
            foreach (var frameId in frameData.Keys)
            {
                OnBoardSendFrameCyclic(deviceName, busType, channel, frameId, frameData[frameId], timeOutFrame, timeOutSubList);
            }
        }

        public void SetGlobalAdapterProjectVariable(string deviceName, string busType, string channel, string variableName, string value)
        {
            try
            {
                int index = GetOnBoardDeviceID(deviceName, busType, channel);
                CheckIsBusCommunication(index);
                if (!(vcfObjs[index].ComObj is Restbus3Wrapper))
                {
                    throw new Exception("Mehtod isn't implemented on this kind of communication object.");
                }
                ((Restbus3Wrapper)vcfObjs[index].ComObj).SetGlobalProjectVariable(variableName, value);
            }
            catch (Exception ex)
            {
                CheckError(ex, "SetGlobalAdapterProjectVariable: " + deviceName + ", " + busType + ", " + channel, false);
            }            
        }

        public string GetGlobalAdapterProjectVariable(string deviceName, string busType, string channel, string variableName)
        {
            try
            {
                int index = GetOnBoardDeviceID(deviceName, busType, channel);
                CheckIsBusCommunication(index);
                if (!(vcfObjs[index].ComObj is Restbus3Wrapper))
                {
                    throw new Exception("Mehtod isn't implemented on this kind of communication object.");
                }
                return ((Restbus3Wrapper)vcfObjs[index].ComObj).GetGlobalProjectVariable(variableName);
            }
            catch (Exception ex)
            {
                CheckError(ex, "GetGlobalAdapterProjectVariable: " + deviceName + ", " + busType + ", " + channel, false);
                throw;
            }
        }

        #endregion Public ALRestbus3

        public void LogMessage(string message, bool logTimeStamp)
        {
            Logging.LogMessage(message, Logging.LogFilePrefixOffBoard, logTimeStamp);
        }

        private string convertObjectToString(object o)
        {
            string outputstr = string.Empty;

            if (o is IEnumerable)
            {
                if (o.GetType() == typeof(string))
                {
                    return o.ToString();
                }
                outputstr += "{";
                foreach (var item in (IEnumerable)o)
                {
                    outputstr += convertObjectToString(item) + ",";
                }
                outputstr = outputstr.Remove(outputstr.Length - 1, 1) + "}";
            }
            else
            {
                return o.ToString();
            }
            return outputstr;
        }

        private delegate void CyclicServiceTimerElapsedEventHandler(object sender, EventArgs e);
        private class CyclicService : IDisposable
        {
            public event CyclicServiceTimerElapsedEventHandler TimerElapsedEvent;
            public System.Diagnostics.Stopwatch AuxTimer;
            public System.Timers.Timer TaskForCyclicService;
            public OffBoardService ServiceSettingForCyclicService;
            public long TaskCounter;
            public string LogicalLinkForCyclicService;
            public int TimerInterval;
            private bool disposed = false;
            
            public CyclicService(string logicalLinkName, OffBoardService serviceSetting, int cycleTime, CyclicServiceTimerElapsedEventHandler eventHandler)
            {
                this.LogicalLinkForCyclicService = logicalLinkName;
                this.ServiceSettingForCyclicService = serviceSetting;
                this.TimerInterval = cycleTime;
                this.AuxTimer = new System.Diagnostics.Stopwatch();
                this.AuxTimer.Start();
                this.TaskCounter = 0;
                this.TaskForCyclicService = new System.Timers.Timer(cycleTime);
                this.TaskForCyclicService.Enabled = true;
                this.TaskForCyclicService.Elapsed += taskForCyclicService_Elapsed;
                this.TimerElapsedEvent = eventHandler;
            }

            private void taskForCyclicService_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                if (this.TimerElapsedEvent != null)
                {
                    Task.Factory.StartNew(() =>
                    {
                        this.TimerElapsedEvent(this, null);
                    });
                }
            }

            public void Stop()
            {
                if (this.TaskForCyclicService != null)
                {
                    this.TaskForCyclicService.Stop();
                }
            }

            public void Dispose()
            {
                if (this.disposed)
                {
                    return;
                }

                this.Stop();
                this.TaskForCyclicService.Dispose();
                this.disposed = true;
            }
            
            ~CyclicService()
            {
                this.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        private hwType getHwType(string deviceName)
        {
            if (onBoards.Count == 0)
            {
                throw new Exception("OnBoards is not configured!");
            }
            if (deviceName == "UseDefault")
            {
                return onBoards[0].HWType;
            }

            foreach (OnBoardConfig config in onBoards)
            {
                if (config.DeviceName == deviceName)
                {
                    return config.HWType;
                }
            }
            throw new Exception("This deviceName:" + deviceName + " does not exist in OnboardConfig");

            //var hwtype = onBoards.FirstOrDefault(x => x != null && x.DeviceName == deviceName);

            //if (hwtype != null)
            //{
            //    return hwtype.HWType;
            //}
            //else
            //{
            //    throw new Exception("HW type is null!");
            //}
        }

        private byte[] StrToByte(string data)
        {
            try
            {
                List<byte> output = new List<byte>();
                foreach (string s in Regex.Split(data, @"\s+").Where(s => s != string.Empty))
                {
                    output.Add(byte.Parse(s, NumberStyles.AllowHexSpecifier));
                }
                return output.ToArray<byte>();
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Wrong data format: {0} can't be readed. Acepted is hexa byte separated by space (1a 1b).", data), e);
            }
        }

        private List<int> StrToInt(string data)
        {
            return StrToByte(data).Select(b => (int)b).ToList();
        }

        private string ByteToStr(byte[] data, uint dataLen)
        {
            StringBuilder sb = new StringBuilder();
            for (uint i = 0; i < dataLen; i++)
            {
                sb.Append(data[i].ToString("X2"));
                sb.Append(" ");
            }

            return sb.ToString();
        }
    }
}

