using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Manager.Common;
using System.Diagnostics;

namespace TestConsole
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MainWindow MainFrameWindow;

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.TraceEvent(TraceEventType.Error, "Unhandled Exception:\r\n{0}", e.Exception);
            MessageBox.Show(e.Exception.Message, "Unhandled Exception");
        }
    }
}
