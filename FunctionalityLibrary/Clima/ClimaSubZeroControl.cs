using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using AutomotiveLighting.MTFCommon;

namespace ClimaChamberDriver
{
    class ClimaSubZeroControl : IClimaChamber
    {


        #region ctor dtor

        public ClimaSubZeroControl(string comPort)
        {
            InitializeSerialPort(comPort);

            this.serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);

            timerForFrameDelimination = Stopwatch.StartNew();

            Run = false;
        }



        #endregion
        #region private fields
        private IMTFSequenceRuntimeContext runtimeContext;
        private System.IO.Ports.SerialPort serialPort1;

        private int incominingFrameLength = 0;
        private AutoResetEvent frameReceived = new AutoResetEvent(false);
        private Stopwatch timerForFrameDelimination;
        List<byte> receivedData = new List<byte>();
        private ValuesClima data = new ValuesClima();


        internal class ValuesClima
        {
            internal double temperatureSet = 0.0;
            internal double temperatureGradient = 0.0;
            internal double humiditySet = 0.0;
            internal double humidityGradient = 0.0;
            internal bool unitSwitch = false;
            internal int errorsNr = 0;
        }

        #endregion

        #region interface
        public double Temperature
        {
            get
            {
                return GetRealValue( ReadOneRegister(Registers.TemperatureProcessValue));
            }
            set
            {
                CheckRange(value, Defines.MinimalTemperatureSetpoint, Defines.MaximalTemperatureSetpoint);
                data.temperatureSet = value;
                data.temperatureGradient = 0;
                SetClimaSetpoints();
            }
        }

        public double Humidity
        {
            get
            {
                return GetRealValue( ReadOneRegister(Registers.HumidityProcessValue));
            }
            set
            {
                CheckRange(value, Defines.MinimalHumiditySetpoint, Defines.MaximalHumiditySetpoint);
                data.humiditySet = value;
                data.humidityGradient = 0;
                SetClimaSetpoints();
            }
        }

        public List<string> Alarms
        {
            get
            {
                List<string> alarms = new List<string>();

                Int16 alarmReg = ReadOneRegister(Registers.ChamberAlarmStatus);

                var bools = new BitArray(new int[] { alarmReg }).Cast<bool>().ToArray();

                if (bools[0]) alarms.Add("Heater High Limit (Plenum A)");
                if (bools[1]) alarms.Add("External Product Safety");
                if (bools[2]) alarms.Add("Boiler Over-Temperature (Plenum A) ");
                if (bools[3]) alarms.Add("Boiler Low Water (Plenum A)");
                if (bools[4]) alarms.Add("Dehumidifier System Fault (System B Boiler Over-Temperature)");
                if (bools[5]) alarms.Add("Motor Overload (Plenum A)");
                if (bools[6]) alarms.Add("Fluid System High Limit (Plenum B Heater High Limit)");
                if (bools[7]) alarms.Add("Fluid System High Pressure (Plenum B Motor Overload)");
                if (bools[8]) alarms.Add("Fluid System Low Flow");
                if (bools[9]) alarms.Add("Door Open");
                if (bools[10]) alarms.Add("(System B Boiler Low Water)");
                if (bools[11]) alarms.Add("(not assigned)");
                if (bools[12]) alarms.Add(" Emergency Stop");
                if (bools[13]) alarms.Add(" Power Failure");
                if (bools[14]) alarms.Add(" Transfer Error");

                return alarms;
            }
        }

        public bool Run
        {
            get
            {
                Int16 reply = ReadOneRegister(Registers.ChamberManualEventControl);
                if ((reply & 0x0001) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    WriteSingleRegisterRequest(Defines.ClimaAddress, Registers.ChamberManualEventControl, ChamberEvents.TurnClimaON);
                }
                else
                {
                    WriteSingleRegisterRequest(Defines.ClimaAddress, Registers.ProfileControlStatus, ProfileControlDefines.StoppAllOff); //stop profile
                    WriteSingleRegisterRequest(Defines.ClimaAddress, Registers.ChamberManualEventControl, ChamberEvents.TurnClimaOFF);
                }
            }
        }

