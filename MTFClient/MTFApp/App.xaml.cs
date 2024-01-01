using System;
using System.Windows;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;

namespace MTFApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            SystemLog.LogDirectory = BaseConstants.ClientSystemLogsPath;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            FrameworkElement.LanguageProperty.OverrideMetadata(
              typeof(FrameworkElement),
              new FrameworkPropertyMetadata(
                  System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (MTFApp.MainWindow.MTFMutex!=null)
            {
                MTFApp.MainWindow.MTFMutex.ReleaseMutex();
            }
            base.OnExit(e);
        }
    }
}
