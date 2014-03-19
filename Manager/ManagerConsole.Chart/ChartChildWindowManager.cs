using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ManagerConsole.Chart
{
    public static class ChildWindowManager
    {
        private static List<ChildWindow> _ChildWindows = new List<ChildWindow>();

        public static void Add(ChildWindow childWindow)
        {
            if (!ChildWindowManager._ChildWindows.Contains(childWindow))
            {
                ChildWindowManager._ChildWindows.Add(childWindow);
                childWindow.Closed += new EventHandler(HandleChildWindowClosedEvent);
            }
        }

        static void HandleChildWindowClosedEvent(object sender, EventArgs e)
        {
            ChildWindow childWindow = (ChildWindow)sender;
            ChildWindowManager._ChildWindows.Remove(childWindow);
            try
            {
                if (Application.Current.RootVisual != null)
                {
                    Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
                }
            }
            catch (Exception exception)
            {

            }
        }

        public static void CloseAll()
        {
            foreach (ChildWindow childWindow in ChildWindowManager._ChildWindows.ToArray())
            {
                childWindow.Close();
            }
        }
    }


    public class ChartChildWindowManager : iExchange4.Chart.Silverlight.Interfaces.IChildWindowManager
    {
        void iExchange4.Chart.Silverlight.Interfaces.IChildWindowManager.Add(ChildWindow childWindow)
        {
            ChildWindowManager.Add(childWindow);
        }
    }
}
