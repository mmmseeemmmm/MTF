using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTFBusCommunication;
using MTFBusCommunication.Structures;
using System.IO;
using System.Collections;
using System.Threading;

namespace LiteCheckDriver
{
    class LiteCheckControl
    {
        private int virtualCanChannel;
        private IMTFBusCommunication busCommunicationDriver;
        private List<int> usedLiteCheckDeviceIDs;

        private bool trigger1 = false;
        private bool trigger2 = false;

        #region ctor, close method
        public LiteCheckControl(IMTFBusCommunication busCommunicationDriver, int virtualChannel, List<int> liteCheckIDs)
        {
            this.busCommunicationDriver = busCommunicationDriver;
            this.virtualCanChannel = virtualChannel;
            this.usedLiteCheckDeviceIDs = liteCheckIDs;

            this.busCommunicationDriver.OnBoards = CreateConfiguration();
            this.busCommunicationDriver.Initialize();
        }

        public void Close()
        {
            if (busCommunicationDriver != null)
            {
                busCommunicationDriver.Dispose();
                busCommunicationDriver = null;
            }
        }
        #endregion

        #region public methods

        public void SetPwm(int liteCheck, int output, int pwmValue)
        {
            string frame = "LCS" + liteCheck.ToString("D2") + "_Set_PWM_K0_K7";
            string signal = "LCS" + liteCheck.ToString("D2") + "_Set_PWM_K" + output.ToString();

            SetSignal(frame, signal, pwmValue.ToString());
        }

        public int GetPwm(int liteCheck, int output)
        {
            string frame = "LCS" + liteCheck.ToString("D2") + "_State_PWM_K0_K7";
            string signal = "LCS" + liteCheck.ToString("D2") + "_State_PWM_K" + output.ToString();

            int returnedPwmValue;
            returnedPwmValue = int.Parse(GetSignal(frame, signal));

            LiteHelper.CheckRange(returnedPwmValue, 0, 100);

            return returnedPwmValue;
        }

        public void SetPwmToAll(int output, int pwmValue)
        {
            //SetSignal("LCS_PWM_Broadcast", "LCS_PWM_Broadcast_K" + output.ToString(), pwmValue.ToString()); //can not be used together with messages LCS01-LCS15_SetPWM 0x711 - 0x71F (SetPWM in this driver)
            foreach (int litecheck in usedLiteCheckDeviceIDs)
            {
                SetPwm(litecheck, output, pwmValue);
            }
        }

        public void SetRelay(int liteCheck, int output, bool state)
        {
            string frame = "LCS" + liteCheck.ToString("D2") + "_Set_Relais_K8_K11";
            string signal = "LCS" + liteCheck.ToString("D2") + "_Set_Relais_K" + output.ToString();
            string value = state ? "1" : "0";

            SetSignal(frame, signal, value);
        }

        public bool GetRelay(int liteCheck, int output)
        {
            string frame = "LCS" + liteCheck.ToString("D2") + "_State_Relais_K8_K11";
            string signal = "LCS" + liteCheck.ToString("D2") + "_State_Relais_K" + output.ToString();

            string returnedRelayState;
            returnedRelayState = GetSignal(frame, signal);

            if (returnedRelayState == "1")
            {
                return true;
            }
            if (returnedRelayState == "0")
            {
                return false;
            }
            else
            {
                throw new Exception("Returned relay state is not valid! Returned value is " + returnedRelayState);
            }
        }

        public void SetRelayToAll(int output, bool state)
        {
            foreach (int litecheck in usedLiteCheckDeviceIDs)
            {
                SetRelay(litecheck, output, state);
            }
        }

        public void SetPwmFrequency(int liteCheck, int pwmFrequency)
        {
            byte[] pwmFrequencyBytes = BitConverter.GetBytes(pwmFrequency);
            byte[] dataRes = CreateRequestAndGetResponse(liteCheck,
                                                        LiteHelper.MsgType.request,
                                                        LiteHelper.MsgQualifier.pwmFreq,
                                                        LiteHelper.DLCs.reqPwmFreq,
                                                        LiteHelper.DLCs.resPwmFreq,
                                                        pwmFrequencyBytes[0],
                                                        pwmFrequencyBytes[1]);

            Int16 result = BitConverter.ToInt16(dataRes, 0);

            if (result != pwmFrequency)
            {
                throw new Exception("Set PWM frequency request was not acknowledged!");
            }

        }

