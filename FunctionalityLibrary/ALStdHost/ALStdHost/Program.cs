using System;
using System.ServiceModel;
using AL.Utils.Logging;
using System.Collections.Generic;
using ALStdHostCommon;

namespace ALStdHost
{
    class Program
    {
        private static ServiceHost host;
        private static string serverAddress;
        private static StdHost alHost = new StdHost();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            serverAddress = Constants.DefaultServerAddress;
            if (args.Length == 1)
            {
                serverAddress = args[0];
            }
            
            Log.LogMessage("ALStdHost starting", true);
            Log.LogMessage(string.Format("ALStdHost working direcotry {0}", Environment.CurrentDirectory), true);

            try
            {
                Log.ExecuteAndLog(() => startServer(serverAddress),
                    string.Format(
                        "Starting WCF server on address {0}. Address can be changed by commandline parameter",
                        serverAddress), "WCF server listening", true);
            }
            catch
            {
                Log.LogMessage("Start server crahsed", true);
            }

            Console.ReadKey();

            try
            {
                host.Abort();
            }
            catch (Exception e)
            {
                Log.LogMessage("Closing WCF host crashed", true);
                Log.LogMessage(e);
            }
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Log.LogMessage("Unhandled exception catched " + unhandledExceptionEventArgs.ExceptionObject);
        }

        private static void startServer(string address)
        {
            List<Uri> baseAddresses = new List<Uri>();

            baseAddresses.Add(new Uri(address));


            host = new ServiceHost(alHost, baseAddresses.ToArray());
            //Use object as singleton
            host.Description.Behaviors.Find<ServiceBehaviorAttribute>().InstanceContextMode = InstanceContextMode.Single;

            var endpoint = new NetTcpBinding();

            endpoint.ReceiveTimeout = TimeSpan.MaxValue;
            endpoint.SendTimeout = TimeSpan.MaxValue;

            endpoint.MaxBufferSize = int.MaxValue;
            endpoint.MaxReceivedMessageSize = int.MaxValue;
            endpoint.Security.Mode = SecurityMode.None;

            host.AddServiceEndpoint(typeof(IStdHost), endpoint, "");

            host.Open();
        }
    }
}
