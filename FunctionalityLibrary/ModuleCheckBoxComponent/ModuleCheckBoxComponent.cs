using System;
using AutomotiveLighting.MTFCommon;
using System.Collections.Generic;
using LED_Module_Check_Common;
using ModuleCheckBox;
using ModuleCheckBox.Models;
using ModuleCheckBox.Enums;

namespace ModuleCheckBoxComponent
{
    [MTFClass(Name = "Module Check Box", Description = "Driver for Module Check Box", ThreadSafeLevel = ThreadSafeLevel.Instance, Icon = MTFIcons.LED)]
    [MTFClassCategory("Control & Measurement")]
    public class ModuleCheckBoxComponent : IDisposable
    {
        private bool disposed;
        readonly ModuleCheckBoxControl moduleCheckBoxControl;

        // TODO: Adapt constructor to helper dynamic filling of ports
        //[MTFConstructor(Description = "Create communication with Module Check Box via selected port.", ParameterHelperClassName = "ModuleCheckBoxComponent.ModuleCheckBoxHelper")]
        [MTFConstructor]
        public ModuleCheckBoxComponent()
        {
            this.moduleCheckBoxControl = new ModuleCheckBoxControl();
        }

        [MTFMethod(Description = "Connect to Module Check Box (Define portName as 'USB' or 'COM1' or 'COM34'...)", DisplayName = "Connect")]
        public void Connect(string portName)
        {
            this.moduleCheckBoxControl.Connect(portName);
        }

        [MTFMethod(Description = "Disconnect from Module Check Box", DisplayName = "Disconnect")]
        public void Disconnect()
        {
            this.moduleCheckBoxControl.Disconnect();
        }

        [MTFMethod(Description = "Init Communication Module, pathToXML not required for BmwLaserSat", DisplayName = "Communication Module - Init")]
        [MTFAllowedParameterValue("communicationModuleType", "BmwLaserSat", "1")]
        [MTFAllowedParameterValue("communicationModuleType", "LMMGeneralControl", "11")]
        [MTFAllowedParameterValue("communicationModuleType", "DmlPavSoftware", "12")]
        public void InitCommunicationModule(int communicationModuleType, string pathToXML)
        {
            this.moduleCheckBoxControl.InitCommunicationModule((CommunicationModuleType)communicationModuleType, pathToXML);
        }

        [MTFMethod(Description = "Get temperature from communication module", DisplayName = "CommunicationModule - GetTemperature")]
        public double CommunicationModuleGetTemperature(string ntcName)
        {
            return this.moduleCheckBoxControl.CommunicationModuleGetTemperature(ntcName);
        }