        public int GetPwmFrequency(int liteCheck)
        {
            byte[] dataRes = CreateRequestAndGetResponse(liteCheck,
                                            LiteHelper.MsgType.query,
                                            LiteHelper.MsgQualifier.pwmFreq,
                                            LiteHelper.DLCs.quePwmFreq,
                                            LiteHelper.DLCs.resPwmFreq,
                                            0,
                                            0);

            Int16 result = BitConverter.ToInt16(dataRes, 0);

            return result;
        }

        public bool GetExternalInput(int liteCheck, int extInput)
        {
            string frame = "LCS" + liteCheck.ToString("D2") + "_StateInfo";
            string signal = "LCS" + liteCheck.ToString("D2") + "_StateExtIn_E" + extInput.ToString();

            string returnedExtInputState;
            returnedExtInputState = GetSignal(frame, signal);

            if (returnedExtInputState == "1")
            {
                return true;
            }
            if (returnedExtInputState == "0")
            {
                return false;
            }
            else
            {
                throw new Exception("Returned ext. input state is not valid! Returned value is " + returnedExtInputState);
            }
        }

        public int GetTemperature(int liteCheck)
        {
            string frame = "LCS" + liteCheck.ToString("D2") + "_StateInfo";
            string signal = "LCS" + liteCheck.ToString("D2") + "_StateTEMP";

            int returnedTemperature;
            returnedTemperature = int.Parse(GetSignal(frame, signal));

            if (returnedTemperature == LiteHelper.temperatureError)
            {
                throw new Exception("GetTemperature error!");
            }

            return returnedTemperature;
        }

        public string GetSerialNumber(int liteCheck)
        {
            string frame = "LCS" + liteCheck.ToString("D2") + "_StateInfo";
            string signal = "LCS" + liteCheck.ToString("D2") + "_StateSERNR";

            string returnedSerialNumber;
            returnedSerialNumber = int.Parse(GetSignal(frame, signal)).ToString("X");

            return returnedSerialNumber;
        }

        public int GetPwmFrequencyFromStateInfo(int liteCheck)
        {
            string frame = "LCS" + liteCheck.ToString("D2") + "_StateInfo";
            string signal = "LCS" + liteCheck.ToString("D2") + "_StatePWMFREQ";

            int returnedPwmFrequency;
            returnedPwmFrequency = int.Parse(GetSignal(frame, signal));

            return returnedPwmFrequency;
        }

        public string GetSwVersion(int liteCheck)
        {
            byte[] dataRes = CreateRequestAndGetResponse(liteCheck,
                    LiteHelper.MsgType.query,
                    LiteHelper.MsgQualifier.versionSw,
                    LiteHelper.DLCs.queVersionSw,
                    LiteHelper.DLCs.resVersionSw,
                    0,
                    0);

            return System.Text.Encoding.ASCII.GetString(dataRes, 0, 4);
        }

        public string GetHwVersion(int liteCheck)
        {
            byte[] dataRes = CreateRequestAndGetResponse(liteCheck,
                    LiteHelper.MsgType.query,
                    LiteHelper.MsgQualifier.versionHw,
                    LiteHelper.DLCs.queVersionHw,
                    LiteHelper.DLCs.resVersionHw,
                    0,
                    0);

            return System.Text.Encoding.ASCII.GetString(dataRes, 0, 3);
        }

        public int GetDeviceID(int liteCheck)
        {
            byte[] dataRes = CreateRequestAndGetResponse(liteCheck,
                    LiteHelper.MsgType.query,
                    LiteHelper.MsgQualifier.deviceId,
                    LiteHelper.DLCs.queDeviceId,
                    LiteHelper.DLCs.resDeviceId,
                    0,
                    0);

            return dataRes[0];
        }

        public string GetFullSerialNumber(int liteCheck)
        {
            byte[] dataRes = CreateRequestAndGetResponse(liteCheck,
                                LiteHelper.MsgType.query,
                                LiteHelper.MsgQualifier.sernr,
                                LiteHelper.DLCs.queSernr,
                                LiteHelper.DLCs.resSernr,
                                0,
                                0);

            string fullSerNr;

            fullSerNr = System.Text.Encoding.ASCII.GetString(dataRes, 0, 2);
            fullSerNr += BitConverter.ToUInt16(dataRes, 2).ToString("X");

            return fullSerNr;
        }

