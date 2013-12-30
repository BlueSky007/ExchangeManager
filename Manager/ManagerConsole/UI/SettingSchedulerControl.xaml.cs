using Infragistics.Controls.Interactions;
using Infragistics.Windows.OutlookBar;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for SettingSchedulerControl.xaml
    /// </summary>
    public partial class SettingSchedulerControl : UserControl
    {
        private NewTaskWindow _NewTaskWindow;
        private TaskSchedulerModel _TaskSchedulerModel = new TaskSchedulerModel();
        public SettingSchedulerControl()
        {
            InitializeComponent();

            this.Loaded += SettingSchedulerControl_Loaded;
        }

        private void SettingSchedulerControl_Loaded(object sender, RoutedEventArgs e)
        {
            this._SettingSchedulerGrid.ItemsSource = TaskSchedulerModel.Instance.TaskSchedulers;
            this._TaskSchedulerModel = TaskSchedulerModel.Instance;
        }

        private void ActionBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "CreateBtn":
                    this.ShowNewTaskWindow(EditMode.AddNew);
                    break;
                case "EditorBtn":
                    this.ShowNewTaskWindow(EditMode.Modify);
                    break;
                case "DeleteBtn":
                    break;
            }
        }

        private void ShowNewTaskWindow(EditMode ditorMode)
        {
            this._NewTaskWindow = new NewTaskWindow(ditorMode);
            this._NewTaskWindow.OnConfirmDialogResult += new NewTaskWindow.ConfirmDialogResultHandle(this.CreateTaskCompleted);
            App.MainWindow.MainFrame.Children.Add(this._NewTaskWindow);
            this._NewTaskWindow.IsModal = true;
            this._NewTaskWindow.Show();
        }

        private void DeleteTask(object sender, RoutedEventArgs e)
        {
            //Task selectTask = this._T
        }

        private void CreateTaskCompleted(bool yesOrNo,TaskScheduler taskScheduler)
        {
            if (yesOrNo)
            {
                this._TaskSchedulerModel.AddTaskScheduler(taskScheduler);
            }
        }
    }
}
