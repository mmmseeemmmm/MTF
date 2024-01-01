using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutomotiveLighting.MTFCommon;
using ALBusComDriver;
using MTFBusCommunication.Structures;
using System.IO;

namespace MTFBusCommunication
{
    [MTFClass(Name = "BusCommunication", Description = "BusCommunication wrapper for MTF", Icon = MTFIcons.Communication)]
    [MTFClassCategory("Communication")]
    public class MTFBusCommunication : IMTFBusCommunication
    {
        private BusCommunicationDriver busCommunicationDriver;
        private List<BaseConfig> virtualChannels;
        private readonly string basePath = string.Empty;

        public IMTFSequenceRuntimeContext RuntimeContext;
        public IMTFComponentConfigContext ConfigurationContex;

        [MTFConstructor(Description = "Create BusComminucation component")]
        [MTFAdditionalParameterInfo(ParameterName = "config", Description = "In case of HSX - DeviceName parameter is name from connection assistant. In case of Vector used by Ediabus - DeviceName is serial number of HW, if Vector is used for onboard communication, name is Application name from Vector Hardware Config tool")]
        [MTFAdditionalParameterInfo(ParameterName = "basePath", Description = "Path to folder where net config files (*.dbc *.ldf) custom config files (*.xml) and sgbd files(*.prg) are located.")]
        public MTFBusCommunication(List<BaseConfig> config, string basePath)
        {
            LogObject.SetLogMethod(logToMTF);

            busCommunicationDriver = new BusCommunicationDriver(Path.Combine(Environment.CurrentDirectory, "mtfLibs\\BusCommunication"));
            virtualChannels = config;
            this.basePath = string.IsNullOrEmpty(basePath) ? string.Empty : basePath;

        }

        private string networkFileNameInternal;
        private bool networkFileNameExists;
        private string networkFileName
        {
            get
            {
                if (ConfigurationContex == null)
                {
                    throw new Exception("This method can be called just from MTF environment.");
                }
                if (!networkFileNameExists)
                {
                    networkFileNameExists = true;
                    string bp = basePath.EndsWith("\\") ? basePath : basePath + "\\";
                    var files = getFiles(bp, new[] { ".dbc", ".ldf", ".fibex.xml", ".ldf.encrypted", ".dbc.encrypted", ".fibex.xml.encrypted" });
                    files.Sort();
                    var networkFiles = new List<string>();
                    foreach (var file in files)
                    {
                        networkFiles.Add(file.Substring(bp.Length, file.Length - bp.Length));
                    }
                    var messageRes = ConfigurationContex.MessageBoxListBox("Choose network configuration file", "Choose network configuration file for reading possible signals.", networkFiles);
                    if (!string.IsNullOrEmpty(messageRes))
                    {
                        networkFileNameInternal = Path.Combine(bp, messageRes);
                    }
                }

                return networkFileNameInternal;
            }
        }

        private string selectedEcusInternal;
        private bool selectedEcusExists;
        private string selectedEcus
        {
            get
            {
                if (ConfigurationContex == null)
                {
                    throw new Exception("This method can be called just from MTF environment.");
                }
                if (!selectedEcusExists)
                {
                    selectedEcusExists = true;
                    if (!string.IsNullOrEmpty(networkFileName))
                    {
                        var ecusList = busCommunicationDriver.OnBoardGetEcus(networkFileName);
                        selectedEcusInternal = ConfigurationContex.MessageBoxMultiChoise("Choose ECUs", "Choose one or more ECUs defined in network configuration file.", ecusList);
                    }
                }

                return selectedEcusInternal;
            }
        }

        [MTFValueListGetterMethod(SubListSeparator = ".")]
        public List<Tuple<string, object>> InputSignals()
        {
            if (string.IsNullOrEmpty(selectedEcus))
            {
                return null;
            }

            var signals = new List<Tuple<string, object>>();
            var signalsList = busCommunicationDriver.OnBoardGetInputSignals(networkFileName, selectedEcus.Replace(';', ','));

            signalsList.ForEach(s => signals.Add(new Tuple<string, object>(s, s.Split('.'))));

            return signals;
        }

        [MTFValueListGetterMethod(SubListSeparator = ".")]
        public List<Tuple<string, object>> OutputSignals()
        {
            if (string.IsNullOrEmpty(selectedEcus))
            {
                return null;
            }

            var signals = new List<Tuple<string, object>>();
            var signalsList = busCommunicationDriver.OnBoardGetOutputSignals(networkFileName, selectedEcus.Replace(';', ','));

            signalsList.ForEach(s => signals.Add(new Tuple<string, object>(s, s.Split('.'))));

            return signals;
        }

        [MTFValueListGetterMethod]
        public List<Tuple<string, object>> NetworkCfgFiles()
        {
            var networkCfgFiles = new List<Tuple<string, object>>();
            var files = getFiles(basePath, new[] { ".dbc", ".ldf", ".fibex.xml" });
            files.Sort();

            foreach (var file in files)
            {
                var fileName = file.Substring(basePath.Length + 1, file.Length - basePath.Length - 1);
                networkCfgFiles.Add(new Tuple<string, object>(fileName, fileName));
            }

            return networkCfgFiles;
        }

        [MTFValueListGetterMethod]
        public List<Tuple<string, object>> CustomCfgFiles()
        {
            var networkCfgFiles = new List<Tuple<string, object>>();
            foreach (var file in getFiles(basePath, new[] { ".xml" }))
            {
                var fileName = file.Substring(basePath.Length + 1, file.Length - basePath.Length - 1);
                networkCfgFiles.Add(new Tuple<string, object>(fileName, fileName));
            }

            return networkCfgFiles;
        }