        public int GetHostID(int liteCheck, byte selectedID)
        {
            byte[] dataRes = CreateRequestAndGetResponse(liteCheck,
                                LiteHelper.MsgType.query,
                                LiteHelper.MsgQualifier.defHostId,
                                LiteHelper.DLCs.queDefHostId,
                                LiteHelper.DLCs.resDefHostId,
                                selectedID,
                                0);

            UInt16 result = BitConverter.ToUInt16(dataRes, 1);

            return result;
        }

        public int GetLightID(int liteCheck, byte selectedID)
        {
            byte[] dataRes = CreateRequestAndGetResponse(liteCheck,
                                            LiteHelper.MsgType.query,
                                            LiteHelper.MsgQualifier.defLightId,
                                            LiteHelper.DLCs.queDefLightId,
                                            LiteHelper.DLCs.resDefLightId,
                                            selectedID,
                                            0);

            UInt16 result = BitConverter.ToUInt16(dataRes, 1);

            return result;
        }

        public void SetTrigger(int liteCheck, byte channel, bool state)
        {
            if (channel == 1)
            {
                trigger1 = state;
            }
            else
            {
                trigger2 = state;
            }

            byte tri0 = trigger1 ? (byte)1 : (byte)0;
            byte tri1 = trigger2 ? (byte)1 : (byte)0;

            byte data0 = (byte)(tri0 | tri1 << 1);

            byte[] dataRes = CreateRequestAndGetResponse(liteCheck,
                                                        LiteHelper.MsgType.request,
                                                        LiteHelper.MsgQualifier.trigOut,
                                                        LiteHelper.DLCs.reqTrigOut,
                                                        LiteHelper.DLCs.resTrigOut,
                                                        data0,
                                                        0);

            if (dataRes[0] != data0)
            {
                throw new Exception("Set trigger request was not acknowledged!");
            }
        }

        public bool GetTrigger(int liteCheck, byte channel)
        {

            byte[] dataRes = CreateRequestAndGetResponse(liteCheck,
                                                        LiteHelper.MsgType.query,
                                                        LiteHelper.MsgQualifier.trigOut,
                                                        LiteHelper.DLCs.queTrigOut,
                                                        LiteHelper.DLCs.resTrigOut,
                                                        0,
                                                        0);

            var bools = new BitArray(new int[] { dataRes[0] }).Cast<bool>().ToArray();

            trigger1 = bools[0];
            trigger2 = bools[1];

            if (channel == 1)
            {
                return trigger1;
            }
            else
            {
                return trigger2;
            }
        }

        public void SetLightID(byte selectedID, UInt16 lightID)
        {
            byte[] dataReq = new byte[LiteHelper.diagFrameLength];
            byte[] lightIdBytes = BitConverter.GetBytes(lightID);

            dataReq[0] = LiteHelper.MsgType.request;
            dataReq[1] = LiteHelper.MsgQualifier.defLightId;
            dataReq[2] = LiteHelper.DLCs.reqDefLightId;
            dataReq[3] = selectedID;
            dataReq[4] = lightIdBytes[0];
            dataReq[5] = lightIdBytes[1];

            SendFrameOnce(LiteHelper.baseDiagRequestMsgId, dataReq);
        }

        public void SetHostID(byte selectedID, UInt16 hostID)
        {
            byte[] dataReq = new byte[LiteHelper.diagFrameLength];
            byte[] hostIdBytes = BitConverter.GetBytes(hostID);

            dataReq[0] = LiteHelper.MsgType.request;
            dataReq[1] = LiteHelper.MsgQualifier.defHostId;
            dataReq[2] = LiteHelper.DLCs.reqDefHostId;
            dataReq[3] = selectedID;
            dataReq[4] = hostIdBytes[0];
            dataReq[5] = hostIdBytes[1];

            SendFrameOnce(LiteHelper.baseDiagRequestMsgId, dataReq);
        }

        public void SetDeviceID(byte deviceID)
        {
            byte[] dataReq = new byte[LiteHelper.diagFrameLength];

            dataReq[0] = LiteHelper.MsgType.request;
            dataReq[1] = LiteHelper.MsgQualifier.deviceId;
            dataReq[2] = LiteHelper.DLCs.reqDeviceId;
            dataReq[3] = deviceID;

            SendFrameOnce(LiteHelper.baseDiagRequestMsgId, dataReq);
        }