        public void TemperatureWithGradient(double temperature, double gradient)
        {
            CheckRange(temperature, Defines.MinimalTemperatureSetpoint, Defines.MaximalTemperatureSetpoint);
            CheckRange(gradient, Defines.MinimalTemperatureGradient, Defines.MaximalTemperatureGradient);
            data.temperatureSet = temperature;
            data.temperatureGradient = gradient;
            SetClimaSetpoints();
        }

        public void HumidityWithGradient(double humidity, double gradient)
        {
            CheckRange(humidity, Defines.MinimalHumiditySetpoint, Defines.MaximalHumiditySetpoint);
            CheckRange(gradient, Defines.MinimalHumidityGradient, Defines.MaximalHumidityGradient);
            data.humiditySet = humidity;
            data.humidityGradient = gradient;
            SetClimaSetpoints();
        }

        public void AckAlarms()
        {
            WriteSingleRegisterRequest(Defines.ClimaAddress, Registers.AlarmAcknowledge, Defines.AlarmSilence);
        }

        public void Close()
        {
            if (this.serialPort1 != null)
            {
                try
                {
                    Run = false;
                }
                catch 
                { 
                    //TODO jde udelat neco jineho nez to zazdit?
                }
                this.serialPort1.Close();
                this.serialPort1 = null;
            }
        }
        #endregion

        #region public methods

        public List<string> GetSystemStatusMonitor()
        {
            List<string> monitors = new List<string>();

            UInt16 alarmReg = 0xFFFF;//ReadOneRegister(Registers.SystemStatusMonitor);

            var bools = new BitArray(new int[] { alarmReg }).Cast<bool>().ToArray();

            if (bools[0]) monitors.Add(" Humidity Water Reservoir Low ");
            if (bools[1]) monitors.Add(" Humidity Disabled (temperature out-of-range)");
            if (bools[2]) monitors.Add(" Humidity High Dewpoint Limit");
            if (bools[3]) monitors.Add(" Humidity Low Dewpoint Limit");
            if (bools[4]) monitors.Add(" Door Open");
            if (bools[5]) monitors.Add(" (not assigned)");
            if (bools[6]) monitors.Add(" (not assigned)");
            if (bools[7]) monitors.Add(" (not assigned)");
            if (bools[8]) monitors.Add(" Service Air Circulators");
            if (bools[9]) monitors.Add(" Service Heating/Cooling System");
            if (bools[10]) monitors.Add("Service Humidity System");
            if (bools[11]) monitors.Add("Service Purge System");
            if (bools[12]) monitors.Add("Service Altitude System");
            if (bools[13]) monitors.Add("Service Transfer Mechanism");
            if (bools[14]) monitors.Add("(not assigned)");

            return monitors;
        }

        #endregion

        #region private communication methods

        private byte[] ReadRegistersRequest(byte adr, UInt16 startRegister, UInt16 count)
        {
            byte[] frameWithoutCrc = new byte[6];

            frameWithoutCrc[0] = adr;
            frameWithoutCrc[1] = Defines.CmdReadRegister;
            frameWithoutCrc[2] = (byte)(startRegister >> 8 & 0x00ff);
            frameWithoutCrc[3] = (byte)(startRegister & 0x00ff);
            frameWithoutCrc[4] = (byte)(count >> 8 & 0x00ff);
            frameWithoutCrc[5] = (byte)(count & 0x00ff);

            SendFrame(frameWithoutCrc);

            incominingFrameLength = 5 + (2 * count);

            byte[] receivedFrame = ReceiveFrame();

            return ProcessReceivedDataForReadRequest(receivedFrame, adr, count);
        }

        private byte[] ProcessReceivedDataForReadRequest(byte[] receivedFrame, byte adr, UInt16 count)
        {
            byte[] result = new byte[2 * count];
            if (receivedFrame[0] != adr || receivedFrame[1] != Defines.CmdReadRegister || receivedFrame[2] != 2 * count)
            {
                throw new Exception("Received frame replying on ReadRegistersCommand is not valid.");
            }

            Array.Copy(receivedFrame, 3, result, 0, 2 * count);//return data part of received frame

            return result;
        }