        private List<string> getFiles(string path, string[] searchPatterns)
        {
            List<string> files = new List<string>();

            if (searchPatterns == null)
            {
                files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
            }
            else
            {
                foreach (var pattern in searchPatterns)
                {
                    files.AddRange(Directory.GetFiles(path, "*" + pattern, SearchOption.AllDirectories));
                }
            }

            return files;
        }

        private void logToMTF(string message)
        {
            if (RuntimeContext != null)
            {
                RuntimeContext.LogMessage(message, LogLevel.Error);
            }
        }

        public void Dispose()
        {
            if (busCommunicationDriver != null)
            {
                busCommunicationDriver.Dispose();
            }
        }

        #region Properties
        [MTFProperty(Name = "OnBoards", Description = "")]
        public List<OnBoardConfiguration> OnBoards
        {
            //get
            //{
            //    return ConvertFromOnboards(busCommunicationDriver.OnBoards, virtualChannels);
            //}
            set
            {
                busCommunicationDriver.OnBoards = ConvertToOnBoards(value, virtualChannels);
            }
        }


        [MTFProperty(Name = "OffBoardDTS", Description = "Set Off Board configuration for DTS")]
        public MTFOffBoardConfig OffBoard
        {
            set { busCommunicationDriver.OffBoard = ConvertToOffBoard(value); }
        }

        [MTFProperty(Name = "OffBoardEdiabas", Description = "Set Off Board configuration for Ediabas")]
        public MTFOffBoardConfigEdiabas OffBoardEdiabas
        {
            set { busCommunicationDriver.OffBoardEdiabas = ConvertToOffBoardEdiabas(value); }
        }

        [MTFProperty(Name = "Status", Description = "Get status of BusCommunication")]
        public MTFBusComDriverStatusEnum Status
        {
            get { return ConvertToBusComDriverStatus(busCommunicationDriver.Status); }
        }



        #endregion

        #region Methods

        [MTFMethod(DisplayName = "Initialize", Description = "Init VCF, INIT DTS, Start VCF communication")]
        public void Initialize()
        {
            busCommunicationDriver.Initialize();
        }

        [MTFMethod(DisplayName = "Force Reinitialize", Description = "Force reinitialize VCF, DTS, Start VCF communication")]
        public void ForceReinitialize()
        {
            busCommunicationDriver.ForceReinitialize();
        }

        [MTFMethod(DisplayName = "OnBoardStart", Description = "Start Cyclic communication")]
        public void OnBoardStart()
        {
            busCommunicationDriver.OnBoardStart();
        }

        [MTFMethod(DisplayName = "OnBoardStop", Description = "Stop Cyclic communication")]
        public void OnBoardStop()
        {
            busCommunicationDriver.OnBoardStop();
        }

        [MTFMethod(DisplayName = "OffBoardStart", Description = "Open LLs, Start Diagnostic communication")]
        public void OffBoardStart()
        {
            busCommunicationDriver.OffBoardStart();
        }

        [MTFMethod(DisplayName = "OffboardStop", Description = "Stop Diagnostic communication, close LLs")]
        public void OffBoardStop()
        {
            busCommunicationDriver.OffBoardStop();
        }

        [MTFMethod(DisplayName = "Start", Description = "Open LLs, Start Diagnostic communication")]
        public void Start()
        {
            busCommunicationDriver.Start();
        }

        [MTFMethod(DisplayName = "Stop", Description = "Stop Diagnostic communication, close LLs")]
        public void Stop()
        {
            busCommunicationDriver.Stop();
        }

        [MTFMethod(DisplayName = "OffBoard Varian Identification and selection", Description = "")]
        public string OffBoardVariantIdentificationAndSelection(string logicalLinkName)
        {
            return busCommunicationDriver.OffBoardVariantIdentificationAndSelection(logicalLinkName);
        }

        [MTFMethod(DisplayName = "OffBoard Execute service", Description = "")]
        [MTFAdditionalParameterInfo(ParameterName = "logicalLinkName", Description = "In case of DTS use logical link from system configurator. In case of Ediabas use sgbd file without extension e.g.(FLE02_L)")]
        public MTFOffBoardResponse OffBoardExecuteService(string logicalLinkName, MTFOffBoardService serviceSetting)
        {
            var result = busCommunicationDriver.OffBoardExecuteService(logicalLinkName, ConvertToOffBoardService(serviceSetting));
            mtfOffBoardResponse = ConvertFromOffBoardResponse(result);
            return mtfOffBoardResponse;
        }

        [MTFMethod(DisplayName = "OffBoard Execute Services In Parallel", Description = "")]
        [MTFAdditionalParameterInfo(ParameterName = "logicalLinkName", Description = "In case of DTS use logical link from system configurator. In case of Ediabas use sgbd file without extension e.g.(FLE02_L)")]
        public List<MTFOffBoardLogicalLinkParallelResponses> OffBoardExecuteServicesInParallel(List<MTFOffBoardLogicalLinkParallelServices> offBoardParallelServicesSetting)
        {
            var result = this.busCommunicationDriver.OffBoardExecuteServicesInParallel(this.ConvertToOffBoardLogicalLinkParallelServices(offBoardParallelServicesSetting));
            return this.ConvertToMtfOffBoardLogicalLinkParallelResponses(result);
        }

