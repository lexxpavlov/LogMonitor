using System;
using System.Windows;
using System.Windows.Threading;
using Logging;

namespace LogMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
#if !DEBUG
            DispatcherUnhandledException += Dispatcher_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e == null)
            {
                Application.Current.Shutdown(3);
                return;
            }
            e.Handled = true;
            LogException(e.Exception);
            Application.Current.Shutdown(1);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogException(e.ExceptionObject as Exception);
            if (!e.IsTerminating)
            {
                Application.Current.Shutdown(2);
            }
        }

        private static void LogException(Exception e)
        {
            Logger.LogException(e, Logger.Levels.Fatal);
            MessageBox.Show(e.Message + "\n\nShow more the log file", "An unhandled exception occured!");
        }
    }
}
