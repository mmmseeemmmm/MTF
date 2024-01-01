using System;
using System.Collections.Generic;
using System.Linq;
using AutomotiveLighting.MTFCommon;
using LED_Module_Check_Common;
using LED_Module_Check_Core;
using LED_Module_Check_Core.Source;
using LED_Module_Check_Core.TechnoTeam;

namespace LEDModuleCheck
{
    public class LEDModuleCheckControl : IDisposable, ICanStop
    {
        private CoreInterface coreInterface = null;
        private System.Timers.Timer m_timerDoMainCyclicJob = null;
        private string errorMessage = string.Empty;
        private string workingDirectory;

        public LEDModuleCheckControl(string workingDirectory)
        {
            try
            {
                this.ConstructCore(workingDirectory);
                //InitTestEquipment();

                if (StaticParams.Settings.HwConnection.AutoHwComPortDetection)
                {
                    this.CoreInterface.StartCore();
                }
                else
                {
                    this.CoreInterface.StartCore(StaticParams.Settings.HwConnection.McbComPort);
                }

                this.workingDirectory = workingDirectory;
            }
            catch (Exception excp)
            {
                var strExcpMsg = "Exception during LEDModuleCheck construction! (" + excp.Message + ")";
                TraceAddLine(strExcpMsg);

                throw new Exception(strExcpMsg);
            }
        }

