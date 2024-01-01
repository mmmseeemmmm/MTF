using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LED_Module_Check_Common;
using CommunicationModule;
using ModuleCheckBox;
using ModuleCheckBox.Enums;
using ModuleCheckBox.Models;

namespace ModuleCheckBoxComponent
{
    public class ModuleCheckBoxControl : IDisposable
    {
        private bool disposed;
        private Driver moduleCheckBox;
        private CommunicationModuleBase communicationModule;

        public void Connect(string portName)
        {
            if (portName == "USB")
            {
                List<string> ports = Utils.ComPortIdent.GetDeviceComPortNamesIfConnected("04D8", "400A");
                portName = ports.FirstOrDefault();
            }

            this.moduleCheckBox = new ModuleCheckBox.Driver(portName, false);
        }

        public void Disconnect()
        {
            if (this.moduleCheckBox != null)
            {
                this.moduleCheckBox.Dispose();
                this.moduleCheckBox = null;
            }
        }

        public void InitCommunicationModule(CommunicationModuleType communicationModuleType, string pathToXML)
        {
            this.DisposeCommunicationModule();
            string xmlFile = string.Empty;
            LEDModule ledModule = new LEDModule();

            if (pathToXML == string.Empty)
            {
                switch (communicationModuleType)
                {
                    case CommunicationModuleType.BmwLaserSat:
                        xmlFile = @"mtfLibs\ModuleCheckBoxComponent\XMLs\BMW_G07_G1x_LaserSat.xml";
                        break;
                    case CommunicationModuleType.LMMGeneralControl:
                        xmlFile = pathToXML;
                        break;
                    case CommunicationModuleType.DmlPavSoftwareControl:
                        xmlFile = @"mtfLibs\ModuleCheckBoxComponent\XMLs\AUDI_AU516_CBEV_DML_TEST.xml";
                        break;
                    default:
                        throw new Exception("Unknown Communication Module Type!");
                }
            }
            else
            {
                xmlFile = pathToXML;
            }

            FileInfo destFile = new FileInfo(xmlFile);
            if (destFile.Exists)
            {
                StreamReader streamToDeser = null;
                try
                {
                    streamToDeser = new StreamReader(xmlFile);
                    System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(ledModule.GetType());
                    ledModule = (LEDModule)x.Deserialize(streamToDeser);
                    
                    streamToDeser.Close();
                }
                catch (Exception ex)
                {
                    Utils.ExceptionHandling.Handle(ex);
                }
                if (streamToDeser != null)
                {
                    streamToDeser.Close();
                }
            }

            LEDModuleData ledModuleData = ledModule.Data.FirstOrDefault(x => x.Communications != null && x.Communications.Count > 0 && x.Communications[0].Enable);

            switch (communicationModuleType)
            {
                case CommunicationModuleType.BmwLaserSat:
                    this.communicationModule = new BmwLaserSat(this.moduleCheckBox, ledModuleData, @"mtfLibs\ModuleCheckBoxComponent\HEXs\");
                    break;
                case CommunicationModuleType.LMMGeneralControl:
                    this.communicationModule = new LmmGeneralControl(this.moduleCheckBox, ledModuleData);
                    break;
                case CommunicationModuleType.DmlPavSoftwareControl:
                    this.communicationModule = new DmlPavSoftwareControl(this.moduleCheckBox, ledModuleData);
                    break;
                default:
                    throw new Exception("Unknown Communication Module Type!");
            }
        }

        public double CommunicationModuleGetTemperature(string ntcName)
        {
            this.checkCommunicationModule();
            return this.communicationModule.GetTemperature(ntcName);
        }

        public void CommunicationModuleSwitchLedOn(LedChannel ledChannel)
        {
            this.checkCommunicationModule();
            this.communicationModule.SwitchLedOn(ledChannel);
        }

        public void CommunicationModuleSwitchLedOff(LedChannel ledChannel)
        {
            this.checkCommunicationModule();
            this.communicationModule.SwitchLedOff(ledChannel);
        }

        public bool CommunicationModuleIsLedOn(LedChannel ledChannel)
        {
            this.checkCommunicationModule();
            return this.communicationModule.IsLedOn(ledChannel);
        }

        public double CommunicationModuleGetLedBinningCode(LedChannel ledChannel)
        {
            this.checkCommunicationModule();
            return this.communicationModule.GetLedBinningCode(ledChannel);
        }

        public double CommunicationModuleGetLedCurrent(LedChannel ledChannel)
        {
            this.checkCommunicationModule();
            return this.communicationModule.GetLedCurrent(ledChannel);
        }

        public double CommunicationModuleGetLedVoltage(LedChannel ledChannel)
        {
            this.checkCommunicationModule();
            return this.communicationModule.GetLedVoltage(ledChannel);
        }

        public bool CommunicationModuleIsCommunicating()
        {
            this.checkCommunicationModule();
            return this.communicationModule.IsCommunicating();
        }

        private void checkCommunicationModule()
        {
            if (this.communicationModule == null)
            {
                throw new Exception("Communication Module Is Not Initialized!");
            }
        }

        public void DisposeCommunicationModule()
        {
            if (this.communicationModule != null)
            {
                this.communicationModule.Dispose();
                this.communicationModule = null;
            }
        }

        public bool ActiveConnection()
        {
            return this.moduleCheckBox.ActiveConnection();
        }

        public void AmisInit()
        {
            this.moduleCheckBox.AmisInit();
        }

        public void CalibrateCurrentSources()
        {
            this.moduleCheckBox.CalibrateCurrentSources();
        }

        public void ControlFan(bool onOff)
        {
            this.moduleCheckBox.ControlFan(onOff);
        }

        public void CurrentSourcesAutomaticInfoRefresh(bool enable)
        {
            this.moduleCheckBox.CurrentSourcesAutomaticInfoRefresh(enable);
        }

        public List<CanFrame> GetAllRecievedCanFrames(Cans can)
        {
            return this.moduleCheckBox.GetAllRecievedCanFrames(can);
        }

        public double GetAmbientTemperature()
        {
            return this.moduleCheckBox.GetAmbientTemperature();
        }

        public byte GetAmisResult(MotorIndexes motor)
        {
            return this.moduleCheckBox.GetAmisResult(motor);
        }

        public bool GetButton(Buttons button)
        {
            return this.moduleCheckBox.GetButton(button);
        }

        public double GetCurrentSourceCurrent(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourceCurrent(currentSource);
        }
        public ushort GetCurrentSourceDiag(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourceDiag(currentSource);
        }
        public byte[] GetCurrentSourceHwId(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourceHwId(currentSource);
        }
        public ushort GetCurrentSourceHwRevision(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourceHwRevision(currentSource);
        }
        public ushort GetCurrentSourceMasterPWMDutyCycle(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourcePwmDutyCycle(currentSource, CurrentSourceModes.Master);
        }
        public ushort GetCurrentSourceMasterPWMFrequency(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourcePwmFrequency(currentSource, CurrentSourceModes.Master);
        }
        public ushort GetCurrentSourceMode(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourceMode(currentSource);
        }
        public ushort GetCurrentSourcePrec10(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourcePrec10(currentSource);
        }
        public ushort GetCurrentSourcePrec100(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourcePrec100(currentSource);
        }
        public ushort GetCurrentSourceSetCurrent(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourceSetCurrent(currentSource);
        }
        public ushort GetCurrentSourceSlavePWMDutyCycle(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourcePwmDutyCycle(currentSource, CurrentSourceModes.Slave);
        }
        public ushort GetCurrentSourceSlavePWMFrequency(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourcePwmFrequency(currentSource, CurrentSourceModes.Slave);
        }
        public ushort GetCurrentSourceStatus(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourceStatus(currentSource);
        }
        public ushort GetCurrentSourceSwRevision(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourceSwRevision(currentSource);
        }
        public ushort GetCurrentSourceVoltage(CurrentSources currentSource)
        {
            return this.moduleCheckBox.GetCurrentSourceVoltage(currentSource);
        }

        public byte GetEncoder(Encoders encoder)
        {
            return this.moduleCheckBox.GetEncoder(encoder);
        }
        public bool GetEncoderPress(Encoders encoder)
        {
            return this.moduleCheckBox.GetEncoderPress(encoder);
        }
        public int GetEncoderValue(Encoders encoder)
        {
            return this.moduleCheckBox.GetEncoderValue(encoder);
        }

        public double GetFanCurrent()
        {
            return this.moduleCheckBox.GetFanCurrent();
        }
        public double GetFanDiagDutyCycle(Fans fan)
        {
            return this.moduleCheckBox.GetFanDiagDutyCycle(fan);
        }
        public double GetFanDiagFrequency(Fans fan)
        {
            return this.moduleCheckBox.GetFanDiagFrequency(fan);
        }
        public double GetFwVersion()
        {
            return this.moduleCheckBox.GetFwVersion();
        }
        public string GetGeneralCurrentSourcesInfo()
        {
            return this.moduleCheckBox.GetGeneralCurrentSourcesInfo();
        }
        public bool GetHall(HallSensors hallSensor)
        {
            return this.moduleCheckBox.GetHall(hallSensor);
        }
        public double GetHashCode()
        {
            return this.moduleCheckBox.GetHashCode();
        }
        public double GetHeatVoltage()
        {
            return this.moduleCheckBox.GetHeatVoltage();
        }

        public string GetHwId()
        {
            return this.moduleCheckBox.GetHwId();
        }

        public int GetHwVersion()
        {
            return this.moduleCheckBox.GetHwVersion();
        }
        public double GetInternalTemperature()
        {
            return this.moduleCheckBox.GetInternalTemperature();
        }
        public int GetMotorActualPosition(MotorIndexes motor)
        {
            return this.moduleCheckBox.GetMotorActualPosition(motor);
        }
        public int GetMotorRange(MotorIndexes motor, Ranges range)
        {
            return this.moduleCheckBox.GetMotorRange(motor, range);
        }
        public List<CanFrame> GetNewRecievedCanFrames(Cans can, bool clearList)
        {
            return this.moduleCheckBox.GetNewRecievedCanFrames(can, clearList);
        }
        public double GetRxCurrent(Rxs rx)
        {
            return this.moduleCheckBox.GetRxCurrent(rx);
        }
        public double GetRxValue(Rxs rx)
        {
            return this.moduleCheckBox.GetRxValue(rx);
        }
        public Safety GetSafetyIn()
        {
            return this.moduleCheckBox.GetSafetyIn();
        }
        public List<CanFrame> GetSendedCanFrames(Cans can)
        {
            return this.moduleCheckBox.GetSendedCanFrames(can);
        }

        public bool IsCurrentSourceEnabled(CurrentSources currentSource)
        {
            return this.moduleCheckBox.IsCurrentSourceEnabled(currentSource);
        }
        public bool IsCurrentSourceStable(CurrentSources currentSource)
        {
            return this.moduleCheckBox.IsCurrentSourceStable(currentSource);
        }

        public bool NewMeasurement(bool amisReset)
        {
            return this.moduleCheckBox.NewMeasurement(amisReset);
        }

        public void RunMotorFindEdge(MotorIndexes motor, bool direction, bool risingEdge)
        {
            this.moduleCheckBox.RunMotorFindEdge(motor, direction, risingEdge);
        }
        public void RunMotorRange(MotorIndexes motor, bool direction, bool risingEdge, int minPosition, int maxPosition)
        {
            this.moduleCheckBox.RunMotorRange(motor, direction, risingEdge, minPosition, maxPosition);
        }

        public void SendCanFrame(Cans can, CanFrame canFrame)
        {
        this.moduleCheckBox.SendCanFrame(can, canFrame);
        }
        public void SendLinFrame(LinFrame linFrame)
        {
            this.moduleCheckBox.SendLinFrame(linFrame);
        }
        public void SendRawData(string data)
        {
            this.moduleCheckBox.SendRawData(data);
        }
        public void SendUartFrameViaCanBus(byte[] data)
        {
            this.moduleCheckBox.SendUartFrameViaCanBus(data);
        }

        public void SetAdjustableSource(ushort Voltage)
        {
        this.moduleCheckBox.SetAdjustableSource(Voltage);
        }
        public void SetCurrentSourceCurrent(CurrentSources currentSource, ushort current)
        {
            this.moduleCheckBox.SetCurrentSourceCurrent(currentSource, current);
        }
        public void SetCurrentSourceCurrentAndMaximalVoltage(CurrentSources currentSource, ushort current, ushort voltage, byte param)
        {
            this.moduleCheckBox.SetCurrentSourceCurrentAndMaximalVoltage(currentSource, current, voltage, param);
        }
        public void SetCurrentSourceMaximalVoltage(CurrentSources currentSource, ushort voltage, byte param)
        {
            this.moduleCheckBox.SetCurrentSourceMaximalVoltage(currentSource, voltage, param);
        }
        public void SetCurrentSourceOnOff(CurrentSources currentSource, bool status)
        {
            this.moduleCheckBox.SetCurrentSourceOnOff(currentSource, status);
        }
        public void SetCurrentSourcePwm(CurrentSources currentSource, CurrentSourceModes mode, ushort frequency, ushort dutyCycle)
        {
            this.moduleCheckBox.SetCurrentSourcePwm(currentSource, mode, frequency, dutyCycle);
        }

        public void SetMode(Modes mode)
        {
            this.moduleCheckBox.SetMode(mode);
        }
        public void SetMotorParameters(MotorIndexes motor, byte iRun, byte iHold, byte vMin, byte vMax, byte acc, byte stepMode)
        {
            this.moduleCheckBox.SetMotorParameters(motor, iRun, iHold, vMin, vMax, acc, stepMode);
        }
        public void SetMotorPosition(MotorIndexes motor, int position, bool blocking)
        {
            this.moduleCheckBox.SetMotorPosition(motor, position, blocking);
        }
        public void SetRelay(bool relay1Value, bool relay2Value)
        {
            this.moduleCheckBox.SetRelay(relay1Value, relay2Value);
        }
        public void SetSafetyOut(Safety value)
        {
            this.moduleCheckBox.SetSafetyOut(value);
        }
        public byte[] UartOverCANReadData(bool clear)
        {
            return this.moduleCheckBox.UartOverCANReadData(clear);
        }
        public LowCurrentTestResult PerformLowCurrentTest(CurrentSources currentSource, ushort pulseWidth, ushort current, ushort maxVoltage)
        {
            return this.moduleCheckBox.PerformLowCurrentTest(currentSource, pulseWidth, current, maxVoltage);
        }
        public CameraTriggerStatus PerformCameraTrigger(CurrentSources currentSource, ushort ledPulseWidth, ushort cameraTriggerDelay, ushort cameraTriggerWidth, ushort current, ushort maxVoltage)
        {
            return (CameraTriggerStatus)this.moduleCheckBox.PerformCameraTrigger(currentSource, ledPulseWidth, cameraTriggerDelay, cameraTriggerWidth, current, maxVoltage).Status;
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
                    this.communicationModule?.Dispose();
                    this.communicationModule = null;
                    
                    this.moduleCheckBox?.Dispose();
                    this.moduleCheckBox = null;                    
                }

                // Unmanaged resources
                disposed = true;
            }
        }
    }
}