        private void WriteSingleRegisterRequest(byte adr, UInt16 register, Int16 data)
        {
            byte[] frameWithoutCrc = new byte[6];
            byte[] dataForCrc = new byte[6];

            frameWithoutCrc[0] = adr;
            frameWithoutCrc[1] = Defines.CmdWriteSingleRegister;
            frameWithoutCrc[2] = (byte)(register >> 8 & 0x00ff);
            frameWithoutCrc[3] = (byte)(register & 0x00ff);
            frameWithoutCrc[4] = (byte)(data >> 8 & 0x00ff);
            frameWithoutCrc[5] = (byte)(data & 0x00ff);

            SendFrame(frameWithoutCrc);

            incominingFrameLength = frameWithoutCrc.Length + 2;

            byte[] receivedFrame = ReceiveFrame();

            CheckWriteRegisterRespond(frameWithoutCrc, receivedFrame);
        }

        private void WriteRegistersRequest(byte adr, UInt16 startRegister, Int16[] data)
        {
            byte[] frameWithoutCrc = new byte[7 + data.Length * 2];
            byte[] expectedResponse = new byte[6];

            frameWithoutCrc[0] = adr;
            frameWithoutCrc[1] = Defines.CmdWriteRegisters;
            frameWithoutCrc[2] = (byte)(startRegister >> 8 & 0x00ff);
            frameWithoutCrc[3] = (byte)(startRegister & 0x00ff);
            frameWithoutCrc[4] = (byte)(data.Length >> 8 & 0x00ff);
            frameWithoutCrc[5] = (byte)(data.Length & 0x00ff);
            frameWithoutCrc[6] = (byte)(data.Length * 2);

            for (int i = 0; i < data.Length; i++)
            {
                frameWithoutCrc[7 + 2 * i] = (byte)(data[i] >> 8 & 0x00ff);
                frameWithoutCrc[8 + 2 * i] = (byte)(data[i] & 0x00ff);
            }

            SendFrame(frameWithoutCrc);

            incominingFrameLength = 8;

            byte[] receivedFrame = ReceiveFrame();

            Array.Copy(frameWithoutCrc, expectedResponse, expectedResponse.Length);

            CheckWriteRegisterRespond(expectedResponse, receivedFrame);
        }

        private void CheckWriteRegisterRespond(byte[] expectedFrame, byte[] receivedFrame)
        {
            for (int i = 0; i < expectedFrame.Length; i++)
            {
                if (receivedFrame[i] != expectedFrame[i])
                {
                    throw new Exception("Frame sent like writeToRegisterCommand was not acknowledged correctly.");
                }
            }
        }

        private void SendFrame(byte[] frameWithoutCRC)
        {
            byte[] dataToSend = new byte[frameWithoutCRC.Length + 2];

            Array.Copy(frameWithoutCRC, dataToSend, frameWithoutCRC.Length);

            UInt16 crcResult = calc_crc(frameWithoutCRC);
            dataToSend[dataToSend.Length - 2] = (byte)(crcResult & 0x00ff);
            dataToSend[dataToSend.Length - 1] = (byte)(crcResult >> 8 & 0x00ff);

            while (timerForFrameDelimination.ElapsedMilliseconds < Defines.MilisecondsBetweenFrames)
            {
                Thread.Sleep(1);
            }

            serialPort1.Write(dataToSend, 0, dataToSend.Length);

            timerForFrameDelimination.Restart();
        }

        private byte[] ReceiveFrame()
        {
            this.frameReceived.WaitOne(1000);
            if (receivedData.Count < 5)
            {
                throw new Exception("Receive frame timeout!");
            }
            byte[] receivedFrame = receivedData.ToArray();
            receivedData.Clear();

            if (CheckCrc(receivedFrame))
            {
                return receivedFrame;
            }
            else
            {
                throw new Exception("Received frame has wrong CRC!");
            }
        }

