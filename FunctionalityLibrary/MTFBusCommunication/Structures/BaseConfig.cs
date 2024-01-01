using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace MTFBusCommunication.Structures
{
    [MTFKnownClass]
    public class BaseConfig
    {
        private int busType;

        [MTFAllowedPropertyValue("HSX", 0)]
        [MTFAllowedPropertyValue("Vector", 1)]
        public int HWType { get; set; }

        [MTFAllowedPropertyValue("CAN 1", 1)]
        [MTFAllowedPropertyValue("CAN 2", 2)]
        [MTFAllowedPropertyValue("CAN 3", 3)]
        [MTFAllowedPropertyValue("CAN 4", 4)]
        [MTFAllowedPropertyValue("CAN 5", 5)]
        [MTFAllowedPropertyValue("CAN 6", 6)]
        [MTFAllowedPropertyValue("CAN 7", 7)]
        [MTFAllowedPropertyValue("CAN 8", 8)]
        [MTFAllowedPropertyValue("CAN 9", 9)]
        [MTFAllowedPropertyValue("CAN 10", 10)]
        [MTFAllowedPropertyValue("LIN 1", 11)]
        [MTFAllowedPropertyValue("LIN 2", 12)]
        [MTFAllowedPropertyValue("LIN 3", 13)]
        [MTFAllowedPropertyValue("LIN 4", 14)]
        [MTFAllowedPropertyValue("LIN 5", 15)]
        [MTFAllowedPropertyValue("LIN 6", 16)]
        [MTFAllowedPropertyValue("LIN 7", 17)]
        [MTFAllowedPropertyValue("LIN 8", 18)]
        [MTFAllowedPropertyValue("LIN 9", 19)]
        [MTFAllowedPropertyValue("LIN 10", 20)]
        public int VirtualChannel
        {
            get { return busType; }
            set { busType = value; }
        }

        public string DeviceName { get; set; }
        public string BusChannel { get; set; }

        public string GetBusType()
        {
            return busType > 10 ? "LIN" : "CAN";
        }
    }
}
