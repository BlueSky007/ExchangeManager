using Infragistics.Controls.Interactions;
using Manager.Common;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using WindowState = Infragistics.Controls.Interactions.WindowState;
using TaskScheduler = Manager.Common.Settings.TaskScheduler;

namespace ManagerConsole.UI
{
    public delegate void SettingTaskClickHandler(object sender, TaskScheduler taskScheduler);
    public partial class TaskSchedulerNotify : XamDialogWindow
    {
        public event SettingTaskClickHandler OnSettingTaskClickEvent;

        private DispatcherTimer _AutoCloseTimer;
        private TaskScheduler _TaskScheduler;
        private MainWindow _App;
        public TaskSchedulerNotify()
        {
            InitializeComponent();
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._AutoCloseTimer = new DispatcherTimer();
            this._AutoCloseTimer.Interval = TimeSpan.FromSeconds(20);
            this._AutoCloseTimer.Tick += new EventHandler(delegate(object sender,EventArgs e) 
            {
                this.Close();
            });
            this.AttachEvent();
        }

        internal void SetTaskScheduler(TaskScheduler taskScheduler)
        {
            this._TaskScheduler = taskScheduler;
            this.TaskTitleTextBlock.Text = taskScheduler.Name;
            this._AutoCloseTimer.Start();
        }

        private void AttachEvent()
        {
            ConsoleClient.Instance.MessageClient.OnSettingTaskRunEvent += new MessageClient.SettingTaskRunEventHandler(this.MessageClient_OnSettingTaskRunEvent);
        }

        private void MessageClient_OnSettingTaskRunEvent(UpdateSettingParameterMessage message)
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                this.ShowChatNotifyWindow(message.TaskScheduler);
            });
        }

        protected override void OnWindowStateChanged(WindowState newWindowState,WindowState previousWindowState)
        {
            base.OnWindowStateChanged(newWindowState, previousWindowState);
            if (newWindowState == Infragistics.Controls.Interactions.WindowState.Hidden)
            {
                this._AutoCloseTimer.Stop();
            }
        }
        private void TaskTitleTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
            if (this.OnSettingTaskClickEvent != null)
            {
                this.OnSettingTaskClickEvent(this, this._TaskScheduler);
            }
        }

        private void XamDialogWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        internal void ShowChatNotifyWindow(TaskScheduler taskScheduler)
        {
            this.SetTaskScheduler(taskScheduler);
            this.StartupPosition = StartupPosition.Manual;
            this.Top = this._App.RenderSize.Height - this.Height;
            this.Left = this._App.RenderSize.Width - this.Width;
            this.Visibility = System.Windows.Visibility.Visible;
            this.Show();
            this.BringToFront();
        }

        private void HandleOnSettingTaskRuningEvent(object sender, TaskScheduler taskScheduler)
        {
            //RunSettingTaskDetail control = new RunSettingTaskDetail();
            //XamDialogWindow window = new XamDialogWindow();
            //window.FontSize = 12;
            //window.StartupPosition = StartupPosition.Center;
            //window.IsModal = false;
            //window.Header = "任务详细";
            //window.Width = 600;
            //window.Height = 400;
            //window.Content = control;
            //window.Show();
        }
    }
}
