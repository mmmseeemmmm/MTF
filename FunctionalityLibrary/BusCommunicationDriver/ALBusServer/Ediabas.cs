using System;
using System.Collections.Generic;
using System.Linq;
using Ediabas;
using vxlapi_NET20;
using System.IO;

namespace ALBusComDriver
{
    public class Ediabas
    {
        private string sgbdFolder = string.Empty;

        public Ediabas(string serialNumber, string channel, string sgbdFolder)
        {
            this.configureHW(serialNumber, channel);
            initAPI(sgbdFolder);
            this.sgbdFolder = sgbdFolder;
        }

        public OffBoardResponse OffBoardExecuteService(string ecu, OffBoardService serviceSetting)
        {
            string fullPath = Path.Combine(sgbdFolder, ecu + ".prg");
            if (!File.Exists(fullPath))
            {
                throw new Exception("SGBD file does not exist on this path: " + fullPath);
            }
            OffBoardResponse result = new OffBoardResponse();

            List<ReceivedResponse> receivedResponses;
            List<ReceivedResponse> filtredResponses;

            string requestParameter;

            byte[] rawReq;
            byte[] rawRes;
            string jobStatus;

            if (serviceSetting.RequestParameters == null)
            {
                requestParameter = string.Empty;
            }
            else
            {
                requestParameter = serviceSetting.RequestParameters[0].Name;
                if (serviceSetting.RequestParameters.Count != 1)
                {
                    throw new Exception("Ediabas has to have only one request parameter! More parameters are separated by ;");
                }
            }
            receivedResponses = executeService(ecu, serviceSetting.ServiceName, requestParameter, "", out rawReq, out rawRes, out jobStatus);

            result.RawRequest = rawReq;
            result.RawResponse = rawRes;
            if (jobStatus == "OKAY" || jobStatus == "ERROR_SVK_INCORRECT_FINGERPRINT")
            {
                result.ExecState = ExecutionState.EXECUTION_OK;
            }
            else
            {
                result.ExecState = ExecutionState.EXECUTION_FAILED;
                throw new Exception("OffBoard Service execution not succesful - " + serviceSetting.ServiceName + ": " + jobStatus);
            }


            if (serviceSetting.Responses != null)
            {
                foreach (OffBoardResponseSetting responseSetting in serviceSetting.Responses)
                {
                    if (responseSetting.Keys == null)
                    {
                        continue;
                    }
                    if (responseSetting.Keys.Count > 2)
                    {
                        throw new Exception("More than 2 keys does not make sense.");
                    }
                    else if (responseSetting.Keys.Count == 2 && responseSetting.Keys[0].Contains("Set") && responseSetting.Keys[0].Length == 7)
                    {
                        //case Set001 JobStatus
                        filtredResponses = filtrResponses(responseSetting.Keys[0], responseSetting.Keys[1], receivedResponses);
                    }
                    else if (responseSetting.Keys.Count == 1 && responseSetting.Keys[0].Contains("Set") && responseSetting.Keys[0].Length == 7)
                    {
                        //only setName
                        filtredResponses = filtrResponses(responseSetting.Keys[0], null, receivedResponses);
                    }
                    else if (responseSetting.Keys.Count == 1 && !String.IsNullOrEmpty(responseSetting.Keys[0]))
                    {
                        //only resultName
                        filtredResponses = filtrResponses(null, responseSetting.Keys[0], receivedResponses);
                    }
                    else
                    {
                        throw new Exception("Wrong keys");
                    }

                    OffBoardServiceResult serviceResult = new OffBoardServiceResult();

                    foreach (ReceivedResponse response in filtredResponses)
                    {

                        if (responseSetting.IgnoredValues != null && responseSetting.IgnoredValues.Contains(response.resultValue))
                        {
                            serviceResult.IgnoredValues.Add(response.resultValue);
                        }
                        else
                        {
                            serviceResult.Keys.Add(response.resultName);
                            serviceResult.Values.Add(response.resultValue);
                        }
                    }

                    result.Results.Add(serviceResult);
                }
            }

            return result;
        }

        public void Close()
        {
            API.apiEnd();
            logMessage("Ediabas was released", true);
            LogObject.LogMessage("Ediabas was released");
        }

