using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
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
using Manager.Common;
using System.IO;

namespace TestConsole.Feeder
{
    /// <summary>
    /// Interaction logic for FeederControl.xaml
    /// </summary>
    public partial class FeederControl : UserControl
    {
        private ObservableCollection<SendTask> _SendTasks;

        public FeederControl()
        {
            InitializeComponent();
            this._SendTasks = this.GetSendTasks();
            this.TasksControl.ItemsSource = this._SendTasks;
        }

        public ObservableCollection<SendTask> SendTasks { get { return this._SendTasks; } }

        private ObservableCollection<SendTask> GetSendTasks()
        {
            try
            {
                ObservableCollection<SendTask> sendTasks = new ObservableCollection<SendTask>();
                string sourcesConfig = ConfigurationManager.AppSettings["Sources"];
                string[] sources = sourcesConfig.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < sources.Length; i++)
                {
                    string config = ConfigurationManager.AppSettings[sources[i] + "_Config"];
                    string[] configItems = config.Split(';');
                    if (!File.Exists(config)) config = string.Empty;
                    sendTasks.Add(new SendTask
                    {
                        IsSelected = true,
                        SourceName = sources[i],
                        LoginName = configItems[0],
                        Password = configItems[1],
                        DataFileName = configItems[2],
                        IsRepeat = false
                    });
                }
                return sendTasks;
            }
            catch (Exception exception)
            {
                Logger.TraceEvent(TraceEventType.Error, "GetSendTasks exception\r\n{0}", exception);
                //MessageBox.Show("GetSendTasks failed");
                return null;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(SendTask sendTask in this._SendTasks)
            {
                if (!File.Exists(sendTask.DataFileName))
                {
                    MessageBox.Show("Quotation File not exists: " + sendTask.DataFileName);
                    return;
                }
            }
            Feeder.Instance.FinishedCallback = this.FinishedCallback;
            Feeder.Instance.Start(this._SendTasks);
            this.StartButton.IsEnabled = false;
        }

        private void FinishedCallback()
        {
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                this.StartButton.IsEnabled = true;
            });
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Feeder.Instance.Stop();
        }
    }
}
