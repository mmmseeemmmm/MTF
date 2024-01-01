using System;
using System.Net;
using System.Net.Sockets;
using PLC.Helpers;

namespace PLC
{
    public class Bosch_UDP : IPLCCommunication
    {
        private DateTime RxTimeSt;
        public bool isListening = false;
        public string output;
        private UdpClient client;

        private IPEndPoint localEndPoint;
        private IPEndPoint remoteEndPoint;

        public bool sending = false;


        public void Start(string IpPC, string PortPC, string IpPLC, string PortPLC, bool onlySending, string PortSendingPLC)
        {
            localEndPoint = CreateEndPoint(IpPC, PortPC);
            remoteEndPoint = CreateEndPoint(IpPLC, PortPLC);

            client = new UdpClient(localEndPoint);

            if (String.IsNullOrEmpty(PortSendingPLC))
            {
                PortSendingPLC = PortPLC;
            }
            //sendInitFrame(IpPLC, PortSendingPLC);

            if (!onlySending)
            {
                client.BeginReceive(new AsyncCallback(ReceiveCallback), null);

                isListening = true;
            }
        }
        ~Bosch_UDP()
        {
            this.Close();
        }

        public void SendData(byte[] byteData)
        {
            client.Send(byteData, byteData.Length, remoteEndPoint);
        }

        public void Close()
        {
            isListening = false;
            if (client != null)
            {
                client.Close();
            }
        }

        private void sendInitFrame(string ipPLC, string portPLC)
        {
            client.Send(new byte[] { }, 0, CreateEndPoint(ipPLC, portPLC));
            //client.Send(new byte[] { }, 0, CreateEndPoint(ipPLC, "1202"));
            //client.Send(new byte[] { }, 0, CreateEndPoint(ipPLC, "1025"));
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                if (!isListening)
                {
                    return;
                }

                byte[] data = client.EndReceive(ar, ref localEndPoint);
                // output = string.Concat(data.Select(b => b.ToString("X2") + " ")) + Environment.NewLine;
                client.BeginReceive(new AsyncCallback(ReceiveCallback), null);

                OnData(this, data);

                RxTimeSt = DateTime.Now;

            }
            catch //(Exception ex)
            {
                //Logging.AddTraceLine(ex.Message);
                //Do nothing, this is callback -> nobody will handle this exception
            }
        }

        public DateTime LastRxTimeStamp()
        {
            return RxTimeSt;
        }

        private IPEndPoint CreateEndPoint(string ipAddress, string port)
        {
            return new IPEndPoint(IPAddress.Parse(ipAddress), int.Parse(port));
        }


        public event OnDataEventHandler OnData;
    }




}