        private void configureHW(string serialNumber, string hwChannel)
        {

            uint serialNumberLoc = uint.Parse(serialNumber);
            byte hwChannelLoc = (byte)(byte.Parse(hwChannel) - 1);

            XLDriver xlDriver = new XLDriver();

            XLClass.XLstatus status = xlDriver.XL_OpenDriver();

            if (status != XLClass.XLstatus.XL_SUCCESS)
            {
                throw new Exception("Cannot open XL Driver. HW configuration failed!");
            }

            XLClass.xl_driver_config config = new XLClass.xl_driver_config();


            xlDriver.XL_GetDriverConfig(ref config);

            XLClass.xl_channel_config channelConfig = config.channel.FirstOrDefault(i => i.serialNumber == serialNumberLoc && i.hwChannel == hwChannelLoc);

            if (channelConfig == null)
            {
                throw new Exception(String.Format("Specified HW by serial number: {0} and channel: {1} is not presented on the machine!", serialNumberLoc, hwChannelLoc));
            }

            status = xlDriver.XL_SetApplConfig("Ediabas", 0, channelConfig.hwType, channelConfig.hwIndex, channelConfig.hwChannel, channelConfig.connectedBusType);


            if (status != XLClass.XLstatus.XL_SUCCESS)
            {
                throw new Exception("HW configuration failed!");
            }

            xlDriver.XL_CloseDriver();
            logMessage("Ediabas HW configured", false);
        }

        private void initAPI(string sgbdFolder)
        {
            if (!API.apiInit())
            {
                throw new Exception(API.apiErrorText());
            }
            logMessage("Ediabas API.apiInit() - done", false);

            if (!API.apiSetConfig("EcuPath", sgbdFolder))
            {
                throw new Exception("Could not set path to SGBD " + sgbdFolder);
            }
            logMessage("Ediabas API.apiSetConfig - done", false);

            if (API.apiState() != API.APIREADY)
            {
                throw new Exception("API is not ready. API Error text: " + API.apiErrorText());
            }
            logMessage("Ediabas API.apiState() - done", false);
        }

