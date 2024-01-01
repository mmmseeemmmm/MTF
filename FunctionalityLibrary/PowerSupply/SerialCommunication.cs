using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerSupply
{
    public static class SerialCommunication
    {
        private static Dictionary<string, SerialPort> openedPorts = new Dictionary<string, SerialPort>();
        private static Dictionary<string, int> portUsage = new Dictionary<string, int>();

        //private static SerialCommunication()
        //{
        //}

        public static SerialPort GetSerialPort(string comPort, int baudRate)
        {
            if (!openedPorts.ContainsKey(comPort))
            {
                openedPorts[comPort] = initSerialPort(comPort, baudRate);
                portUsage[comPort] = 0;
            }
            if (openedPorts[comPort].BaudRate != baudRate)
            {
                throw new Exception("baud rate differs for the same bus");
            }
            portUsage[comPort]++;
            return openedPorts[comPort];

        }

        public static void CloseSerailPort(string comPort)
        {
            portUsage[comPort]--;

            if (portUsage[comPort] == 0)
            {
                openedPorts[comPort].Close();
                openedPorts[comPort].Dispose();
                openedPorts.Remove(comPort);
            }
        }


        private static SerialPort initSerialPort(string comPort, int baudRate)
        {

            try
            {
                var serialPort = new SerialPort();
                serialPort.PortName = comPort;
                serialPort.BaudRate = baudRate;
                //serialPort.WriteTimeout = this.timeOut;
                //serialPort.ReadTimeout = this.timeOut;
                serialPort.NewLine = "\r";
                serialPort.Open();
                return serialPort;
            }
            catch (Exception e)
            {
                throw new Exception("Serial port was not initialized: " + e.ToString());

            }
        }
    }
}
