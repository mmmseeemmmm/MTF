using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;

namespace MTFServer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            SystemLog.LogDirectory = BaseConstants.ServerSystemLogsPath;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ICLRRuntimeInfo clrRuntimeInfo = (ICLRRuntimeInfo)RuntimeEnvironment.GetRuntimeInterfaceAsObject(Guid.Empty, typeof(ICLRRuntimeInfo).GUID);
            clrRuntimeInfo.BindAsLegacyV2Runtime();

            base.OnStartup(e);
            killAllServers();

            MainWindow mainWindow;
            if (e.Args.Length > 0)
            {
                mainWindow = new MainWindow(e.Args[0]);
            }
            else
            {
                mainWindow = new MainWindow(null);
            }
            //mainWindow.Show();
        }

        private void killAllServers()
        {
            var mtfServers = System.Diagnostics.Process.GetProcesses().Where(p => p.ProcessName == "MTFServer");
            var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            foreach (var mtfServerProcess in mtfServers)
            {
                if (currentProcess.Id != mtfServerProcess.Id)
                {
                    mtfServerProcess.Kill();
                }
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            SystemLog.LogException(e.Exception);
        }

        void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            SystemLog.LogException(e.Exception);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception)
            {
                SystemLog.LogException(e.ExceptionObject as Exception);
            }
            else
            {
                SystemLog.LogMessage(e.ExceptionObject.ToString());
            }
        }
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("BD39D1D2-BA2F-486A-89B0-B4B0CB466891")]
    internal interface ICLRRuntimeInfo
    {
        void xGetVersionString();
        void xGetRuntimeDirectory();
        void xIsLoaded();
        void xIsLoadable();
        void xLoadErrorString();
        void xLoadLibrary();
        void xGetProcAddress();
        void xGetInterface();
        void xSetDefaultStartupFlags();
        void xGetDefaultStartupFlags();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void BindAsLegacyV2Runtime();
    }
}