        private List<ReceivedResponse> executeService(string ecu, string serviceName, string requestParam, string response, out byte[] rawRequest, out byte[] rawResponse, out string jobStatus)
        {
            rawRequest = null;
            rawResponse = null;
            jobStatus = null;

            LogObject.LogMessage("Executing service: " + serviceName);
            logMessage("Executing service: " + serviceName, true);
            LogObject.LogMessage("Request parameter: " + requestParam);
            logMessage("Request parameter: " + requestParam, false);

            API.apiJob(ecu, serviceName, requestParam, response);
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Restart();
            while (API.apiState() == API.APIBUSY && timer.ElapsedMilliseconds < 10000)
            {
                //try again immediately
            }

            logMessage("Api error after api JOB", false);
            LogObject.LogMessage("Api error after api JOB");

            string text;

            int progress = API.apiJobInfo(out text);
            logMessage("Job info: " + progress, false);
            LogObject.LogMessage("Job info: " + progress);

            ushort countOfResultSets = 0;
            bool boolresult;

            boolresult = API.apiResultSets(out countOfResultSets);

            if (!boolresult)
            {
                throw new Exception("Restult of API job was not received succesfully. API Error text: " + API.apiErrorText());
            }

            logMessage("Job Result: " + boolresult, false);
            LogObject.LogMessage("Job Result: " + boolresult);

            List<ReceivedResponse> receivedResponses = new List<ReceivedResponse>();

            for (ushort setNo = 0; setNo < countOfResultSets + 1; setNo++)//sets are indexed from 0 and countOfResultSets actually means max index !!! :-)
            {
                string setName = "Set " + setNo.ToString("000");
                ushort countOfResultsInOneResultSet;
                API.apiResultNumber(out countOfResultsInOneResultSet, setNo);

                for (ushort resNo = 1; resNo < countOfResultsInOneResultSet + 1; resNo++)//results are indexed from 1 and countOfResultsInOneResultSet is really count of results
                {
                    string resName;
                    API.apiResultName(out resName, resNo, setNo);

                    int resFormat;
                    API.apiResultFormat(out resFormat, resName, setNo);

                    string resultAsString = string.Empty;
                    #region switch
                    switch (resFormat)
                    {
                        case API.APIFORMAT_BINARY:
                            {
                                byte[] resultBin;
                                ushort length;
                                if (!API.apiResultBinary(out resultBin, out length, resName, setNo))
                                {
                                    throw new Exception("Reading APIFORMAT_BINARY error! API Error text: " + API.apiErrorText());
                                }
                                resultAsString = "{";
                                for (int i = 0; i < length; i++)
                                {
                                    resultAsString += resultBin[i].ToString() + ",";
                                }
                                resultAsString = resultAsString.Remove(resultAsString.Length - 1, 1) + "}";

                                if (resName == "_REQUEST")
                                {
                                    rawRequest = new byte[length];
                                    Array.Copy(resultBin, rawRequest, length);
                                }
                                if (resName == "_RESPONSE")
                                {
                                    rawResponse = new byte[length];
                                    Array.Copy(resultBin, rawResponse, length);
                                }
                                break;
                            }

                        case API.APIFORMAT_BYTE:
                            {
                                byte resultByte;
                                if (!API.apiResultByte(out resultByte, resName, setNo))
                                {
                                    throw new Exception("Reading APIFORMAT_BYTE error! API Error text: " + API.apiErrorText());
                                }
                                resultAsString = resultByte.ToString();
                                break;
                            }

                        case API.APIFORMAT_DWORD:
                            {
                                uint resultUint;
                                if (!API.apiResultDWord(out resultUint, resName, setNo))
                                {
                                    throw new Exception("Reading APIFORMAT_DWORD error! API Error text: " + API.apiErrorText());
                                }
                                resultAsString = resultUint.ToString();
                                break;
                            }

                        case API.APIFORMAT_CHAR:
                            {
                                char resultChar;
                                if (!API.apiResultChar(out resultChar, resName, setNo))
                                {
                                    throw new Exception("Reading APIFORMAT_CHAR error! API Error text: " + API.apiErrorText());
                                }
                                resultAsString = resultChar.ToString();
                                break;
                            }

                        case API.APIFORMAT_INTEGER:
                            {
                                short resultShort;
                                if (!API.apiResultInt(out resultShort, resName, setNo))
                                {
                                    throw new Exception("Reading APIFORMAT_INTEGER error! API Error text: " + API.apiErrorText());
                                }
                                resultAsString = resultShort.ToString();
                                break;
                            }

                        case API.APIFORMAT_LONG:
                            {
                                int resultInt;
                                if (!API.apiResultLong(out resultInt, resName, setNo))
                                {
                                    throw new Exception("Reading APIFORMAT_LONG error! API Error text: " + API.apiErrorText());
                                }
                                resultAsString = resultInt.ToString();
                                break;
                            }

                        case API.APIFORMAT_REAL:
                            {
                                double resultDouble;
                                if (!API.apiResultReal(out resultDouble, resName, setNo))
                                {
                                    throw new Exception("Reading APIFORMAT_REAL error! API Error text: " + API.apiErrorText());
                                }
                                resultAsString = resultDouble.ToString();
                                break;
                            }

                        case API.APIFORMAT_TEXT:
                            {
                                string resultString;
                                if (!API.apiResultText(out resultString, resName, setNo, ""))
                                {
                                    throw new Exception("Reading APIFORMAT_TEXT error! API Error text: " + API.apiErrorText());
                                }
                                resultAsString = resultString;
                                if (resName == "JOB_STATUS")
                                {
                                    jobStatus = resultString;
                                }
                                break;
                            }

                        case API.APIFORMAT_WORD:
                            {
                                ushort resultUshort;
                                if (!API.apiResultWord(out resultUshort, resName, setNo))
                                {
                                    throw new Exception("Reading APIFORMAT_WORD error! API Error text: " + API.apiErrorText());
                                }
                                resultAsString = resultUshort.ToString();
                                break;
                            }

                        default:
                            {
                                throw new Exception("Unknown type received as result. API Error text: " + API.apiErrorText());
                            }
                    }
                    #endregion

                    ReceivedResponse receivedResponse = new ReceivedResponse();
                    receivedResponse.resultName = resName;
                    receivedResponse.setName = setName;
                    receivedResponse.resultValue = resultAsString;

                    receivedResponses.Add(receivedResponse);

                    logMessage(string.Format("SetNo: {1} ResNo: {2} Result name: {0} Value: {4} Format: {3}", resName, setName, resNo, resFormat, resultAsString), false);
                    LogObject.LogMessage(string.Format("SetNo: {1} ResNo: {2} Result name: {0} Value: {4} Format: {3}", resName, setName, resNo, resFormat, resultAsString));

                }
            }
            logMessage("*****************************************************************************", false);
            LogObject.LogMessage("*****************************************************************************");

            return receivedResponses;
        }

        private List<ReceivedResponse> filtrResponses(string setName, string resultName, List<ReceivedResponse> dataSet)
        {
            List<ReceivedResponse> results = new List<ReceivedResponse>();

            foreach (ReceivedResponse response in dataSet)
            {
                if (string.IsNullOrEmpty(setName) || (response.setName == setName))
                {
                    if (string.IsNullOrEmpty(resultName) || (response.resultName == resultName))
                    {
                        results.Add(response);
                    }
                }
            }

            return results;
        }

        private void logMessage(string message, bool logTimeStamp)
        {
            Logging.LogMessage(message, Logging.LogFilePrefixOffBoard, logTimeStamp);
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
}
