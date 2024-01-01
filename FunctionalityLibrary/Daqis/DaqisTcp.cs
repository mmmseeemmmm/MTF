using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using AutomotiveLighting.MTFCommon;

namespace AlDaqis
{
    [MTFClass(Icon = MTFIcons.DataCollection, Name = "Daqis TCP")]
    [MTFClassCategory("Report")]
    public class DaqisTcp : IDisposable, ICanStop
    {
        private readonly string ipAddress;
        private readonly int port;
        private bool waitingForRequest;
        private string responseString;
        private bool? processingRequestResult;
        private int counter = 0;

        [MTFConstructor]
        [MTFAdditionalParameterInfo(ParameterName = "ipAddress",DisplayName = "IP address")]
        [MTFAdditionalParameterInfo(ParameterName = "port", DisplayName = "Port")]
        public DaqisTcp(string ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            
        }

        [MTFMethod]
        [MTFAdditionalParameterInfo(ParameterName = "lineName", DisplayName = "Line name")]
        [MTFAdditionalParameterInfo(ParameterName = "workPlaceNumber", DisplayName = "Workplace number")]
        [MTFAdditionalParameterInfo(ParameterName = "partialWorkplaceNumber", DisplayName = "Partial Workplace Number")]
        [MTFAdditionalParameterInfo(ParameterName = "partialWorkplaceNumber2", DisplayName = "Partial Workplace Number 2", DefaultValue = "0")]
        [MTFAdditionalParameterInfo(ParameterName = "serialNumber", DisplayName = "Serial Number")]
        public bool ProcessingRequest(int lineName, int workPlaceNumber, int partialWorkplaceNumber,int partialWorkplaceNumber2, string serialNumber)
        {
            var client = new TcpClient();
            client.Connect(ipAddress, port);

            if (counter== int.MaxValue)
            {
                counter = 0;
            }
            else
            {
                counter++;
            }

            var now = DateTime.Now;
            var packet = new byte[1024];
            var dateString = $"{now:yyMMddhhmmss}{counter:D4}";

            var completeString = $"{dateString}{lineName:D4}{workPlaceNumber:D4}{partialWorkplaceNumber:D2}{partialWorkplaceNumber2:D2}DPF{serialNumber}";
            var bytes = Encoding.ASCII.GetBytes(completeString);

            bytes.CopyTo(packet, 0);

            Stream stream = client.GetStream();

            waitingForRequest = true;
            processingRequestResult = null;
            responseString = null;

            BeginRead(stream);
            stream.Write(packet, 0, packet.Length);

            while (waitingForRequest && !Stop)
            {
                Thread.Sleep(100);
            }

            if (Stop)
            {
                return false;
            }

            if (!processingRequestResult.HasValue)
            {
                throw new Exception($"Wrong Daqis response\n{responseString}");
            }

            client.Close();

            return processingRequestResult.Value;
        }


        private void BeginRead(Stream stream)
        {
            var buffer = new byte[31];
            stream.BeginRead(buffer, 0, buffer.Length, EndRead, buffer);
        }

        private void EndRead(IAsyncResult result)
        {
            var buffer = (byte[])result.AsyncState;
            var bytesAvailable = buffer.Length;

            if (bytesAvailable > 16)
            {
                switch (buffer[16])
                {
                    case 1:
                        processingRequestResult = true;
                        break;
                    case 2:
                        processingRequestResult = false;
                        break;
                }
            }

            responseString = Encoding.ASCII.GetString(buffer, 0, bytesAvailable);
            waitingForRequest = false;
        }

        public void Dispose()
        {
            //client?.Close();
        }

        public bool Stop { get; set; }
    }
}