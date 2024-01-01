using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;
using PLC.Helpers;

namespace PLC
{
    [MTFClass(Name = "PLC", Description = "", Icon = MTFIcons.PLC)]
    [MTFClassCategory("Machine Control")]
    public class PLC_Driver : IDisposable
    {
        private PLC_Protocol plc = null;

        [MTFConstructor(Description = "Create PLC component")]
        [MTFAllowedParameterValueAttribute("plcType", "Bosh UDP", PLCTypes.BoshUdp)]
        public PLC_Driver(string IpPC, string PortPC, string IpPLC, string PortPLC, int plcType) 
        {
            PlcEnums.PLCType type = PlcEnums.PLCType.Bosch_UDP;
            if (plcType == 1)
            {
                type = PlcEnums.PLCType.Bosch_UDP;
            }

            plc = new PLC_Protocol(IpPC, PortPC, IpPLC, PortPLC, type);
        }

        ~PLC_Driver()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (plc != null)
                plc.Close();
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            if (plc != null)
                plc.Close();
        }


        #region PropertiesToPLC
        public DateTime LastRxTimeStamp
        {
            get
            {
                if (plc != null)
                {
                    return plc.LastRxTimeStamp;
                }
                else return DateTime.MinValue;
            }
        }

        public byte[] RawDataToPLC
        {
            get
            {
                if (plc != null)
                {
                    return plc.RawDataToPLC;
                }
                else
                    return new byte[1];
            }
        }
        public byte[] RawDataFromPLC
        {
            get
            {
                if (plc != null)
                {
                    return plc.RawDataFromPLC;
                }
                else
                    return new byte[1];
            }
        }

        [MTFProperty(Name = "Test Pc Done OK", Description = "")]
        public bool TestPcDoneOK
        {
            set { plc.setSignal("TestPcDoneOK", value); }
        }

        [MTFProperty(Name = "Test Pc Done NOK", Description = "")]
        public bool TestPcDoneNOK
        {
            set { plc.setSignal("TestPcDoneNOK", value); }
        }

        [MTFProperty(Name = "Pc Ready", Description = "")]
        public bool PcReady
        {
            set { plc.setSignal("PcReady", value); }
        }

        [MTFProperty(Name = "Pc Error", Description = "")]
        public bool PcError
        {
            set { plc.setSignal("PcError", value); }
        }

        [MTFProperty(Name = "C15", Description = "")]
        public bool C15
        {
            set { plc.setSignal("C15", value); }
        }

        [MTFProperty(Name = "LowBeam", Description = "")]
        public bool LowBeam
        {
            set { plc.setSignal("LowBeam", value); }
        }

        [MTFProperty(Name = "HiBeam", Description = "")]
        public bool HiBeam
        {
            set { plc.setSignal("HiBeam", value); }
        }

        [MTFProperty(Name = "Turn Indicator", Description = "")]
        public bool TurnIndicator
        {
            set { plc.setSignal("TurnIndicator", value); }
        }

        [MTFProperty(Name = "HiBeamSpot", Description = "")]
        public bool HiBeamSpot
        {
            set { plc.setSignal("HiBeamSpot", value); }
        }

        [MTFProperty(Name = "CornerLight", Description = "")]
        public bool CornerLight
        {
            set { plc.setSignal("CornerLight", value); }
        }

        [MTFProperty(Name = "PosLight", Description = "")]
        public bool PosLight
        {
            set { plc.setSignal("PosLight", value); }
        }

        [MTFProperty(Name = "Daytime Running Light", Description = "")]
        public bool DaytimeRunningLight
        {
            set { plc.setSignal("DaytimeRunningLight", value); }
        }

        [MTFProperty(Name = "Side Marker", Description = "")]
        public bool SideMarker
        {
            set { plc.setSignal("SideMarker", value); }
        }

        [MTFProperty(Name = "Collective", Description = "")]
        public bool Collective
        {
            set { plc.setSignal("Collective", value); }
        }

