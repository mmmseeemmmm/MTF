using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using PowerSupplyExtensionMethods;

namespace PowerSupply
{
    class TTiTCP : PowerSupplyParent
    {
        private string ipAdress;
        private int port;
        private int channel;
        private TcpSingleton connector;

        public TTiTCP(string ipAdress, int port, int channel)
        {
            this.ipAdress = ipAdress;
            this.port = port;
            this.channel = channel;
        }


        public override void Init()
        {
            try
            {
                connector = TcpSingleton.CreateConnector(ipAdress, port);
                connectionSucceed = true;
                identification = connector.SendQuery("*IDN?");

                Output = false;
            }

            catch (Exception e)
            {
                this.connectionSucceed = false;
                throw new Exception("Communication failed: " + e.ToString());
            }
        }

        public override void Rst()
        {
            connector.Write("*RST");
        }

        public override string Idn
        {
            get { return identification; }
        }

        public override double Volt
        {
            get { return connector.SendQuery(string.Format("V{0}?", channel)).Replace("V", string.Empty).GetDouble(); }
            set { connector.Write(string.Format("V{0} {1}", channel, value.FormatIntoString())); }
        }

        public override double Current
        {
            get { return connector.SendQuery(string.Format("I{0}?", channel)).Replace("V", string.Empty).GetDouble(); }
            set { connector.Write(string.Format("I{0} {1}", channel, value.FormatIntoString())); }
        }

        public override double MeasCurrent
        {
            get { return connector.SendQuery(string.Format("I{0}O?", channel)).Replace("A", string.Empty).GetDouble(); }
        }

        public override double MeasVolt
        {
            get { return connector.SendQuery(string.Format("V{0}O?", channel)).Replace("V", string.Empty).GetDouble(); }
        }

        public override double Power { get; set; }
        public override bool Output 
        {
            get
            {
                return connector.SendQuery(string.Format("OP{0}?", channel)) == "1";
            }
            set
            {
                connector.Write(string.Format("OP{0} {1}", channel, value ? "1" : "0"));
            } 
        }

        public override void VoltageRamp(double startVolt, double finVolt, double current, double dwell)
        {
            throw new NotImplementedException();
        }

        public override void CurrentRamp(double startCurrent, double finCurrent, double volt, double dwell)
        {
            throw new NotImplementedException();
        }

        public override void VoltageCurrentRamp(double startVolt, double finVolt, double startCurrent, double finCurrent, double dwell)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            try
            {
                Output = false;
            }
            catch
            {
            }
            if (connector != null)
            {
                connector.Dispose();
            }
        }
    }
}
