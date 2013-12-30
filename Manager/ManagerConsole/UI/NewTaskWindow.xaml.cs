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
using CommonParameterDefine = Manager.Common.Settings.ParameterDefine;
using CommonTaskScheduler = Manager.Common.Settings.TaskScheduler;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for NewTaskWindow.xaml
    /// </summary>
    public partial class NewTaskWindow : XamDialogWindow
    {
        public delegate void ConfirmDialogResultHandle(bool yesOrNo, TaskScheduler taskScheduler);
        public event ConfirmDialogResultHandle OnConfirmDialogResult;

        private EditMode _EditMode;
        private TaskScheduler _TaskScheduler;
        private ManagerConsole.MainWindow _App;
        public NewTaskWindow(EditMode editMode)
        {
            InitializeComponent();
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._EditMode = editMode;
            this.Loaded += this.NewTaskWindow_Loaded;
        }

        private void NewTaskWindow_Loaded(object sender,RoutedEventArgs e)
        {
            if (this._EditMode == EditMode.AddNew)
            {
                this._TaskScheduler = new TaskScheduler();
                this.LoadParameterSetting();
            }
        }

        private void LoadParameterSetting()
        {
            ConsoleClient.Instance.LoadParameterDefine(this.LoadParameterDefineCallback);
        }

        private void SetBinding()
        {
            if (this._EditMode == EditMode.AddNew)
            {
                this.LayOutGrid.DataContext = this._TaskScheduler;
                this._ParameterSettingGrid.ItemsSource = this._TaskScheduler.ParameterSettings;
            }
            else
            { 
            }

            this.SetBindingComboBox();
        }

        private void ConvertToParameterSetting(List<CommonParameterDefine> parameterDefines)
        {
            foreach (CommonParameterDefine entity in parameterDefines)
            {
                ParameterSetting parameterSetting = new ParameterSetting(entity);
                this._TaskScheduler.ParameterSettings.Add(parameterSetting);
            }
        }

        private void SetBindingComboBox()
        {
            this._ActionTimePicker.Value = DateTime.Now;
            this.TaskTypeComboBox.ItemsSource = System.Enum.GetNames(typeof(TaskType));
            this.TaskTypeComboBox.SelectedIndex = 0;

            this.ActionTypeComboBox.ItemsSource = System.Enum.GetNames(typeof(ActionType));
            this.ActionTypeComboBox.SelectedIndex = 3;

            this.SettingsTypeComboBox.ItemsSource = System.Enum.GetNames(typeof(SettingParameterType));
            this.SettingsTypeComboBox.SelectedIndex = 0;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.CheckData(this._TaskScheduler);
            this.SaveTaskScheduler();
            
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.OnConfirmDialogResult != null)
            {
                this.OnConfirmDialogResult(false, null);
            }
            this.Close();
        }

        private void CheckData(TaskScheduler taskEntity)
        {
            if (string.IsNullOrEmpty(taskEntity.Name))
            {
                this._App._CommonDialogWin.ShowDialogWin("Task Name not empty!", "Alert");
                return;
            }
        }

        private void SaveTaskScheduler()
        {
            CommonTaskScheduler taskScheduler = this._TaskScheduler.ToCommonTaskScheduler();
            ConsoleClient.Instance.CreateTaskScheduler(taskScheduler, this.CreateTaskSchedulerCallback);
        }

        #region Callback
        private void LoadParameterDefineCallback(List<CommonParameterDefine> parameterDefines)
        {
            this.Dispatcher.BeginInvoke((Action<List<CommonParameterDefine>>)delegate(List<CommonParameterDefine> result)
            {
                this.ConvertToParameterSetting(parameterDefines);
                this.SetBinding();
            }, parameterDefines);
        }

        private void CreateTaskSchedulerCallback(bool isSucceed)
        {
            this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool result)
            {
                if (result)
                {
                    if (this.OnConfirmDialogResult != null)
                    {
                        this.OnConfirmDialogResult(true, this._TaskScheduler);
                    }
                }
                else
                {
                    this._App._CommonDialogWin.ShowDialogWin("Create task failed!","Error");
                }
            }, isSucceed);
        }
        #endregion
    }
}
