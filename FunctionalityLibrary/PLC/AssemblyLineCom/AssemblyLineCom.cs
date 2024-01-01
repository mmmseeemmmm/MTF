using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;
using PLC.Helpers;
using System.Threading;

namespace PLC.AssemblyLineCom
{
    [MTFClass(Name = "Assembly Line Communication", Description = "",Icon=MTFIcons.Conveyor)]
    [MTFClassCategory("Machine Control")]
    class AssemblyLineCom : ICanStop
    {

        private AssemblyLineComProtocol plc = null;
        private GluingStationComProtocol plcG = null;


        [MTFConstructor(Description = "Create component for communication with transfer system PLC. Default COB IDs are used. ToPLC = 10026; FromPLC = 20026")]
        public AssemblyLineCom(string IpPC, string PortPC, string IpPLC, string PortPLC, string PortSendingPLC)
        {
            PlcEnums.PLCType type = PlcEnums.PLCType.Bosch_UDP;

            plc = new AssemblyLineComProtocol(IpPC, PortPC, IpPLC, PortPLC, type, PortSendingPLC);

        }

        [MTFConstructor(Description = "Create component for communication with transfer system PLC. optional COB ID")]
        public AssemblyLineCom(string IpPC, string PortPC, string IpPLC, string PortPLC, string PortSendingPLC, UInt16 CobIdToPLC, UInt16 CobIdFromPLC)
        {
            PlcEnums.PLCType type = PlcEnums.PLCType.Bosch_UDP;

            plc = new AssemblyLineComProtocol(IpPC, PortPC, IpPLC, PortPLC, type, PortSendingPLC, CobIdToPLC, CobIdFromPLC);

        }

        [MTFConstructor(Description = "Create component for communication with gluing PLC")]
        public AssemblyLineCom(string IpPC, string PortPC, string IpPLC, string PortPLC, UInt16 CobIdToPLC)
        {
            PlcEnums.PLCType type = PlcEnums.PLCType.Bosch_UDP;

            plcG = new GluingStationComProtocol(IpPC, PortPC, IpPLC, PortPLC, type, CobIdToPLC);
        }


        ~AssemblyLineCom()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (plc != null)
            {
                plc.Close();
            }

            if (plcG != null)
            {
                plcG.Close();
            }
            GC.SuppressFinalize(this);
        }

        [MTFMethod]
        public bool GetProcessOrNot()
        {
            plc.WtRdy = true;
            plc.SendData();

            while (!(plc.ProccesHL || plc.NotproccesHL))
            {
                Thread.Sleep(100);
                if (stop)
                {
                    return false;
                }
            }

            if (plc.ProccesHL && plc.NotproccesHL)
            {
                var errorMessage = "Invalid state of signals. Cannot get procces and notprocces at the same time";
                //Logging.AddTraceLine(errorMessage);
                throw new Exception(errorMessage);
            }

            if (plc.ProccesHL)
            {
                return true;
            }
            if (plc.NotproccesHL)
            {
                return false;
            }

            var errorMsg = "Invalid state of signals. Procces or notprocces must be true";
            //Logging.AddTraceLine(errorMsg);
            throw new Exception(errorMsg);

        }

        [MTFMethod]
        public void SendResult(bool OKresult)
        {
            plc.OK = OKresult;
            plc.NOK = !OKresult;
            plc.SendData();

            while (plc.ProccesHL || plc.NotproccesHL)
            {
                Thread.Sleep(100);
                if (stop)
                {
                    return;
                }
            }

            plc.OK = false;
            plc.NOK = false;
            plc.SendData();
            Thread.Sleep(50);
            plc.WtRdy = false;

        }

        [MTFMethod]
        public string PalleteID()
        {
            return plc.PalleteID;
        }

        [MTFMethod]
        public void AddSerialNumberToList(string serialNumber)
        {
            try
            {
                plcG.AddHeadlamp(serialNumber);
            }
            catch (Exception ex)
            {
                var msg = string.Format("{0}{1}{2}", ex.Message, Environment.NewLine, Environment.StackTrace);
                //Logging.AddTraceLine(msg);
                throw new Exception(msg);
            }
        }

        private bool stop = false;
        public bool Stop
        {
            set { stop = value; }
        }
    }
}