        public void SetSerialNumber(string serNr)
        {
            byte[] dataReq = new byte[LiteHelper.diagFrameLength];

             byte[] part1;
             byte[] part2;

            try
            {
                part1 = System.Text.Encoding.ASCII.GetBytes(serNr.Substring(0, 2));
                part2 = BitConverter.GetBytes(UInt16.Parse(serNr.Substring(2),System.Globalization.NumberStyles.HexNumber));
            }
            catch
            {
                throw new Exception("Serial number is not in correct format!");
            }

            dataReq[0] = LiteHelper.MsgType.request;
            dataReq[1] = LiteHelper.MsgQualifier.sernr;
            dataReq[2] = LiteHelper.DLCs.reqSernr;
            dataReq[3] = part1[0];
            dataReq[4] = part1[1];
            dataReq[5] = part2[0];
            dataReq[6] = part2[1];

            SendFrameOnce(LiteHelper.baseDiagRequestMsgId, dataReq);
        }
        
        #endregion

        #region private methods

        private void SetSignal(string frame, string signal, string value)
        {
            if (busCommunicationDriver != null)
            {
                busCommunicationDriver.OnBoardSetSignal(virtualCanChannel, frame, signal, value);
            }
        }

        private string GetSignal(string frame, string signal)
        {
            if (busCommunicationDriver != null)
            {
                return busCommunicationDriver.OnBoardGetSignal(virtualCanChannel, frame, signal).Value;
            }
            else
            {
                return string.Empty;
            }
        }

        private byte[] RecieveFrameOnce(uint reqFrameId, byte[] reqData, byte expectedReceiveDlc)
        {
            if (busCommunicationDriver != null)
            {
                if (reqData[0] == LiteHelper.MsgType.request)
                {
                    Thread.Sleep(200);
                }
                byte[] resData = busCommunicationDriver.OnBoardReceiveFrameOnce(virtualCanChannel, reqFrameId, reqFrameId + 0x60, reqData, LiteHelper.diagTimeout);
                CheckResponse(reqData, resData, expectedReceiveDlc);

                return resData;
            }
            else
            {
                return null;
            }
        }

        private void SendFrameOnce(uint frameID, byte[] data)
        {
            if (busCommunicationDriver != null)
            {
                if (data[0] == LiteHelper.MsgType.request)
                {
                    Thread.Sleep(200);
                }

                busCommunicationDriver.OnBoardSendFrameOnce(virtualCanChannel, frameID, data);
            }
        }

        private List<OnBoardConfiguration> CreateConfiguration()
        {
            List<OnBoardConfiguration> configs = new List<OnBoardConfiguration>();
            OnBoardConfiguration onBoardConfig = new OnBoardConfiguration();

            string busNodes = string.Empty;
            foreach (int adr in usedLiteCheckDeviceIDs)
            {
                if (string.IsNullOrEmpty(busNodes))
                {
                    busNodes = "LITECHECK_" + adr.ToString("D2");
                }
                else
                {
                    busNodes += ",LITECHECK_" + adr.ToString("D2");
                }
            }
            onBoardConfig.VirtualChannel = virtualCanChannel;
            onBoardConfig.NetcCfgFile = Path.Combine(Directory.GetCurrentDirectory(), "mtfLibs\\LiteCheck", LiteHelper.dbcForLiteCheck);
            onBoardConfig.BusNodes = busNodes;

            configs.Add(onBoardConfig);
            return configs; ;
        }

        private void CheckResponse(byte[] reqData, byte[] resData, byte expectedDlc)
        {
            if ((resData.Length != LiteHelper.diagFrameLength) || (resData[0] != LiteHelper.MsgType.response) || (reqData[1] != resData[1]))
            {
                throw new Exception("Inconsistent frame has been received.");
            }

            if (resData[2] != expectedDlc)
            {
                throw new Exception("Received frame has different internal DLC than was expected.");
            }
        }

        private byte[] CreateRequestAndGetResponse(int liteCheck, byte msgType, byte msgQualifier, byte reqDlc, byte resDlc, byte data0, byte data1)
        {
            byte[] dataReq = new byte[LiteHelper.diagFrameLength];
            byte[] dataRes = new byte[LiteHelper.diagFrameLength];
            byte[] pureData = new byte[5];

            dataReq[0] = msgType;
            dataReq[1] = msgQualifier;
            dataReq[2] = reqDlc;
            dataReq[3] = data0;
            dataReq[4] = data1;
            dataReq[5] = 0;
            dataReq[6] = 0;
            dataReq[7] = 0;

            dataRes = RecieveFrameOnce(LiteHelper.baseDiagRequestMsgId + (uint)liteCheck, dataReq, resDlc);

            Array.Copy(dataRes, 3, pureData, 0, pureData.Length);

            return pureData;
        }

        #endregion
    }
}
