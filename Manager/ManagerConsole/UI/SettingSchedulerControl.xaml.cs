using Infragistics.Controls.Grids;
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
using CommonTaskScheduler = Manager.Common.Settings.TaskScheduler;
using TaskType = Manager.Common.TaskType;
using ActionType = Manager.Common.ActionType;
using SettingParameterType = Manager.Common.SettingParameterType;
using System.Xml.Linq;


namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for SettingSchedulerControl.xaml
    /// </summary>
    public partial class SettingSchedulerControl : UserControl,IControlLayout
    {
        private NewTaskWindow _NewTaskWindow;
        private TaskSchedulerModel _TaskSchedulerModel = new TaskSchedulerModel();
        private MainWindow _App;
        public SettingSchedulerControl()
        {
            InitializeComponent();
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);

            this.Loaded += SettingSchedulerControl_Loaded;
        }

        private void SettingSchedulerControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetBindingComboBox();
            this._SettingSchedulerGrid.ItemsSource = TaskSchedulerModel.Instance.TaskSchedulers;
            this._TaskSchedulerModel = TaskSchedulerModel.Instance;
        }

        private void SetBindingComboBox()
        {
            this._ActionTimePicker.Value = DateTime.Now;
            this._TaskTypeComboBox.ItemsSource = System.Enum.GetNames(typeof(TaskType));
            this._TaskTypeComboBox.SelectedIndex = 0;

            this._ActionTypeComboBox.ItemsSource = System.Enum.GetNames(typeof(ActionType));
            this._ActionTypeComboBox.SelectedIndex = 3;
        }

        private void RefreshData()
        {
            this._SettingSchedulerGrid.ItemsSource = TaskSchedulerModel.Instance.TaskSchedulers;
            this._TaskSchedulerModel = TaskSchedulerModel.Instance;
        }

        private void SettingSchedulerGrid_RowSelected(object sender, CellClickedEventArgs e)
        {
            TaskScheduler currentTaskScheduler = this._SettingSchedulerGrid.ActiveCell.Row.Data as TaskScheduler;
            this._TaskDetailGrid.DataContext = currentTaskScheduler;
            this._ParameterSettingGrid.ItemsSource = currentTaskScheduler.ParameterSettings;
            this._TaskTypeComboBox.SelectedIndex = (int)currentTaskScheduler.TaskType;
            this._ActionTypeComboBox.SelectedIndex = (int)currentTaskScheduler.ActionType;
        }

        private void SettingSchedulerGrid_CellDoubleClicked(object sender, CellClickedEventArgs e)
        {
            TaskScheduler currentTaskScheduler = e.Cell.Row.Data as TaskScheduler;

            this.ShowNewTaskWindow(EditMode.Modify, currentTaskScheduler);
        }

        private bool CheckActiveRow()
        {
            if (this._SettingSchedulerGrid.ActiveCell == null)
            {
                this._App._CommonDialogWin.ShowDialogWin("No Select row Record!", "Manager");
                return false;
            }
            return true;
        }

        private void ActionBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            TaskScheduler currentTaskScheduler = null;
           
            switch (btn.Name)
            {
                case "CreateBtn":
                    this.ShowNewTaskWindow(EditMode.AddNew,null);
                    break;
                case "EditorBtn":
                    if (!this.CheckActiveRow()) return;
                    currentTaskScheduler = this._SettingSchedulerGrid.ActiveCell.Row.Data as TaskScheduler;
                    if (currentTaskScheduler == null) return;
                    this.ShowNewTaskWindow(EditMode.Modify, currentTaskScheduler);
                    break;
                case "DeleteBtn":
                    if (!this.CheckActiveRow()) return;
                     currentTaskScheduler = this._SettingSchedulerGrid.ActiveCell.Row.Data as TaskScheduler;
                    if (currentTaskScheduler == null) return;
                    this.DeleteTaskScheduler(currentTaskScheduler);
                    break;
                case "RefreshBtn":
                    this.RefreshData();
                    break;
                case "RunTaskBtn":
                    if (!this.CheckActiveRow()) return;
                    currentTaskScheduler = this._SettingSchedulerGrid.ActiveCell.Row.Data as TaskScheduler;
                    if (currentTaskScheduler == null) return;
                    this.StartRunTaskScheduler(currentTaskScheduler);
                    break;
                case "DisableTaskBtn":
                    if (!this.CheckActiveRow()) return;
                    currentTaskScheduler = this._SettingSchedulerGrid.ActiveCell.Row.Data as TaskScheduler;
                    if (currentTaskScheduler == null) return;
                    this.DisableTaskScheduler(currentTaskScheduler);
                    this.HiddenActionMenuItem(true);
                    break;
                case "EnableTaskBtn":
                    if (!this.CheckActiveRow()) return;
                    currentTaskScheduler = this._SettingSchedulerGrid.ActiveCell.Row.Data as TaskScheduler;
                    if (currentTaskScheduler == null) return;
                    this.EnableTaskScheduler(currentTaskScheduler);
                    this.HiddenActionMenuItem(false);
                    break;
                case "StopBtn":
                    if (!this.CheckActiveRow()) return;
                    currentTaskScheduler = this._SettingSchedulerGrid.ActiveCell.Row.Data as TaskScheduler;
                    if (currentTaskScheduler == null) return;
                    this.EnableTaskScheduler(currentTaskScheduler);
                    this.HiddenActionMenuItem(false);
                    break;
                default:
                    break;
            }
        }

        private void HiddenActionMenuItem(bool isDisable)
        {
            if (isDisable)
            {
                this.DisableTaskBtn.Visibility = Visibility.Collapsed;
                this.RunTaskBtn.Visibility = Visibility.Collapsed;
                this.StopBtn.Visibility = Visibility.Collapsed;
                this.EnableTaskBtn.Visibility = Visibility.Visible;
            }
            else
            {
                this.DisableTaskBtn.Visibility = Visibility.Visible;
                this.RunTaskBtn.Visibility = Visibility.Visible;
                this.StopBtn.Visibility = Visibility.Visible;
                this.EnableTaskBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowNewTaskWindow(EditMode ditorMode,TaskScheduler taskScheduler)
        {
            this._NewTaskWindow = new NewTaskWindow(ditorMode, taskScheduler);
            this._NewTaskWindow.OnConfirmDialogResult += new NewTaskWindow.ConfirmDialogResultHandle(this.CreateTaskCompleted);
            App.MainFrameWindow.MainFrame.Children.Add(this._NewTaskWindow);
            this._NewTaskWindow.IsModal = true;
            this._NewTaskWindow.Show();
        }

        private void DeleteTaskScheduler(TaskScheduler taskScheduler)
        {
            CommonTaskScheduler commonTaskScheduler = taskScheduler.ToCommonTaskScheduler();
            ConsoleClient.Instance.DeleteTaskScheduler(commonTaskScheduler, this.DeleteTaskSchedulerCallback);
        }

        private void StartRunTaskScheduler(TaskScheduler taskScheduler)
        {
            CommonTaskScheduler commonTaskScheduler = taskScheduler.ToCommonTaskScheduler();
            ConsoleClient.Instance.StartRunTaskScheduler(commonTaskScheduler, this.StartRunTaskCallback);
        }

        private void DisableTaskScheduler(TaskScheduler taskScheduler)
        {
            this._TaskSchedulerModel.ChangeTaskStatus(taskScheduler,true);
            CommonTaskScheduler commonTaskScheduler = taskScheduler.ToCommonTaskScheduler();
            ConsoleClient.Instance.DeleteTaskScheduler(commonTaskScheduler,this.DeleteTaskSchedulerCallback);
        }

        private void EnableTaskScheduler(TaskScheduler taskScheduler)
        {
            this._TaskSchedulerModel.ChangeTaskStatus(taskScheduler,false);
            CommonTaskScheduler commonTaskScheduler = taskScheduler.ToCommonTaskScheduler();
            ConsoleClient.Instance.EnableTaskScheduler(commonTaskScheduler);
        }

        private void StartRunTaskCallback()
        {
            this._App._CommonDialogWin.ShowDialogWin("Run Task succeed!", "Manager");
        }

        private void DeleteTaskSchedulerCallback(Guid taskSchedulerId, bool result)
        {
            if (result)
            {
                this._TaskSchedulerModel.RemoveTaskScheduler(taskSchedulerId);
            }
        }

        private void CreateTaskCompleted(EditMode editMode,TaskScheduler taskScheduler)
        {
            if (editMode == EditMode.AddNew)
            {
                this._TaskSchedulerModel.AddTaskScheduler(taskScheduler);
            }
            else
            {
                this._TaskSchedulerModel.UpdateTaskScheduler(taskScheduler);
            }
        }

        #region 布局
        /// <summary>
        /// Layout format:
        /// <GridSettings>
        ///    <GridSetting Name="" ColumnWidth="53,0,194,70,222,60,89,60,80,80,80,70,80,80,80,60,60,59,80,80,80,100,80,150,80,"/>
        /// </GridSettings>

        public string GetLayout()
        {
            StringBuilder layoutBuilder = new StringBuilder();
            layoutBuilder.Append("<GridSettings>");
            layoutBuilder.Append(ColumnWidthPersistence.GetGridColumnsWidthString(this._SettingSchedulerGrid));
            layoutBuilder.Append(ColumnWidthPersistence.GetGridColumnsWidthString(this._ParameterSettingGrid));
            layoutBuilder.Append("</GridSettings>");
            return layoutBuilder.ToString();
        }

        public void SetLayout(XElement layout)
        {
            try
            {
                if (layout.HasElements)
                {
                    IEnumerable<XElement> settings = from el in layout.Element("GridSettings").Elements() select el;

                    foreach(XElement setting in settings)
                    {
                        switch (setting.Attribute("Name").Value)
                        {
                            case "_SettingSchedulerGrid":
                                ColumnWidthPersistence.LoadGridColumnsWidth(this._SettingSchedulerGrid, setting);
                                break;
                            case "_ParameterSettingGrid":
                                ColumnWidthPersistence.LoadGridColumnsWidth(this._ParameterSettingGrid, setting);
                                break;
                        }
                    }    
                }
            }
            catch (Exception ex)
            {
                Manager.Common.Logger.AddEvent(System.Diagnostics.TraceEventType.Error, "SettingSchedulerControl.SetLayout\r\n{0}", ex.ToString());
            }
        }
        #endregion
    }
}
