using Manager.Common.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for RunSettingTaskDetail.xaml
    /// </summary>
    public partial class RunSettingTaskDetail : UserControl
    {
        public event RoutedEventHandler OnExited;
        private static char[] seperator = new char[] { '\n' };
        public RunSettingTaskDetail(TaskScheduler task)
        {
            InitializeComponent();
            this.LayoutRoot.DataContext = task;

            this.ContentTextBox.Text = this.GetParameterSettings(task);
        }

        private string GetParameterSettings(TaskScheduler task)
        {
            string parameterString = string.Empty;

            foreach (ParameterSettingTask setting in task.ParameterSettings)
            {
                parameterString += setting.ParameterKey + ":" + setting.ParameterValue;
            }
            return parameterString;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.OnExited != null)
            {
                this.OnExited(this, new RoutedEventArgs());
            }
        }
    }
}