        [MTFMethod(DisplayName = "OffBoard Execute service cyclically", Description = "")]
        [MTFAdditionalParameterInfo(ParameterName = "logicalLinkName", Description = "In case of DTS use logical link from system configurator. In case of Ediabas use sgbd file without extension e.g.(FLE02_L)")]
        [MTFAdditionalParameterInfo(ParameterName = "cycleTime", DisplayName = "cycleTime [ms]")]
        public void OffBoardExecuteServiceCyclically(string logicalLinkName, MTFOffBoardService serviceSetting, int cycleTime)
        {
            busCommunicationDriver.OffBoardExecuteServiceCyclically(logicalLinkName, ConvertToOffBoardService(serviceSetting), cycleTime);
        }

        [MTFMethod(DisplayName = "Stop OffBoard Execute service cyclically", Description = "Stop cyclical execution of service which is started by method 'OffBoard Execute service cyclically'")]
        public void StopOffBoardExecuteServiceCyclically()
        {
            busCommunicationDriver.StopOffBoardExecuteServiceCyclically();
        }



        [MTFMethod(DisplayName = "OffBoard Flash Job", Description = "")]
        public List<MTFOffBoardFlashJobResult> OffBoardFlashJob(List<MTFOffBoardFlashJobSetting> flashJobSetttings)
        {
            offBoardFlashJobResults = ConvertToMTFFlashJobResults(busCommunicationDriver.OffBoardFlashJob(ConvertFromMTFFlashJobSettings(flashJobSetttings)));
            return offBoardFlashJobResults;
        }




        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFMethod(DisplayName = "OnBoard Send frame once", Description = "")]
        public void OnBoardSendFrameOnce(int virtualChannel, uint frameId, byte[] data)
        {
            var channel = GetVirtualChannel(virtualChannel);
            busCommunicationDriver.OnBoardSendFrameOnce(channel.DeviceName, channel.GetBusType(), channel.BusChannel, frameId, data);
        }

        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFMethod(DisplayName = "OnBoard Receive frame once", Description = "")]
        public byte[] OnBoardReceiveFrameOnce(int virtualChannel, uint frameId, uint resFrameId, byte[] data, uint timeout)
        {
            var channel = GetVirtualChannel(virtualChannel);
            return busCommunicationDriver.OnBoardReceiveFrameOnce(channel.DeviceName, channel.GetBusType(), channel.BusChannel, frameId, resFrameId, data, timeout);
        }


        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFAdditionalParameterInfo(ParameterName = "frameName", ValueListName = "InputSignals")]
        [MTFAdditionalParameterInfo(ParameterName = "signalName", ValueListName = "InputSignals", ValueListLevel = 1, ValueListParentName = "frameName")]
        [MTFMethod(DisplayName = "OnBoard Get Signal", Description = "")]
        public MTFOnBoardSignal OnBoardGetSignal(int virtualChannel, string frameName, string signalName)
        {
            var channel = GetVirtualChannel(virtualChannel);
            var output = busCommunicationDriver.OnBoardGetSignal(channel.DeviceName, channel.GetBusType(), channel.BusChannel, frameName, signalName);
            return ConvertToMTFOnBoardSignal(output);
        }



        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFAdditionalParameterInfo(ParameterName = "frameName", ValueListName = "OutputSignals")]
        [MTFAdditionalParameterInfo(ParameterName = "signalName", ValueListName = "OutputSignals", ValueListLevel = 1, ValueListParentName = "frameName")]
        [MTFMethod(DisplayName = "OnBoard Set Signal", Description = "")]
        public void OnBoardSetSignal(int virtualChannel, string frameName, string signalName, string signalValue)
        {
            var channel = GetVirtualChannel(virtualChannel);
            busCommunicationDriver.OnBoardSetSignal(channel.DeviceName, channel.GetBusType(), channel.BusChannel, frameName, signalName, signalValue);
        }

        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFMethod(DisplayName = "OnBoard Get Global Variable", Description = "")]
        public string OnBoardGetGlobalVariable(int virtualChannel, string variableName)
        {
            var channel = GetVirtualChannel(virtualChannel);
            return busCommunicationDriver.OnBoardGetGlobalVariable(channel.DeviceName, channel.GetBusType(), channel.BusChannel, variableName);
        }

        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFMethod(DisplayName = "OnBoard Set Global Variable", Description = "")]
        public void OnBoardSetGlobalVariable(int virtualChannel, string variableName, string variableValue)
        {
            var channel = GetVirtualChannel(virtualChannel);
            busCommunicationDriver.OnBoardSetGlobalVariable(channel.DeviceName, channel.GetBusType(), channel.BusChannel, variableName, variableValue);
        }

        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFMethod(DisplayName = "OnBoard Active Shedule Table", Description = "")]
        public void OnBoardActiveScheduleTable(int virtualChannel, string scheduleTable)
        {
            var channel = GetVirtualChannel(virtualChannel);
            busCommunicationDriver.OnBoardActiveScheduleTable(channel.DeviceName, channel.GetBusType(), channel.BusChannel, scheduleTable);
        }

        [MTFMethod(DisplayName = "Set Logging", Description = "This activity is obsolete. Set logging in ALUtils.dll configuration file")]
        [MTFAdditionalParameterInfo(ParameterName = "enable", Description = "Enable or disable log files generating")]
        [MTFAdditionalParameterInfo(ParameterName = "logPath", Description = @"Path to log files. Leave empty for default path (\MTF\Server)")]
        [MTFAdditionalParameterInfo(ParameterName = "maxLogFilesCount", Description = "Maximal count of kept files before delete. 0 means do not delete anything.")]
        public void SetLogging(bool enable, string logPath, int maxLogFilesCount)
        {
            //busCommunicationDriver.SetLogging(enable, logPath, maxLogFilesCount);
        }

        [MTFMethod(DisplayName = "OffBoardExecuteOTX", Description = "")]
        [MTFAdditionalParameterInfo(ParameterName = "serviceSetting", DisplayName = "service name is OTX Script or Project", Description = "service name of the OTX script with extension. e.g. MyScript.otx")]
        public MTFOffBoardResponse OffBoardExecuteOTX(MTFOffBoardService serviceSetting)
        {
            serviceSetting.ServiceName = Path.Combine(basePath, serviceSetting.ServiceName);
            var result = busCommunicationDriver.OffBoardExecuteOTX(ConvertToOffBoardService(serviceSetting));
            mtfOffBoardResponse = ConvertFromOffBoardResponse(result);
            return mtfOffBoardResponse;
        }

        [MTFMethod(DisplayName = "OffBoard Read Error Memory", Description = "")]
        [MTFAdditionalParameterInfo(ParameterName = "logicalLinkName", Description = "In case of DTS use logical link from system configurator. In case of Ediabas use sgbd file without extension e.g.(FLE02_L)")]
        public MTFOffBoardErrorMemoryResult OffBoardReadErrorMemory(string logicalLinkName, MTFOffBoardErrorMemoryRequest serviceSetting)
        {
            return offBoardReadErrorMemory(logicalLinkName, serviceSetting);
        }

        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFMethod(DisplayName = "OnBoard Send CANFD Frame Cyclic")]
        public void OnBoardSendCANFDFrameCyclic(int virtualChannel, uint frameId, string data, uint timeOutFrame)
        {
            var channel = GetVirtualChannel(virtualChannel);
            busCommunicationDriver.OnBoardSendCANFDFrameCyclic(channel.DeviceName, channel.GetBusType(), channel.BusChannel, frameId, data, timeOutFrame);
        }

        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFMethod(DisplayName = "OnBoard Send CANFD Frames Cyclic")]
        public void OnBoardSendCANFDFramesCyclic(int virtualChannel, uint frameId, IEnumerable<string> data, uint timeOutFrame, uint timeOutSubList)
        {
            var channel = GetVirtualChannel(virtualChannel);
            busCommunicationDriver.OnBoardSendCANFDFrameCyclic(channel.DeviceName, channel.GetBusType(), channel.BusChannel, frameId, data, timeOutFrame, timeOutSubList);
        }

        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFMethod(DisplayName = "OnBoard Send CANFD Frame Data Cyclic")]
        public void OnBoardSendCANFDFrameDataCyclic(int virtualChannel, List<MTFFrameData> frameData, uint timeOutFrame, uint timeOutSubList)
        {
            var channel = GetVirtualChannel(virtualChannel);
            Dictionary<uint, IEnumerable<string>> data = new Dictionary<uint, IEnumerable<string>>();
            frameData.ForEach(d => data[d.Id] = d.Data);
            busCommunicationDriver.OnBoardSendCANFDFrameCyclic(channel.DeviceName, channel.GetBusType(), channel.BusChannel, data, timeOutFrame, timeOutSubList);
        }

        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFMethod(DisplayName = "OnBoard Send Frame Cyclic")]
        public void OnBoardSendFrameCyclic(int virtualChannel, uint frameId, string data, uint timeOutFrame)
        {
            var channel = GetVirtualChannel(virtualChannel);
            busCommunicationDriver.OnBoardSendFrameCyclic(channel.DeviceName, channel.GetBusType(), channel.BusChannel, frameId, data, timeOutFrame);
        }

        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFMethod(DisplayName = "OnBoard Send Frames Cyclic")]
        public void OnBoardSendFramesCyclic(int virtualChannel, uint frameId, IEnumerable<string> data, uint timeOutFrame, uint timeOutSubList)
        {
            var channel = GetVirtualChannel(virtualChannel);
            busCommunicationDriver.OnBoardSendFrameCyclic(channel.DeviceName, channel.GetBusType(), channel.BusChannel, frameId, data, timeOutFrame, timeOutSubList);
        }

        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFMethod(DisplayName = "OnBoard Send Frame Data Cyclic")]
        public void OnBoardSendFrameDataCyclic(int virtualChannel, List<MTFFrameData> frameData, uint timeOutFrame, uint timeOutSubList)
        {
            var channel = GetVirtualChannel(virtualChannel);
            Dictionary<uint, IEnumerable<string>> data = new Dictionary<uint, IEnumerable<string>>();
            frameData.ForEach(d => data[d.Id] = d.Data);
            busCommunicationDriver.OnBoardSendFrameCyclic(channel.DeviceName, channel.GetBusType(), channel.BusChannel, data, timeOutFrame, timeOutSubList);
        }

        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFMethod(DisplayName = "Set Global Adapter Project Variable")]
        public void SetGlobalAdapterProjectVariable(int virtualChannel, string variableName, string value)
        {
            var channel = GetVirtualChannel(virtualChannel);
            busCommunicationDriver.SetGlobalAdapterProjectVariable(channel.DeviceName, channel.GetBusType(), channel.BusChannel, variableName, value);
        }

        [MTFAllowedParameterValue("virtualChannel", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannel", "CAN 10", 10)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 1", 11)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 2", 12)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 3", 13)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 4", 14)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 5", 15)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 6", 16)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 7", 17)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 8", 18)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 9", 19)]
        [MTFAllowedParameterValue("virtualChannel", "LIN 10", 20)]
        [MTFMethod(DisplayName = "Get Global Adapter Project Variable")]
        public string GetGlobalAdapterProjectVariable(int virtualChannel, string variableName)
        {
            var channel = GetVirtualChannel(virtualChannel);
            return busCommunicationDriver.GetGlobalAdapterProjectVariable(channel.DeviceName, channel.GetBusType(), channel.BusChannel, variableName);
        }
        
        [MTFMethod(DisplayName = "Get OffBoard Request Parameters From Validation Table", Description = "")]
        [MTFAdditionalParameterInfo(ParameterName = "validationTableName", Description = "Name of validation table where 'row name' = 'parameter name' and 'row actual value' = 'parameter value'.")]
        public List<MTFOffBoardRequestParameter> GetOffBoardRequestParametersFromValidationTable(string validationTableName)
        {
            List<MTFOffBoardRequestParameter> requestParameters = new List<MTFOffBoardRequestParameter>();

            var validationTableRows = RuntimeContext.GetValidationTableRows(validationTableName);

            foreach (var row in validationTableRows)
            {
                requestParameters.Add(new MTFOffBoardRequestParameter
                                      {
                                          Name = row.RowName,
                                          Value = (string)row.Value
                                      });
            }

            return requestParameters;
        }

        [MTFMethod(DisplayName = "Get OffBoard Response Settings From Validation Table", Description = "")]
        [MTFAdditionalParameterInfo(ParameterName = "validationTableName", Description = "Name of validation table where 'row name' = 'parameter key'.")]
        public List<MTFOffBoardResponseSetting> GetOffBoardResponseSettingsFromValidationTable(string validationTableName)
        {
            List<MTFOffBoardResponseSetting> responseSettings = new List<MTFOffBoardResponseSetting>();

            var validationTableRows = RuntimeContext.GetValidationTableRows(validationTableName);

            foreach (var row in validationTableRows)
            {
                responseSettings.Add(new MTFOffBoardResponseSetting
                {
                    Keys = new List<string> { row.RowName }
                });
            }

            return responseSettings;
        }

        [MTFMethod(DisplayName = "Check OffBoard Result Against Validation Table", Description = "")]
        public void CheckOffBoardResultAgainstValidationTable(MTFOffBoardResponse offboardResponse, string sourceValidationTableName, string emptyValidationTableName, bool validate)
        {
            var sourceValidationTableRows = RuntimeContext.GetValidationTableRows(sourceValidationTableName);
            List<ValidationRowContainer> rowsToValidate = new List<ValidationRowContainer>();

            foreach (var result in offboardResponse.Results)
            {
                if (result.Keys.Count > 0 && result.Values.Count > 0)
                {
                    string requiredValue = string.Empty;
                    var sourceValidationTableRow = sourceValidationTableRows.FirstOrDefault(x => x.RowName == result.Keys[0]);

                    if (sourceValidationTableRow != null)
                    {
                        requiredValue = (string)sourceValidationTableRow.Value;
                    }

                    string readedStringValue = result.Values[0];
                    decimal readedDoubleValue, requredDoubleValue;
                    string modifiedReadedValue = readedStringValue.Replace(',', '.');
                    if (decimal.TryParse(modifiedReadedValue, out readedDoubleValue) && decimal.TryParse(requiredValue, out requredDoubleValue))
                    {
                        int rounding = 0;
                        if (requiredValue.Contains('.'))
                        {
                            rounding = requiredValue.Substring(requiredValue.IndexOf('.') + 1).Length;
                        }
                        readedStringValue = Math.Round(readedDoubleValue, rounding).ToString("F" + rounding, CultureInfo.InvariantCulture);
                    }

                    if (validate)
                    {
                        rowsToValidate.Add(new ValidationRowContainer
                        {
                            RowName = result.Keys[0],
                            Value = readedStringValue,
                            ValidationColumns = new List<ValidationColumn> { new ValidationColumn() { Name = "Required",Value = new[] {requiredValue}}}
                        });
                    }
                    else
                    { 
                        rowsToValidate.Add(new ValidationRowContainer
                        {
                            RowName = result.Keys[0],
                            Value = readedStringValue,
                        });
                    }
                }
            }
            
            RuntimeContext.AddRangeToValidationTable(emptyValidationTableName, rowsToValidate);
        }

        [MTFMethod(DisplayName = "Daimler Create Coding Package", Description = "")]
        [MTFAdditionalParameterInfo(ParameterName = "logicalLinkName", Description = "In case of DTS use logical link from system configurator. In case of Ediabas use sgbd file without extension e.g.(FLE02_L)")]
        public List<MTFOffBoardLogicalLinkParallelServices> DaimlerPrepareCodingPackage(List<MTFOffBoardLogicalLinkParallelServices> offBoardParallelServicesSetting)
        {
            return offBoardParallelServicesSetting;
        }

        [MTFMethod(DisplayName = "Daimler Coding", Description = "")]
        [MTFAdditionalParameterInfo(ParameterName = "logicalLinkName", Description = "In case of DTS use logical link from system configurator. In case of Ediabas use sgbd file without extension e.g.(FLE02_L)")]
        public List<MTFDaimlerCodingOffboardResponses> DaimlerCoding(List<MTFOffBoardLogicalLinkParallelServices> offBoardParallelServicesSetting)
        {
            var result = this.busCommunicationDriver.OffBoardExecuteServicesInParallel(this.ConvertToOffBoardLogicalLinkParallelServices(offBoardParallelServicesSetting));
            return this.ConvertToMTFDaimlerCodingOffboardResponses(result);
        }

        #endregion


        #region private

        private List<MTFOffBoardFlashJobResult> ConvertToMTFFlashJobResults(List<OffBoardFlashJobResult> input)
        {
            var output = new List<MTFOffBoardFlashJobResult>();
            input.ForEach(x => output.Add(new MTFOffBoardFlashJobResult()
            {
                ExecutionState = x.ExecutionState,
                FlashJobName = x.FlashJobName,
                JobResult = x.JobResult,
                LogicalLink = x.LogicalLink,
                SessionName = x.SessionName
            }));
            return output;
        }

        private List<OffBoardFlashJobSetting> ConvertFromMTFFlashJobSettings(List<MTFOffBoardFlashJobSetting> flashJobSetttings)
        {
            var output = new List<OffBoardFlashJobSetting>();
            flashJobSetttings.ForEach(x => output.Add(new OffBoardFlashJobSetting()
            {
                FlashJobName = x.FlashJobName,
                LogicalLink = x.LogicalLink,
                SessionName = x.SessionName
            }));
            return output;
        }

        private MTFBusComDriverStatusEnum ConvertToBusComDriverStatus(BusCommunicationDriver.StatusEnum statusEnum)
        {
            switch (statusEnum)
            {
                case BusCommunicationDriver.StatusEnum.Configured:
                    return MTFBusComDriverStatusEnum.Configured;
                case BusCommunicationDriver.StatusEnum.Initialized:
                    return MTFBusComDriverStatusEnum.Initialized;
                case BusCommunicationDriver.StatusEnum.Connected:
                    return MTFBusComDriverStatusEnum.Connected;
                case BusCommunicationDriver.StatusEnum.Released:
                    return MTFBusComDriverStatusEnum.Released;
                case BusCommunicationDriver.StatusEnum.NotDefined:
                    return MTFBusComDriverStatusEnum.NotDefined;
                default:
                    return MTFBusComDriverStatusEnum.NotDefined;
            }
        }

        private MTFOnBoardSignal ConvertToMTFOnBoardSignal(OnBoardSignal output)
        {
            return new MTFOnBoardSignal()
            {
                Unit = output.Unit,
                Value = output.Value
            };
        }

        private List<OffBoardLogicalLinkParallelServices> ConvertToOffBoardLogicalLinkParallelServices(List<MTFOffBoardLogicalLinkParallelServices> mtfOffBoardLogicalLinkParallelServices)
        {
            List<OffBoardLogicalLinkParallelServices> offBoardLogicalLinkParallelServices = new List<OffBoardLogicalLinkParallelServices>();

            foreach (var mtfOffBoardLogicalLinkParallelService in mtfOffBoardLogicalLinkParallelServices)
            {
                var offBoardLogicalLinkParallelService = new OffBoardLogicalLinkParallelServices {LogicalLink = mtfOffBoardLogicalLinkParallelService.LogicalLink, OffBoardParallelServices = new List<OffBoardParallelServices>()};
                
                foreach (var offBoardService in mtfOffBoardLogicalLinkParallelService.OffBoardServices)
                {
                    offBoardLogicalLinkParallelService.OffBoardParallelServices.Add(new OffBoardParallelServices{OffBoardService = this.ConvertToOffBoardService(offBoardService)});
                }

                offBoardLogicalLinkParallelServices.Add(offBoardLogicalLinkParallelService);
            }

            return offBoardLogicalLinkParallelServices;
        }

        private List<MTFOffBoardLogicalLinkParallelResponses> ConvertToMtfOffBoardLogicalLinkParallelResponses(List<OffBoardLogicalLinkParallelServices> offBoardLogicalLinkParallelServices)
        {
            List<MTFOffBoardLogicalLinkParallelResponses> mtfOffBoardLogicalLinkParallelResponses = new List<MTFOffBoardLogicalLinkParallelResponses>();

            foreach (var offBoardLogicalLinkParallelService in offBoardLogicalLinkParallelServices)
            {
                var mtfOffBoardLogicalLinkParallelResponse = new MTFOffBoardLogicalLinkParallelResponses {LogicalLink = offBoardLogicalLinkParallelService.LogicalLink, OffBoardResponses = new List<MTFOffBoardResponse>()};

                foreach (var OffBoardParallelService in offBoardLogicalLinkParallelService.OffBoardParallelServices)
                {
                    mtfOffBoardLogicalLinkParallelResponse.OffBoardResponses.Add(this.ConvertFromOffBoardResponse(OffBoardParallelService.OffBoardResponse));
                }

                mtfOffBoardLogicalLinkParallelResponses.Add(mtfOffBoardLogicalLinkParallelResponse);
            }

            return mtfOffBoardLogicalLinkParallelResponses;
        }

        private List<MTFDaimlerCodingOffboardResponses> ConvertToMTFDaimlerCodingOffboardResponses(List<OffBoardLogicalLinkParallelServices> offBoardLogicalLinkParallelServices)
        {
            List<MTFDaimlerCodingOffboardResponses> mtfDaimlerCodingOffboardResponses = new List<MTFDaimlerCodingOffboardResponses>();

            foreach (var offBoardLogicalLinkParallelService in offBoardLogicalLinkParallelServices)
            {
                MTFDaimlerCodingOffboardResponses mtfDaimlerCodingOffboardResponse = new MTFDaimlerCodingOffboardResponses { LogicalLink = offBoardLogicalLinkParallelService.LogicalLink, 
                    OffBoardResponses = new List<MTFOffBoardResponse>(), RawRequestForZenZefi = new List<byte>()};

                foreach (var offBoardParallelService in offBoardLogicalLinkParallelService.OffBoardParallelServices)
                {
                    mtfDaimlerCodingOffboardResponse.OffBoardResponses.Add(this.ConvertFromOffBoardResponse(offBoardParallelService.OffBoardResponse));
                    mtfDaimlerCodingOffboardResponse.RawRequestForZenZefi.AddRange(offBoardParallelService.OffBoardResponse.RawRequest.Skip(1));
                }

                mtfDaimlerCodingOffboardResponses.Add(mtfDaimlerCodingOffboardResponse);
            }

            return mtfDaimlerCodingOffboardResponses;
        }

        private List<MTFOffBoardResponse> ConvertFromOffBoardResponses(List<OffBoardResponse> offBoardResponses)
        {
            List<MTFOffBoardResponse> mtfOffBoardResponses = new List<MTFOffBoardResponse>();

            foreach (var offBoardResponse in offBoardResponses)
            {
                mtfOffBoardResponses.Add(this.ConvertFromOffBoardResponse(offBoardResponse));
            }

            return mtfOffBoardResponses;
        }

        private OffBoardService ConvertToOffBoardService(MTFOffBoardService serviceSetting)
        {
            if (serviceSetting == null)
            {
                return null;
            }
            var output = new OffBoardService();
            output.RequestParameters = ConvertToOffBoardRequestParameters(serviceSetting.RequestParameters);
            output.Responses = ConvertResponses(serviceSetting.Responses);
            output.ServiceName = serviceSetting.ServiceName;
            return output;
        }

        private List<OffBoardRequestParameter> ConvertToOffBoardRequestParameters(List<MTFOffBoardRequestParameter> input)
        {
            if (input == null)
            {
                return null;
            }
            var output = new List<OffBoardRequestParameter>();
            input.ForEach(x => output.Add(new OffBoardRequestParameter() { Name = x.Name, Value = x.Value }));
            return output;
        }

        private List<OffBoardResponseSetting> ConvertResponses(List<MTFOffBoardResponseSetting> source)
        {
            if (source == null)
            {
                return null;
            }
            var output = new List<OffBoardResponseSetting>();
            source.ForEach(x =>
            {
                var item = new OffBoardResponseSetting();
                item.Keys = x.Keys;
                item.IgnoredValues = x.IgnoredValues;
                output.Add(item);
            });
            return output;
        }

        private MTFOffBoardResponse ConvertFromOffBoardResponse(OffBoardResponse result)
        {
            var output = new MTFOffBoardResponse();
            output.RawRequest = result.RawRequest;
            output.RawResponse = result.RawResponse;
            output.ExecState = result.ExecState;
            if (result.Results != null)
            {
                output.Results = new List<MTFOffBoardServiceResult>();
                result.Results.ForEach(r =>
                {
                    var item = new MTFOffBoardServiceResult();
                    if (r.Keys != null)
                    {
                        item.Keys = new List<string>();
                        r.Keys.ForEach(k => item.Keys.Add(k));
                    }
                    if (r.Values != null)
                    {
                        item.Values = new List<string>();
                        r.Values.ForEach(v => item.Values.Add(v));
                    }
                    if (r.IgnoredValues != null)
                    {
                        item.IgnoredValues = new List<string>();
                        r.IgnoredValues.ForEach(i => item.IgnoredValues.Add(i));
                    }
                    output.Results.Add(item);
                });
            }
            return output;
        }

        private OffBoardConfig ConvertToOffBoard(MTFOffBoardConfig value)
        {
            return new OffBoardConfig()
            {
                ByteTraceInterfaceName = value.ByteTraceInterfaceName,
                EnableByteTrace = value.EnableByteTrace,
                LogicalLinks = value.LogicalLinks,
                ProjectName = value.ProjectName,
                VIT = value.VIT
            };
        }

        private OffBoardConfigEdiabas ConvertToOffBoardEdiabas(MTFOffBoardConfigEdiabas value)
        {
            var channel = GetVirtualChannel(value.VirtualChannel);
            return new OffBoardConfigEdiabas()
            {
                SerialNumber = channel.DeviceName,
                BusChannel = channel.BusChannel,
                SgbdFolder = basePath,
            };
        }

        private List<OnBoardConfig> ConvertToOnBoards(List<OnBoardConfiguration> value, List<BaseConfig> baseConfig)
        {
            List<OnBoardConfig> output = new List<OnBoardConfig>();
            value.ForEach(x => output.Add(CreateOnBoardConfigItem(x, baseConfig)));
            return output;
        }

        private OnBoardConfig CreateOnBoardConfigItem(OnBoardConfiguration config, List<BaseConfig> baseConfigList)
        {
            var item = new OnBoardConfig();
            var baseConfig = baseConfigList.FirstOrDefault(x => x.VirtualChannel == config.VirtualChannel);
            if (baseConfig == null)
            {
                throw new Exception("Check your components configuration!");
            }
            item.DeviceName = baseConfig.DeviceName;
            item.BusType = baseConfig.GetBusType();
            item.BusChannel = baseConfig.BusChannel;
            if (string.IsNullOrEmpty(config.NetcCfgFile))
            {
                item.NetcCfgFile = string.Empty;
            }
            else
            {
                item.NetcCfgFile = Path.Combine(basePath, config.NetcCfgFile);
            }
            if (string.IsNullOrEmpty(config.CustomCgfFile))
            {
                item.CustomCgfFile = string.Empty;
            }
            else
            {
                item.CustomCgfFile = baseConfig.HWType == 1 ? config.CustomCgfFile : Path.Combine(basePath, config.CustomCgfFile);
            }
            item.BusNodes = config.BusNodes;
            item.LINScheduleTable = config.LINSheduleTable;
            item.LINMasterNode = config.LINMasterNode;
            item.ClusterName = config.ClusterName;

            if (baseConfig.HWType == 0)
            {
                item.HWType = hwType.HSX;
            }
            if (baseConfig.HWType == 1)
            {
                item.HWType = hwType.Vector;
            }

            return item;
        }

        private List<OnBoardConfiguration> ConvertFromOnboards(List<OnBoardConfig> value, List<BaseConfig> baseConfig)
        {
            var output = new List<OnBoardConfiguration>();
            value.ForEach(x => output.Add(CreateOnBoardConfigurationItem(x, baseConfig)));
            return output;
        }

        private OnBoardConfiguration CreateOnBoardConfigurationItem(OnBoardConfig config, List<BaseConfig> baseConfigList)
        {
            var item = new OnBoardConfiguration();
            var baseConfig = baseConfigList.FirstOrDefault(x => x.DeviceName == config.DeviceName && x.BusChannel == config.BusChannel && x.GetBusType() == config.BusType);
            if (baseConfig == null)
            {
                throw new Exception("Check your components configuration!");
            }
            item.VirtualChannel = baseConfig.VirtualChannel;
            if (config.NetcCfgFile == null)
            {
                item.NetcCfgFile = null;
            }
            else
            {
                item.NetcCfgFile = config.NetcCfgFile.Replace(basePath + Path.DirectorySeparatorChar, string.Empty);
            }
            if (config.CustomCgfFile == null)
            {
                item.CustomCgfFile = null;
            }
            else
            {
                item.CustomCgfFile = config.CustomCgfFile.Replace(basePath + Path.DirectorySeparatorChar, string.Empty);
            }

            item.BusNodes = config.BusNodes;
            item.LINSheduleTable = config.LINScheduleTable;
            item.LINMasterNode = config.LINMasterNode;
            return item;
        }

        private BaseConfig GetVirtualChannel(int virtualChannel)
        {
            var virtualCh = virtualChannels.FirstOrDefault(x => x.VirtualChannel == virtualChannel);

            if (virtualCh == null)
            {
                throw new Exception("This virtual CAN channel is not assigned to any real CAN channel! Check your components configuration.");
            }
            else
            {
                return virtualCh;
            }
        }

        private MTFOffBoardErrorMemoryResult offBoardReadErrorMemory(string logicalLinkName, MTFOffBoardErrorMemoryRequest serviceSetting)
        {
            MTFOffBoardService offboardService = new MTFOffBoardService();

            if (serviceSetting != null)
            {
                offboardService.ServiceName = serviceSetting.ServiceName;
                offboardService.RequestParameters = serviceSetting.RequestParameters;

                offboardService.Responses = new List<MTFOffBoardResponseSetting>
                {
                    new MTFOffBoardResponseSetting()
                    {
                        Keys = new List<string>
                        {
                            serviceSetting.KeyForErrorCode
                        }
                    },
                    new MTFOffBoardResponseSetting()
                    {
                        Keys = new List<string>
                        {
                            serviceSetting.KeyForErrorText
                        }
                    }
                };
            }

            MTFOffBoardErrorMemoryResult resultFinal = new MTFOffBoardErrorMemoryResult();

            var res = OffBoardExecuteService(logicalLinkName, offboardService);

            if (res != null && res.Results.Count >= 2)
            {
                resultFinal.SetErrorCodesAll(res.Results[0].Values);
                resultFinal.SetErrorTextsAll(res.Results[1].Values);

                List<int> unignoredIndexes = new List<int>();

                for (int i = 0; i < res.Results[0].Values.Count; i++)
                {
                    if (serviceSetting.IgnoredErrorCodes != null)
                    {
                        if (!serviceSetting.IgnoredErrorCodes.Contains(res.Results[0].Values[i]))
                        {
                            unignoredIndexes.Add(i);
                        }
                    }
                    else
                    {
                        unignoredIndexes.Add(i);
                    }
                  
                }

                foreach (var item in unignoredIndexes)
                {
                    if((res.Results[0].Values.Count>item))
                    {
                        resultFinal.AddErrorCodesUnignored(res.Results[0].Values[item]);
                    }
                    if ((res.Results[1].Values.Count > item))
                    {
                        resultFinal.AddErrorTextsUnignored(res.Results[1].Values[item]);
                    }
                }
            }
            else
            {
                throw new Exception("OffBoard result is null or not complete. Please check service setting if request parameters, keys for error codes and error texts are filled.");
            }
            

            return resultFinal;
        }


        #endregion

        #region Remove!!!!
        private MTFOffBoardResponse mtfOffBoardResponse = null;
        private List<MTFOffBoardFlashJobResult> offBoardFlashJobResults = null;

        [MTFMethod(DisplayName = "Get Value by Index")]
        public string GetValue(int resultIndex, int valueIndex)
        {
            return mtfOffBoardResponse.Results[resultIndex].Values[valueIndex];
        }

        [MTFMethod(DisplayName = "Get Key by Index")]
        public string GetKey(int resultIndex, int valueIndex)
        {
            return mtfOffBoardResponse.Results[resultIndex].Keys[valueIndex];
        }

        [MTFMethod(DisplayName = "Get OnBoards Item by index")]
        public OnBoardConfiguration GetOnBoardItem(int index)
        {
            return ConvertFromOnboards(busCommunicationDriver.OnBoards, virtualChannels)[index];
        }

        [MTFMethod(DisplayName = "Get OffBoardFlashJobResult Item by index")]
        public MTFOffBoardFlashJobResult GetFlashJobResult(int index)
        {
            return offBoardFlashJobResults[index];
        }
        #endregion


    }

}
