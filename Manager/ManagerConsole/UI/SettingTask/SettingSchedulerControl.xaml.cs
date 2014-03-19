using Infragistics.Controls.Grids;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CommonTaskScheduler = Manager.Common.Settings.TaskScheduler;
using TaskType = Manager.Common.TaskType;
using ActionType = Manager.Common.ActionType;
using System.Xml.Linq;
using ExchangInstrument = Manager.Common.Settings.ExchangInstrument;


namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for SettingSchedulerControl.xaml
    /// </summary>
    public partial class SettingSchedulerControl : UserControl,IControlLayout
    {
        private NewTaskWindow _NewTaskWindow;
        private TaskSchedulerModel _TaskSchedulerModel = new TaskSchedulerModel();
        private ObservableCollection<TaskMenuItemEntity> _TaskMenuItems = new ObservableCollection<TaskMenuItemEntity>();
        private ObservableCollection<TaskMenuItemEntity> _InstrumentItems = new ObservableCollection<TaskMenuItemEntity>();
        private MainWindow _App;
        private bool _IsInitilizedTaskMenu = false;
        public SettingSchedulerControl()
        {
            InitializeComponent();
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);

            this.Loaded += SettingSchedulerControl_Loaded;

            this.SetBindingComboBox();
            this._SettingSchedulerGrid.ItemsSource = TaskSchedulerModel.Instance.TaskSchedulers;
            this._TaskSchedulerModel = TaskSchedulerModel.Instance;
            if (this._IsInitilizedTaskMenu) return;
            this.IntilizeTaskMenu();
        }

        private void SettingSchedulerControl_Loaded(object sender, RoutedEventArgs e)
        {
            //切换窗体，重新装载
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
            this.ExchangeTextBox.Text = currentTaskScheduler.ExchangeCode;
            //if (currentTaskScheduler.ExchangInstruments.Count > 0)
            //{
            //    this._InstrumentListBox.Visibility = Visibility.Visible;
            //    this.InstrumentCaption.Visibility = Visibility.Visible;
                
            //    this._InstrumentListBox.ItemsSource = currentTaskScheduler.ExchangInstruments;
            //}
            //else
            //{
            //    this._InstrumentListBox.Visibility = Visibility.Collapsed;
            //    this.InstrumentCaption.Visibility = Visibility.Collapsed;
            //}

            this.GetInstrumentList(currentTaskScheduler.ExchangInstruments);
        }

        private void GetInstrumentList(ObservableCollection<ExchangInstrument> exchangeInstruments)
        {
            this.InstrumentList.ItemsSource = null;
            this._InstrumentItems.Clear();
            TaskMenuItemEntity menuItem = new TaskMenuItemEntity();
            menuItem.MenuTitle = "Instrument List";
            foreach (ExchangInstrument item in exchangeInstruments)
            {
                TaskMenuEventItem eventItem = new TaskMenuEventItem() { ExchangeCode = item.ExchangeCode, InstrumentCode = item.InstrumentCode, ImageUri = ImageSources.Instance.EnableTaskImage };
                menuItem.TaskMenuEventItems.Add(eventItem);
            }
            this._InstrumentItems.Add(menuItem);
            this.InstrumentList.ItemsSource = this._InstrumentItems;
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

        private void HiddenActionMenuItem(bool isDisable)
        {
            //if (isDisable)
            //{
            //    this.DisableTaskBtn.Visibility = Visibility.Collapsed;
            //    this.RunTaskBtn.Visibility = Visibility.Collapsed;
            //    this.StopBtn.Visibility = Visibility.Collapsed;
            //    this.EnableTaskBtn.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    this.DisableTaskBtn.Visibility = Visibility.Visible;
            //    this.RunTaskBtn.Visibility = Visibility.Visible;
            //    this.StopBtn.Visibility = Visibility.Visible;
            //    this.EnableTaskBtn.Visibility = Visibility.Collapsed;
            //}
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

        private void IntilizeTaskMenu()
        {
            TaskMenuItemEntity menuItem = new TaskMenuItemEntity();
            menuItem.MenuTitle = "Task Scheduler";
            TaskMenuEventItem eventItem1 = new TaskMenuEventItem() { ActionName = "Create Tasks", ActionType = TaskEventType.CreateTask, ImageUri = ImageSources.Instance.CreateTaskImage };
            TaskMenuEventItem eventItem2 = new TaskMenuEventItem() { ActionName = "Editor Task", ActionType = TaskEventType.EditorTask, ImageUri = ImageSources.Instance.EditorTaskImage };
            TaskMenuEventItem eventItem3 = new TaskMenuEventItem() { ActionName = "Deleted Tasks", ActionType = TaskEventType.DeleteTask, ImageUri = ImageSources.Instance.DeleteTaskImage };
            TaskMenuEventItem eventItem4 = new TaskMenuEventItem() { ActionName = "Refresh Task", ActionType = TaskEventType.RefreshTask, ImageUri = ImageSources.Instance.RefreshTaskImage };
            menuItem.TaskMenuEventItems.Add(eventItem1);
            menuItem.TaskMenuEventItems.Add(eventItem2);
            menuItem.TaskMenuEventItems.Add(eventItem3);
            menuItem.TaskMenuEventItems.Add(eventItem4);

            TaskMenuItemEntity eventMenuItem = new TaskMenuItemEntity();
            eventMenuItem.MenuTitle = "Task Action";
            TaskMenuEventItem eventItem5 = new TaskMenuEventItem() { ActionName = "Run Tasks", ActionType = TaskEventType.RunTask, ImageUri = ImageSources.Instance.RunTaskImage };
            TaskMenuEventItem eventItem6 = new TaskMenuEventItem() { ActionName = "Enabled Tasks", ActionType = TaskEventType.EnableTask, ImageUri = ImageSources.Instance.EnableTaskImage };
            TaskMenuEventItem eventItem7 = new TaskMenuEventItem() { ActionName = "Disabled Task", ActionType = TaskEventType.DisabledTask, ImageUri = ImageSources.Instance.DisabledTaskImage };
            TaskMenuEventItem eventItem8 = new TaskMenuEventItem() { ActionName = "Stop Task", ActionType = TaskEventType.StopTask, ImageUri = ImageSources.Instance.StopTaskImage };
            eventMenuItem.TaskMenuEventItems.Add(eventItem5);
            eventMenuItem.TaskMenuEventItems.Add(eventItem6);
            eventMenuItem.TaskMenuEventItems.Add(eventItem7);
            eventMenuItem.TaskMenuEventItems.Add(eventItem8);

            this._TaskMenuItems.Add(menuItem);
            this._TaskMenuItems.Add(eventMenuItem);
            this.TaskMenuCanva.ItemsSource = this._TaskMenuItems;

            this.TaskMenuCanva.ItemClicked += new RoutedEventHandler(this.ItemClickHandler);
            this._IsInitilizedTaskMenu = true;
        }

        private void ItemClickHandler(object sender, RoutedEventArgs e)
        {
            TaskEventType eventType = (TaskEventType)sender;
            TaskScheduler currentTaskScheduler = null;

            switch (eventType)
            {
                case TaskEventType.CreateTask:
                    this.ShowNewTaskWindow(EditMode.AddNew, null);
                    break;
                case TaskEventType.EditorTask:
                    if (!this.CheckActiveRow()) return;
                    currentTaskScheduler = this._SettingSchedulerGrid.ActiveCell.Row.Data as TaskScheduler;
                    if (currentTaskScheduler == null) return;
                    this.ShowNewTaskWindow(EditMode.Modify, currentTaskScheduler);
                    break;
                case TaskEventType.DeleteTask:
                    if (!this.CheckActiveRow()) return;
                    currentTaskScheduler = this._SettingSchedulerGrid.ActiveCell.Row.Data as TaskScheduler;
                    if (currentTaskScheduler == null) return;
                    this.DeleteTaskScheduler(currentTaskScheduler);
                    break;
                case TaskEventType.RefreshTask:
                    this.RefreshData();
                    break;
                case TaskEventType.RunTask:
                    if (!this.CheckActiveRow()) return;
                    currentTaskScheduler = this._SettingSchedulerGrid.ActiveCell.Row.Data as TaskScheduler;
                    if (currentTaskScheduler == null) return;
                    this.StartRunTaskScheduler(currentTaskScheduler);
                    break;
                case TaskEventType.DisabledTask:
                    if (!this.CheckActiveRow()) return;
                    currentTaskScheduler = this._SettingSchedulerGrid.ActiveCell.Row.Data as TaskScheduler;
                    if (currentTaskScheduler == null) return;
                    this.DisableTaskScheduler(currentTaskScheduler);
                    this.HiddenActionMenuItem(true);
                    break;
                case TaskEventType.EnableTask:
                    if (!this.CheckActiveRow()) return;
                    currentTaskScheduler = this._SettingSchedulerGrid.ActiveCell.Row.Data as TaskScheduler;
                    if (currentTaskScheduler == null) return;
                    this.EnableTaskScheduler(currentTaskScheduler);
                    this.HiddenActionMenuItem(false);
                    break;
                case TaskEventType.StopTask:
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


        #region 布局
        /// <summary>
        /// Layout format:
        /// <GridSettings>
        ///    <GridSetting Name="" ColumnWidth="53,0,194,70,222,60,89,60,80,80,80,70,80,80,80,60,60,59,80,80,80,100,80,150,80,"/>
        /// </GridSettings>

        public string GetLayout()
        {
            return ColumnWidthPersistence.GetGridColumnsWidthString(new List<XamGrid> { this._SettingSchedulerGrid, this._ParameterSettingGrid });
        }

        public void SetLayout(XElement layout)
        {
            try
            {
                ColumnWidthPersistence.LoadGridColumnsWidth(new ObservableCollection<XamGrid> { this._SettingSchedulerGrid, this._ParameterSettingGrid }, layout);
            }
            catch (Exception ex)
            {
                Manager.Common.Logger.AddEvent(System.Diagnostics.TraceEventType.Error, "SettingSchedulerControl.SetLayout\r\n{0}", ex.ToString());
            }
        }
        #endregion
    }
}
