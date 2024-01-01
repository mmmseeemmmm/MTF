using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ALStdHostCommon
{
    public static class ALStdHostHelper
    {
        private static Process ALStdhostProcess;
        private static string ServerAddress;

        public static void StartALStdHostServer(string basePath)
        {
            StartALStdHostServer(basePath, Constants.DefaultServerAddress);
        }

        public static void StartALStdHostServer(string basePath, string serverAddress)
        {
            startProcess(basePath, serverAddress);
            ServerAddress = serverAddress;
        }

        public static void StopALStdHostServer()
        {
            if (ALStdhostProcess != null && !ALStdhostProcess.HasExited)
            {
                ALStdhostProcess.Kill();
                ALStdhostProcess.Dispose();
                ALStdhostProcess = null;
            }
        }

        public static IStdHost Connect()
        {
            var myBinding = new NetTcpBinding
            {
                ReceiveTimeout = new TimeSpan(0, 20, 0),
                SendTimeout = new TimeSpan(0, 20, 0),
                MaxReceivedMessageSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                Security = { Mode = SecurityMode.None }
            };

            var myEndpoint = new EndpointAddress(ServerAddress);
            var myChannelFactory = new ChannelFactory<IStdHost>(myBinding, myEndpoint);

            return myChannelFactory.CreateChannel();
        }

        private static void startProcess(string basePath, string serverAddress)
        {
            var fileName = Path.GetFullPath(Path.Combine(basePath,"ALStdHost.exe"));
            if (!File.Exists(fileName))
            {
                throw new Exception(string.Format("File {0} not found. Check ALStdHost.exe location.", fileName));
            }

            ALStdhostProcess = Process.Start(new ProcessStartInfo
            {
                Arguments = serverAddress,
                FileName = fileName,
                WorkingDirectory = Path.GetFullPath(basePath),
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            });

        }
    }
}