        private bool CheckCrc(byte[] frame)
        {
            byte[] dataForCrc = new byte[frame.Length - 2];

            Array.Copy(frame, dataForCrc, dataForCrc.Length);
            UInt16 crcInFrame = System.BitConverter.ToUInt16(frame, frame.Length - 2);
            UInt16 crcComputed = calc_crc(dataForCrc);

            if (crcInFrame == crcComputed)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private UInt16 calc_crc(byte[] data)
        {
            UInt16 crc;
            int bit_count;
            int char_ptr;
            /* Start at the beginning of the packet */
            char_ptr = 0;
            /* Initialize CRC */
            crc = 0xFFFF;
            /* Loop through the entire packet */
            do
            {
                /* Exclusive-OR the byte with the CRC */
                crc ^= data[char_ptr];
                /* Loop through all 8 data bits */
                bit_count = 0;
                do
                {
                    /* If the LSB is 1, shift the CRC and XOR the polynomial mask with the CRC */
                    if ((crc & 0x0001) == 1)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    /* If the LSB is 0, shift the CRC only */
                    else
                    {
                        crc >>= 1;
                    }
                } while (bit_count++ < 7);
            } while (++char_ptr < data.Length);
            return (crc);
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            System.IO.Ports.SerialPort port = sender as System.IO.Ports.SerialPort;
            byte[] data = new byte[port.BytesToRead];
            port.Read(data, 0, data.Length);

            if (data.Length <= incominingFrameLength)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    receivedData.Add(data[i]);
                }
            }
            else
            {
                throw new Exception("Received packet is inconsistent");
            }

