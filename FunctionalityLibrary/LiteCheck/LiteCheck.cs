using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTFBusCommunication;
using MTFBusCommunication.Structures;
using AutomotiveLighting.MTFCommon;

namespace LiteCheckDriver
{
    [MTFClass(Name = "LiteCheck", Description = "Driver for LiteCheck", Icon = MTFIcons.Oscilloscope)]
    [MTFClassCategory("Control & Measurement")]

    public class LiteCheck : IDisposable, IMTFBusCommunication
    {

        private IMTFBusCommunication busCommunicationDriver;
        private List<LiteCheckConfig> virtualLiteChecks;
        private int virtualCanChannel;
        private LiteCheckControl liteCheckCtr;

        #region ctor,dtor, dispose

        [MTFConstructor(Description = "Create LiteCheck component")]
        [MTFAllowedParameterValue("virtualChannelForLiteCheck", "CAN 1", 1)]
        [MTFAllowedParameterValue("virtualChannelForLiteCheck", "CAN 2", 2)]
        [MTFAllowedParameterValue("virtualChannelForLiteCheck", "CAN 3", 3)]
        [MTFAllowedParameterValue("virtualChannelForLiteCheck", "CAN 4", 4)]
        [MTFAllowedParameterValue("virtualChannelForLiteCheck", "CAN 5", 5)]
        [MTFAllowedParameterValue("virtualChannelForLiteCheck", "CAN 6", 6)]
        [MTFAllowedParameterValue("virtualChannelForLiteCheck", "CAN 7", 7)]
        [MTFAllowedParameterValue("virtualChannelForLiteCheck", "CAN 8", 8)]
        [MTFAllowedParameterValue("virtualChannelForLiteCheck", "CAN 9", 9)]
        [MTFAllowedParameterValue("virtualChannelForLiteCheck", "CAN 10", 10)]
        public LiteCheck(List<LiteCheckConfig> liteCheckConfigs, List<BaseConfig> canConfig, int virtualChannelForLiteCheck, string basePathForDbcs)
        {
            virtualLiteChecks = liteCheckConfigs;
            virtualCanChannel = virtualChannelForLiteCheck;
            busCommunicationDriver = new MTFBusCommunication.MTFBusCommunication(canConfig, basePathForDbcs);
            liteCheckCtr = new LiteCheckControl(busCommunicationDriver, virtualChannelForLiteCheck, GetListOfDeviceIDs());
        }

        public void Dispose()
        {
            if (liteCheckCtr != null)
            {
                liteCheckCtr.Close();
                liteCheckCtr = null;
            }
            GC.SuppressFinalize(this);
        }

        ~LiteCheck()
        {
            Dispose();
        }

        #endregion

        #region MTF methods

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        [MTFAllowedParameterValue("output", "K0", 0)]
        [MTFAllowedParameterValue("output", "K1", 1)]
        [MTFAllowedParameterValue("output", "K2", 2)]
        [MTFAllowedParameterValue("output", "K3", 3)]
        [MTFAllowedParameterValue("output", "K4", 4)]
        [MTFAllowedParameterValue("output", "K5", 5)]
        [MTFAllowedParameterValue("output", "K6", 6)]
        [MTFAllowedParameterValue("output", "K7", 7)]
        public void SetPwm(int liteCheck, int output, int pwmValue)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);
            LiteHelper.CheckRange(output, 0, 7);