        [MTFProperty(Name = "Variant CAN Switch", Description = "")]
        public bool VariantCANSwitch
        {
            set { plc.setSignal("VariantCANSwitch", value); }
        }

        [MTFProperty(Name = "Additional ECU", Description = "")]
        public bool AdditionalECU
        {
            set { plc.setSignal("AdditionalECU", value); }
        }

        [MTFProperty(Name = "Pwm Out", Description = "")]
        public UInt16 PwmOut
        {
            set { plc.setSignal("PwmOut", value); }
        }

        [MTFProperty(Name = "Analog Out", Description = "")]
        public UInt16 AnalogOut
        {
            set { plc.setSignal("AnalogOut", value); }
        }
        #endregion

        #region PropertiesFromPLC
        [MTFProperty(Name = "PLC Error Status", Description = "")]
        public string PlcErrorStatus
        {
            get { return plc.errorStatus; }
        }

        [MTFProperty(Name = "PLC To Pc Start", Description = "")]
        public bool PlcToPcStart
        {
            get { return (bool)plc.getSignal("PlcToPcStart"); }
        }

        [MTFProperty(Name = "PLC Error", Description = "")]
        public bool PlcError
        {
            get { return (bool)plc.getSignal("PlcError"); }
        }

        [MTFProperty(Name = "Left DUT", Description = "")]
        public bool LeftDUT
        {
            get { return (bool)plc.getSignal("LeftDUT"); }
        }

        [MTFProperty(Name = "Right DUT", Description = "")]
        public bool RightDUT
        {
            get { return (bool)plc.getSignal("RightDUT"); }
        }

        [MTFProperty(Name = "PLC Test NOK", Description = "")]
        public bool PlcTestNOK
        {
            get { return (bool)plc.getSignal("PlcTestNOK"); }
        }

        [MTFProperty(Name = "PLC Test OK", Description = "")]
        public bool PlcTestOK
        {
            get { return (bool)plc.getSignal("PlcTestOK"); }
        }

        [MTFProperty(Name = "Voltage UBat", Description = "")]
        public UInt16 VoltageUBat
        {
            get { return (UInt16)plc.getSignal("VoltageUBat"); }
        }

        [MTFProperty(Name = "Voltage Analog In", Description = "")]
        public UInt16 VoltageAnalogIn
        {
            get { return (UInt16)plc.getSignal("VoltageAnalogIn"); }
        }

        [MTFProperty(Name = "Current C15", Description = "")]
        public UInt16 CurrentC15
        {
            get { return (UInt16)plc.getSignal("CurrentC15"); }
        }

        [MTFProperty(Name = "Current LowBeam", Description = "")]
        public UInt16 CurrentLowBeam
        {
            get { return (UInt16)plc.getSignal("CurrentLowBeam"); }
        }

        [MTFProperty(Name = "Fixture Coding", Description = "")]
        public Byte FixtureCoding
        {
            get { return (Byte)plc.getSignal("FixtureCoding"); }
        }

        [MTFProperty(Name = "Bar Code", Description = "")]
        public String BarCode
        {
            get 
            { return plc.getSignal("BarCode").ToString(); }
        }

        [MTFProperty(Name = "Bar Code 2", Description = "")]
        public String BarCode2
        {
            get
            { return plc.getSignal("BarCode2").ToString(); }
        }

        [MTFProperty(Name = "Bar Code 3", Description = "")]
        public String BarCode3
        {
            get
            { return plc.getSignal("BarCode3").ToString(); }
        }

        [MTFProperty(Name = "Bar Code 4", Description = "")]
        public String BarCode4
        {
            get
            { return plc.getSignal("BarCode4").ToString(); }
        }

        [MTFProperty(Name = "User Chip", Description = "")]
        public String UserChip
        {
            get { return plc.getSignal("UserChip").ToString(); }
        }

        #endregion

    }
}