            if (receivedData.Count == incominingFrameLength)
            {
                incominingFrameLength = 0;
                frameReceived.Set();
            }
        }

        private void InitializeSerialPort(string comPort)
        {
                this.serialPort1 = new System.IO.Ports.SerialPort();
                this.serialPort1.PortName = comPort;
                this.serialPort1.Parity = System.IO.Ports.Parity.Even;
                this.serialPort1.StopBits = System.IO.Ports.StopBits.One;
                this.serialPort1.BaudRate = 9600;
                this.serialPort1.Open();
        }

        #endregion

        #region private helper methods

        private void CreateSetpointsProfile(bool preferHumidity)
        {
            WriteSingleRegisterRequest(Defines.ClimaAddress, Registers.ProfileControlStatus, ProfileControlDefines.StopOff); //stop profile

            double timeInMinutes;
            double processTemperature = Temperature;

            if (preferHumidity)
            {
                double processHumidity = Humidity;
                double humidityDelta = Math.Abs(processHumidity - data.humiditySet);
                timeInMinutes = humidityDelta / data.humidityGradient;
            }
            else
            {
                
                double temperatureDelta = Math.Abs(processTemperature - data.temperatureSet);
                timeInMinutes = temperatureDelta / data.temperatureGradient;
            }


            Int16[] profileData = CreateProfileData();
            List<Int16[]> stepsData = new List<Int16[]>();

            Int16[] step1Data = CreateStepData(processTemperature, data.humiditySet, 0);
            Int16[] step2Data = CreateStepData(data.temperatureSet, data.humiditySet, timeInMinutes);

            stepsData.Add(step1Data);
            stepsData.Add(step2Data);

            SendProfileToDevice(profileData, stepsData);

            if (ReadOneRegister(Registers.OnlineDownloadingProfile) != 0)
            {
                throw new Exception("Not online!");
            }

            WriteSingleRegisterRequest(Defines.ClimaAddress, Registers.ProfileStartStep, 1);

            WriteSingleRegisterRequest(Defines.ClimaAddress, Registers.ProfileControlStatus, ProfileControlDefines.RunResume);
        }

        private Int16[] CreateProfileData()
        {
            Int16[] profileData = new Int16[15];

            byte[] arrayFinal = new byte[10];

            byte[] nameArray = Encoding.ASCII.GetBytes(string.Format("{0:#.0}", data.temperatureSet) + " C");

            Array.Copy(nameArray, arrayFinal, nameArray.Length);

            
            profileData[0] = 0;
            profileData[1] = 0;
            profileData[2] = 0;
            profileData[3] = 0;
            profileData[4] = BitConverter.ToInt16(arrayFinal, 0);
            profileData[5] = BitConverter.ToInt16(arrayFinal, 2);
            profileData[6] = BitConverter.ToInt16(arrayFinal, 4);
            profileData[7] = BitConverter.ToInt16(arrayFinal, 6);
            profileData[8] = BitConverter.ToInt16(arrayFinal, 8);
            profileData[9] = 2;//steps in profile
            profileData[10] = 0;
            profileData[11] = 0;
            profileData[12] = 0;
            profileData[13] = 0;
            profileData[14] = 0;

            return profileData;
        }

        public Int16[] CreateStepData(double temp, double humidity, double timeInMinutes)
        {
            Int16[] stepData = new Int16[15];

            Int16 hours = (Int16)(timeInMinutes / 60);
            byte minutes = (byte)(timeInMinutes % 60);
            byte seconds = (byte)((timeInMinutes - Math.Truncate(timeInMinutes)) * 60);

            stepData[0] = hours;
            stepData[1] = (Int16)(minutes << 8 | seconds);
            stepData[2] = (humidity == 0) ? ChamberEvents.HumidityUnitOFF : ChamberEvents.HumidityUnitON;
            stepData[3] = 0;
            stepData[4] = 0;
            stepData[5] = 0;
            stepData[6] = 0;
            stepData[7] = 0;
            stepData[8] = 0;
            stepData[9] = 0;
            stepData[10] = GetRawValue(temp);//temp
            stepData[11] = GetRawValue(humidity);
            stepData[12] = 0;//product
            stepData[13] = 0;
            stepData[14] = 0;

            return stepData;
        }

        private void SendProfileToDevice(Int16[] profileData, List<Int16[]> stepsData)
        {
            WriteRegistersRequest(Defines.ClimaAddress, Registers.ProfileDataStart, profileData);
            Thread.Sleep(1000);
            for (int i = 0; i < stepsData.Count; i++)
            {
                UInt16 startRegister = (UInt16)((UInt16)(0x0F * i) + Registers.ProfileStep1DataStart);
                WriteRegistersRequest(Defines.ClimaAddress, startRegister, stepsData[i]);
                Thread.Sleep(1000);
            }
        }

        private void CheckRange(double value, double min, double max)
        {
            if (value < min || value > max)
                throw new Exception("Value " + value + " is out of range. Range is <" + min + "," + max + ">");
        }

        private Int16 ReadOneRegister(UInt16 register)
        {
            byte[] received = ReadRegistersRequest(Defines.ClimaAddress, register, 1);
            Array.Reverse(received);//big endian to little endian
            Int16 result = System.BitConverter.ToInt16(received, 0);
            return result;
        }

        private void SetClimaSetpoints()
        {

            if (data.temperatureGradient == 0 && data.humiditySet == 0)
            {
                SetJustTemp();
            }
            else if (data.temperatureGradient == 0 && data.humidityGradient == 0)
            {
                SetTempHumidity();
            }
            else if (data.temperatureGradient == 0)
            {
                //temp humidity with gradient
                CreateSetpointsProfile(true);
            }
            else
            {
                CreateSetpointsProfile(false);
                //temp with gradient + humidity
            }
        }

        private void SetJustTemp()
        {
            WriteSingleRegisterRequest(Defines.ClimaAddress, Registers.ProfileControlStatus, ProfileControlDefines.StopOff); //stop profile
            WriteSingleRegisterRequest(Defines.ClimaAddress, Registers.ChamberManualEventControl, ChamberEvents.HumidityUnitOFF); //hum event off
            WriteSingleRegisterRequest(Defines.ClimaAddress, Registers.TemperatureSetpoint, GetRawValue(data.temperatureSet));//set temp
        }

        private void SetTempHumidity()
        {
            WriteSingleRegisterRequest(Defines.ClimaAddress, Registers.ProfileControlStatus, ProfileControlDefines.StopOff); //stop profile
            WriteSingleRegisterRequest(Defines.ClimaAddress, Registers.ChamberManualEventControl, ChamberEvents.HumidityUnitON); //hum event on
            WriteSingleRegisterRequest(Defines.ClimaAddress, Registers.TemperatureSetpoint, GetRawValue(data.temperatureSet));//set temp
            WriteSingleRegisterRequest(Defines.ClimaAddress, Registers.HumiditySetpoint, GetRawValue(data.humiditySet));//set humi
        }

        private Int16 GetRawValue(double realValue)
        {
            Int16 rawValue = (Int16)(realValue * 10);
            return rawValue;
        }

        private double GetRealValue(Int16 rawValue)
        {
            return rawValue / 10.0;
        }

        #endregion


        public void TemperatureRamp(double startTemperature, double finTemperature, double dwell)
        {
            throw new NotImplementedException();
        }

        public void HumidityRamp(double startHumidity, double finHumidity, double dwell)
        {
            throw new NotImplementedException();
        }


        public double Fan
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public IMTFSequenceRuntimeContext RuntimeContext
        {
            get
            {
                return runtimeContext;
            }
            set
            {
                runtimeContext = value;
            }
        }
    }

    #region defines
    public static class Registers
    {
        // TODO maybe 40001 need to be add because of relative numbers!!! 
        public const UInt16 OperationalMode = 0x00;
        public const UInt16 LightControl = 0x15;
        public const UInt16 ChamberManualEventControl = 0x16;
        public const UInt16 ProfileControlStatus = 0x18;
        public const UInt16 ProfileStartStep = 0x25;
        public const UInt16 ProfileTemperatureSetpoint = 0x2E;//just for check when profile is running
        public const UInt16 AlarmAcknowledge = 0x36;
        public const UInt16 ChamberAlarmStatus = 0x39;
        public const UInt16 SystemStatusMonitor = 0x3B;

        public const UInt16 TemperatureSetpoint = 0x3C;
        public const UInt16 TemperatureProcessValue = 0x3D;
        public const UInt16 HumiditySetpoint = 0x48;
        public const UInt16 HumidityProcessValue = 0x49;

        public const UInt16 TemperatureUpperSetpointLimit = 0x40;
        public const UInt16 TemperatureLowerSetpointLimit = 0x41;


        public const UInt16 OnlineDownloadingProfile = 0xB4;

        public const UInt16 ProfileDataStart = 0xC8;
        public const UInt16 ProfileStep1DataStart = 0xD7;
    }

    public static class Defines
    {
        public const byte ClimaAddress = 0x01;
        public const byte CmdReadRegister = 0x03;
        public const byte CmdWriteSingleRegister = 0x06;
        public const byte CmdWriteRegisters = 0x10;

        public const double MinimalTemperatureSetpoint = -50;
        public const double MaximalTemperatureSetpoint = 190;

        public const double MinimalHumiditySetpoint = 5;
        public const double MaximalHumiditySetpoint = 95;

        public const double MinimalTemperatureGradient = 0;
        public const double MaximalTemperatureGradient = 10;

        public const double MinimalHumidityGradient = 0;
        public const double MaximalHumidityGradient = 10;

        public const Int16 AlarmSilence = 0x01;

        public const int MilisecondsBetweenFrames = 3;

        public const int MinutesFor999Hours = 59940;
    }

    public static class ProfileControlDefines
    {
        public const byte StopOff = 0x00;
        public const byte StoppAllOff = 0x01;
        public const byte Hold = 0x02;
        public const byte RunResume = 0x04;
        public const byte Autostart = 0x08;
        public const byte Wait = 0x10;
        public const byte Ramp = 0x20;
        public const byte Soak = 0x40;
        public const byte GuarenteedSoak = 0x80;
    }

    public static class ChamberEvents
    {
        public const Int16 TurnClimaOFF = 0x00;
        public const Int16 TurnClimaON = 0x01;
        public const Int16 HumidityUnitOFF = 0x01;
        public const Int16 HumidityUnitON = 0x03;
    }
    #endregion
}