            liteCheckCtr.SetPwm(GetVirtualLite(liteCheck).DeviceID, output, pwmValue);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        [MTFAllowedParameterValue("output", "K0", 0)]
        [MTFAllowedParameterValue("output", "K1", 1)]
        [MTFAllowedParameterValue("output", "K2", 2)]
        [MTFAllowedParameterValue("output", "K3", 3)]
        [MTFAllowedParameterValue("output", "K4", 4)]
        [MTFAllowedParameterValue("output", "K5", 5)]
        [MTFAllowedParameterValue("output", "K6", 6)]
        [MTFAllowedParameterValue("output", "K7", 7)]
        public int GetPwm(int liteCheck, int output)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);
            LiteHelper.CheckRange(output, 0, 7);

            return liteCheckCtr.GetPwm(GetVirtualLite(liteCheck).DeviceID, output);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("output", "K0", 0)]
        [MTFAllowedParameterValue("output", "K1", 1)]
        [MTFAllowedParameterValue("output", "K2", 2)]
        [MTFAllowedParameterValue("output", "K3", 3)]
        [MTFAllowedParameterValue("output", "K4", 4)]
        [MTFAllowedParameterValue("output", "K5", 5)]
        [MTFAllowedParameterValue("output", "K6", 6)]
        [MTFAllowedParameterValue("output", "K7", 7)]
        public void SetPwmToAll(int output, int pwmValue)
        {
            LiteHelper.CheckRange(output, 0, 7);
            liteCheckCtr.SetPwmToAll(output, pwmValue);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        [MTFAllowedParameterValue("output", "K8", 8)]
        [MTFAllowedParameterValue("output", "K9", 9)]
        [MTFAllowedParameterValue("output", "K10", 10)]
        [MTFAllowedParameterValue("output", "K11", 11)]
        public void SetRelay(int liteCheck, int output, bool state)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);
            LiteHelper.CheckRange(output, 8, 11);

            liteCheckCtr.SetRelay(GetVirtualLite(liteCheck).DeviceID, output, state);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        [MTFAllowedParameterValue("output", "K8", 8)]
        [MTFAllowedParameterValue("output", "K9", 9)]
        [MTFAllowedParameterValue("output", "K10", 10)]
        [MTFAllowedParameterValue("output", "K11", 11)]
        public bool GetRelay(int liteCheck, int output)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);
            LiteHelper.CheckRange(output, 8, 11);

            return liteCheckCtr.GetRelay(GetVirtualLite(liteCheck).DeviceID, output);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("output", "K8", 8)]
        [MTFAllowedParameterValue("output", "K9", 9)]
        [MTFAllowedParameterValue("output", "K10", 10)]
        [MTFAllowedParameterValue("output", "K11", 11)]
        public void SetRelayToAll(int output, bool state)
        {
            LiteHelper.CheckRange(output, 8, 11);
            liteCheckCtr.SetRelayToAll(output, state);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        public void SetPwmFrequency(int liteCheck, int pwmFrequency)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);
            LiteHelper.CheckRange(pwmFrequency, 10, 400);

            liteCheckCtr.SetPwmFrequency(GetVirtualLite(liteCheck).DeviceID, (Int16)pwmFrequency);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        public int GetPwmFrequency(int liteCheck)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);

            return liteCheckCtr.GetPwmFrequency(GetVirtualLite(liteCheck).DeviceID);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        [MTFAllowedParameterValue("trigger", "Trigger 1", 1)]
        [MTFAllowedParameterValue("trigger", "Trigger 2", 2)]
        public void SetTrigger(int liteCheck, int trigger, bool state)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);
            LiteHelper.CheckRange(trigger, 1, 2);

            liteCheckCtr.SetTrigger(GetVirtualLite(liteCheck).DeviceID, (byte)trigger, state);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        [MTFAllowedParameterValue("trigger", "Trigger 1", 1)]
        [MTFAllowedParameterValue("trigger", "Trigger 2", 2)]
        public bool GetTrigger(int liteCheck, int trigger)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);
            LiteHelper.CheckRange(trigger, 1, 2);

            return liteCheckCtr.GetTrigger(GetVirtualLite(liteCheck).DeviceID, (byte)trigger);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        [MTFAllowedParameterValue("externalInput", "E1", 1)]
        [MTFAllowedParameterValue("externalInput", "E2", 2)]
        [MTFAllowedParameterValue("externalInput", "E3", 3)]
        [MTFAllowedParameterValue("externalInput", "E4", 4)]
        public bool GetExternalInput(int liteCheck, int externalInput)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);
            LiteHelper.CheckRange(externalInput, 1, 4);

            return liteCheckCtr.GetExternalInput(GetVirtualLite(liteCheck).DeviceID, externalInput);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        public int GetTemperature(int liteCheck)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);

            return liteCheckCtr.GetTemperature(GetVirtualLite(liteCheck).DeviceID);
        }


        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        public string GetSerialNumber(int liteCheck)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);

            return liteCheckCtr.GetSerialNumber(GetVirtualLite(liteCheck).DeviceID);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        public int GetPwmFrequencyFromStateInfo(int liteCheck)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);

            return liteCheckCtr.GetPwmFrequencyFromStateInfo(GetVirtualLite(liteCheck).DeviceID);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        public string GetSwVersion(int liteCheck)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);

            return liteCheckCtr.GetSwVersion(GetVirtualLite(liteCheck).DeviceID);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        public string GetHwVersion(int liteCheck)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);

            return liteCheckCtr.GetHwVersion(GetVirtualLite(liteCheck).DeviceID);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        public int GetDeviceID(int liteCheck)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);

            return liteCheckCtr.GetDeviceID(GetVirtualLite(liteCheck).DeviceID);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        public string GetFullSerialNumber(int liteCheck)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);

            return liteCheckCtr.GetFullSerialNumber(GetVirtualLite(liteCheck).DeviceID);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 0", 0)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 1", 1)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 2", 2)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 3", 3)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 4", 4)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 5", 5)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 6", 6)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 7", 7)]
        public int GetDefinedHostID(int liteCheck, int hostIdSelected)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);
            LiteHelper.CheckRange(hostIdSelected, 0, 7);

            return liteCheckCtr.GetHostID(GetVirtualLite(liteCheck).DeviceID, (byte)hostIdSelected);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck1", 1)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck2", 2)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck3", 3)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck4", 4)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck5", 5)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck6", 6)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck7", 7)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck8", 8)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck9", 9)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck10", 10)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck11", 11)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck12", 12)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck13", 13)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck14", 14)]
        [MTFAllowedParameterValue("liteCheck", "LiteCheck15", 15)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 0", 0)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 1", 1)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 2", 2)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 3", 3)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 4", 4)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 5", 5)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 6", 6)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 7", 7)]
        public int GetDefinedLightID(int liteCheck, int lightIdSelected)
        {
            LiteHelper.CheckRange(liteCheck, 1, 15);
            LiteHelper.CheckRange(lightIdSelected, 0, 7);

            return liteCheckCtr.GetLightID(GetVirtualLite(liteCheck).DeviceID, (byte)lightIdSelected);
        }

        [MTFMethod]
        public void SetDeviceID(int deviceID)
        {
            LiteHelper.CheckRange(deviceID, 1, 15);

            liteCheckCtr.SetDeviceID((byte)deviceID);
        }

        [MTFMethod]
        public void SetSerialNumber(string serialNumber)
        {
            liteCheckCtr.SetSerialNumber(serialNumber);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("hostIdSelected", "ID 0", 0)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 1", 1)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 2", 2)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 3", 3)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 4", 4)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 5", 5)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 6", 6)]
        [MTFAllowedParameterValue("hostIdSelected", "ID 7", 7)]
        public void SetDefinedHostID(int hostIdSelected, UInt16 hostID)
        {
            LiteHelper.CheckRange(hostIdSelected, 0, 7);

            liteCheckCtr.SetHostID((byte)hostIdSelected, hostID);
        }

        [MTFMethod]
        [MTFAllowedParameterValue("lightIdSelected", "ID 0", 0)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 1", 1)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 2", 2)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 3", 3)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 4", 4)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 5", 5)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 6", 6)]
        [MTFAllowedParameterValue("lightIdSelected", "ID 7", 7)]
        public void SetDefinedLightID(int lightIdSelected, UInt16 lightID)
        {
            LiteHelper.CheckRange(lightIdSelected, 0, 7);

            liteCheckCtr.SetLightID((byte)lightIdSelected, lightID);
        }

        #endregion

        #region BusCommunication
        [MTFMethod]
        public MTFOffBoardFlashJobResult GetFlashJobResult(int index)
        {
            return busCommunicationDriver.GetFlashJobResult(index);
        }

        [MTFMethod]
        public string GetKey(int resultIndex, int valueIndex)
        {
            return busCommunicationDriver.GetKey(resultIndex, valueIndex);
        }

        [MTFMethod]
        public OnBoardConfiguration GetOnBoardItem(int index)
        {
            return busCommunicationDriver.GetOnBoardItem(index);
        }

        [MTFMethod]
        public string GetValue(int resultIndex, int valueIndex)
        {
            return busCommunicationDriver.GetValue(resultIndex, valueIndex);
        }

        [MTFMethod]
        public void Initialize()
        {
            busCommunicationDriver.Initialize();
        }

        [MTFProperty]
        public MTFOffBoardConfig OffBoard
        {
            set { busCommunicationDriver.OffBoard = value; }
        }

        [MTFMethod]
        public MTFOffBoardResponse OffBoardExecuteService(string logicalLinkName, MTFBusCommunication.Structures.MTFOffBoardService serviceSetting)
        {
            return busCommunicationDriver.OffBoardExecuteService(logicalLinkName, serviceSetting);
        }

        public List<MTFOffBoardLogicalLinkParallelResponses> OffBoardExecuteServicesInParallel(List<MTFOffBoardLogicalLinkParallelServices> offBoardParallelServicesSetting)
        {
            throw new NotImplementedException();
        }

        [MTFMethod]
        public List<MTFOffBoardFlashJobResult> OffBoardFlashJob(List<MTFOffBoardFlashJobSetting> flashJobSetttings)
        {
            return busCommunicationDriver.OffBoardFlashJob(flashJobSetttings);
        }

        [MTFMethod]
        public string OffBoardVariantIdentificationAndSelection(string logicalLinkName)
        {
            return busCommunicationDriver.OffBoardVariantIdentificationAndSelection(logicalLinkName);
        }

        [MTFMethod]
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
        public void OnBoardActiveScheduleTable(int virtualChannel, string scheduleTable)
        {
            busCommunicationDriver.OnBoardActiveScheduleTable(virtualChannel, scheduleTable);
        }

        [MTFMethod]
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
        public string OnBoardGetGlobalVariable(int virtualChannel, string variableName)
        {
            return busCommunicationDriver.OnBoardGetGlobalVariable(virtualChannel, variableName);
        }

        [MTFMethod]
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
        public MTFOnBoardSignal OnBoardGetSignal(int virtualChannel, string frameName, string signalName)
        {
            return busCommunicationDriver.OnBoardGetSignal(virtualChannel, frameName, signalName);
        }

        [MTFMethod]
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
        public byte[] OnBoardReceiveFrameOnce(int virtualChannel, uint frameId, uint resFrameId, byte[] data, uint timeout)
        {
            return busCommunicationDriver.OnBoardReceiveFrameOnce(virtualChannel, frameId, resFrameId, data, timeout);
        }

        [MTFProperty]
        public List<MTFBusCommunication.Structures.OnBoardConfiguration> OnBoards
        {
            set { busCommunicationDriver.OnBoards = value; }
        }

        [MTFMethod]
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
        public void OnBoardSendFrameOnce(int virtualChannel, uint frameId, byte[] data)
        {
            busCommunicationDriver.OnBoardSendFrameOnce(virtualChannel, frameId, data);
        }

        [MTFMethod]
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
        public void OnBoardSetGlobalVariable(int virtualChannel, string variableName, string variableValue)
        {
            busCommunicationDriver.OnBoardSetGlobalVariable(virtualChannel, variableName, variableValue);
        }

        [MTFMethod]
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
        public void OnBoardSetSignal(int virtualChannel, string frameName, string signalName, string signalValue)
        {
            busCommunicationDriver.OnBoardSetSignal(virtualChannel, frameName, signalName, signalValue);
        }

        [MTFMethod]
        public void Start()
        {
            busCommunicationDriver.Start();
        }

        [MTFProperty]
        public MTFBusComDriverStatusEnum Status
        {
            get { return busCommunicationDriver.Status; }
        }

        [MTFMethod]
        public void Stop()
        {
            busCommunicationDriver.Stop();
        }

        [MTFMethod(DisplayName = "Set Logging", Description = "This activity is obsolete. Set logging in ALUtils.dll configuration file")]
        [MTFAdditionalParameterInfo(ParameterName = "enable", Description = "Enable or disable log files generating")]
        [MTFAdditionalParameterInfo(ParameterName = "logPath", Description = @"Path to log files. Leave empty for default path (\MTF\Server)")]
        [MTFAdditionalParameterInfo(ParameterName = "maxLogFilesCount", Description = "Maximal count of kept files before delete. 0 means do not delete anything.")]
        public void SetLogging(bool enable, string logPath, int maxLogFilesCount)
        {
        //    busCommunicationDriver.SetLogging(enable, logPath, maxLogFilesCount);
        }
        #endregion

        #region privateMethods

        private LiteCheckConfig GetVirtualLite(int virtualLite)
        {
            var virtualL = virtualLiteChecks.FirstOrDefault(x => x.VirtualLiteCheck == virtualLite);

            if (virtualL == null)
            {
                throw new Exception("This virtual LiteCheck is not assigned to any real device! Check your components configuration.");
            }
            else
            {
                return virtualL;
            }
        }

        private List<int> GetListOfDeviceIDs()
        {
            List<int> liteCheckDeviceIDs = new List<int>();
            foreach (var cnfg in virtualLiteChecks)
            {
                liteCheckDeviceIDs.Add(cnfg.DeviceID);
            }
            return liteCheckDeviceIDs;
        }
        #endregion


        [MTFMethod(DisplayName = "OffBoard Execute service cyclically", Description = "")]
        [MTFAdditionalParameterInfo(ParameterName = "logicalLinkName", Description = "In case of DTS use logical link from system configurator. In case of Ediabas use sgbd file without extension e.g.(FLE02_L)")]
        [MTFAdditionalParameterInfo(ParameterName = "cycleTime", DisplayName = "cycleTime [ms]")]
        public void OffBoardExecuteServiceCyclically(string logicalLinkName, MTFOffBoardService serviceSetting, int cycleTime)
        {
            busCommunicationDriver.OffBoardExecuteServiceCyclically(logicalLinkName, serviceSetting, cycleTime);
        }

        [MTFMethod(DisplayName = "Stop OffBoard Execute service cyclically", Description = "Stop cyclical execution of service which is started by method 'OffBoard Execute service cyclically'")]
        public void StopOffBoardExecuteServiceCyclically()
        {
            busCommunicationDriver.StopOffBoardExecuteServiceCyclically();
        }

        [MTFMethod(DisplayName = "OffBoardExecuteOTX", Description = "")]
        [MTFAdditionalParameterInfo(ParameterName = "serviceSetting", DisplayName = "service name is OTX Script or Project", Description = "service name of the OTX script with extension. e.g. MyScript.otx")]
        public MTFBusCommunication.Structures.MTFOffBoardResponse OffBoardExecuteOTX(MTFBusCommunication.Structures.MTFOffBoardService serviceSetting)
        {
            return busCommunicationDriver.OffBoardExecuteOTX(serviceSetting);
        }
    }

}