        public void Dispose()
        {
            try
            {
                TraceAddLine("Dispose() entered...");

                Stop = true;    // ToDo - Usage of 'Stop' variable should be replace with enAppStatus.

                //CloseTestEquipment(true);
                this.CloseCore();

                //cAppControlIfc = null;
                //cCoreMain = null;
            }
            catch (Exception excp)
            {
                var strExcpMsg = "Exception during Disposing - LEDModuleCheck closing! (" + excp.Message + ")";
                TraceAddLine(strExcpMsg);

                throw new Exception(strExcpMsg);
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        ~LEDModuleCheckControl()
        {
            Dispose();
        }

        //public IMTFSequenceRuntimeContext RuntimeContext;
        private void HandleAsynMsgToUser(string message)
        {
            TraceAddLine("HandleAsynMsgToUser() entered... message: " + message, true);
            HandleAsynMsgToUser(new Exception(message));
        }

        private void HandleAsynMsgToUser(Exception excp)
        {
            TraceAddLine("HandleAsynMsgToUser() entered... Exception.Message: " + excp.Message, true);

            throw excp;
            // ToDo - From unknown reason the call below is not working well, so just throwing exception (line above) is done
            //RuntimeContext.RaiseException(this, excp, ExceptionLevel.JustInfo);
        }

        private void HandleAsynErrorOrExcp(string message)
        {
            TraceAddLine("HandleAsynErrorOrExcp() entered... message: " + message, true);
            HandleAsynErrorOrExcp(new Exception(message));
        }

        private void HandleAsynErrorOrExcp(Exception excp)
        {
            TraceAddLine("HandleAsynErrorOrExcp() entered... Exception.Message: " + excp.Message, true);

            throw excp;
            // ToDo - From unknown reason the call below is not working well, so just throwing exception (line above) is done
            //RuntimeContext.RaiseException(this, excp, ExceptionLevel.CriticalAsynchronousException);
        }

        public bool FillValidationTable(string tableName, string rowPrefix, double result, int ledIndex, IMTFSequenceRuntimeContext runtimeContext)
        {
            var passed = false;
            if (string.IsNullOrEmpty(tableName))
            {
                throw new Exception("Vision: Please create validation table with Min, Max, Required value and add it to the activity or remove test check in component config");
            }
            try
            {
                var max = GetMaxVoltage(ledIndex);
                var min = GetMinVoltage(ledIndex);

                var tableOutput = new List<ValidationRowContainer>
                {
                    new ValidationRowContainer
                    {
                        RowName = CreateRowName("Measured values", rowPrefix),
                        Value = result,
                        ValidationColumns = new List<ValidationColumn> { new ValidationColumn { Name = "Min", Value = min }, new ValidationColumn { Name = "Max", Value = max } }
                    }
                };

                passed = true & (result >= min) & (result <= max);
              
                runtimeContext.AddRangeToValidationTable(tableName, tableOutput);
            }
            catch (Exception e)
            {
                throw new Exception($"{e.Message}");
            }
            return passed;
        }

        private string CreateRowName(string name, string prefix)
        {
            return string.IsNullOrEmpty(prefix) ? name : $"{prefix} {name}";
        }

        private double GetMinVoltage(int index)
        {
            if (this.CoreInterface.LedModuleData.LEDs.Count - 1 < index)
            {
                var errorText = "LED Module does not have this LED!";
                TraceAddLine("GetMinVoltage - " + errorText);
                throw new Exception(errorText);
            }

            var indexLed = this.CoreInterface.LedModuleData.LEDs[index].ValidBinning;
            if (indexLed >= 0)
            {
                return this.CoreInterface.LedModuleData.LEDs[index].Voltage[this.CoreInterface.LedModuleData.LEDs[index].ValidBinning].Min;
            }

            throw new Exception("GetMinVoltage - binning isn't set!");
        }

        private double GetMaxVoltage(int index)
        {
            if (this.CoreInterface.LedModuleData.LEDs.Count - 1 < index)
            {
                var errorText = "LED Module does not have this LED!";
                TraceAddLine("GetMaxVoltage - " + errorText);
                throw new Exception(errorText);
            }

            var indexLed = this.CoreInterface.LedModuleData.LEDs[index].ValidBinning;
            if (indexLed >= 0)
            {
                return this.CoreInterface.LedModuleData.LEDs[index].Voltage[this.CoreInterface.LedModuleData.LEDs[index].ValidBinning].Max;
            }

            throw new Exception("GetMaxVoltage - binning isn't set!");
        }

        // !!! Exactly the same piece of code has to be in LEDModuleCheck-GUI.Main.cs (and all others LEDModuleCheck-Core aplication)
        #region LEDModuleCheck-Core Control
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private enum enAppStatus { AppStartUp = 0, UpdateRunning, HwInitCheckRunning, HwInitCheckError, HwIgnored, TestingReadyToStart, CoreTestInit, CoreTestRunning, CoreTestErrorContinue, CoreTestErrorStopping, CoreTestClosing, AppClosing, ExtendedMode }
        private enAppStatus AppStatus = enAppStatus.AppStartUp;

        private void SetAppStatus(enAppStatus a_AppStatus)
        {
            this.AppStatus = a_AppStatus;
        }

        public CoreInterface CoreInterface
        {
            get => this.coreInterface;
            set
            {
                if (this.coreInterface == value)
                {
                    return;
                }

                this.coreInterface = value;
            }
        }

        public void ConstructCore(string workingDirectory = "")
        {
            try
            {
                this.CoreInterface = new CoreInterface(workingDirectory);

                if (this.CoreInterface == null)
                {
                    throw new Exception("Core has not been constructed!");
                }

                //this.CoreInterface.PropertyChanged += this.OnCoreInterfacePropertyChanged;
                //this.CoreInterface.LEDModuleCheckStateChanged += this.OnCoreMainLEDModuleCheckStateChanged;
                //this.CoreInterface.HwStateChanged += this.OnCoreInterfaceHwStateChanged;
                this.CoreInterface.MessageReceived += this.OnCoreInterfaceMessageReceived;
                //this.CoreInterface.Progress += this.OnCoreInterfaceProgress;
            }
            catch (Exception exception)
            {
                var strExcpMsg = "Exception during LED Module Check Core construction! (" + exception.Message + ")";
                TraceAddLine(strExcpMsg);
                throw new Exception(strExcpMsg);
            }
        }

        private void OnCoreInterfaceMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            this.errorMessage = messageEventArgs.Message.Text;
        }

        private void CloseCore()
        {
            try
            {
                TraceAddLine("CloseCore() entered...");

                if (this.CoreInterface != null)
                {
                    //ToDo - this.AppStatusType = AppStatusType.CoreTestClosing;
                    this.CoreInterface.CloseCore();

                    //this.CoreInterface.PropertyChanged -= this.OnCoreInterfacePropertyChanged;
                    //this.CoreInterface.LEDModuleCheckStateChanged -= this.OnCoreMainLEDModuleCheckStateChanged;
                    //this.CoreInterface.HwStateChanged -= this.OnCoreInterfaceHwStateChanged;
                    this.CoreInterface.MessageReceived -= this.OnCoreInterfaceMessageReceived;
                    //this.CoreInterface.Progress -= this.OnCoreInterfaceProgress;

                    this.CoreInterface = null;
                }
                else
                {
                    TraceAddLine("CloseCore() - 'this.CoreInterface' is null!...");
                }

                TraceAddLine("CloseCore() end");
            }
            catch (Exception excp)
            {
                var strExcpMsg = "Exception during LED Module Check Core closing! (" + excp.Message + ")";
                TraceAddLine(strExcpMsg);
                throw new Exception(strExcpMsg);
            }
        }

        private void AutoTestHandling()
        {
            if (null != this.CoreInterface.AutomaticTest.CtrlReqToUser && this.CoreInterface.AutomaticTest.CtrlReqToUser.Requsted)
            {
                try
                {
                    this.CoreInterface.AutomaticTest.CtrlReqToUser.Running = true;
                    this.CoreInterface.AutomaticTest.CtrlReqToUser.Requsted = false;
                }
                catch (Exception excp)
                {
                    TraceAddLine("Exception during 'CoreMain.AutomaticTest.CtrlReqToUser' handling! (" + excp.Message + ")");
                }
                finally
                {
                    this.CoreInterface.AutomaticTest.CtrlReqToUser.Running = false;
                }
            }
        }

        private void CheckIfSelectedProjectValid(string ProjectToBeTested)
        {
            //TraceAddLine("CheckIfSelectedProjectValid() entered... ProjectName: " + ProjectToBeTested);
            if (ProjectToBeTested == string.Empty || ProjectToBeTested == "")
            {
                string strExcpMsg = "NO LED Module selected!";
                TraceAddLine("Selected Project not valid! - " + strExcpMsg);
                throw new Exception(strExcpMsg);
            }

            var strFullPathToProjectConfigFile = StaticParams.ScriptsPath + @"\" + ProjectToBeTested + ".xml";
            if (false == System.IO.File.Exists(strFullPathToProjectConfigFile))
            {
                var strExcpMessage = "Configuration file '" + strFullPathToProjectConfigFile + "' does not exists!";
                TraceAddLine("Selected Project not valid! - " + strExcpMessage);
                throw new Exception(strExcpMessage);
            }
        }

        // Start Timer doing Main Cyclic Job
        private void StartDoMainCyclicJobTimer()
        {
            try
            {
                TraceAddLine("StartDoMainCyclicJobTimer() is entered...");

                m_timerDoMainCyclicJob = new System.Timers.Timer();
                m_timerDoMainCyclicJob.Elapsed += this.DoCyclicJob;
                m_timerDoMainCyclicJob.Interval = 200;  // Should be the same or lower then LED and Motors ReqHandling cycle
                m_timerDoMainCyclicJob.Enabled = true;
                m_timerDoMainCyclicJob.Start();
            }
            catch (Exception a_excp)
            {
                var strErrMsg = "Exception during Starting DoMainCyclicJobTimer (" + a_excp.Message + ")";
                TraceAddLine(strErrMsg);

                throw new Exception(strErrMsg);
            }
        }

        // Stopping Timer doing Main Cyclic Job
        private void StopDoMainCyclicJobTimer()
        {
            try
            {
                TraceAddLine("StopDoMainCyclicJobTimer() is entered...");

                if (null == m_timerDoMainCyclicJob)
                {
                    return;
                }

                TraceAddLine("Timer 'm_timerDoCyclicJob' going to be stopped.");

                m_timerDoMainCyclicJob.Elapsed -= this.DoCyclicJob;
                m_timerDoMainCyclicJob.Enabled = false;
                m_timerDoMainCyclicJob.Stop();
            }
            catch (Exception a_excp)
            {
                var strErrMsg = "Exception during Stopping DoMainCyclicJobTimer (" + a_excp.Message + ")";
                TraceAddLine(strErrMsg);
            }
        }

        // Timer to do Main Cyclic Job
        private void DoCyclicJob(object sender, EventArgs e)
        {
            try
            {
                //MessagesFromCoreHandling();
                //Automatic Test handling
                if (null != this.CoreInterface.AutomaticTest)
                {
                    AutoTestHandling();
                }

                //if (this.CoreInterface.TTT_Initialised)
                //{
                //    TTTestHandling();
                //}

                Utils.Tracing.SaveLogs();
            }
            catch (Exception a_excp)
            {
                var strErrMsg = "Exception during CyclicJob (" + a_excp.Message + ")";
                TraceAddLine(strErrMsg);
                throw new Exception(strErrMsg);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion

        private void TraceAddLine(string a_strTraceText, bool a_bSaveLogs = false)
        {
            Utils.Tracing.TraceAddLine("MTFC: " + a_strTraceText);

            if (a_bSaveLogs)
            {
                Utils.Tracing.SaveLogs();
            }
        }

        #region MTF LED Module Check Control Methods
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region SW Versions

        public string CoreVersion()
        {
            return this.coreInterface.CoreVersion;
        }

        public string ConfigFilesVersion()
        {
            return this.coreInterface.ConfigFilesVersion;
        }

        public string CommunicationModuleVersion()
        {
            return this.coreInterface.CommunicationModuleVersion;
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region HW

        public HwType HwType()
        {
            return this.coreInterface.HwType;
        }

        public string HwSerialNo()
        {
            return this.coreInterface.HwSerialNo;
        }

        public string HwFirmwareVersion()
        {
            return this.coreInterface.HwFirmwareVersion;
        }

        public string HwDriverVersion()
        {
            return this.coreInterface.HwDriverVersion;
        }

        public STATUS HwStatus()
        {
            return this.coreInterface.HwStatus;
        }

        public int GetHwLedCurrentSourcesCount()
        {
            return this.coreInterface.GetHwLedCurrentSourcesCount();
        }

        public double GetHwLedCurrentSourceMaxCurrent(string currentSourceID)
        {
            return this.coreInterface.GetHwLedCurrentSourceMaxCurrent(currentSourceID);
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Update

        public string GetDefaultUpdateServerPath()
        {
            return this.coreInterface.GetDefaultUpdateServerPath();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Safety

        public bool IsLaserSafetyRequired()
        {
            return this.coreInterface.IsLaserSafetyRequired();
        }

        public void SetLaserSafetyAccepted(bool isAccepted)
        {
            this.coreInterface.SetLaserSafetyAccepted(isAccepted);
        }

        public GlobalSafetyStatuses CheckLaserSafetyAccepted()
        {
            return this.coreInterface.CheckLaserSafetyAccepted();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Start/Stop/Prepare Testing Methods

        //public void StartTestingViaTTNrOrSerialNo(string ttnrOrSerialNo)
        //{
        //    TraceAddLine("StartTestingViaTTNrOrDMC() - entered... TTNr/SerialNo: " + ttnrOrSerialNo);
        //    this.CoreInterface.StartTesting(ttnrOrSerialNo, null, null, LED.SafetyClassType.NoDanger);
        //}

        public void StartTesting(string projectToBeTested, string ttnrOrSerialNo, int safetyMode)
        {
            TraceAddLine("StartTesting() - entered... projectToBeTested: " + projectToBeTested);
            if (!this.CoreInterface.StartTesting(projectToBeTested, ttnrOrSerialNo, null, null, convertIntToSafetyClassType(safetyMode)))
            {
                throwException("Start testing failed!");
            }
        }

        public void StartTestingViaProjectName(string projectToBeTested, int safetyMode)
        {
            TraceAddLine("StartTesting() - entered... projectToBeTested: " + projectToBeTested);
            if (!this.CoreInterface.StartTesting(projectToBeTested, string.Empty, null, null, convertIntToSafetyClassType(safetyMode)))
            {
                throwException("Start testing failed!");
            }
        }

        public void StartTestingViaTTNr(string ttnr, int safetyMode)
        {
            TraceAddLine("StartTestingViaTTNr() - entered... TTNr: " + ttnr);
            if (!this.CoreInterface.StartTesting(ttnr, null, null, convertIntToSafetyClassType(safetyMode)))
            {
                throwException("Start testing failed!");
            }
        }

        private void throwException(string message)
        {
            if (string.IsNullOrEmpty(this.errorMessage))
            {
                throw new Exception(message);
            }
            throw new Exception(this.errorMessage);
        }

        public void StopTesting()
        {
            try
            {
                TraceAddLine("StopTesting() entered...");
                this.CoreInterface.StopTesting();
                TraceAddLine("StopTesting() end");
            }
            catch (Exception excp)
            {
                var strExcpMsg = "Exception during StopTesting: " + excp.Message;
                TraceAddLine(strExcpMsg);
                throw new Exception(strExcpMsg);
            }
        }

        //public void UpdateOfConfigFilesHasBeenJustDone()
        //{
        //    this.coreInterface.UpdateOfConfigFilesHasBeenJustDone();
        //}

        public void HwInit(string serialMCBComPort = "")
        {
            this.coreInterface.HwInit(serialMCBComPort);
        }

        public List<string> GetListOfConfigFiles()
        {
            return this.coreInterface.AllProjects;
        }

        //public void TestedProjectHasJustBeenSelected(string projectToBeTested)
        //{
        //    this.coreInterface.TestedProjectHasJustBeenSelected(projectToBeTested);
        //}

        //public void TestingHasBeenStopped(Action actionBlock)
        //{
        //    this.coreInterface.TestingHasBeenStopped(actionBlock);
        //}

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Control Methods - LEDs + MOTORs + FANs + Communication Module

        // Returns number of LEDs in the current configuration
        public int GetLedsCount()
        {
            return this.CoreInterface.GetLedsCount();
        }

        public LED GetLed(int ledIndex)
        {
            return this.coreInterface.GetLed(ledIndex);
        }

        public STATUS.StatusEnum GetLedStatus(int ledIndex)
        {
            return this.coreInterface.GetLed(ledIndex).Status.Value;
        }

        public LED.LEDTestStatusEnum GetTestStatus(int ledIndex)
        {
            return this.coreInterface.GetLed(ledIndex).GetTestStatus();
        }

        //public CURRENT GetLedCurrent(int ledIndex)
        //{
        //    return this.coreInterface.GetLedCurrent(ledIndex);
        //}

        public double GetLEDCurrent(int Index)
        {
            TraceAddLine("GetLEDCurrent - entered, Index: " + Index);
            var errorText = "LED Module does not have this LED!";
            if (this.CoreInterface.LedModuleData.LEDs.Count() < (Index + 1))
            {
                TraceAddLine("GetLEDCurrent - " + errorText);
                throw new Exception(errorText);
            }

            var Current = this.CoreInterface.LedModuleData.LEDs[Index].Current[this.CoreInterface.LedModuleData.LEDs[Index].ValidBinning].Value;
            TraceAddLine("GetLEDCurrent - leave Current:" + Current);

            return Current;
        }

        public double GetLedCurrentAndValidation(int ledIndex, string tableName, string rowName, IMTFSequenceRuntimeContext runtimeContext)
        {
            var led = GetLedByIndex(ledIndex);

            var current = this.GetLEDCurrent(ledIndex);
            FillValidationTable(tableName, rowName, current, runtimeContext, led.CurrentLimitLow, led.CurrentLimitHigh);
            return current;
        }

        //public VOLTAGE GetLedVoltage(int ledIndex)
        //{
        //    return this.coreInterface.GetLedVoltage(ledIndex);
        //}

        public double GetLEDVoltage(int index)
        {
            TraceAddLine("GetLEDVoltage - entered, index: " + index);
            if (this.CoreInterface.LedModuleData.LEDs.Count() < (index + 1))
            {
                var errorText = "LED Module does not have this LED!";
                TraceAddLine("GetLEDVoltage - " + errorText);
                throw new Exception(errorText);
            }
            TraceAddLine("GetLEDVoltage - leave");
            return this.CoreInterface.LedModuleData.LEDs[index].Voltage[this.CoreInterface.LedModuleData.LEDs[index].ValidBinning].Value; // test binning (-1 když neznáme not valid binning)
        }
        
        public double GetLedVoltageAndValidation(int ledIndex, string tableName, string rowName, IMTFSequenceRuntimeContext runtimeContext)
        {
            var led = GetLedByIndex(ledIndex);
            if (led.ValidBinning == LED.BINNING_NOT_DEFINED_YET)
            {
                throw new Exception("GetMinVoltage - binning isn't set!");
            }

            var voltage = this.GetLEDVoltage(ledIndex);
            FillValidationTable(tableName, rowName, voltage, runtimeContext, led.Voltage[led.ValidBinning].Min, led.Voltage[led.ValidBinning].Max);
            return voltage;
        }

        public int GetLedNtcsCount(int ledIndex)
        {
            return this.coreInterface.GetLedNtcsCount(ledIndex);
        }

        public TEMPERATURE GetLedNtcTemperature(int ledIndex, int ntcIndex)
        {
            return this.coreInterface.GetLedNtcTemperature(ledIndex, ntcIndex);
        }

        public List<TEMPERATURE> GetLedNtcTemperature(int ledIndex)
        {
            return this.coreInterface.GetLedNtcTemperature(ledIndex);
        }

        public void LEDSwitchON(int ledIndex, bool block = false)
        {
            this.coreInterface.LEDSwitchON(ledIndex, block);
        }

        public void LEDSwitchON(int ledIndex, int ledBinningIndex, bool block = false)
        {
            this.coreInterface.LEDSwitchON(ledIndex, ledBinningIndex, block);
        }

        public void LEDSwitchON(int ledIndex, string ledBinningName, bool block = false)
        {
            this.coreInterface.LEDSwitchON(ledIndex, ledBinningName, block);
        }

        public bool LEDSwitchONAndAcceptSafety(int ledIndex, int safetyMode, bool block)
        {
            return this.coreInterface.LEDSwitchONAndAcceptSafety(ledIndex, convertIntToSafetyClassType(safetyMode), block);
        }

        public bool AllLEDsSwitchONAndAcceptSafety(int safetyMode, bool block)
        {
            return this.coreInterface.AllLEDsSwitchONAndAcceptSafety(convertIntToSafetyClassType(safetyMode), block);
        }

        public bool AllLEDsSwitchOFF()
        {
            return this.coreInterface.AllLEDsSwitchOFF();
        }

        public void LEDSetLaserSafety(int ledIndex, bool laserSafetyAccepted)
        {
            this.coreInterface.LEDSetLaserSafety(ledIndex, laserSafetyAccepted);
        }

        public int GetLEDBinningIndex(int ledIndex, string ledBinningName)
        {
            return this.coreInterface.GetLEDBinningIndex(ledIndex, ledBinningName);
        }

        public void LEDSwitchOnOff(int ledIndex)
        {
            this.coreInterface.LEDSwitchOnOff(ledIndex);
        }

        public void LEDSwitchOFF(int ledIndex, bool block = false)
        {
            this.coreInterface.LEDSwitchOFF(ledIndex, block);
        }

        public void PerformLowCurrentTest(int ledIndex)
        {
            this.coreInterface.PerformLowCurrentTest(ledIndex);
        }

        public void PerformCameraTrigger(int ledIndex, ushort ledPulseWidth = 30, ushort cameraTriggerDelay = 5, ushort cameraTriggerWidth = 20)
        {
            this.coreInterface.PerformCameraTrigger(ledIndex, ledPulseWidth, cameraTriggerDelay, cameraTriggerWidth);
        }

        public void LEDCurrentCoeffSet(int indexOfLed, double currentCoeff)
        {
            this.coreInterface.LEDCurrentCoeffSet(indexOfLed, currentCoeff);
        }

        public double LEDCurrentCoeffGet(int indexOfLed)
        {
            return this.coreInterface.LEDCurrentCoeffGet(indexOfLed);
        }

        public object ExecuteLedCommunicationModuleCommand(string communicationModuleCommand, List<double> parameters)
        {
            TraceAddLine("Execute LED Communication Module - entered");

            return convertResult(this.CoreInterface.ExecuteLedCommunicationModuleCommand(communicationModuleCommand, parameters.ToArray()));
        }

        public object ExecuteLedCommunicationModuleCommandWithoutParameters(string communicationModuleCommandName)
        {
            TraceAddLine("Execute LED Communication Module - entered");

            return convertResult(this.CoreInterface.ExecuteLedCommunicationModuleCommand(communicationModuleCommandName));
        }

        public IEnumerable<string> GetLedCommunicationModuleCommands()
        {
            var listCommands = this.CoreInterface.GetLedCommunicationModuleCommands();
            return listCommands.Select(command => command.Name).ToList();
        }

        public IEnumerable<string> GetLedCommunicationModuleCommandsFromName(CommunicationModuleType nameOfCommModule)
        {
            return this.CoreInterface.GetLedCommunicationModuleCommandsFromName(nameOfCommModule);
        }

        public bool CheckIsCommunicationModuleInitialised()
        {
            return this.coreInterface.CheckIsCommunicationModuleInitialised();
        }

        public CommunicationModuleResult GetCommunicationStatus()
        {
            TraceAddLine("Get Communication Status - entered");

            return this.CoreInterface.GetCommunicationStatus();
        }

        //public STATUS GetStatusOfCommunicationModuleOperationResult(object comModuleOperationResult)
        //{
        //    return this.coreInterface.GetStatusOfCommunicationModuleOperationResult(comModuleOperationResult);
        //}

        private object convertResult(object resultValue)
        {
            if (resultValue is CommunicationModuleOperationResult result)
            {
                return result.OperationStatus;
            }
            var status = resultValue as STATUS;
            return status?.Value ?? resultValue;
        }

        public void DoNotTestModuleItems(bool led1, bool led2, bool led3, bool led4, bool led5, bool led6, bool motor1, bool motor2, bool fan, string configFileExtension)
        {
            var ledModuleTestRequirement = new LedModuleTestRequirement()
            {
                Led1 = led1,
                Led2 = led2,
                Led3 = led3,
                Led4 = led4,
                Led5 = led5,
                Led6 = led6,
                Motor1 = motor1,
                Motor2 = motor2,
                Fan = fan,
                ConfigFileExtension = configFileExtension
            };

            this.coreInterface.DoNotTestModuleItems(ledModuleTestRequirement);
        }

        public void DoNotTestMotors()
        {
            this.coreInterface.DoNotTestMotors();
        }

        public int GetMotorsCount()
        {
            return this.coreInterface.GetMotorsCount();
        }

        public Motor GetMotor(int indexOfMotor)
        {
            return this.coreInterface.GetMotor(indexOfMotor);
        }

        public STATUS.StatusEnum GetMotorStatus(int indexOfMotor)
        {
            var motor = this.coreInterface.GetMotor(indexOfMotor);
            return motor.Status.Value;
        }

        public bool IsMotorBasicControlEnabled(int indexOfMotor)
        {
            return this.CoreInterface.IsMotorBasicControlEnabled(indexOfMotor);
        }

        //public bool IsMotorBasicControlEnabled(Motor motor)
        //{
        //    return this.CoreInterface.IsMotorBasicControlEnabled(motor);
        //}

        public bool IsMotorFullControlEnabled(int indexOfMotor)
        {
            return this.coreInterface.IsMotorFullControlEnabled(indexOfMotor);
        }

        //public bool IsMotorFullControlEnabled(Motor motor)
        //{
        //    return this.coreInterface.IsMotorFullControlEnabled(motor);
        //}

        public bool MotorGoToPositionIfValid(int indexOfMotor, int reqPosition, bool blockMotorMovement = false)
        {
            return this.coreInterface.MotorGoToPositionIfValid(indexOfMotor, reqPosition, blockMotorMovement);
        }

        //public bool MotorGoToPositionIfValid(Motor motor, int reqPosition, bool blockMotorMovement = false)
        //{
        //    return this.coreInterface.MotorGoToPositionIfValid(motor, reqPosition, blockMotorMovement);
        //}

        //public void ChangeMotorPositionWithIncrements(Motor motor, int numberOfIncrements, bool blockMotorMovement)
        //{
        //    this.coreInterface.ChangeMotorPositionWithIncrements(motor, numberOfIncrements, blockMotorMovement);
        //}

        public void ChangeMotorPositionWithIncrements(int indexOfMotor, int numberOfIncrements, bool blockMotorMovement)
        {
            this.coreInterface.ChangeMotorPositionWithIncrements(indexOfMotor, numberOfIncrements, blockMotorMovement);
        }

        //public void ChangeMotorPositionWithIncrementsToRight(Motor motor, int numberOfIncrements, bool blockMotorMovement)
        //{
        //    this.coreInterface.ChangeMotorPositionWithIncrementsToRight(motor, numberOfIncrements, blockMotorMovement);
        //}

        public void ChangeMotorPositionWithIncrementsToRight(int indexOfMotor, int numberOfIncrements, bool blockMotorMovement)
        {
            this.coreInterface.ChangeMotorPositionWithIncrementsToRight(indexOfMotor, numberOfIncrements, blockMotorMovement);
        }

        //public void ChangeMotorPositionWithIncrementsToLeft(Motor motor, int numberOfIncrements, bool blockMotorMovement)
        //{
        //    this.coreInterface.ChangeMotorPositionWithIncrementsToLeft(motor, numberOfIncrements, blockMotorMovement);
        //}

        public void ChangeMotorPositionWithIncrementsToLeft(int indexOfMotor, int numberOfIncrements, bool blockMotorMovement)
        {
            this.coreInterface.ChangeMotorPositionWithIncrementsToLeft(indexOfMotor, numberOfIncrements, blockMotorMovement);
        }

        public MotorIncrement GetMotorIncrement(int indexOfMotor)
        {
            return this.coreInterface.GetMotorIncrement(indexOfMotor);
        }

        //public MotorIncrement GetMotorIncrement(Motor motor)
        //{
        //    return this.coreInterface.GetMotorIncrement(motor);
        //}

        public void SetMotorIncrement(int indexOfMotor, int stepInctement)
        {
            this.coreInterface.SetMotorIncrement(indexOfMotor, convertIntToMotorIncrement(stepInctement));
        }

        //public void SetMotorIncrement(Motor motor, int stepInctement)
        //{
        //    this.coreInterface.SetMotorIncrement(motor, convertIntToMotorIncrement(stepInctement));
        //}

        public void ChangeMotorIncrement(int indexOfMotor)
        {
            this.coreInterface.ChangeMotorIncrement(indexOfMotor);
        }

        //public void ChangeMotorIncrement(Motor motor)
        //{
        //    this.coreInterface.ChangeMotorIncrement(motor);
        //}

        public int[] MotorGetRange(int indexOfMotor, bool blockMotorMovement = false)
        {
            return this.coreInterface.MotorGetRange(indexOfMotor, blockMotorMovement);
        }

        //public int[] MotorGetRange(Motor motor, bool blockMotorMovement = false)
        //{
        //    return this.coreInterface.MotorGetRange(motor, blockMotorMovement);
        //}

        public bool MotorCheckRange(int indexOfMotor, bool alwaysRunRangeTest = false)
        {
            return this.coreInterface.MotorCheckRange(indexOfMotor, alwaysRunRangeTest);
        }

        //public bool MotorCheckRange(Motor motor, bool alwaysRunRangeTest = false)
        //{
        //    return this.coreInterface.MotorCheckRange(motor, alwaysRunRangeTest);
        //}

        public void MotorStartRange(int indexOfMotor)
        {
            this.coreInterface.MotorStartRange(indexOfMotor);
        }

        //public void MotorStartRange(Motor motor)
        //{
        //    this.coreInterface.MotorStartRange(motor);
        //}

        public void MotorDoReference(int indexOfMotor, bool blockMotorMovement = false)
        {
            this.coreInterface.MotorDoReference(indexOfMotor, blockMotorMovement);
        }

        //public void MotorDoReference(Motor motor, bool blockMotorMovement = false)
        //{
        //    this.coreInterface.MotorDoReference(motor, blockMotorMovement);
        //}

        public void FindEdgeOfMotorSensor(int indexOfMotor, bool blockMotorMovement = false)
        {
            this.coreInterface.FindEdgeOfMotorSensor(indexOfMotor, blockMotorMovement);
        }

        //public void FindEdgeOfMotorSensor(Motor motor, bool blockMotorMovement = false)
        //{
        //    this.coreInterface.FindEdgeOfMotorSensor(motor, blockMotorMovement);
        //}

        public bool GetMotorSensorValue(int indexOfMotor)
        {
            return this.coreInterface.GetMotorSensorValue(indexOfMotor);
        }

        public FAN GetFan(int indexOfFan)
        {
            return this.coreInterface.GetFan(indexOfFan);
        }

        public STATUS.StatusEnum GetFanStatus(int indexOfFan)
        {
            var fan = this.coreInterface.GetFan(indexOfFan);
            return fan.Status.Value;
        }

        public HeatSink GetHeatSink()
        {
            return this.coreInterface.GetHeatSink();
        }

        public STATUS.StatusEnum GetHeatSinkStatus()
        {
            return this.coreInterface.GetHeatSink().Status.Value;
        }

        public bool IsLedModuleDanger(string TTNrOrDMC)
        {
            return this.coreInterface.IsLedModuleDanger(TTNrOrDMC);
        }

        public bool IsLedModuleWithSafetyClass(string TTNrOrDMC, int safetyMode)
        {
            return this.coreInterface.IsLedModuleWithSafetyClass(TTNrOrDMC, convertIntToSafetyClassType(safetyMode));
        }

        public string GetLedComModuleName(int ledIndex)
        {
            if (this.CoreInterface.LedModuleData.LEDs.Count < (ledIndex + 1))
            {
                var errorText = "LED Module does not have this LED!";
                TraceAddLine("GetLedComModuleName - " + errorText);
                throw new Exception(errorText);
            }
            return this.coreInterface.LedModuleData.LEDs[ledIndex].CommunicationName;
        }

        public LED.SafetyClassType GetLedSafetyClass(int ledIndex)
        {
            if (this.CoreInterface.LedModuleData.LEDs.Count < (ledIndex + 1))
            {
                var errorText = "LED Module does not have this LED!";
                TraceAddLine("GetLedComModuleName - " + errorText);
                throw new Exception(errorText);
            }
            return this.coreInterface.LedModuleData.LEDs[ledIndex].SafetyClass;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Control Methods - IOs

        public double GetAnalogInputVoltage(int index)
        {
            return this.CoreInterface.GetAnalogInputVoltage(index);
        }

        public ExternIOs.enIOState GetAnalogInputState(int index)
        {
            return this.coreInterface.GetAnalogInputState(index);
        }

        public bool IsExtOut1ON()
        {
            return this.coreInterface.IsExtOut1ON();
        }

        public bool IsExtOut2ON()
        {
            return this.coreInterface.IsExtOut2ON();
        }

        public bool SwitchExtOut1()
        {
            return this.coreInterface.SwitchExtOut1();
        }

        public bool SwitchExtOut1(bool onOff)
        {
            return this.coreInterface.SwitchExtOut1(onOff);
        }

        public bool SwitchExtOut2()
        {
            return this.coreInterface.SwitchExtOut2();
        }

        public bool SwitchExtOut2(bool onOff)
        {
            return this.coreInterface.SwitchExtOut2(onOff);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Serial Number and Module Description handling

        public void SetLedModuleSerialNo(string ledModuleSerialNo)
        {
            this.coreInterface.SetLedModuleSerialNo(ledModuleSerialNo);
        }

        public string GetLedModuleSerialNo()
        {
            return this.coreInterface.GetLedModuleSerialNo();
        }

        public void SetSubModuleSerialNo(string subModuleSerialNo)
        {
            this.coreInterface.SetSubModuleSerialNo(subModuleSerialNo);
        }

        public void SetSubModuleSerialNo(string subModuleSerialNo, string subModuleSerialName)
        {
            this.coreInterface.SetSubModuleSerialNo(subModuleSerialNo, subModuleSerialName);
        }

        public string GetLedModuleDescription()
        {
            return this.coreInterface.GetLedModuleDescription();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Control Methods - Handle TEST RESULTs

        //public TestResult GetTestResults()
        //{
        //    return this.coreInterface.GetTestResults();
        //}

        public string GetTestResults()
        {
            return $"This method is just a demo. For functionality of this method contact Radek Křivský or Radek Vopálenský.{Environment.NewLine}" +
                   $"After stop testing full test result in xml format are stored in directory: {workingDirectory}Results\\Xml_Results\\{coreInterface.GetTestResults().moduleName}\\ " +
                   $"and on the server.{Environment.NewLine}You can see result via InternetExplorer on the address http://10.102.104.50/ too.";
        }

        public void StartCopyResults(bool alwaysStartNewProcess)
        {
            this.coreInterface.StartCopyResults(alwaysStartNewProcess);
        }

        public bool IsCopyResultsJustRunning()
        {
            return this.coreInterface.IsCopyResultsJustRunning();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region EOL Specific Control Methods

        public string GetTestResultsSummaryInString(bool addheader = false)
        {
            return this.coreInterface.GetTestResultsSummaryInString(addheader);
        }

        public bool GenerateTestResultsFileInEolFormat(bool addheader)
        {
            return this.coreInterface.GenerateTestResultsFileInEolFormat(addheader);
        }

        public bool EOL_BmwLaserSat_AssemblingCheck(string binnigOfLaserDiode)
        {
            return this.coreInterface.EOL_BmwLaserSat_AssemblingCheck(binnigOfLaserDiode);
        }

        public bool EOL_BmwLaserHBSpot_Station1Init()
        {
            return this.coreInterface.EOL_BmwLaserHBSpot_Station1Init();
        }

        public bool EOL_BmwLaserHBSpot_Station2Init()
        {
            return this.coreInterface.EOL_BmwLaserHBSpot_Station2Init();
        }

        public bool EOL_BmwLaserHBSpot_Station2Calibration()
        {
            return this.coreInterface.EOL_BmwLaserHBSpot_Station2Calibration();
        }

        public bool EOL_BmwLaserIcon_Check()
        {
            return this.coreInterface.EOL_BmwLaserIcon_Check();
        }

        // ToDo
        //public bool EOL_BmwLaserIcon_Station1Init(bool pwmAlways100)
        //{
        //    return this.coreInterface.EOL_BmwLaserHBSpot_Station1Init(pwmAlways100);
        //}

        //public bool EOL_BmwLaserIcon_Station2Init(bool pwmAlways100 = false)
        //{
        //    return this.coreInterface.EOL_BmwLaserHBSpot_Station2Init(pwmAlways100);
        //}

        //public bool BmwLaserSat_BulbShieldCheck()
        //{
        //    return this.coreInterface.BmwLaserSat_BulbShieldCheck();
        //}

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Methods with handling in this class

        public void LEDControl(List<LEDControlItem> LEDs)
        {
            TraceAddLine("LEDControl - entered");

            var errMessage = string.Empty;
            if (LEDs != null)
            {
                var listOfLedStatuses = new List<LedStatus>();
                var traceIndex = 0;

                foreach (var element in LEDs)
                {
                    if (element != null)
                    {
                        TraceAddLine("LEDControl - LED " + element.LEDID + ": " + element.On);
                        traceIndex++;
                    }
                }

                if (LEDs.Count > this.CoreInterface.GetLedsCount())
                {
                    var exceptionText = "Can not control more than " + this.CoreInterface.LedModuleData.LEDs.Count.ToString() + " LEDs";
                    TraceAddLine("LEDControl - " + exceptionText);
                    throw new Exception(exceptionText);
                }

                var counter = 0;
                foreach (var element in LEDs)
                {
                    if (element != null)
                    {
                        var tempIndex = 0;
                        var tempLED = this.CoreInterface.LedModuleData.LEDs.FirstOrDefault(x => x.Name == element.LEDID);

                        if (tempLED != null)
                        {
                            tempIndex = this.CoreInterface.LedModuleData.LEDs.IndexOf(tempLED);
                        }
                        else
                        {
                            try
                            {
                                tempIndex = Convert.ToInt32(element.LEDID);
                            }
                            catch
                            {
                                throw new Exception("Incorrect LED name or ID.");
                            }
                        }

                        listOfLedStatuses.Add(new LedStatus(tempIndex, element.On));

                        if (tempIndex < this.CoreInterface.LedModuleData.LEDs.Count())
                        {
                            if (element.On)
                            {
                                if (double.NaN != element.CurrentCoeff)
                                {
                                    this.CoreInterface.LEDCurrentCoeffSet(tempIndex, element.CurrentCoeff);
                                }

                                if (element.Binning == "" || element.Binning == string.Empty || element.Binning == null)
                                {
                                    this.CoreInterface.LEDSwitchON(tempIndex);
                                }
                                else
                                {
                                    // If binning is set
                                    if (this.CoreInterface.LedModuleData.LEDs[tempIndex].ValidBinning == -1
                                     || this.CoreInterface.LedModuleData.LEDs[tempIndex].ValidBinning == this.CoreInterface.GetLEDBinningIndex(tempIndex, element.Binning))
                                    {
                                        if (this.CoreInterface.LedModuleData.LEDs[counter].IsOnExpected == false)
                                        {
                                            try
                                            {
                                                this.CoreInterface.LEDSwitchON(tempIndex, element.Binning);
                                            }
                                            catch
                                            {
                                                try
                                                {
                                                    var tempBinning = Convert.ToInt32(element.Binning);
                                                    this.CoreInterface.LEDSwitchON(tempIndex, tempBinning);
                                                }
                                                catch
                                                {
                                                    throw new Exception("Incorrect LED Binning.");
                                                }
                                            }
                                        }
                                    }
                                    // If binning is set, but doesn't correspond with measured binning
                                    else
                                    {
                                        if (this.CoreInterface.LedModuleData.LEDs[counter].IsOnExpected == false)
                                        {
                                            this.CoreInterface.LEDSwitchON(tempIndex);
                                        }
                                    }
                                }
                            }

                            else
                            {
                                // RK 07.02.2017, ToDo - Clarify why the following line has to be commented out (it doesn't switched off the LED HB Spot during testing 'BMW_G07_Gx_HBSpotAndLaserSat')
                                //if (cAppControlIfc.cLEDModuleData.LEDs[counter].IsOn == true)
                                {
                                    this.CoreInterface.LEDSwitchOFF(tempIndex);
                                }
                            }
                            counter++;
                        }
                    }
                }

                if (listOfLedStatuses.Count < this.CoreInterface.LedModuleData.LEDs.Count)
                {
                    for (var i = LEDs.Count; i < this.CoreInterface.LedModuleData.LEDs.Count; i++)
                    {
                        var tempLedStatus = listOfLedStatuses.FirstOrDefault(x => x.Index == i);
                        if (tempLedStatus == null)
                        {
                            this.CoreInterface.LEDSwitchOFF(i);
                            listOfLedStatuses.Add(new LedStatus(i, false));
                        }
                    }
                }


                // Check if LEDs are really switchted ON/OFF
                var allLEDsSet = false;
                var duration = 0;
                while (allLEDsSet == false)
                {
                    allLEDsSet = true;

                    foreach (var element in listOfLedStatuses)
                    {
                        if (element.IsOn != this.CoreInterface.LedModuleData.LEDs[element.Index].IsOnWithNominalCurrent())
                        {
                            allLEDsSet = false;
                        }
                    }

                    System.Threading.Thread.Sleep(10);
                    duration++;
                    if (duration >= 100) // 1s
                    {
                        allLEDsSet = true;
                    }
                }

                // Check only LED status                
                System.Threading.Thread.Sleep(100);
                foreach (var element in listOfLedStatuses)
                {
                    if ((element != null) && (this.CoreInterface.LedModuleData.LEDs[element.Index].Status.Value == STATUS.StatusEnum.ERROR))
                    {
                        errMessage = errMessage + this.CoreInterface.LedModuleData.LEDs[element.Index].Status.ErrorText + Environment.NewLine;
                    }
                }

                if (errMessage != string.Empty && errMessage != "")
                {
                    TraceAddLine("LEDControl - " + errMessage);
                    throw new Exception(errMessage);
                }

                CheckLEDGlobalStatus();
            }

            TraceAddLine("LEDControl - leave");
        }

        public string CheckLEDGlobalStatus()
        {
            System.Threading.Thread.Sleep(750);

            if (this.CoreInterface.LedModuleData.GlobalTestStatus.Value == STATUS.StatusEnum.ERROR)
            {
                throw new Exception(this.CoreInterface.LedModuleData.GlobalTestStatus.ErrorText);
            }

            //MessagesFromCoreHandling();
            return this.CoreInterface.LedModuleData.GlobalTestStatus.Value.ToString();
        }

        public RangeResult Range(Action<string> Status)
        {
            TraceAddLine("Range - entered");

            var motor = this.CoreInterface.GetMotor(0);   // ToDo - Theoretically can be measured range at whatever motor

            this.CoreInterface.MotorGetRange(motor, true);

            if (motor.Status.Value == STATUS.StatusEnum.ERROR)
            {
                var errrorText = motor.Status.ErrorCode + " - " + motor.Status.ErrorText;
                TraceAddLine("Range - " + errrorText);
                // throw new Exception(errrorText);
            }
            else
            {
                Status("Range done OK!");
            }

            Status("End Positions: " + motor.PosRange1 + ", " + motor.PosRange2);

            var returnResult = new RangeResult();
            returnResult.Status = motor.Status.Value.ToString();
            returnResult.EndPosition1 = motor.PosRange1;
            returnResult.EndPosition2 = motor.PosRange2;

            TraceAddLine("Range  - leave");

            return returnResult;
        }

        public ReferenceResult Reference(string MotorChoice, Action<string> Status)
        {
            TraceAddLine("Reference - entered, MotorChoice: " + MotorChoice);
            if (MotorChoice == null)
            {
                var errorText = "No motor has been selected!";
                TraceAddLine("Reference - " + errorText);
                throw new Exception(errorText);
            }

            var returnResult = new ReferenceResult();
            Motor motor = null;

            if (MotorChoice == "ReferenceMotor1")
            {
                motor = this.CoreInterface.GetMotor(0);
            }
            else if (MotorChoice == "ReferenceMotor2")
            {
                motor = this.CoreInterface.GetMotor(1);
            }

            if (motor.Enable == false)
            {
                var errorText = "LED Module has no " + motor.Name + "!";
                TraceAddLine("Reference - " + errorText);
                throw new Exception(errorText);
            }

            if (this.CoreInterface.IsMotorBasicControlEnabled(motor))
            {
                Status("Waiting while previous reference is not done...");
            }

            this.CoreInterface.MotorDoReference(motor, true);
            Status("Reference in progress...");

            if (motor.Status.Value == STATUS.StatusEnum.ERROR)
            {
                var errorText = motor.Status.ErrorCode + " - " + motor.Status.ErrorText;
                TraceAddLine("Reference - " + errorText);
                throw new Exception(errorText);
            }

            Status("Reference done!");
            returnResult.Status = motor.Status.Value.ToString();
            returnResult.ReferneceOffset = motor.ReferneceOffset;
            returnResult.PosRefRun = motor.PosRefRun;

            TraceAddLine("Reference - leave");
            return returnResult;
        }

        public void SetPosition(int Position, string Motor)
        {
            TraceAddLine("SetPosition - entered, Position: " + Position + " Moror: " + Motor);

            var motor = this.CoreInterface.GetMotor(0);
            switch (Motor)
            {
                case "Motor1":
                    motor = this.CoreInterface.GetMotor(0);
                    break;
                case "Motor2":
                    motor = this.CoreInterface.GetMotor(1);
                    break;
            }

            if (motor.Enable == false)
            {
                var errorText = "LED Module has no " + motor.Name + "!";
                TraceAddLine("SetPosition - " + errorText);
                throw new Exception(errorText);
            }

            this.CoreInterface.MotorGoToPositionIfValid(motor, Position, true);
            motor.PosRequired = Position;

            TraceAddLine("SetPosition - leave");
        }

        public string ThermalTest(List<LEDThermalTestControlItem> LEDs, Action<string> Status)
        {
            TraceAddLine("ThermalTestStart - entered");
            var returnString = string.Empty;
            var newList = new List<LEDControlItem>();

            // Check if FUT has not been done
            foreach (var element in this.CoreInterface.LedModuleData.LEDs)
            {
                if (element.GetTestStatus() == LED.LEDTestStatusEnum.DoneOK)
                {
                    var errorText = "ThermalTestStart - LED " + element.Name + " Thermal test already has been successfully done. Can not be run twice.";
                    TraceAddLine("ThermalTestStart - " + errorText);
                    throw new Exception(errorText);
                }
                if (element.GetTestStatus() == LED.LEDTestStatusEnum.Error)
                {
                    var errorText = "ThermalTestStart - LED " + element.Name + " Thermal test already has been unsuccessfully done. Can not be run twice.";
                    TraceAddLine("ThermalTestStart - " + errorText);
                    throw new Exception(errorText);
                }
                if (element.DoFirstUsageTest == false || element.GetTestStatus() == LED.LEDTestStatusEnum.NotRequired)
                {
                    var errorText = "ThermalTestStart - LED " + element.Name + " Thermal test not required.";
                    TraceAddLine("ThermalTestStart - " + errorText);
                    throw new Exception(errorText);
                }
            }

            // Turn on all LEDs
            for (var i = 0; i < this.CoreInterface.LedModuleData.LEDs.Count(); i++)
            {
                // TODO BINNING ISSUES
                newList.Add(new LEDControlItem());
                newList.Last().On = true;
                newList.Last().CurrentCoeff = 1;
            }

            foreach (var element in LEDs)
            {
                var tempIndex = 0;
                var tempLED = this.CoreInterface.LedModuleData.LEDs.FirstOrDefault(x => x.Name == element.LEDID);

                if (tempLED != null)
                {
                    tempIndex = this.CoreInterface.LedModuleData.LEDs.IndexOf(tempLED);
                }
                else
                {
                    try
                    {
                        tempIndex = Convert.ToInt32(element.LEDID);
                    }
                    catch
                    {
                        throw new Exception("Incorrect LED name or ID.");
                    }
                }

                if (tempIndex <= newList.Count())
                {
                    newList[tempIndex].Binning = element.Binning;
                }
            }

            LEDControl(newList);

            for (var i = 0; i < this.CoreInterface.LedModuleData.LEDs.Count(); i++)
            {
                TraceAddLine("ThermalTestStart - LED" + i.ToString() + ": " + this.CoreInterface.LedModuleData.LEDs[i].GetTestStatus());
            }

            // Wait until FUT is not done
            var actualStatus = new LED.LEDTestStatusEnum();
            var prevStatus = new List<string>();
            var allDone = false;
            var duration = 0;
            while (allDone == false)
            {
                allDone = true;

                // collect all statuses
                for (var i = 0; i < this.CoreInterface.LedModuleData.LEDs.Count(); i++)
                {
                    actualStatus = this.CoreInterface.LedModuleData.LEDs[i].GetTestStatus();
                    //TraceAddLine("ThermalTestStart - WHILE - LED" + i.ToString() + ": " + actualStatus);
                    //TODO: Check this conditions
                    if (actualStatus != LED.LEDTestStatusEnum.Error
                     && actualStatus != LED.LEDTestStatusEnum.Stopped
                     && actualStatus != LED.LEDTestStatusEnum.NotRequired
                     && actualStatus != LED.LEDTestStatusEnum.DoneOK
                     && actualStatus != LED.LEDTestStatusEnum.TooDiffTemp
                     && actualStatus != LED.LEDTestStatusEnum.AmbTempOutOfRange
                     && actualStatus != LED.LEDTestStatusEnum.NotAllLedsON)
                    {
                        allDone = false;
                    }

                    if (prevStatus.Count() <= i + 1)
                    {
                        prevStatus.Add("");
                    }

                    if (prevStatus[i] != this.CoreInterface.LedModuleData.LEDs.First().GetTestStatus().ToString())
                    {
                        prevStatus[i] = this.CoreInterface.LedModuleData.LEDs.First().GetTestStatus().ToString();
                        Status(DateTime.Now.ToString() + " LED " + this.CoreInterface.LedModuleData.LEDs[i].Name + ": \t" + this.CoreInterface.LedModuleData.LEDs.First().GetTestStatus().ToString());
                    }
                }

                // waiting, no stack overflow exc
                System.Threading.Thread.Sleep(1000);
                duration++;
                if (duration >= ((this.CoreInterface.LedModuleData.LEDs.First().FUTDuration * 1000) * 1.2))
                {
                    allDone = true;
                }
            }

            for (var i = 0; i < this.CoreInterface.LedModuleData.LEDs.Count(); i++)
            {
                returnString = returnString + "LED" + i.ToString() + ": " + this.CoreInterface.LedModuleData.LEDs[i].GetTestStatus().ToString() + "; ";
            }

            // TURN OF ALL LEDs
            for (var i = 0; i < this.CoreInterface.LedModuleData.LEDs.Count(); i++)
            {
                newList[i].On = false;
            }
            LEDControl(newList);

            for (var i = 0; i < this.CoreInterface.LedModuleData.LEDs.Count(); i++)
            {
                if (this.CoreInterface.LedModuleData.LEDs[i].Status.ErrorText != "")
                {
                    Status("LED " + this.CoreInterface.LedModuleData.LEDs[i].Name + " result: " + this.CoreInterface.LedModuleData.LEDs[i].Status.ErrorText);
                }
            }

            TraceAddLine("ThermalTestStart - leave");
            return returnString;
        }

        public int GetPosition(int Motor)
        {
            TraceAddLine("GetPosition - entered, Motor: " + Motor);
            var returnValue = 0;

            try
            {
                if (Motor == 1)
                {
                    returnValue = this.CoreInterface.LedModuleData.Motor1.PosActual;
                }

                if (Motor == 2)
                {
                    returnValue = this.CoreInterface.LedModuleData.Motor2.PosActual;
                }

                if (returnValue < -1000)
                {
                    throw new Exception("Can´t get position, reference of this motor is needed!");
                }
            }
            catch (Exception ex)
            {
                TraceAddLine("GetPosition - " + ex.Message);
                throw new Exception("GetPosition Error, can´t get actual position.");
            }

            TraceAddLine("GetPosition - entered, Motor: " + Motor + " Positon: " + returnValue);
            return returnValue;
        }

        public double GetLEDTemperature(int Index)
        {
            TraceAddLine("GetLEDTempereture - entered, Index: " + Index);
            if (this.CoreInterface.LedModuleData.LEDs.Count() < (Index + 1))
            {
                var errorText = "LED Module does not have this LED!";
                TraceAddLine("GetLEDTempereture - " + errorText);
                throw new Exception(errorText);
            }

            while (this.CoreInterface.LedModuleData.LEDs[Index].Temperature.Last().Value == 0)
            {
                System.Threading.Thread.Sleep(10);
            }

            TraceAddLine("GetLEDTempereture - leave");
            return this.CoreInterface.LedModuleData.LEDs[Index].Temperature.Last().Value;
        }

        public double GetLedTemperatureAndValidation(int ledIndex, string tableName, string rowName, IMTFSequenceRuntimeContext runtimeContext)
        {
            var led = GetLedByIndex(ledIndex);

            if (led.Temperature.Count == 0)
            {
                var errorText = "Selected LED hasn't temperature information!!!";
                TraceAddLine("GetLEDTemperature - " + errorText);
                throw new Exception(errorText);
            }

            // ToDo - min
            var temperature = this.GetLEDTemperature(ledIndex);
            FillValidationTable(tableName, rowName, temperature, runtimeContext, 0, led.Temperature.First().Max);
            return temperature;
        }

        public double GetLEDCurrentCoeff(int Index)
        {
            TraceAddLine("GetLEDCurrentCoeff - entered, Index: " + Index);
            if (this.CoreInterface.LedModuleData.LEDs.Count() < (Index + 1))
            {
                var errorText = "LED Module does not have this LED!";
                TraceAddLine("GetLEDCurrentCoeff - " + errorText);
                throw new Exception(errorText);
            }

            var CurrentCoeff = this.CoreInterface.LEDCurrentCoeffGet(Index);
            TraceAddLine("GetLEDCurrentCoeff - leave - CurrentCoeff: " + CurrentCoeff);
            return CurrentCoeff;
        }

        public double GetAmbientTemperature()
        {
            TraceAddLine("GetAmbientTemperature - entered");
            return this.CoreInterface.AmbTemp;
        }

        public ExternIOs.enIOState GetInputState(int index)
        {
            return this.CoreInterface.GetAnalogInputState(index);
        }

        public bool IsExtOutON(int index)
        {
            TraceAddLine("IsExtOutON - entered, index: " + index);
            var returnValue = false;

            switch (index)
            {
                case 1:
                    {
                        returnValue = this.CoreInterface.IsExtOut1ON();
                        break;
                    }
                case 2:
                    {
                        returnValue = this.CoreInterface.IsExtOut2ON();
                        break;
                    }
                default:
                    {
                        TraceAddLine("IsExtOutON - index not found !!!!");

                        returnValue = false;
                        break;
                    }
            }

            return returnValue;
        }

        public bool SwitchExtOut(int index, bool switchOnOff)
        {
            TraceAddLine("SwitchExtOut - entered, index: " + index + ", switchOnOff: " + switchOnOff);
            var returnValue = false;

            switch (index)
            {
                case 1:
                    {
                        returnValue = this.CoreInterface.SwitchExtOut1(switchOnOff);
                        break;
                    }
                case 2:
                    {
                        returnValue = this.CoreInterface.SwitchExtOut2(switchOnOff);
                        break;
                    }
                default:
                    {
                        TraceAddLine("SwitchExtOut - index not found !!!!");

                        returnValue = false;
                        break;
                    }
            }

            return returnValue;
        }

        public bool SwitchExtOut12V(bool switchOnOff)
        {
            TraceAddLine("SwitchExtOut12V - entered, switchOnOff: " + switchOnOff);

            return this.SwitchExtOut(1, switchOnOff);
        }

        public bool SwitchExtOut24V(bool switchOnOff)
        {
            TraceAddLine("SwitchExtOut24V - entered, switchOnOff: " + switchOnOff);

            return this.SwitchExtOut(2, switchOnOff);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Get GlobalStatus, ErrorCode and ErrorText

        public STATUS.StatusEnum GetGlobalTestStatus()
        {
            return coreInterface.LedModuleData.GlobalTestStatus.Value;
        }

        public int GetGlobalTestErrorCode()
        {
            return coreInterface.LedModuleData.GlobalTestStatus.ErrorCode;
        }

        public string GetGlobalTestErrorText()
        {
            return coreInterface.LedModuleData.GlobalTestStatus.ErrorText;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region TTTechnoTeam

        public bool TTT_Initialised()
        {
            return CoreInterface.TTT_Initialised;
        }

        public enTTTestStateMachineState TTT_GetTTTestStateMachineState()
        {
            return CoreInterface.TTT_GetTTTestStateMachineState();
        }

        public TechnoTeamTest TTT_GetTechnoTeamTest()
        {
            return CoreInterface.TTT_GetTechnoTeamTest();
        }

        public enTTTestState TTT_GetTTTestState()
        {
            return CoreInterface.TTT_GetTTTestState();
        }

        public void TTT_TTTestStart(string selectedTtTestVariant,
            bool isScatteredLightCorrectionEnabled,
            bool isSaveImageEnabled,
            bool isGenerateProtocolEnabled,
            bool isCloseTestProtocolEnabled)
        {
            CoreInterface.TTT_TTTestStart(selectedTtTestVariant, isScatteredLightCorrectionEnabled, isSaveImageEnabled,
                isGenerateProtocolEnabled, isCloseTestProtocolEnabled);
        }

        public int TTT_GetSelectedVariantIndex()
        {
            return CoreInterface.TTT_GetSelectedVariantIndex();
        }

        public DateTime TTT_GetTimeOfLedsSwitchedOn()
        {
            return CoreInterface.TTT_GetTimeOfLedsSwitchedOn();
        }

        public string TTT_GetPathToTTTestResult()
        {
            return CoreInterface.TTT_GetPathToTTTestResult();
        }

        public string TTT_GetAndRemoveOneTraceCommTT()
        {
            return CoreInterface.TTT_GetAndRemoveOneTraceCommTT();
        }

        public bool TTT_IsSelectedVariantAllowedForThisModule(int reqVariantIndex)
        {
            return CoreInterface.TTT_IsSelectedVariantAllowedForThisModule(reqVariantIndex);
        }

        public int TTT_TestVariantCount()
        {
            return coreInterface.TTT_TTTestVariantsCount;
        }

        public uint TTT_GetTimeSinceLedsSwitchedOnInSeconds()
        {
            return coreInterface.TTT_GetTimeSinceLedsSwitchedOnInSeconds();
        }

        public List<string> TTT_TestVariantsForTestedModule()
        {
            return coreInterface.TTT_TTTestVariantsForTestedModule.ToList();
        }

        #endregion

        #region PRIVATE FUNCTIONS

        private void FillValidationTable(string tableName, string rowName, double result, IMTFSequenceRuntimeContext runtimeContext, double min, double max)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new Exception("Vision: Please create validation table with Min, Max, Required value and add it to the activity or remove test check in component config");
            }
            try
            {
                var tableOutput = new List<ValidationRowContainer>
                {
                    new ValidationRowContainer
                    {
                        RowName = rowName,
                        Value = result,
                        ValidationColumns = new List<ValidationColumn> { new ValidationColumn { Name = "Min", Value = min }, new ValidationColumn { Name = "Max", Value = max } }
                    }
                };

                runtimeContext.AddRangeToValidationTable(tableName, tableOutput);
            }
            catch (Exception e)
            {
                throw new Exception($"{e.Message}");
            }
        }

        private LED GetLedByIndex(int index)
        {
            TraceAddLine($"GetLedByIndex - entered, index: {index}");
            if (this.CoreInterface.LedModuleData.LEDs.Count < index + 1)
            {
                var errorText = "LED Module does not have this LED!";
                TraceAddLine($"GetLedByIndex - {errorText}");
                throw new Exception(errorText);
            }
            TraceAddLine("GetLedByIndex - leave");
            return this.CoreInterface.LedModuleData.LEDs[index];
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool Stop { set; get; }  //ToDo - May be it could be possible to use AppStatus instead of 'Stop'

        private MotorIncrement convertIntToMotorIncrement(int safetyMode)
        {
            switch (safetyMode)
            {
                case 0:
                    return MotorIncrement.Predefined__Positions;
                case 1:
                    return MotorIncrement._1__HS;
                case 2:
                    return MotorIncrement._10__HS;
                case 3:
                    return MotorIncrement._100__HS;
                default:
                    return MotorIncrement.Predefined__Positions;
            }
        }

        private LED.SafetyClassType convertIntToSafetyClassType(int safetyMode)
        {
            switch (safetyMode)
            {
                case 0:
                    return LED.SafetyClassType.NotDefined;
                case 1:
                    return LED.SafetyClassType.NoDanger;
                case 2:
                    return LED.SafetyClassType.Danger_IR;
                case 3:
                    return LED.SafetyClassType.Danger_Laser;
                default:
                    return LED.SafetyClassType.NoDanger;
            }
        }
    }

    public class LedStatus
    {
        public int Index { set; get; }
        public bool IsOn { set; get; }

        public LedStatus(int Index, bool IsOn)
        {
            this.Index = Index;
            this.IsOn = IsOn;
        }
    }

    [MTFKnownClass]
    public class RangeResult
    {
        public string Status { set; get; }
        public int EndPosition1 { set; get; }
        public int EndPosition2 { set; get; }
    }

    [MTFKnownClass]
    public class ReferenceResult
    {
        public string Status { set; get; }
        public int PosRefRun { set; get; }
        public int ReferneceOffset { set; get; }
    }

    [MTFKnownClass]
    public class LEDControlItem
    {
        [MTFProperty(Name = "LED ID", Description = "LED name or LED index")]
        public string LEDID { set; get; }
        [MTFProperty(Name = "Binning", Description = "Binning name or binning index")]
        public string Binning { set; get; }
        [MTFProperty(Name = "ON", Description = "Turn LED ON (true) or OFF (false)")]
        public bool On { set; get; }
        [MTFProperty(Name = "Current Coefficient", Description = "Ration of Nominal LED Current, Range 30% (0.3) to 120% (1.2)", DefaultValue = "1")]
        public double CurrentCoeff { set; get; }
    }

    [MTFKnownClass]
    public class LEDThermalTestControlItem
    {
        [MTFProperty(Name = "LED ID", Description = "Name or index of LED")]
        public string LEDID { set; get; }
        [MTFProperty(Name = "Binning", Description = "Binning name or binning index")]
        public string Binning { set; get; }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #endregion
}
