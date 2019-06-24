using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LogMonitor.Utils
{
    public static class DispatcherHelper
    {
        public static void Invoke(Action action)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(action);
            }
        }

        public static void BeginInvoke(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, action);
        }

        public static void BeginInvoke(Action action, DispatcherPriority priority)
        {
            Application.Current.Dispatcher.BeginInvoke(priority, action);
        }

        public static void InvalidateRequerySuggested()
        {
            Invoke(CommandManager.InvalidateRequerySuggested);
        }
    }
}