        [MTFMethod(Description = "Switch communication module LED on", DisplayName = "CommunicationModule - SwitchLedOn")]
        [MTFAllowedParameterValue("ledChannel", "All Led Channels", "0")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 1", "1")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 2", "2")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 3", "3")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 4", "4")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 5", "5")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 6", "6")]
        public void CommunicationModuleSwitchLedOn(int ledChannel)
        {
            this.moduleCheckBoxControl.CommunicationModuleSwitchLedOn((LedChannel)ledChannel);
        }

        [MTFMethod(Description = "Switch communication module LED off", DisplayName = "CommunicationModule - SwitchLedOff")]
        [MTFAllowedParameterValue("ledChannel", "All Led Channels", "0")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 1", "1")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 2", "2")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 3", "3")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 4", "4")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 5", "5")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 6", "6")]
        public void CommunicationModuleSwitchLedOff(int ledChannel)
        {
            this.moduleCheckBoxControl.CommunicationModuleSwitchLedOff((LedChannel)ledChannel);
        }

        [MTFMethod(Description = "Check if communication module LED is on", DisplayName = "CommunicationModule - IsLedOn")]
        [MTFAllowedParameterValue("ledChannel", "All Led Channels", "0")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 1", "1")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 2", "2")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 3", "3")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 4", "4")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 5", "5")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 6", "6")]
        public bool CommunicationModuleIsLedOn(int ledChannel)
        {
            return this.moduleCheckBoxControl.CommunicationModuleIsLedOn((LedChannel)ledChannel);
        }

        [MTFMethod(Description = "Get binning code from communication module", DisplayName = "CommunicationModule - GetLedBinningCode")]
        [MTFAllowedParameterValue("ledChannel", "All Led Channels", "0")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 1", "1")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 2", "2")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 3", "3")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 4", "4")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 5", "5")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 6", "6")]
        public double CommunicationModuleGetLedBinningCode(int ledChannel)
        {
            return this.moduleCheckBoxControl.CommunicationModuleGetLedBinningCode((LedChannel)ledChannel);
        }

        [MTFMethod(Description = "Get current from communication module", DisplayName = "CommunicationModule - GetLedCurrent")]
        [MTFAllowedParameterValue("ledChannel", "All Led Channels", "0")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 1", "1")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 2", "2")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 3", "3")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 4", "4")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 5", "5")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 6", "6")]
        public double CommunicationModuleGetLedCurrent(int ledChannel)
        {
            return this.moduleCheckBoxControl.CommunicationModuleGetLedCurrent((LedChannel)ledChannel);
        }

        [MTFMethod(Description = "Get voltage from communication module", DisplayName = "CommunicationModule - GetLedVoltage")]
        [MTFAllowedParameterValue("ledChannel", "All Led Channels", "0")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 1", "1")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 2", "2")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 3", "3")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 4", "4")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 5", "5")]
        [MTFAllowedParameterValue("ledChannel", "Led Channel 6", "6")]
        public double CommunicationModuleGetLedVoltage(int ledChannel)
        {
            return this.moduleCheckBoxControl.CommunicationModuleGetLedVoltage((LedChannel)ledChannel);
        }

        [MTFMethod(Description = "Check if communication module is communicating", DisplayName = "CommunicationModule - IsCommunicating")]
        public bool CommunicationModuleIsCommunicating()
        {
            return this.moduleCheckBoxControl.CommunicationModuleIsCommunicating();
        }

        [MTFMethod(Description = "Close communication module", DisplayName = "CommunicationModule - Dispose")]
        public void DisposeCommunicationModule()
        {
            this.moduleCheckBoxControl.DisposeCommunicationModule();
        }

        [MTFMethod(Description = "Method to see if a connection is active", DisplayName = "Active Connection")]
        public bool ActiveConnection()
        {
            return this.moduleCheckBoxControl.ActiveConnection();
        }

        [MTFMethod(Description = "Motor movement init", DisplayName = "Amis Init")]
        public void AmisInit()
        {
            this.moduleCheckBoxControl.AmisInit();
        }

        [MTFMethod(DisplayName = "Calibrate Current Sources")]
        public void CalibrateCurrentSources()
        {
            this.moduleCheckBoxControl.CalibrateCurrentSources();
        }

        [MTFMethod(DisplayName = "Control Fan")]
        public void ControlFan(bool State)
        {
            this.moduleCheckBoxControl.ControlFan(State);
        }

        [MTFMethod(DisplayName = "Current Sources Automatic Info Refresh")]
        public void CurrentSourcesAutomaticInfoRefresh(bool State)
        {
            this.moduleCheckBoxControl.CurrentSourcesAutomaticInfoRefresh(State);
        }

        [MTFMethod(DisplayName = "Get All Recieved CAN Frames")]
        [MTFAllowedParameterValue("Can", "Can 1", "1")]
        [MTFAllowedParameterValue("Can", "Can 2", "2")]
        public List<CanFrame> GetAllRecievedCanFrames(int Can)
        {
            return this.moduleCheckBoxControl.GetAllRecievedCanFrames(getCan(Can));
        }

        [MTFMethod(Description = "Method to provide ambient temperature", DisplayName = "Get Ambient Temperature")]
        public double GetAmbientTemperature()
        {
            return this.moduleCheckBoxControl.GetAmbientTemperature();
        }

        [MTFMethod(DisplayName = "Get Amis Result")]
        [MTFAllowedParameterValue("Motor", "Motor 1", "1")]
        [MTFAllowedParameterValue("Motor", "Motor 2", "2")]
        public byte GetAmisResult(int Motor)
        {
            return this.moduleCheckBoxControl.GetAmisResult(getMotor(Motor));
        }

        [MTFMethod(DisplayName = "Get Amis Result By Index")]
        public byte GetAmisResultByIndex(int Motor)
        {
            return this.moduleCheckBoxControl.GetAmisResult(getMotor(Motor));
        }

        [MTFMethod(DisplayName = "Get Button")]
        [MTFAllowedParameterValue("Button", "Button 1", "1")]
        [MTFAllowedParameterValue("Button", "Button 2", "2")]
        [MTFAllowedParameterValue("Button", "Button 3", "3")]
        [MTFAllowedParameterValue("Button", "Button 4", "4")]
        [MTFAllowedParameterValue("Button", "Button 5", "5")]
        [MTFAllowedParameterValue("Button", "Button 6", "6")]
        [MTFAllowedParameterValue("Button", "Button 7", "7")]
        [MTFAllowedParameterValue("Button", "Button 8", "8")]
        [MTFAllowedParameterValue("Button", "Button 9", "9")]
        public bool GetButton(int Button)
        {
            return this.moduleCheckBoxControl.GetButton(getButton(Button));
        }

        [MTFMethod(DisplayName = "Get Current Source Current")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public double GetCurrentSourceCurrent(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceCurrent(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Diag")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public ushort GetCurrentSourceDiag(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceDiag(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source HW ID")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public byte[] GetCurrentSourceHwId(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceHwId(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source HW Revision")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public ushort GetCurrentSourceHwRevision(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceHwRevision(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Master PWM Duty Cycle")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public ushort GetCurrentSourceMasterPWMDutyCycle(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceMasterPWMDutyCycle(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Mode")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public ushort GetCurrentSourceMode(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceMode(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Prec10")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public ushort GetCurrentSourcePrec10(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourcePrec10(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Prec100")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public ushort GetCurrentSourcePrec100(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourcePrec100(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Set Current")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public ushort GetCurrentSourceSetCurrent(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceSetCurrent(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Slave PWM Duty Cycle")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public ushort GetCurrentSourceSlavePWMDutyCycle(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceSlavePWMDutyCycle(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Slave PWM Frequency")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public ushort GetCurrentSourceSlavePWMFrequency(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceSlavePWMFrequency(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Status")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public ushort GetCurrentSourceStatus(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceStatus(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source SW Revision")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public ushort GetCurrentSourceSwRevision(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceSwRevision(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Voltage")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public ushort GetCurrentSourceVoltage(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceVoltage(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Is Current Source Enabled")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public bool IsCurrentSourceEnabled(int CurrentSource)
        {
            return this.moduleCheckBoxControl.IsCurrentSourceEnabled(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Set Current Source Current")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public void SetCurrentSourceCurrent(int CurrentSource, ushort Current)
        {
            this.moduleCheckBoxControl.SetCurrentSourceCurrent(getSource(CurrentSource), Current);
        }

        [MTFMethod(DisplayName = "Set Current Source Current And Maximal Voltage")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public void SetCurrentSourceCurrentAndMaximalVoltage(int CurrentSource, ushort Current, ushort Voltage, byte Param)
        {
            this.moduleCheckBoxControl.SetCurrentSourceCurrentAndMaximalVoltage(getSource(CurrentSource), Current, Voltage, Param);
        }

        [MTFMethod(DisplayName = "Set Current Source Maximal Voltage")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public void SetCurrentSourceMaximalVoltage(int CurrentSource, ushort Voltage, byte Param)
        {
            this.moduleCheckBoxControl.SetCurrentSourceMaximalVoltage(getSource(CurrentSource), Voltage, Param);
        }

        [MTFMethod(DisplayName = "Set Current Source On/Off")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        public void SetCurrentSourceOnOff(int CurrentSource, bool Status)
        {
            this.moduleCheckBoxControl.SetCurrentSourceOnOff(getSource(CurrentSource), Status);
        }

        [MTFMethod(DisplayName = "Set Current Source Pwm")]
        [MTFAllowedParameterValue("CurrentSource", "All Current Sources", "0")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("CurrentSource", "Current Source 6", "6")]
        [MTFAllowedParameterValue("Mode", "Current Source Mode Master", "0")]
        [MTFAllowedParameterValue("Mode", "Current Source Mode Slave", "0")]
        public void SetCurrentSourcePwm(int CurrentSource, int Mode, ushort Frequency, ushort DutyCycle)
        {
            this.moduleCheckBoxControl.SetCurrentSourcePwm(getSource(CurrentSource), getSourceMode(Mode), Frequency, DutyCycle);
        }

        [MTFMethod(DisplayName = "Get Current Source Current By Index")]
        public double GetCurrentSourceCurrentByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceCurrent(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Diag By Index")]
        public ushort GetCurrentSourceDiagByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceDiag(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source HW ID By Index")]
        public byte[] GetCurrentSourceHwIdByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceHwId(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source HW Revision By Index")]
        public ushort GetCurrentSourceHwRevisionByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceHwRevision(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Master PWM Duty Cycle By Index")]
        public ushort GetCurrentSourceMasterPWMDutyCycleByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceMasterPWMDutyCycle(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Mode By Index")]
        public ushort GetCurrentSourceModeByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceMode(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Prec10 By Index")]
        public ushort GetCurrentSourcePrec10ByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourcePrec10(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Prec100 By Index")]
        public ushort GetCurrentSourcePrec100ByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourcePrec100(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Set Current By Index")]
        public ushort GetCurrentSourceSetCurrentByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceSetCurrent(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Slave PWM Duty Cycle By Index")]
        public ushort GetCurrentSourceSlavePWMDutyCycleByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceSlavePWMDutyCycle(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Slave PWM Frequency By Index")]
        public ushort GetCurrentSourceSlavePWMFrequencyByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceSlavePWMFrequency(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Status By Index")]
        public ushort GetCurrentSourceStatusByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceStatus(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source SW Revision By Index")]
        public ushort GetCurrentSourceSwRevisionByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceSwRevision(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Get Current Source Voltage By Index")]
        public ushort GetCurrentSourceVoltageByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.GetCurrentSourceVoltage(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Is Current Source Enabled By Index")]
        public bool IsCurrentSourceEnabledByIndex(int CurrentSource)
        {
            return this.moduleCheckBoxControl.IsCurrentSourceEnabled(getSource(CurrentSource));
        }

        [MTFMethod(DisplayName = "Set Current Source Current By Index")]
        public void SetCurrentSourceCurrentByIndex(int CurrentSource, ushort Current)
        {
            this.moduleCheckBoxControl.SetCurrentSourceCurrent(getSource(CurrentSource), Current);
        }

        [MTFMethod(DisplayName = "Set Current Source Current And Maximal Voltage By Index")]
        public void SetCurrentSourceCurrentAndMaximalVoltageByIndex(int CurrentSource, ushort Current, ushort Voltage, byte Param)
        {
            this.moduleCheckBoxControl.SetCurrentSourceCurrentAndMaximalVoltage(getSource(CurrentSource), Current, Voltage, Param);
        }

        [MTFMethod(DisplayName = "Set Current Source Maximal Voltage By Index")]
        public void SetCurrentSourceMaximalVoltageByIndex(int CurrentSource, ushort Voltage, byte Param)
        {
            this.moduleCheckBoxControl.SetCurrentSourceMaximalVoltage(getSource(CurrentSource), Voltage, Param);
        }

        [MTFMethod(DisplayName = "Set Current Source On/Off By Index")]
        public void SetCurrentSourceOnOffByIndex(int CurrentSource, bool Status)
        {
            this.moduleCheckBoxControl.SetCurrentSourceOnOff(getSource(CurrentSource), Status);
        }

        [MTFMethod(DisplayName = "Set Current Source Pwm By Index")]
        public void SetCurrentSourcePwmByIndex(int CurrentSource, int Mode, ushort Frequency, ushort DutyCycle)
        {
            this.moduleCheckBoxControl.SetCurrentSourcePwm(getSource(CurrentSource), getSourceMode(Mode), Frequency, DutyCycle);
        }

        [MTFMethod(DisplayName = "Get Encoder")]
        [MTFAllowedParameterValue("Encoder", "Encoder 1", "1")]
        [MTFAllowedParameterValue("Encoder", "Encoder 2", "2")]
        public byte GetEncoder(int Encoder)
        {
            return this.moduleCheckBoxControl.GetEncoder(getEncoder(Encoder));
        }
        [MTFMethod(DisplayName = "Get Encoder Press")]
        [MTFAllowedParameterValue("Encoder", "Encoder 1", "1")]
        [MTFAllowedParameterValue("Encoder", "Encoder 2", "2")]
        public bool GetEncoderPress(int Encoder)
        {
            return this.moduleCheckBoxControl.GetEncoderPress(getEncoder(Encoder));
        }
        [MTFMethod(DisplayName = "Get Encoder Value")]
        [MTFAllowedParameterValue("Encoder", "Encoder 1", "1")]
        [MTFAllowedParameterValue("Encoder", "Encoder 2", "2")]
        public int GetEncoderValue(int Encoder)
        {
            return this.moduleCheckBoxControl.GetEncoderValue(getEncoder(Encoder));
        }

        [MTFMethod(DisplayName = "Get Fan Current")]
        public double GetFanCurrent()
        {
            return this.moduleCheckBoxControl.GetFanCurrent();
        }

        [MTFMethod(DisplayName = "Get Fan Diag Duty Cycle")]
        [MTFAllowedParameterValue("Fan", "Fan 1", "1")]
        [MTFAllowedParameterValue("Fan", "Fan 2", "2")]
        public double GetFanDiagDutyCycle(int Fan)
        {
            return this.moduleCheckBoxControl.GetFanDiagDutyCycle(getFan(Fan));
        }
        [MTFMethod(DisplayName = "Get Fan Diag Frequency")]
        [MTFAllowedParameterValue("Fan", "Fan 1", "1")]
        [MTFAllowedParameterValue("Fan", "Fan 2", "2")]
        public double GetFanDiagFrequency(int Fan)
        {
            return this.moduleCheckBoxControl.GetFanDiagFrequency(getFan(Fan));
        }

        [MTFMethod(DisplayName = "Get FW Version")]
        public double GetFwVersion()
        {
            return this.moduleCheckBoxControl.GetFwVersion();
        }
        [MTFMethod(DisplayName = "Get General Current Sources Info")]
        public string GetGeneralCurrentSourcesInfo()
        {
            return this.moduleCheckBoxControl.GetGeneralCurrentSourcesInfo();
        }

        [MTFMethod(DisplayName = "Get Hall")]
        [MTFAllowedParameterValue("HallSensor", "Hall Sensor 1", "1")]
        [MTFAllowedParameterValue("HallSensor", "Hall Sensor 2", "2")]
        public bool GetHall(int HallSensor)
        {
            return this.moduleCheckBoxControl.GetHall(getHallSensor(HallSensor));
        }

        [MTFMethod(DisplayName = "Get Hash Code")]
        public double GetHashCode()
        {
            return this.moduleCheckBoxControl.GetHashCode();
        }
        [MTFMethod(DisplayName = "Get Heat Voltage")]
        public double GetHeatVoltage()
        {
            return this.moduleCheckBoxControl.GetHeatVoltage();
        }

        [MTFMethod(Description = "Method to provide HW ID of Module Check Box", DisplayName = "Get HW ID")]
        public string GetHwId()
        {
            return this.moduleCheckBoxControl.GetHwId();
        }

        [MTFMethod(DisplayName = "Get HW Version")]
        public int GetHwVersion()
        {
            return this.moduleCheckBoxControl.GetHwVersion();
        }

        [MTFMethod(DisplayName = "Get Internal Temperature")]
        public double GetInternalTemperature()
        {
            return this.moduleCheckBoxControl.GetInternalTemperature();
        }

        [MTFMethod(DisplayName = "Get Motor Actual Position")]
        [MTFAllowedParameterValue("Motor", "Motor 1", "1")]
        [MTFAllowedParameterValue("Motor", "Motor 2", "2")]
        public int GetMotorActualPosition(int Motor)
        {
            return this.moduleCheckBoxControl.GetMotorActualPosition(getMotor(Motor));
        }

        [MTFMethod(DisplayName = "Get Motor Actual Position By Index")]
        public int GetMotorActualPositionByIndex(int Motor)
        {
            return this.moduleCheckBoxControl.GetMotorActualPosition(getMotor(Motor));
        }

        [MTFMethod(DisplayName = "Get Motor Range")]
        [MTFAllowedParameterValue("Motor", "Motor 1", "1")]
        [MTFAllowedParameterValue("Motor", "Motor 2", "2")]
        [MTFAllowedParameterValue("Range", "Range 1", "1")]
        [MTFAllowedParameterValue("Range", "Range 2", "2")]
        public int GetMotorRange(int Motor, int Range)
        {
            return this.moduleCheckBoxControl.GetMotorRange(getMotor(Motor), getRange(Range));
        }

        [MTFMethod(DisplayName = "Get Motor Range By Indexes")]
        public int GetMotorRangeByIndexes(int Motor, int Range)
        {
            return this.moduleCheckBoxControl.GetMotorRange(getMotor(Motor), getRange(Range));
        }

        [MTFMethod(DisplayName = "Get New Recieved Can Frames")]
        [MTFAllowedParameterValue("Can", "Can 1", "1")]
        [MTFAllowedParameterValue("Can", "Can 2", "2")]
        public List<CanFrame> GetNewRecievedCanFrames(int Can, bool ClearList)
        {
            return this.moduleCheckBoxControl.GetNewRecievedCanFrames(getCan(Can), ClearList);
        }
        [MTFMethod(DisplayName = "Get RX Current")]
        [MTFAllowedParameterValue("Rx", "Rx 1", "1")]
        [MTFAllowedParameterValue("Rx", "Rx 2", "2")]
        [MTFAllowedParameterValue("Rx", "Rx 3", "3")]
        [MTFAllowedParameterValue("Rx", "Rx 4", "4")]
        [MTFAllowedParameterValue("Rx", "Rx 5", "5")]
        [MTFAllowedParameterValue("Rx", "Rx 6", "6")]
        [MTFAllowedParameterValue("Rx", "Rx 7", "7")]
        [MTFAllowedParameterValue("Rx", "Rx 8", "8")]
        public double GetRxCurrent(int Rx)
        {
            return this.moduleCheckBoxControl.GetRxCurrent(getRx(Rx));
        }
        [MTFMethod(DisplayName = "Get RX Value")]
        [MTFAllowedParameterValue("Rx", "Rx 1", "1")]
        [MTFAllowedParameterValue("Rx", "Rx 2", "2")]
        [MTFAllowedParameterValue("Rx", "Rx 3", "3")]
        [MTFAllowedParameterValue("Rx", "Rx 4", "4")]
        [MTFAllowedParameterValue("Rx", "Rx 5", "5")]
        [MTFAllowedParameterValue("Rx", "Rx 6", "6")]
        [MTFAllowedParameterValue("Rx", "Rx 7", "7")]
        [MTFAllowedParameterValue("Rx", "Rx 8", "8")]
        public double GetRxValue(int Rx)
        {
            return this.moduleCheckBoxControl.GetRxValue(getRx(Rx));
        }
        [MTFMethod(DisplayName = "Get Safety In")]
        public Safety GetSafetyIn()
        {
            return this.moduleCheckBoxControl.GetSafetyIn();
        }
        [MTFMethod(DisplayName = "Get Sended Can Frames")]
        [MTFAllowedParameterValue("Can", "Can 1", "1")]
        [MTFAllowedParameterValue("Can", "Can 2", "2")]
        public List<CanFrame> GetSendedCanFrames(int Can)
        {
            return this.moduleCheckBoxControl.GetSendedCanFrames(getCan(Can));
        }
        
        [MTFMethod(DisplayName = "New Measurement")]
        public bool NewMeasurement(bool AmisReset)
        {
            return this.moduleCheckBoxControl.NewMeasurement(AmisReset);
        }
        
        [MTFMethod(DisplayName = "Run Motor Find Edge")]
        [MTFAllowedParameterValue("Motor", "Motor 1", "1")]
        [MTFAllowedParameterValue("Motor", "Motor 2", "2")]
        public void RunMotorFindEdge(int Motor, bool Direction, bool RisingEdge)
        {
            this.moduleCheckBoxControl.RunMotorFindEdge(getMotor(Motor), Direction, RisingEdge);
        }

        [MTFMethod(DisplayName = "Run Motor Find Edge By Index")]
        public void RunMotorFindEdgeByIndex(int Motor, bool Direction, bool RisingEdge)
        {
            this.moduleCheckBoxControl.RunMotorFindEdge(getMotor(Motor), Direction, RisingEdge);
        }

        [MTFMethod(DisplayName = "Run Motor Range")]
        [MTFAllowedParameterValue("Motor", "Motor 1", "1")]
        [MTFAllowedParameterValue("Motor", "Motor 2", "2")]
        public void RunMotorRange(int Motor, bool Direction, bool RisingEdge, int MinPosition, int MaxPosition)
        {
            this.moduleCheckBoxControl.RunMotorRange(getMotor(Motor), Direction, RisingEdge, MinPosition, MaxPosition);
        }

        [MTFMethod(DisplayName = "Run Motor Range By Index")]
        public void RunMotorRangeByIndex(int Motor, bool Direction, bool RisingEdge, int MinPosition, int MaxPosition)
        {
            this.moduleCheckBoxControl.RunMotorRange(getMotor(Motor), Direction, RisingEdge, MinPosition, MaxPosition);
        }

        [MTFMethod(DisplayName = "Send Can Frame")]
        [MTFAllowedParameterValue("Can", "Can 1", "1")]
        [MTFAllowedParameterValue("Can", "Can 2", "2")]
        public void SendCanFrame(int Can, MtfCANframe mtfCanFrame)
        {
            CanFrame canFrame = new CanFrame(mtfCanFrame.Delay, mtfCanFrame.ID,
                mtfCanFrame.EIDLow, mtfCanFrame.EIDHigh, mtfCanFrame.Data);

            this.moduleCheckBoxControl.SendCanFrame(getCan(Can), canFrame);
        }

        [MTFMethod(DisplayName = "Send Lin Frame")]
        public void SendLinFrame(LinFrame LinFrame)
        {
            this.moduleCheckBoxControl.SendLinFrame(LinFrame);
        }

        [MTFMethod(DisplayName = "Send Raw Data")]
        public void SendRawData(string Data)
        {
            this.moduleCheckBoxControl.SendRawData(Data);
        }

        [MTFMethod(DisplayName = "Send Uart Frame Via Can Bus")]
        public void SendUartFrameViaCanBus(byte[] Data)
        {
            this.moduleCheckBoxControl.SendUartFrameViaCanBus(Data);
        }

        [MTFMethod(DisplayName = "Set Adjustable Source")]
        public void SetAdjustableSource(ushort Voltage)
        {
            this.moduleCheckBoxControl.SetAdjustableSource(Voltage);
        }

        [MTFMethod(DisplayName = "Set Mode")]
        [MTFAllowedParameterValue("Mode", "CAN2 To UART", "0")]
        [MTFAllowedParameterValue("Mode", "Default", "1")]
        [MTFAllowedParameterValue("Mode", "FAN1 Diag Pull Up Off Pull Down On", "2")]
        public void SetMode(int Mode)
        {
            this.moduleCheckBoxControl.SetMode(getMode(Mode));
        }

        [MTFMethod(DisplayName = "Set Motor Parameters")]
        [MTFAllowedParameterValue("Motor", "Motor 1", "1")]
        [MTFAllowedParameterValue("Motor", "Motor 2", "2")]
        public void SetMotorParameters(int Motor, byte iRun, byte iHold, byte vMin, byte vMax, byte Acc, byte StepMode)
        {
            this.moduleCheckBoxControl.SetMotorParameters(getMotor(Motor), iRun, iHold, vMin, vMax, Acc, StepMode);
        }

        [MTFMethod(DisplayName = "Set Motor Parameters By Index")]
        public void SetMotorParametersByIndex(int Motor, byte iRun, byte iHold, byte vMin, byte vMax, byte Acc, byte StepMode)
        {
            this.moduleCheckBoxControl.SetMotorParameters(getMotor(Motor), iRun, iHold, vMin, vMax, Acc, StepMode);
        }

        [MTFMethod(DisplayName = "Set Motor Position")]
        [MTFAllowedParameterValue("Motor", "Motor 1", "1")]
        [MTFAllowedParameterValue("Motor", "Motor 2", "2")]
        public void SetMotorPosition(int Motor, int Position, bool Blocking)
        {
            this.moduleCheckBoxControl.SetMotorPosition(getMotor(Motor), Position, Blocking);
        }

        [MTFMethod(DisplayName = "Set Motor Position By Index")]
        public void SetMotorPositionByIndex(int Motor, int Position, bool Blocking)
        {
            this.moduleCheckBoxControl.SetMotorPosition(getMotor(Motor), Position, Blocking);
        }

        [MTFMethod(DisplayName = "Set Relay")]
        public void SetRelay(bool Relay1Value, bool Relay2Value)
        {
            this.moduleCheckBoxControl.SetRelay(Relay1Value, Relay2Value);
        }
        [MTFMethod(DisplayName = "Set Safety Out")]
        [MTFAllowedParameterValue("Value", "Not Safe", "0")]
        [MTFAllowedParameterValue("Value", "Not Set", "1")]
        [MTFAllowedParameterValue("Value", "Safe", "2")]
        public void SetSafetyOut(int Value)
        {
            this.moduleCheckBoxControl.SetSafetyOut(getSafety(Value));
        }
        [MTFMethod(DisplayName = "Uart Over CAN Read Data")]
        public byte[] UartOverCANReadData(bool Clear)
        {
            return this.moduleCheckBoxControl.UartOverCANReadData(Clear);
        }
        [MTFMethod(DisplayName = "Perform Low Current Test")]
        [MTFAllowedParameterValue("currentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("currentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("currentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("currentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("currentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("currentSource", "Current Source 6", "6")]
        [MTFAdditionalParameterInfo(ParameterName = "pulseWidth", Description = "[ms]")]
        [MTFAdditionalParameterInfo(ParameterName = "current", Description = "[100μA]")]
        [MTFAdditionalParameterInfo(ParameterName = "maxVoltage", Description = "[mV]")]
        public MtfLowCurrentTestResult PerformLowCurrentTest(int currentSource, ushort pulseWidth, ushort current, ushort maxVoltage)
        {
            var result = this.moduleCheckBoxControl.PerformLowCurrentTest(getSource(currentSource), pulseWidth, current, maxVoltage);

            return new MtfLowCurrentTestResult()
            {
                Voltage = result.Voltage,
                Samples = result.Samples,
                Status = (MtfLowCurrentTestStatus)result.Status
            };
        }
        [MTFMethod(DisplayName = "Perform Camera Trigger")]
        [MTFAllowedParameterValue("currentSource", "Current Source 1", "1")]
        [MTFAllowedParameterValue("currentSource", "Current Source 2", "2")]
        [MTFAllowedParameterValue("currentSource", "Current Source 3", "3")]
        [MTFAllowedParameterValue("currentSource", "Current Source 4", "4")]
        [MTFAllowedParameterValue("currentSource", "Current Source 5", "5")]
        [MTFAllowedParameterValue("currentSource", "Current Source 6", "6")]
        [MTFAdditionalParameterInfo(ParameterName = "ledPulseWidth", Description ="[ms]")]
        [MTFAdditionalParameterInfo(ParameterName = "cameraTriggerDelay", Description = "[ms]")]
        [MTFAdditionalParameterInfo(ParameterName = "cameraTriggerWidth", Description = "[ms]")]
        [MTFAdditionalParameterInfo(ParameterName = "current", Description = "[mA]")]
        [MTFAdditionalParameterInfo(ParameterName = "maxVoltage", Description = "[mV]")]
        public MtfCameraTriggerStatus PerformCameraTrigger(int currentSource, ushort ledPulseWidth, ushort cameraTriggerDelay, ushort cameraTriggerWidth, ushort current, ushort maxVoltage)
        {
            return (MtfCameraTriggerStatus)this.moduleCheckBoxControl.PerformCameraTrigger(getSource(currentSource), ledPulseWidth, cameraTriggerDelay, cameraTriggerWidth, current, maxVoltage);
        }

        private CurrentSources getSource(int source)
        {
            switch (source)
            {
                case 0:
                    {
                        return CurrentSources.AllCurrentSources;
                    }
                case 1:
                    {
                        return CurrentSources.CurrentSource1;
                    }
                case 2:
                    {
                        return CurrentSources.CurrentSource2;
                    }
                case 3:
                    {
                        return CurrentSources.CurrentSource3;
                    }
                case 4:
                    {
                        return CurrentSources.CurrentSource4;
                    }
                case 5:
                    {
                        return CurrentSources.CurrentSource5;
                    }
                case 6:
                    {
                        return CurrentSources.CurrentSource6;
                    }
                default:
                    {
                        throw new Exception("Invalid Current Source Index");
                    }
            }
        }
        private MotorIndexes getMotor(int motor)
        {
            switch (motor)
            {
                case 1:
                    {
                        return MotorIndexes.Motor1;
                    }
                case 2:
                    {
                        return MotorIndexes.Motor2;
                    }
                default:
                {
                    throw new Exception("Invalid Motor Index");
                }
            }
        }
        private Safety getSafety(int safety)
        {
            switch (safety)
            {
                case 0:
                    {
                        return Safety.NotSafe;
                    }
                case 1:
                    {
                        return Safety.NotSet;
                    }
                case 2:
                    {
                        return Safety.Safe;
                    }
                default:
                    {
                        throw new Exception("Invalid Safety Index");
                    }
            }
        }
        private Cans getCan(int can)
        {
            switch (can)
            {
                case 1:
                    {
                        return Cans.Can1;
                    }
                case 2:
                    {
                        return Cans.Can2;
                    }
                default:
                    {
                        throw new Exception("Invalid Can Index");
                    }
            }
        }

        private Ranges getRange(int range)
        {
            switch (range)
            {
                case 1:
                    {
                        return Ranges.Range1;
                    }
                case 2:
                    {
                        return Ranges.Range2;
                    }
                default:
                    {
                        throw new Exception("Invalid Range Index");
                    }
            }
        }
        private CurrentSourceModes getSourceMode(int mode)
        {
            switch (mode)
            {
                case 0:
                    {
                        return CurrentSourceModes.Master;
                    }
                case 1:
                    {
                        return CurrentSourceModes.Slave;
                    }
                default:
                    {
                        throw new Exception("Invalid Source Mode Index");
                    }
            }
        }
        private Modes getMode(int mode)
        {
            switch (mode)
            {
                case 0:
                    {
                        return Modes.CAN2ToUART;
                    }
                case 1:
                    {
                        return Modes.Default;
                    }
                case 2:
                    {
                        return Modes.FAN1DiagPullUpOffPullDownOn;
                    }
                default:
                    {
                        throw new Exception("Invalid Mode Index");
                    }
            }
        }
        private Rxs getRx(int rx)
        {
            switch (rx)
            {
                case 1:
                    {
                        return Rxs.Rx1;
                    }
                case 2:
                    {
                        return Rxs.Rx2;
                    }
                case 3:
                    {
                        return Rxs.Rx3;
                    }
                case 4:
                    {
                        return Rxs.Rx4;
                    }
                case 5:
                    {
                        return Rxs.Rx5;
                    }
                case 6:
                    {
                        return Rxs.Rx6;
                    }
                case 7:
                    {
                        return Rxs.Rx7;
                    }
                case 8:
                    {
                        return Rxs.Rx8;
                    }
                default:
                    {
                        throw new Exception("Invalid Rx Index");
                    }
            }
        }
        private HallSensors getHallSensor(int sensor)
        {
            switch (sensor)
            {
                case 1:
                    {
                        return HallSensors.HallSensor1;
                    }
                case 2:
                    {
                        return HallSensors.HallSensor2;
                    }
                default:
                    {
                        throw new Exception("Invalid Hall Sensor Index");
                    }
            }
        }
        private Fans getFan(int fan)
        {
            switch (fan)
            {
                case 1:
                    {
                        return Fans.Fan1;
                    }
                case 2:
                    {
                        return Fans.Fan2;
                    }
                default:
                    {
                        throw new Exception("Invalid Fan Index");
                    }
            }
        }
        private Encoders getEncoder(int encoder)
        {
            switch (encoder)
            {
                case 1:
                    {
                        return Encoders.Encoder1;
                    }
                case 2:
                    {
                        return Encoders.Encoder2;
                    }
                default:
                    {
                        throw new Exception("Invalid Encoder Index");
                    }
            }
        }
        private Buttons getButton(int button)
        {
            switch (button)
            {
                case 1:
                    {
                        return Buttons.Button1;
                    }
                case 2:
                    {
                        return Buttons.Button2;
                    }
                case 3:
                    {
                        return Buttons.Button3;
                    }
                case 4:
                    {
                        return Buttons.Button4;
                    }
                case 5:
                    {
                        return Buttons.Button5;
                    }
                case 6:
                    {
                        return Buttons.Button6;
                    }
                case 7:
                    {
                        return Buttons.Button7;
                    }
                case 8:
                    {
                        return Buttons.Button8;
                    }
                case 9:
                    {
                        return Buttons.Button9;
                    }
                default:
                    {
                        throw new Exception("Invalid Button Index");
                    }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Managed resources
                    this.moduleCheckBoxControl.Dispose();
                }

                // Unmanaged resources

                disposed = true;
            }
        }
    }

    [MTFKnownClass]
    public class MtfCANframe
    {
        public UInt32 Delay { get; set; }
        public ushort ID { get; set; }
        public ushort EIDLow { get; set; }
        public ushort EIDHigh { get; set; }
        public byte[] Data { get; set; }
    }

    [MTFKnownClass]
    public class MtfLowCurrentTestResult
    {
        public ushort Voltage { get; set; }
        public ushort Samples { get; set; }
        public MtfLowCurrentTestStatus Status { get; set; }
    }

    public enum MtfLowCurrentTestStatus
    {
        NoResponse = 0,
        NotCorrectCsSw = 1,
        NoSamples = 2,
        Ok = 65534,
        Error = 65535
    }

    public enum MtfCameraTriggerStatus
    {
        NoResponse = 0,
        NotCorrectCsSw = 1,
        Ok = 65534,
        Error = 65535
    }
}
