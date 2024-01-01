using System;
using System.Collections.Generic;
using System.IO;
using LED_Module_Check_Core.Source;
using AutomotiveLighting.MTFCommon;
using LED_Module_Check_Common;
using LED_Module_Check_Core.TechnoTeam;

namespace LEDModuleCheck
{
    [MTFClass(Name = "LED Module Check", Description = "Driver for LED Module Check", ThreadSafeLevel = ThreadSafeLevel.Instance, Icon = MTFIcons.LED)]
    [MTFClassCategory("Control & Measurement")]

    public class LEDModuleCheck : IDisposable
    {
        private LEDModuleCheckControl ledModuleCheckControl;
        private CommunicationModuleType communicationModuleType = CommunicationModuleType.None;
        public IMTFSequenceRuntimeContext RuntimeContext;

        [MTFConstructor]
        [MTFAllowedParameterValue("communicationModuleType", "None", "None")]
        [MTFAllowedParameterValue("communicationModuleType", "BmwLaserSat", "BmwLaserSat")]
        [MTFAllowedParameterValue("communicationModuleType", "LMMGeneralControl", "LMMGeneralControl")]
        [MTFAllowedParameterValue("communicationModuleType", "DmlPavSoftwareControl", "DmlPavSoftwareControl")]
        [MTFAllowedParameterValue("communicationModuleType", "MlcGeneralControl", "MlcGeneralControl")]
        public LEDModuleCheck(string workingDirectory, string communicationModuleType)
        {
            if (!Path.IsPathRooted(workingDirectory))
            {
                workingDirectory = Path.GetFullPath(workingDirectory);
            }
            this.ledModuleCheckControl = new LEDModuleCheckControl(workingDirectory);
            this.communicationModuleType = (CommunicationModuleType)Enum.Parse(typeof(CommunicationModuleType), communicationModuleType, true);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region SW Versions

        [MTFMethod(Description = "Method for getting version of Core", DisplayName = "Get Core Version")]
        public string CoreVersion()
        {
            return this.ledModuleCheckControl.CoreVersion();
        }

        [MTFMethod(Description = "Method for getting version of configuration files", DisplayName = "Get Config Files Version")]
        public string GetFilesVersion()
        {
            return this.ledModuleCheckControl.ConfigFilesVersion();
        }

        [MTFMethod(Description = "Method for getting version of CommModule", DisplayName = "Get CommModule Version")]
        public string CommunicationModuleVersion()
        {
            return this.ledModuleCheckControl.CommunicationModuleVersion();
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region HW

        [MTFMethod(Description = "Method for getting type", DisplayName = "Get Type")]
        public HwType HwType()
        {
            return this.ledModuleCheckControl.HwType();
        }

        [MTFMethod(Description = "Method for getting serial No", DisplayName = "Get Serial No")]
        public string HwSerialNo()
        {
            return this.ledModuleCheckControl.HwSerialNo();
        }

        [MTFMethod(Description = "Method for getting version of Firmware", DisplayName = "Get Firmware Version")]
        public string HwFirmwareVersion()
        {
            return this.ledModuleCheckControl.HwFirmwareVersion();
        }

        [MTFMethod(Description = "Method for getting version of Driver", DisplayName = "Get Driver Version")]
        public string HwDriverVersion()
        {
            return this.ledModuleCheckControl.HwDriverVersion();
        }

        [MTFMethod(Description = "Method for getting status", DisplayName = "Get Status")]
        public STATUS HwStatus()
        {
            return this.ledModuleCheckControl.HwStatus();
        }

        [MTFMethod(Description = "Method for getting number of LED Current sources", DisplayName = "Get LED Current Source Count")]
        public int GetHwLedCurrentSourcesCount()
        {
            return this.ledModuleCheckControl.GetHwLedCurrentSourcesCount();
        }

        [MTFMethod(Description = "Method for getting max Current of LED sources", DisplayName = "Get LED CS Max Current")]
        public double GetHwLedCurrentSourceMaxCurrent(string currentSourceID)
        {
            return this.ledModuleCheckControl.GetHwLedCurrentSourceMaxCurrent(currentSourceID);
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Update

        [MTFMethod(Description = "Method for getting default update server path", DisplayName = "Get Default Update Server Path")]
        public string GetDefaultUpdateServerPath()
        {
            return this.ledModuleCheckControl.GetDefaultUpdateServerPath();
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Safety

        [MTFMethod(Description = "Method for checking whether laser safety is required", DisplayName = "Is Laser Safety Required")]
        public bool IsLaserSafetyRequired()
        {
            return this.ledModuleCheckControl.IsLaserSafetyRequired();
        }

        [MTFMethod(Description = "Method for accepting laser safety", DisplayName = "Set Laser Safety Accepted")]
        public void SetLaserSafetyAccepted(bool isAccepted)
        {
            ledModuleCheckControl.SetLaserSafetyAccepted(isAccepted);
        }

        [MTFMethod(Description = "Method for checking whether is accepted laser safety", DisplayName = "Check Laser Safety Accepted")]
        public GlobalSafetyStatuses CheckLaserSafetyAccepted()
        {
            return ledModuleCheckControl.CheckLaserSafetyAccepted();
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Start/Stop/Prepare Testing Methods

        [MTFValueListGetterMethod]
        public List<Tuple<string, object>> GetProjectToBeTested()
        {
            var listProjects = new List<Tuple<string, object>>();
            foreach (var item in ledModuleCheckControl.CoreInterface.AllProjects)
            {
                listProjects.Add(new Tuple<string, object>(item, item));
            }
            return listProjects;
        }

        [MTFAdditionalParameterInfo(ParameterName = "projectToBeTested", ValueListName = "GetProjectToBeTested")]
        [MTFMethod(Description = "Method for start test", DisplayName = "Testing START")]
        [MTFAllowedParameterValue("safetyMode", "NotDefined", "0")]
        [MTFAllowedParameterValue("safetyMode", "NoDanger", "1")]
        [MTFAllowedParameterValue("safetyMode", "Danger_IR", "2")]
        [MTFAllowedParameterValue("safetyMode", "Danger_Laser", "3")]
        public void StartTesting(string projectToBeTested, string ttnrOrSerialNo, int safetyMode)
        {
            this.ledModuleCheckControl.StartTesting(projectToBeTested, ttnrOrSerialNo, safetyMode);
        }

        [MTFAdditionalParameterInfo(ParameterName = "projectToBeTested", ValueListName = "GetProjectToBeTested")]
        [MTFMethod(Description = "Method for start test via project name", DisplayName = "Testing START via Project Name")]
        [MTFAllowedParameterValue("safetyMode", "NotDefined", "0")]
        [MTFAllowedParameterValue("safetyMode", "NoDanger", "1")]
        [MTFAllowedParameterValue("safetyMode", "Danger_IR", "2")]
        [MTFAllowedParameterValue("safetyMode", "Danger_Laser", "3")]
        public void StartTestingViaProjectName(string projectToBeTested, int safetyMode)
        {
            this.ledModuleCheckControl.StartTestingViaProjectName(projectToBeTested, safetyMode);
        }

        [MTFMethod(Description = "Method for start test via TTNr", DisplayName = "Testing START via TTNr")]
        [MTFAllowedParameterValue("safetyMode", "NotDefined", "0")]
        [MTFAllowedParameterValue("safetyMode", "NoDanger", "1")]
        [MTFAllowedParameterValue("safetyMode", "Danger_IR", "2")]
        [MTFAllowedParameterValue("safetyMode", "Danger_Laser", "3")]
        public void TestingStartViaTTNr(string TTNrOfModuleToBeTested, int safetyMode)
        {
            this.ledModuleCheckControl.StartTestingViaTTNr(TTNrOfModuleToBeTested, safetyMode);
        }

        [MTFMethod(Description = "Method for stop test", DisplayName = "Testing STOP")]
        public void TestingStop()
        {
            this.ledModuleCheckControl.StopTesting();
        }

        [MTFMethod(Description = "Method for init HW", DisplayName = "HW Init")]
        public void HwInit(string serialMCBComPort)
        {
            this.ledModuleCheckControl.HwInit(serialMCBComPort);
        }

        [MTFMethod(Description = "Method for getting list of all config files", DisplayName = "Get All Config Files")]
        public List<string> GetListOfConfigFiles()
        {

            return this.ledModuleCheckControl.GetListOfConfigFiles();
        }

        //[MTFMethod(Description = "Method for ", DisplayName = "")]
        //public void UpdateOfConfigFilesHasBeenJustDone()
        //{
        //    this.ledModuleCheckControl.UpdateOfConfigFilesHasBeenJustDone();
        //}

        //[MTFMethod(Description = "Method for  ", DisplayName = "")]
        //public void TestedProjectHasJustBeenSelected(string projectToBeTested)
        //{
        //    this.ledModuleCheckControl.TestedProjectHasJustBeenSelected(projectToBeTested);
        //}

        //[MTFMethod(Description = "Method for ", DisplayName = "")]
        //public void TestingHasBeenStopped(Action actionBlock)
        //{
        //    this.ledModuleCheckControl.TestingHasBeenStopped(actionBlock);
        //}

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Control Methods - LEDs + MOTORs + FANs + Communication Module

        [MTFMethod(Description = "Method for getting number of LEDs", DisplayName = "Get LEDs Count")]
        public int GetLedsCount()
        {
            return this.ledModuleCheckControl.GetLedsCount();
        }

        [MTFMethod(Description = "Method for getting LED", DisplayName = "Get LED")]
        public LED GetLed(int ledIndex)
        {
            return this.ledModuleCheckControl.GetLed(ledIndex);
        }

        [MTFMethod(Description = "Method for getting LED status", DisplayName = "Get LED Status")]
        [MTFAllowedParameterValue("index", "LED #0", "0")]
        [MTFAllowedParameterValue("index", "LED #1", "1")]
        [MTFAllowedParameterValue("index", "LED #2", "2")]
        [MTFAllowedParameterValue("index", "LED #3", "3")]
        [MTFAllowedParameterValue("index", "LED #4", "4")]
        [MTFAllowedParameterValue("index", "LED #5", "5")]
        [MTFAllowedParameterValue("index", "LED #6", "6")]
        [MTFAllowedParameterValue("index", "LED #7", "7")]
        public STATUS.StatusEnum GetLedStatus(int index)
        {
            return this.ledModuleCheckControl.GetLedStatus(index);
        }

        [MTFMethod(Description = "Method for getting thermal test status of specific LED", DisplayName = "Get LED Thermal Test Status")]
        [MTFAllowedParameterValue("index", "LED #0", "0")]
        [MTFAllowedParameterValue("index", "LED #1", "1")]
        [MTFAllowedParameterValue("index", "LED #2", "2")]
        [MTFAllowedParameterValue("index", "LED #3", "3")]
        [MTFAllowedParameterValue("index", "LED #4", "4")]
        [MTFAllowedParameterValue("index", "LED #5", "5")]
        [MTFAllowedParameterValue("index", "LED #6", "6")]
        [MTFAllowedParameterValue("index", "LED #7", "7")]
        public LED.LEDTestStatusEnum GetTestStatus(int index)
        {
            return this.ledModuleCheckControl.GetTestStatus(index);
        }

        [MTFMethod(Description = "Method for getting current of specific LED", DisplayName = "Get LED Current")]
        [MTFAllowedParameterValue("index", "LED #0", "0")]
        [MTFAllowedParameterValue("index", "LED #1", "1")]
        [MTFAllowedParameterValue("index", "LED #2", "2")]
        [MTFAllowedParameterValue("index", "LED #3", "3")]
        [MTFAllowedParameterValue("index", "LED #4", "4")]
        [MTFAllowedParameterValue("index", "LED #5", "5")]
        [MTFAllowedParameterValue("index", "LED #6", "6")]
        [MTFAllowedParameterValue("index", "LED #7", "7")]
        public double GetLEDCurrent(int index)
        {
            return ledModuleCheckControl.GetLEDCurrent(index);
        }

        [MTFMethod(Description = "Method for getting current of specific LED and validation in the validation table", DisplayName = "Get LED Current and Validation")]
        [MTFAllowedParameterValue("index", "LED #0", "0")]
        [MTFAllowedParameterValue("index", "LED #1", "1")]
        [MTFAllowedParameterValue("index", "LED #2", "2")]
        [MTFAllowedParameterValue("index", "LED #3", "3")]
        [MTFAllowedParameterValue("index", "LED #4", "4")]
        [MTFAllowedParameterValue("index", "LED #5", "5")]
        [MTFAllowedParameterValue("index", "LED #6", "6")]
        [MTFAllowedParameterValue("index", "LED #7", "7")]
        public double GetLedCurrentAndValidation(int index, string tableName, string rowName)
        {
            return ledModuleCheckControl.GetLedCurrentAndValidation(index, tableName, rowName, this.RuntimeContext);
        }

        [MTFMethod(Description = "Method for getting voltage of specific LED", DisplayName = "Get LED Voltage")]
        [MTFAllowedParameterValue("index", "LED #0", "0")]
        [MTFAllowedParameterValue("index", "LED #1", "1")]
        [MTFAllowedParameterValue("index", "LED #2", "2")]
        [MTFAllowedParameterValue("index", "LED #3", "3")]
        [MTFAllowedParameterValue("index", "LED #4", "4")]
        [MTFAllowedParameterValue("index", "LED #5", "5")]
        [MTFAllowedParameterValue("index", "LED #6", "6")]
        [MTFAllowedParameterValue("index", "LED #7", "7")]
        public double GetLEDVoltage(int index)
        {
            return this.ledModuleCheckControl.GetLEDVoltage(index);
        }

        [MTFMethod(Description = "Method for getting voltage of specific LED and validation in the validation table", DisplayName = "Get LED Voltage and Validation")]
        [MTFAllowedParameterValue("index", "LED #0", "0")]
        [MTFAllowedParameterValue("index", "LED #1", "1")]
        [MTFAllowedParameterValue("index", "LED #2", "2")]
        [MTFAllowedParameterValue("index", "LED #3", "3")]
        [MTFAllowedParameterValue("index", "LED #4", "4")]
        [MTFAllowedParameterValue("index", "LED #5", "5")]
        [MTFAllowedParameterValue("index", "LED #6", "6")]
        [MTFAllowedParameterValue("index", "LED #7", "7")]
        public double GetLedVoltageAndValidation(int index, string tableName, string rowName)
        {
            return ledModuleCheckControl.GetLedVoltageAndValidation(index, tableName, rowName, this.RuntimeContext);
        }

        [MTFMethod(Description = "Method for getting number of LED NTC", DisplayName = "Get Count LED NTC")]
        public int GetLedNtcsCount(int ledIndex)
        {
            return this.ledModuleCheckControl.GetLedNtcsCount(ledIndex);
        }

        [MTFMethod(Description = "Method for getting temperature on the LED NTC", DisplayName = "Get Temperature on LED NTC")]
        public TEMPERATURE GetLedNtcTemperature(int ledIndex, int ntcIndex)
        {
            return this.ledModuleCheckControl.GetLedNtcTemperature(ledIndex, ntcIndex);
        }

        [MTFMethod(Description = "Method for getting list with temperature on the LED NTC", DisplayName = "Get List Temperatures on LED NTC")]
        public List<TEMPERATURE> GetLedNtcTemperature(int ledIndex)
        {
            return this.ledModuleCheckControl.GetLedNtcTemperature(ledIndex);
        }

        [MTFMethod(Description = "Method for switching on LED", DisplayName = "LED Switch On")]
        public void LEDSwitchON(int ledIndex, bool block = false)
        {
            this.ledModuleCheckControl.LEDSwitchON(ledIndex, block);
        }

        [MTFMethod(Description = "Method for switching on LED by binning index (int)", DisplayName = "LED Switch On by BinnIndex (int)")]
        public void LEDSwitchONByBinnIndexInt(int ledIndex, int ledBinningIndex, bool block = false)
        {
            this.ledModuleCheckControl.LEDSwitchON(ledIndex, ledBinningIndex, block);
        }

        [MTFMethod(Description = "Method for switching on LED by binning index (string)", DisplayName = "LED Switch On by BinnIndex (string)")]
        public void LEDSwitchONByBinnIndexBool(int ledIndex, string ledBinningName, bool block = false)
        {
            this.ledModuleCheckControl.LEDSwitchON(ledIndex, ledBinningName, block);
        }

        [MTFMethod(Description = "Method for switching on LED and accepting safety", DisplayName = "LED Switch On And Accept Safety")]
        [MTFAllowedParameterValue("safetyMode", "NotDefined", "0")]
        [MTFAllowedParameterValue("safetyMode", "NoDanger", "1")]
        [MTFAllowedParameterValue("safetyMode", "Danger_IR", "2")]
        [MTFAllowedParameterValue("safetyMode", "Danger_Laser", "3")]
        public bool LEDSwitchONAndAcceptSafety(int ledIndex, int safetyMode, bool block)
        {
            return ledModuleCheckControl.LEDSwitchONAndAcceptSafety(ledIndex, safetyMode, block);
        }

        [MTFMethod(Description = "Method for switching on all LEDs and accepting safety", DisplayName = "All LED Switch On And Accept Safety")]
        [MTFAllowedParameterValue("safetyMode", "NotDefined", "0")]
        [MTFAllowedParameterValue("safetyMode", "NoDanger", "1")]
        [MTFAllowedParameterValue("safetyMode", "Danger_IR", "2")]
        [MTFAllowedParameterValue("safetyMode", "Danger_Laser", "3")]
        public bool AllLEDsSwitchONAndAcceptSafety(int safetyMode, bool block)
        {
            return ledModuleCheckControl.AllLEDsSwitchONAndAcceptSafety(safetyMode, block);
        }

        [MTFMethod(Description = "Method for switching off all LEDs", DisplayName = "All LEDs Switch Off")]
        public bool AllLEDsSwitchOFF()
        {
            return ledModuleCheckControl.AllLEDsSwitchOFF();
        }

        [MTFMethod(Description = "Method for setting LED Laser Safety", DisplayName = "Set LED Laser Safety")]
        public void LEDSetLaserSafety(int ledIndex, bool laserSafetyAccepted)
        {
            this.ledModuleCheckControl.LEDSetLaserSafety(ledIndex, laserSafetyAccepted);
        }

        [MTFMethod(Description = "Method for getting LED binning index", DisplayName = "Get LED Binning Index")]
        public int GetLEDBinningIndex(int ledIndex, string ledBinningName)
        {
            return this.ledModuleCheckControl.GetLEDBinningIndex(ledIndex, ledBinningName);
        }

        [MTFMethod(Description = "Method for switching On/Off LED", DisplayName = "LED Switch On/Off")]
        public void LEDSwitchOnOff(int ledIndex)
        {
            this.ledModuleCheckControl.LEDSwitchOnOff(ledIndex);
        }

        [MTFMethod(Description = "Method for switching off LED", DisplayName = "LED Switch Off")]
        public void LEDSwitchOFF(int ledIndex, bool block = false)
        {
            this.ledModuleCheckControl.LEDSwitchOFF(ledIndex, block);
        }

        [MTFMethod(Description = "Method for Low current test", DisplayName = "Low Current Test")]
        public void PerformLowCurrentTest(int ledIndex)
        {
            this.ledModuleCheckControl.PerformLowCurrentTest(ledIndex);
        }

        [MTFMethod(Description = "Method for Camera trigger", DisplayName = "Camera Trigger")]
        public void PerformCameraTrigger(int ledIndex, ushort ledPulseWidth, ushort cameraTriggerDelay, ushort cameraTriggerWidth)
        {
            this.ledModuleCheckControl.PerformCameraTrigger(ledIndex, ledPulseWidth, cameraTriggerDelay, cameraTriggerWidth);
        }

        [MTFMethod(Description = "Method for setting coefficient of LED current", DisplayName = "Set Coeff. LED Curr.")]
        public void LEDCurrentCoeffSet(int indexOfLed, double currentCoeff)
        {
            this.ledModuleCheckControl.LEDCurrentCoeffSet(indexOfLed, currentCoeff);
        }

        [MTFMethod(Description = "Method for getting coefficient of LED current", DisplayName = "Get Coeff. LED Curr.")]
        public double LEDCurrentCoeffGet(int indexOfLed)
        {
            return this.ledModuleCheckControl.LEDCurrentCoeffGet(indexOfLed);
        }

        [MTFAdditionalParameterInfo(ParameterName = "CommandName", ValueListName = "GetCommandsForActualProject")]
        [MTFMethod(Description = "Method for executing LED CommModule", DisplayName = "Execute LED CommModule")]
        public object ExecuteLedCommunicationModuleCommand(string CommandName, List<double> parameters)
        {
            return this.ledModuleCheckControl.ExecuteLedCommunicationModuleCommand(CommandName, parameters);
        }

        [MTFAdditionalParameterInfo(ParameterName = "CommandName", ValueListName = "GetCommandsForActualProject")]
        [MTFMethod(Description = "Method for executing LED CommModule without parameters", DisplayName = "Execute LED CommModule Without Parameters")]
        public object ExecuteLedCommunicationModuleCommandWithoutParameters(string CommandName)
        {
            return this.ledModuleCheckControl.ExecuteLedCommunicationModuleCommandWithoutParameters(CommandName);
        }

        [MTFValueListGetterMethod]
        public List<Tuple<string, object>> GetCommandsForActualProject()
        {
            var listProjects = new List<Tuple<string, object>>();
            foreach (var item in this.ledModuleCheckControl.GetLedCommunicationModuleCommandsFromName(this.communicationModuleType))
            {
                listProjects.Add(new Tuple<string, object>(item, item));
            }
            return listProjects;
        }

        [MTFMethod(Description = "Method for getting LED CommModule commands", DisplayName = "Get LED CommModule Commands")]
        public IEnumerable<string> GetLedCommunicationModuleCommands()
        {
            return this.ledModuleCheckControl.GetLedCommunicationModuleCommands();
        }

        [MTFMethod(Description = "Method for checking whether CommModule is initialized", DisplayName = "Is CommModule Initialized")]
        public bool CheckIsCommunicationModuleInitialised()
        {
            return this.ledModuleCheckControl.CheckIsCommunicationModuleInitialised();
        }

        [MTFMethod(Description = "Method for getting communication status", DisplayName = "Get Communication Status")]
        public CommunicationModuleResult GetCommunicationStatus()
        {
            return this.ledModuleCheckControl.GetCommunicationStatus();
        }

        [MTFMethod(Description = "Method for selecting items for module test", DisplayName = "Do Not Test Module Items")]
        public void DoNotTestModuleItems(bool Led1, bool Led2, bool Led3, bool Led4, bool Led5, bool Led6, bool Motor1, bool Motor2, bool Fan, string ConfigFileExtension)
        {
            this.ledModuleCheckControl.DoNotTestModuleItems(Led1, Led2, Led3, Led4, Led5, Led6, Motor1, Motor2, Fan, ConfigFileExtension);
        }

        [MTFMethod(Description = "Method excluding motor from the tests", DisplayName = "Exclude Motor From Test")]
        public void DoNotTestMotors()
        {
            this.ledModuleCheckControl.DoNotTestMotors();
        }

        [MTFMethod(Description = "Method for getting number of motors", DisplayName = "Get Motors Count")]
        public int GetMotorsCount()
        {
            return this.ledModuleCheckControl.GetMotorsCount();
        }

        [MTFMethod(Description = "Method for getting motor", DisplayName = "Get Motor")]
        public Motor GetMotor(int indexOfMotor)
        {
            return this.ledModuleCheckControl.GetMotor(indexOfMotor);
        }

        [MTFMethod(Description = "Method for getting status of motor", DisplayName = "Get Motor Status")]
        public STATUS.StatusEnum GetMotorStatus(int indexOfMotor)
        {
            return this.ledModuleCheckControl.GetMotorStatus(indexOfMotor);
        }

        [MTFMethod(Description = "Method for checking whether is enable basic control of motor", DisplayName = "Is Enable Basic Control Motor")]
        public bool IsMotorBasicControlEnabled(int indexOfMotor)
        {
            return this.ledModuleCheckControl.IsMotorBasicControlEnabled(indexOfMotor);
        }

        [MTFMethod(Description = "Method for checking whether is enable full control of motor", DisplayName = "Is Enable Full Control Motor")]
        public bool IsMotorFullControlEnabled(int indexOfMotor)
        {
            return this.ledModuleCheckControl.IsMotorFullControlEnabled(indexOfMotor);
        }

        [MTFMethod(Description = "Method for changing the position of the motor, if position is valid", DisplayName = "Change Position Motor If Valid")]
        public bool MotorGoToPositionIfValid(int indexOfMotor, int reqPosition, bool blockMotorMovement = false)
        {
            return this.ledModuleCheckControl.MotorGoToPositionIfValid(indexOfMotor, reqPosition, blockMotorMovement);
        }

        [MTFMethod(Description = "Method for changing the position of the motor with increments", DisplayName = "Change Position Motor With Increments")]
        public void ChangeMotorPositionWithIncrements(int indexOfMotor, int numberOfIncrements, bool blockMotorMovement)
        {
            this.ledModuleCheckControl.ChangeMotorPositionWithIncrements(indexOfMotor, numberOfIncrements, blockMotorMovement);
        }

        [MTFMethod(Description = "Method for changing the position of the motor with increments to right", DisplayName = "Change Position Motor With Increments To Right")]
        public void ChangeMotorPositionWithIncrementsToRight(int indexOfMotor, int numberOfIncrements, bool blockMotorMovement)
        {
            this.ledModuleCheckControl.ChangeMotorPositionWithIncrementsToRight(indexOfMotor, numberOfIncrements, blockMotorMovement);
        }

        [MTFMethod(Description = "Method for changing the position of the motor with increments to left", DisplayName = "Change Position Motor With Increments To Left")]
        public void ChangeMotorPositionWithIncrementsToLeft(int indexOfMotor, int numberOfIncrements, bool blockMotorMovement)
        {
            this.ledModuleCheckControl.ChangeMotorPositionWithIncrementsToLeft(indexOfMotor, numberOfIncrements, blockMotorMovement);
        }

        [MTFMethod(Description = "Method for getting increment ot the motor", DisplayName = "Get Increment Motor")]
        public MotorIncrement GetMotorIncrement(int indexOfMotor)
        {
            return this.ledModuleCheckControl.GetMotorIncrement(indexOfMotor);
        }

        [MTFMethod(Description = "Method for setting increment of the motor", DisplayName = "Set Increment Motor")]
        [MTFAllowedParameterValue("stepIncrement", "Predefined__Positions", "0")]
        [MTFAllowedParameterValue("stepIncrement", "_1__HS", "1")]
        [MTFAllowedParameterValue("stepIncrement", "_10__HS", "2")]
        [MTFAllowedParameterValue("stepIncrement", "_100__HS", "3")]
        public void SetMotorIncrement(int indexOfMotor, int stepIncrement)
        {
            this.ledModuleCheckControl.SetMotorIncrement(indexOfMotor, stepIncrement);
        }

        [MTFMethod(Description = "Method for changing increment of the motor", DisplayName = "Change Increment Motor")]
        public void ChangeMotorIncrement(int indexOfMotor)
        {
            this.ledModuleCheckControl.ChangeMotorIncrement(indexOfMotor);
        }

        [MTFMethod(Description = "Method for getting range of the motor", DisplayName = "Get Range Motor")]
        public int[] MotorGetRange(int indexOfMotor, bool blockMotorMovement = false)
        {
            return this.ledModuleCheckControl.MotorGetRange(indexOfMotor, blockMotorMovement);
        }

        [MTFMethod(Description = "Method for checking range of the motor", DisplayName = "Check Range Motor")]
        public bool MotorCheckRange(int indexOfMotor, bool alwaysRunRangeTest = false)
        {
            return this.ledModuleCheckControl.MotorCheckRange(indexOfMotor, alwaysRunRangeTest);
        }

        [MTFMethod(Description = "Method for starting range test", DisplayName = "Start Range Test")]
        public void MotorStartRange(int indexOfMotor)
        {
            this.ledModuleCheckControl.MotorStartRange(indexOfMotor);
        }

        [MTFMethod(Description = "Method for making the motor reference", DisplayName = "Make Motor Reference")]
        public void MotorDoReference(int indexOfMotor, bool blockMotorMovement = false)
        {
            this.ledModuleCheckControl.MotorDoReference(indexOfMotor, blockMotorMovement);
        }

        [MTFMethod(Description = "Method for getting the edge of the motor sensor", DisplayName = "Find Motor Sensor Edge")]
        public void FindEdgeOfMotorSensor(int indexOfMotor, bool blockMotorMovement = false)
        {
            this.ledModuleCheckControl.FindEdgeOfMotorSensor(indexOfMotor, blockMotorMovement);
        }
        
        [MTFMethod(Description = "Method for getting the value of the motor sensor", DisplayName = "Get Motor Sensor Value")]
        public bool GetMotorSensorValue(int indexOfMotor)
        {
            return this.ledModuleCheckControl.GetMotorSensorValue(indexOfMotor);
        }

        [MTFMethod(Description = "Method for getting fan", DisplayName = "Get Fan")]
        public FAN GetFan(int indexOfFan)
        {
            return this.ledModuleCheckControl.GetFan(indexOfFan);
        }

        [MTFMethod(Description = "Method for getting status of fan", DisplayName = "Get Fan Status")]
        public STATUS.StatusEnum GetFanStatus(int indexOfFan)
        {
            return this.ledModuleCheckControl.GetFanStatus(indexOfFan);
        }

        [MTFMethod(Description = "Method for getting HeatSink", DisplayName = "Get HeatSink")]
        public HeatSink GetHeatSink()
        {
            return this.ledModuleCheckControl.GetHeatSink();
        }

        [MTFMethod(Description = "Method for getting status of HeatSink", DisplayName = "Get HeatSink Status")]
        public STATUS.StatusEnum GetHeatSinkStatus()
        {
            return this.ledModuleCheckControl.GetHeatSinkStatus();
        }

        [MTFMethod(Description = "Method for getting whether is LED Module danger", DisplayName = "Is LED Module Danger")]
        public bool IsLedModuleDanger(string TTNrOrDMC)
        {
            return this.ledModuleCheckControl.IsLedModuleDanger(TTNrOrDMC);
        }

        [MTFMethod(Description = "Method for getting whether is LED Module with safety class", DisplayName = "Is LED Module With Safety")]
        [MTFAllowedParameterValue("safetyMode", "NotDefined", "0")]
        [MTFAllowedParameterValue("safetyMode", "NoDanger", "1")]
        [MTFAllowedParameterValue("safetyMode", "Danger_IR", "2")]
        [MTFAllowedParameterValue("safetyMode", "Danger_Laser", "3")]
        public bool IsLedModuleWithSafetyClass(string TTNrOrDMC, int safetyMode)
        {
            return this.ledModuleCheckControl.IsLedModuleWithSafetyClass(TTNrOrDMC, safetyMode);
        }

        [MTFMethod(Description = "Method for getting name of Communication Module for each LED", DisplayName = "Get LED Comm Module Name")]
        public string GetLedComModuleName(int ledIndex)
        {
            return this.ledModuleCheckControl.GetLedComModuleName(ledIndex);
        }

        [MTFMethod(Description = "Method for getting Safety Class for each LED", DisplayName = "Get LED Safety Class")]
        public LED.SafetyClassType GetLedSafetyClass(int ledIndex)
        {
            return this.ledModuleCheckControl.GetLedSafetyClass(ledIndex);
        }

        [MTFMethod(Description = "Method for filling measured value, minimum and maximum to the validation table", DisplayName = "Fill Validation Table")]
        public bool FillValidationTable(string tableName, string rowPrefix, double result, int ledIndex)
        {
             return this.ledModuleCheckControl.FillValidationTable(tableName, rowPrefix, result, ledIndex, RuntimeContext);
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Control Methods - IOs

        [MTFMethod(Description = "Method for getting voltage of external input", DisplayName = "Get Input Voltage")]
        public double GetAnalogInputVoltage(int index)
        {
            return ledModuleCheckControl.GetAnalogInputVoltage(index);
        }

        [MTFMethod(Description = "Method for getting state of external input", DisplayName = "Get Analog Input State")]
        public ExternIOs.enIOState GetAnalogInputState(int index)
        {
            return this.ledModuleCheckControl.GetAnalogInputState(index);
        }

        [MTFMethod(Description = "Method for checking whether is external output1 ON", DisplayName = "Is Ext. Output1 On")]
        public bool IsExtOut1ON()
        {
            return this.ledModuleCheckControl.IsExtOut1ON();
        }

        [MTFMethod(Description = "Method for checking whether is external output2 ON", DisplayName = "Is Ext. Output2 On")]
        public bool IsExtOut2ON()
        {
            return this.ledModuleCheckControl.IsExtOut2ON();
        }

        [MTFMethod(Description = "Method for switching external output1", DisplayName = "Switch Ext. Output1 (without parameter)")]
        public bool SwitchExtOut1()
        {
            return this.ledModuleCheckControl.SwitchExtOut1();
        }

        [MTFMethod(Description = "Method for switching external output1", DisplayName = "Switch Ext. Output1")]
        public bool SwitchExtOut1(bool onOff)
        {
            return this.ledModuleCheckControl.SwitchExtOut1(onOff);
        }

        [MTFMethod(Description = "Method for switching external output2", DisplayName = "Switch Ext. Output2 (without parameter)")]
        public bool SwitchExtOut2()
        {
            return this.ledModuleCheckControl.SwitchExtOut2();
        }

        [MTFMethod(Description = "Method for switching external output2", DisplayName = "Switch Ext. Output2")]
        public bool SwitchExtOut2(bool onOff)
        {
            return this.ledModuleCheckControl.SwitchExtOut2(onOff);
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Serial Number and Module Description handling

        [MTFMethod(Description = "Method for setting LED Module serial No", DisplayName = "Set LED Module Serial No")]
        public void SetLedModuleSerialNo(string ledModuleSerialNo)
        {
            this.ledModuleCheckControl.SetLedModuleSerialNo(ledModuleSerialNo);
        }

        [MTFMethod(Description = "Method for getting LED Module serial No", DisplayName = "Get LED Module Serial No")]
        public string GetLedModuleSerialNo()
        {
            return this.ledModuleCheckControl.GetLedModuleSerialNo();
        }

        [MTFMethod(Description = "Method for setting SubModule serial No", DisplayName = "Set SubModule Serial No")]
        public void SetSubModuleSerialNo(string subModuleSerialNo)
        {
            this.ledModuleCheckControl.SetSubModuleSerialNo(subModuleSerialNo);
        }

        [MTFMethod(Description = "Method for setting SubModule serial No", DisplayName = "Set SubModule Serial No (Serial name)")]
        public void SetSubModuleSerialNo(string subModuleSerialNo, string subModuleSerialName)
        {
            this.ledModuleCheckControl.SetSubModuleSerialNo(subModuleSerialNo, subModuleSerialName);
        }

        [MTFMethod(Description = "Method for getting SubModule serial No", DisplayName = "Get SubModule Serial No")]
        public string GetLedModuleDescription()
        {
            return this.ledModuleCheckControl.GetLedModuleDescription();
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Control Methods - Handle TEST RESULTs

        [MTFMethod(Description = "Method for getting test results", DisplayName = "Get Test Results")]
        public string GetTestResults()
        {
            return this.ledModuleCheckControl.GetTestResults();
        }

        //[MTFMethod(Description = "Method for getting test results", DisplayName = "Get Test Results")]
        //public TestResult GetTestResults()
        //{
        //    return this.ledModuleCheckControl.GetTestResults();
        //}

        [MTFMethod(Description = "Method for starting CopyResults", DisplayName = "Start CopyResults")]
        public void StartCopyResults(bool allwaysStartNewProcess = false)
        {
            this.ledModuleCheckControl.StartCopyResults(allwaysStartNewProcess);
        }

        [MTFMethod(Description = "Method for checking whether is CopyResults just running", DisplayName = "Is CopyResults Just Running")]
        public bool IsCopyResultsJustRunning()
        {
            return this.ledModuleCheckControl.IsCopyResultsJustRunning();
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region EOL Specific Control Methods

        [MTFMethod(Description = "Method for getting summary test results", DisplayName = "Get Summary Test Results")]
        public string GetTestResultsSummaryInString(bool addHeader = false)
        {
            return this.ledModuleCheckControl.GetTestResultsSummaryInString(addHeader);
        }

        [MTFMethod(Description = "Method for genering test result file (EOL format)", DisplayName = "Generate Test Result")]
        public bool GenerateTestResultsFileInEolFormat(bool addHeader)
        {
            return this.ledModuleCheckControl.GenerateTestResultsFileInEolFormat(addHeader);
        }

        [MTFMethod(Description = "EOL BMW Laser Sat Assembling Check", DisplayName = "Assembling Check EOL BmwLaserSat")]
        public bool EOL_BmwLaserSat_AssemblingCheck(string binnigOfLaserDiode)
        {
            return this.ledModuleCheckControl.EOL_BmwLaserSat_AssemblingCheck(binnigOfLaserDiode);
        }

        [MTFMethod(Description = "EOL BMW Laser HB Spot Station1 Init", DisplayName = "EOL BMWLaserHBSpot Init Station1")]
        public bool EOL_BmwLaserHBSpot_Station1Init()
        {
            return this.ledModuleCheckControl.EOL_BmwLaserHBSpot_Station1Init();
        }

        [MTFMethod(Description = "EOL BMW Laser HB Spot Station2 Init", DisplayName = "EOL BMWLaserHBSpot Init Station1")]
        public bool EOL_BmwLaserHBSpot_Station2Init()
        {
            return this.ledModuleCheckControl.EOL_BmwLaserHBSpot_Station2Init();
        }

        [MTFMethod(Description = "EOL BMW Laser HB Spot Station2 Calibration", DisplayName = "EOL BMWLaserHBSpot Calibration Station2")]
        public bool EOL_BmwLaserHBSpot_Station2Calibration()
        {
            return this.ledModuleCheckControl.EOL_BmwLaserHBSpot_Station2Calibration();
        }

        [MTFMethod(Description = "EOL BMW Laser Icon Check", DisplayName = "EOL BmwLaserIcon Check")]
        public bool EOL_BmwLaserIcon_Check()
        {
            return this.ledModuleCheckControl.EOL_BmwLaserIcon_Check();
        }

        //[MTFMethod(Description = "BMW Laser Sat Check BulbShield", DisplayName = "BMWLaserSat Check BulbShield")]
        //public bool BmwLaserSat_BulbShieldCheck()
        //{
        //    return this.ledModuleCheckControl.BmwLaserSat_BulbShieldCheck();
        //}

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Methods with handling in this class

        [MTFMethod(Description = "Method for LED controling", DisplayName = "LED Control")]
        public void LEDControl(List<LEDControlItem> LEDs)
        {
            this.ledModuleCheckControl.LEDControl(LEDs);
        }

        [MTFMethod(Description = "Method for checking LED global status", DisplayName = "Check LED Global Status")]
        public string CheckLEDGlobalStatus()
        {
            return this.ledModuleCheckControl.CheckLEDGlobalStatus();
        }

        [MTFMethod(Description = "Method for range test of motor 1", DisplayName = "Range Test")]
        public RangeResult Range()
        {
            return this.ledModuleCheckControl.Range((status) =>
            {
                RuntimeContext.TextNotification(status);
            });
        }

        [MTFMethod(Description = "Method for referencing motor 1 or 2", DisplayName = "Reference")]
        [MTFAllowedParameterValue("motorChoice", "Motor 1", "ReferenceMotor1")]
        [MTFAllowedParameterValue("motorChoice", "Motor 2", "ReferenceMotor2")]
        public ReferenceResult Reference(string motorChoice)
        {
            return this.ledModuleCheckControl.Reference(motorChoice, (status) =>
            {
                RuntimeContext.TextNotification(status);
            });
        }

        [MTFMethod(Description = "Method for setting position of motor 1 or 2", DisplayName = "Set Position")]
        [MTFAllowedParameterValue("motorChoice", "Motor 1", "Motor1")]
        [MTFAllowedParameterValue("motorChoice", "Motor 2", "Motor2")]
        public void SetPosition(int position, string motorChoice)
        {
            this.ledModuleCheckControl.SetPosition(position, motorChoice);
        }

        [MTFMethod(Description = "Method for starting thermal test", DisplayName = "Thermal Test")]
        public string ThermalTest(List<LEDThermalTestControlItem> LEDs)
        {
            return this.ledModuleCheckControl.ThermalTest(LEDs, (status) =>
            {
                RuntimeContext.TextNotification(status);
            });
        }

        [MTFMethod(Description = "Method for getting position of motor 1 or 2", DisplayName = "Get Position")]
        [MTFAllowedParameterValue("motor", "Motor 1", "1")]
        [MTFAllowedParameterValue("motor", "Motor 2", "2")]
        public int GetPosition(int motor)
        {
            return this.ledModuleCheckControl.GetPosition(motor);
        }

        [MTFMethod(Description = "Method for getting temperature of specific LED", DisplayName = "Get LED Temperature")]
        [MTFAllowedParameterValue("index", "LED #0", "0")]
        [MTFAllowedParameterValue("index", "LED #1", "1")]
        [MTFAllowedParameterValue("index", "LED #2", "2")]
        [MTFAllowedParameterValue("index", "LED #3", "3")]
        [MTFAllowedParameterValue("index", "LED #4", "4")]
        [MTFAllowedParameterValue("index", "LED #5", "5")]
        [MTFAllowedParameterValue("index", "LED #6", "6")]
        [MTFAllowedParameterValue("index", "LED #7", "7")]
        public double GetLEDTemperature(int index)
        {
            return this.ledModuleCheckControl.GetLEDTemperature(index);
        }

        [MTFMethod(Description = "Method for getting temperature of specific LED and validation in the validation table", DisplayName = "Get LED Temperature and Validation")]
        [MTFAllowedParameterValue("index", "LED #0", "0")]
        [MTFAllowedParameterValue("index", "LED #1", "1")]
        [MTFAllowedParameterValue("index", "LED #2", "2")]
        [MTFAllowedParameterValue("index", "LED #3", "3")]
        [MTFAllowedParameterValue("index", "LED #4", "4")]
        [MTFAllowedParameterValue("index", "LED #5", "5")]
        [MTFAllowedParameterValue("index", "LED #6", "6")]
        [MTFAllowedParameterValue("index", "LED #7", "7")]
        public double GetLedTemperatureAndValidation(int index, string tableName, string rowName)
        {
            return this.ledModuleCheckControl.GetLedTemperatureAndValidation(index, tableName, rowName, RuntimeContext);
        }

        [MTFMethod(Description = "Method for getting current coefficient of specific LED", DisplayName = "Get LED Current Coefficient")]
        [MTFAllowedParameterValue("index", "LED #0", "0")]
        [MTFAllowedParameterValue("index", "LED #1", "1")]
        [MTFAllowedParameterValue("index", "LED #2", "2")]
        [MTFAllowedParameterValue("index", "LED #3", "3")]
        [MTFAllowedParameterValue("index", "LED #4", "4")]
        [MTFAllowedParameterValue("index", "LED #5", "5")]
        [MTFAllowedParameterValue("index", "LED #6", "6")]
        [MTFAllowedParameterValue("index", "LED #7", "7")]
        public double GetLEDCurrentCoeff(int index)
        {
            return this.ledModuleCheckControl.GetLEDCurrentCoeff(index);
        }

        [MTFMethod(Description = "Method for getting ambient temperature", DisplayName = "Get Ambient Temperature")]
        public double GetAmbientTemperature()
        {
            return this.ledModuleCheckControl.GetAmbientTemperature();
        }

        [MTFMethod(Description = "Method for getting State of external Input", DisplayName = "Get Input State")]
        public ExternIOs.enIOState GetInputState(int index)
        {
            return this.ledModuleCheckControl.GetInputState(index);
        }

        [MTFMethod(Description = "Method for checking if external output is switched", DisplayName = "Is Output On/Off")]
        [MTFAllowedParameterValue("index", "LED #0", "0")]
        [MTFAllowedParameterValue("index", "LED #1", "1")]
        public bool IsExtOutON(int index)
        {
            return this.ledModuleCheckControl.IsExtOutON(index);
        }

        [MTFMethod(Description = "Method for switching an external output", DisplayName = "Switch Output On/Off")]
        [MTFAllowedParameterValue("index", "LED #0", "0")]
        [MTFAllowedParameterValue("index", "LED #1", "1")]
        public bool SwitchExtOut(int index, bool switchOnOf)
        {
            return this.ledModuleCheckControl.SwitchExtOut(index, switchOnOf);
        }

        [MTFMethod(Description = "Method for switching 12V to external output", DisplayName = "Switch On/Off 12V")]
        public bool SwitchExtOut12V(bool switchOnOf)
        {
            return this.ledModuleCheckControl.SwitchExtOut12V(switchOnOf);
        }

        [MTFMethod(Description = "Method for switching 24V to external output", DisplayName = "Switch On/Off 24V")]
        public bool SwitchExtOut24V(bool switchOnOf)
        {
            return this.ledModuleCheckControl.SwitchExtOut24V(switchOnOf);
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Get GlobalStatus, ErrorCode and ErrorText

        [MTFMethod(Description = "Method for getting global test status", DisplayName = "Get Global Status")]
        public STATUS.StatusEnum GetGlobalTestStatus()
        {
            return this.ledModuleCheckControl.GetGlobalTestStatus();
        }

        [MTFMethod(Description = "Method for getting error code", DisplayName = "Get Error Code")]
        public int GetGlobalTestErrorCode()
        {
            return this.ledModuleCheckControl.GetGlobalTestErrorCode();
        }

        [MTFMethod(Description = "Method for getting error text", DisplayName = "Get Error Text")]
        public string GetGlobalTestErrorText()
        {
            return this.ledModuleCheckControl.GetGlobalTestErrorText();
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region TTTechnoTeam

        [MTFMethod(Description = "Method for TTT initialised", DisplayName = "TTT Initialised")]
        public bool TTT_Initialised()
        {
            return this.ledModuleCheckControl.TTT_Initialised();
        }

        [MTFMethod(Description = "Method for getting TTT machine state", DisplayName = "TTT Get Machine State")]
        public enTTTestStateMachineState TTT_GetTTTestStateMachineState()
        {
            return this.ledModuleCheckControl.TTT_GetTTTestStateMachineState();
        }

        [MTFMethod(Description = "Method for getting TTTest", DisplayName = "TTT Get TTTest")]
        public TechnoTeamTest TTT_GetTechnoTeamTest()
        {
            return this.ledModuleCheckControl.TTT_GetTechnoTeamTest();
        }

        [MTFMethod(Description = "Method for getting TTTest state", DisplayName = "TTT Get Test State")]
        public enTTTestState TTT_GetTTTestState()
        {
            return this.ledModuleCheckControl.TTT_GetTTTestState();
        }

        [MTFMethod(Description = "Method for start TTTest", DisplayName = "TTT Start Test")]
        public void TTT_TTTestStart(string selectedTTTestVariant,
            bool isScatteredLightCorrectionEnabled,
            bool isSaveImageEnabled,
            bool isGenerateProtocolEnabled,
            bool isCloseTestProtocolEnabled)
        {
            this.ledModuleCheckControl.TTT_TTTestStart(selectedTTTestVariant, isScatteredLightCorrectionEnabled, isSaveImageEnabled,
                isGenerateProtocolEnabled, isCloseTestProtocolEnabled);
        }

        [MTFMethod(Description = "Method for getting variant index", DisplayName = "TTT Get Variant Index")]
        public int TTT_GetSelectedVariantIndex()
        {
            return this.ledModuleCheckControl.TTT_GetSelectedVariantIndex();
        }

        [MTFMethod(Description = "Method for getting time of Leds switched ON", DisplayName = "TTT Get Time Leds On")]
        public DateTime TTT_GetTimeOfLedsSwitchedOn()
        {
            return this.ledModuleCheckControl.TTT_GetTimeOfLedsSwitchedOn();
        }

        [MTFMethod(Description = "Method for getting path to TTTest result", DisplayName = "TTT Get Path to TestResult")]
        public string TTT_GetPathToTTTestResult()
        {
            return this.ledModuleCheckControl.TTT_GetPathToTTTestResult();
        }

        [MTFMethod(Description = "Method for getting and deleting trace of communication", DisplayName = "TTT Get & Remove Trace Comm")]
        public string TTT_GetAndRemoveOneTraceCommTT()
        {
            return this.ledModuleCheckControl.TTT_GetAndRemoveOneTraceCommTT();
        }

        [MTFMethod(Description = "Find out if is allowed selected variant for this module", DisplayName = "TTT Is Select Variant Allowed")]
        public bool TTT_IsSelectedVariantAllowedForThisModule(int reqVariantIndex)
        {
            return this.ledModuleCheckControl.TTT_IsSelectedVariantAllowedForThisModule(reqVariantIndex);
        }

        [MTFMethod(Description = "Method for getting count of tests variant", DisplayName = "TTT Get Test Variant Count")]
        public int TTT_TestVariantCount()
        {
            return this.ledModuleCheckControl.TTT_TestVariantCount();
        }

        [MTFMethod(Description = "Method for getting time since LED Switched On [s]", DisplayName = "TTT Get Time Since LED Switch On")]
        public uint TTT_GetTimeSinceLedsSwitchedOnInSeconds()
        {
            return this.ledModuleCheckControl.TTT_GetTimeSinceLedsSwitchedOnInSeconds();
        }

        [MTFMethod(Description = "Method for getting list of test variants for tested module", DisplayName = "TTT Get List of Test Variant")]
        public List<string> TTT_TestVariantsForTestedModule()
        {
            return this.ledModuleCheckControl.TTT_TestVariantsForTestedModule();
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Dispose()
        {
            this.ledModuleCheckControl.Dispose();
            GC.SuppressFinalize(this);
        }

        ~LEDModuleCheck()
        {
            this.Dispose();
        }
    }
}
