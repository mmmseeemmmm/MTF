using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using ALBusComDriver;

namespace ALBusServer
{
    class Program
    {
        private static ServiceHost host;
        private static IBusCommunicationWCF instance = new BusCommunicationWCF();
        static void Main(string[] args)
        {
            logMessage("Application starting", true);

            logMessage(string.Format("Application working direcotry {0}", Environment.CurrentDirectory), true);

            //AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //Task.Run(() => { serverLogging.LogMessage("wait for crash", true); Thread.Sleep(5000); serverLogging.LogMessage("crashing", true); Crash(0); });

            logMessage("Create new bus communication driver instance", true);
            ALBusComDriverServer.BusCommunicationDriver busCommunicationDriver = new ALBusComDriverServer.BusCommunicationDriver();
            logMessage("New bus communication driver instance created", true);

            logMessage("Starting WCF server", true);
            startServer();
            logMessage("WCF server listening", true);

            Console.ReadKey();
        }

        static void Crash(int i)
        {
            Crash(i++);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logMessage("Unhandled exception catched: " + Environment.NewLine, true);
            Environment.Exit(1);
        }

        static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            logMessage("First chance exception catched: " + Environment.NewLine + e.Exception.ToString(), true);
        }

        private static void startServer()
        {
            List<Uri> baseAddresses = new List<Uri>();

            baseAddresses.Add(new Uri("net.tcp://localhost:3553/BusCommunication"));


            host = new ServiceHost(instance, baseAddresses.ToArray());
            //Use object as singleton
            host.Description.Behaviors.Find<ServiceBehaviorAttribute>().InstanceContextMode = InstanceContextMode.Single;

            var endpoint = new NetTcpBinding();

            endpoint.ReceiveTimeout = TimeSpan.MaxValue;
            endpoint.SendTimeout = TimeSpan.MaxValue;

            endpoint.MaxBufferSize = int.MaxValue;
            endpoint.MaxReceivedMessageSize = int.MaxValue;
            endpoint.Security.Mode = SecurityMode.None;

            host.AddServiceEndpoint(typeof(IBusCommunicationWCF), endpoint, "");

            host.Open();
        }

        private static void logMessage(string message, bool logTimeStamp)
        { 
            Logging.LogMessage(message, Logging.LogFilePrefixBusComServer, logTimeStamp);
        }
    }

}
