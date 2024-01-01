using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace LiteCheckDriver
{
    public static class LiteHelper
    {
        public const string busNodesForLiteCheck = "LITECHECK";
        public const string dbcForLiteCheck = "LITECHECK.dbc";
        public const int temperatureError = 0x7F;
        public const int baseDiagRequestMsgId = 0x700;
        public const int baseDiagResponseMsgId = 0x760;
        public const int diagTimeout = 1000;
        public const int diagFrameLength = 8;

        public static void CheckRange(double value,double min,double max)
        {
            if (value < min || value > max)
                throw new Exception("Value " + value + " is out of range. Range is <" + min + "," + max + ">");
        }

        public static class MsgType
        {
            public const byte request = 0x80;
            public const byte response = 0xFE;
            public const byte query = 0xFF;
        }

        public static class MsgQualifier
        {
            public const byte pwmFreq = 0x1;
            public const byte trigOut = 0x2;
            public const byte versionSw = 0x10;
            public const byte versionHw = 0x11;
            public const byte deviceId = 0x20;
            public const byte sernr = 0x21;
            public const byte defHostId = 0x22;
            public const byte defLightId = 0x23;
        }

        public static class DLCs
        {
            public const byte reqPwmFreq = 0x2;
            public const byte quePwmFreq = 0x0;
            public const byte resPwmFreq = 0x2;
                        
            public const byte reqTrigOut = 0x1;
            public const byte queTrigOut = 0x0;
            public const byte resTrigOut = 0x1;
                        
            public const byte queVersionSw = 0x0;
            public const byte resVersionSw = 0x4;
                        
            public const byte queVersionHw = 0x0;
            public const byte resVersionHw = 0x3;

            public const byte reqDeviceId = 0x1;
            public const byte queDeviceId = 0x0;
            public const byte resDeviceId = 0x1;

            public const byte reqSernr = 0x4;             
            public const byte queSernr = 0x0;
            public const byte resSernr = 0x4;

            public const byte reqDefHostId = 0x3;            
            public const byte queDefHostId = 0x1;
            public const byte resDefHostId = 0x3;

            public const byte reqDefLightId = 0x3;
            public const byte queDefLightId = 0x1;
            public const byte resDefLightId = 0x3;
        }
    }

    [MTFKnownClass]
    public class LiteCheckConfig
    {
        [MTFAllowedPropertyValue("LiteCheck1", 1)]
        [MTFAllowedPropertyValue("LiteCheck2", 2)]
        [MTFAllowedPropertyValue("LiteCheck3", 3)]
        [MTFAllowedPropertyValue("LiteCheck4", 4)]
        [MTFAllowedPropertyValue("LiteCheck5", 5)]
        [MTFAllowedPropertyValue("LiteCheck6", 6)]
        [MTFAllowedPropertyValue("LiteCheck7", 7)]
        [MTFAllowedPropertyValue("LiteCheck8", 8)]
        [MTFAllowedPropertyValue("LiteCheck9", 9)]
        [MTFAllowedPropertyValue("LiteCheck10", 10)]
        [MTFAllowedPropertyValue("LiteCheck11", 11)]
        [MTFAllowedPropertyValue("LiteCheck12", 12)]
        [MTFAllowedPropertyValue("LiteCheck13", 13)]
        [MTFAllowedPropertyValue("LiteCheck14", 14)]
        [MTFAllowedPropertyValue("LiteCheck15", 15)]
        public int VirtualLiteCheck { get; set; }
        public int DeviceID { get; set; }
    }
}
