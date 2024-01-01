using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PowerSupply
{
    public class UdpClientWrapper : IDisposable
    {
        private UdpClient udpClient;
        private IPEndPoint localEndPoint;
        private IPEndPoint remoteEndPoint;
        private bool receivedResponse;
        private string receivedData;
        private bool isListenning;

        public UdpClientWrapper(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            if (localEndPoint == null)
            {
                throw new ArgumentNullException("localEndPoint");
            }

            if (remoteEndPoint == null)
            {
                throw new ArgumentNullException("remoteEndPoint");
            }

            this.localEndPoint = localEndPoint;
            this.remoteEndPoint = remoteEndPoint;
            this.udpClient = new UdpClient(this.localEndPoint);
            this.udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
            this.isListenning = true;
        }

        public void Send(string data)
        {
            this.ResetReceiver();

            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException("data");
            }

            byte[] bytes = Encoding.ASCII.GetBytes(data);

            this.udpClient.Send(bytes, bytes.Length, this.remoteEndPoint);
        }

        public string SendReceive(string data)
        {
            this.ResetReceiver();

            this.Send(data);

            var stopwatch = new Stopwatch();
            int testCount = 0;

            stopwatch.Start();

            while (!this.receivedResponse)
            {
                if (testCount > 5)
                {
                    throw new Exception("Cannot received data.");
                }

                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    testCount++;

                    this.Send(data);

                    stopwatch.Restart();
                }

                Thread.Sleep(35);
            }

            stopwatch.Stop();

            return this.receivedData;
        }

        private void ResetReceiver()
        {
            this.receivedResponse = false;
            this.receivedData = null;
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            if (!this.isListenning)
            {
                return;
            }

            try
            {
                byte[] data = this.udpClient.EndReceive(asyncResult, ref this.localEndPoint);
                this.receivedData = Encoding.ASCII.GetString(data, 0, data.Length);
                this.receivedResponse = true;

                this.udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void Dispose()
        {
            if (this.udpClient != null)
            {
                this.isListenning = false;

                this.udpClient.Close();
            }
        }
    }
}
