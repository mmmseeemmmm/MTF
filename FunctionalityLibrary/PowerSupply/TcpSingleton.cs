using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PowerSupply
{
    class TcpSingleton : IDisposable
    {
        private static Dictionary<string, TcpSingleton> createdConnectors = new Dictionary<string, TcpSingleton>();

        public static TcpSingleton CreateConnector(string ipAddress, int port)
        {
            if (!createdConnectors.ContainsKey(ipAddress))
            {
                createdConnectors[ipAddress] = new TcpSingleton(ipAddress, port);
            }
            createdConnectors[ipAddress].usages++;

            return createdConnectors[ipAddress];
        }

        private int usages;
        private TcpClient client = null;
        private NetworkStream stream = null;
        private string ipAddress;

        private TcpSingleton(string ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            try
            {
                client = new TcpClient(ipAddress, port);
                stream = client.GetStream();
            }

            catch (Exception e)
            {
                throw new Exception("Communication failed: " + e.ToString());
            }
        }

        public void Dispose()
        {
            usages--;
            if (usages == 0)
            {
                createdConnectors.Remove(ipAddress);
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }
                if (client != null)
                {
                    client.Close();
                    client = null;
                }
            }
        }
        public void Write(string cmd)
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(cmd);

            // Send the message to the connected TcpServer. 
            if (stream.CanRead)
            {
                stream.Write(data, 0, data.Length);
                System.Threading.Thread.Sleep(100);
            }
            else
            {
                throw new Exception("Command send failed");
            }
        }

        public string Read()
        {
            StringBuilder identification = new StringBuilder();

            if (stream.CanRead)
            {
                byte[] myReadBuffer = new byte[1024];
                int numberOfBytesRead = 0;
                // Incoming message may be larger than the buffer size. 
                do
                {
                    numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                    identification.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

                }
                while (stream.DataAvailable);
                return identification.ToString();
            }

            throw new Exception("Read Response Failed");
        }

        public string SendQuery(string query)
        {
            Write(query);
            return Read().Trim('\n').Trim('\r');
        }

    }
}
