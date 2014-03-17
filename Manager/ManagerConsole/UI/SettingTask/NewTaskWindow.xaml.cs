using Infragistics;
using Infragistics.Controls.Editors;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Interactions;
using Manager.Common;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ExchangInstrument = Manager.Common.Settings.ExchangInstrument;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for NewTaskWindow.xaml
    /// </summary>
    public partial class NewTaskWindow : XamDialogWindow
    {
        public delegate void ConfirmDialogResultHandle(EditMode editMode, TaskScheduler taskScheduler);
        public event ConfirmDialogResultHandle OnConfirmDialogResult;

        private EditMode _EditMode;
        private TaskScheduler _TaskScheduler;
        private TaskScheduler _EditorTaskScheduler;
        private ManagerConsole.MainWindow _App;
        private ObservableCollection<ExchangInstrument> _InstrumentList = new ObservableCollection<ExchangInstrument>();
        private bool _IsLoaded = false;
        public NewTaskWindow(EditMode editMode, TaskScheduler taskScheduler)
        {
            InitializeComponent();
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._EditMode = editMode;

            if (this._EditMode == EditMode.AddNew)
            {
                this._TaskScheduler = new TaskScheduler();
                this._TaskScheduler.IsEditorName = true;
                this.LoadParameterSetting();
            }
            else
            {
                this._EditorTaskScheduler = taskScheduler.Clone();
                this.SettingsUIBinding();
            }
            this._IsLoaded = true;
        }

        private void LoadParameterSetting()
        {
            ConsoleClient.Instance.LoadParameterDefine(this.LoadParameterDefineCallback);
        }

        private void SettingsUIBinding()
        {
            if (this._EditMode == EditMode.AddNew)
            {
                this.LayOutGrid.DataContext = this._TaskScheduler;
                this._ParameterSettingGrid.ItemsSource = this._TaskScheduler.ParameterSettings;
            }
            else
            {
                this._EditorTaskScheduler.IsEditorName = false;
                this.LayOutGrid.DataContext = this._EditorTaskScheduler;
                this._ParameterSettingGrid.ItemsSource = this._EditorTaskScheduler.ParameterSettings;
                foreach (ExchangInstrument exchangeInstrument in this._EditorTaskScheduler.ExchangInstruments)
                {
                    this._InstrumentComboBox.SelectedItems.Add(exchangeInstrument);
                }
            }

            this.SetBindingComboBox();
        }

        private void ConvertToParameterSetting(List<CommonParameterDefine> parameterDefines)
        {
            foreach (CommonParameterDefine entity in parameterDefines)
            {
                if (entity.SettingParameterType == SettingParameterType.InstrumentParameter)
                {
                    ParameterSetting parameterSetting = new ParameterSetting(entity);
                    this._TaskScheduler.ParameterSettings.Add(parameterSetting);
                }
            }
        }

        private void SetBindingComboBox()
        {
            this._ActionTimePicker.Value = DateTime.Now;
            this.TaskTypeComboBox.ItemsSource = System.Enum.GetNames(typeof(TaskType));
            this.TaskTypeComboBox.SelectedIndex = 0;

            this.ActionTypeComboBox.ItemsSource = System.Enum.GetNames(typeof(ActionType));
            this.ActionTypeComboBox.SelectedIndex = 3;

            this.SettingsTypeComboBox.ItemsSource = System.Enum.GetNames(typeof(SettingTaskType));
            this.SettingsTypeComboBox.SelectedIndex = 0;

            ExchangInstrument allInstrument = new ExchangInstrument();
            allInstrument.InstrumentCode = "All";

            foreach (string exchangeCode in this._App.ExchangeDataManager.ExchangeCodes)
            {
                ExchangeSettingManager settingManager = this._App.ExchangeDataManager.GetExchangeSetting(exchangeCode);
                foreach (InstrumentClient instrument in settingManager.Instruments.Values)
                {
                    this._InstrumentList.Add(instrument.ToExchangeInstrument());
                }
            }

           // this._InstrumentList.Insert(0, allInstrument);

            this._InstrumentComboBox.ItemsSource = this._InstrumentList;
            this._InstrumentComboBox.DisplayMemberPath = "InstrumentCode";

            this.ExchangeComboBox.ItemsSource = this._App.ExchangeDataManager.ExchangeCodes;
            this.ExchangeComboBox.SelectedItem = this._App.ExchangeDataManager.ExchangeCodes[0];
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if(!this.CheckData())return;
            this.SaveTaskScheduler();
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool CheckData()
        {
            TaskScheduler taskEntity = this._EditMode == EditMode.AddNew ? this._TaskScheduler : this._EditorTaskScheduler;
            if (string.IsNullOrEmpty(taskEntity.Name))
            {
                this._App._CommonDialogWin.ShowDialogWin("Task Name not empty!", "Alert");
                return false;
            }
            if (taskEntity.RunTime < DateTime.Now.AddSeconds(10))
            {
                this._App._CommonDialogWin.ShowDialogWin("Task runTime must more than now 10s!", "Alert");
                return false;
            }
            return true;
        }

        private void SaveTaskScheduler()
        {
            string exchangeCode = this.ExchangeComboBox.SelectedItem.ToString();
            TaskScheduler taskEntity = this._EditMode == EditMode.AddNew ? this._TaskScheduler : this._EditorTaskScheduler;
            ObservableCollection<ExchangInstrument> instruments = this.GetExchangeInstruments();
            taskEntity.ExchangInstruments = instruments;
            taskEntity.CreateDateTime = DateTime.Now;
            taskEntity.ExchangeCode = exchangeCode;
            CommonTaskScheduler taskScheduler = taskEntity.ToCommonTaskScheduler();
            if (this._EditMode == EditMode.AddNew)
            {
                ConsoleClient.Instance.CreateTaskScheduler(taskScheduler, this.CreateTaskSchedulerCallback);
            }
            else
            {
                ConsoleClient.Instance.EditorTaskScheduler(taskScheduler, this.EditorTaskSchedulerCallback);
            }
        }

        private ObservableCollection<ExchangInstrument> GetExchangeInstruments()
        {
            ObservableCollection<ExchangInstrument> instruments = new ObservableCollection<ExchangInstrument>();
            foreach (ExchangInstrument item in this._InstrumentComboBox.SelectedItems)
            {
                instruments.Add(item);
            }
            return instruments;
        }

        #region Callback
        private void LoadParameterDefineCallback(List<CommonParameterDefine> parameterDefines)
        {
            this.Dispatcher.BeginInvoke((Action<List<CommonParameterDefine>>)delegate(List<CommonParameterDefine> result)
            {
                this.ConvertToParameterSetting(parameterDefines);
                this.SettingsUIBinding();
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
                        this.OnConfirmDialogResult(EditMode.AddNew, this._TaskScheduler);
                    }
                }
                else
                {
                    this._App._CommonDialogWin.ShowDialogWin("Create task failed!","Error");
                }
            }, isSucceed);
        }

        private void EditorTaskSchedulerCallback(bool isSucceed)
        {
            this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool result)
            {
                if (result)
                {
                    this._App._CommonDialogWin.ShowDialogWin("Editor task Succeed!", "Manager");
                    if (this.OnConfirmDialogResult != null)
                    {
                        this.OnConfirmDialogResult(EditMode.Modify, this._EditorTaskScheduler);
                    }
                }
                else
                {
                    this._App._CommonDialogWin.ShowDialogWin("Create task failed!", "Error");
                }
            }, isSucceed);
        }
        
        #endregion

        private void SettingsTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this._IsLoaded) return;
            SettingTaskType settingType = (SettingTaskType)Enum.ToObject(typeof(SettingTaskType), this.SettingsTypeComboBox.SelectedIndex);

            //if (settingType == SettingTaskType.InstrumentParameter)
            //{
            //    this.ExchangeCodeCaption.Visibility = Visibility.Visible;
            //    this.ExchangeComboBox.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    this.ExchangeCodeCaption.Visibility = Visibility.Collapsed;
            //    this.ExchangeComboBox.Visibility = Visibility.Collapsed;
            //}
            this.FilterSettingParameter(settingType);
        }


        private void InstrumentComboBox_SelectionChanged(object sender, EventArgs e)
        {
            if (!this._IsLoaded) return;
            ExchangInstrument exchangInstrument = this._InstrumentComboBox.SelectedItem as ExchangInstrument;
            var item = this._InstrumentComboBox.SelectedIndex;
            var item1 = this._InstrumentComboBox.Tag;
            var item2 = this._InstrumentComboBox.SelectedItems;
            ObservableCollection<ExchangInstrument> instruments = this.GetExchangeInstruments();   
        }

        private void FilterSettingParameter(SettingTaskType settingType)
        {
            //this.FilterOrderTask(this._ParameterSettingGrid, settingType.ToString(), "SettingTaskType", ComparisonOperator.Equals);
        }

        private void FilterOrderTask(XamGrid grid,string filterValue, string columnName, ComparisonOperator comparisonOperator)
        {
            grid.FilteringSettings.AllowFiltering = Infragistics.Controls.Grids.FilterUIType.FilterMenu;
            grid.FilteringSettings.FilteringScope = FilteringScope.ColumnLayout;

            grid.Columns.DataColumns[columnName].FilterColumnSettings.FilterCellValue = filterValue;
            foreach (FilterOperand f in grid.Columns.DataColumns[columnName].FilterColumnSettings.RowFilterOperands)
            {
                if (f.ComparisonOperatorValue == comparisonOperator)
                {
                    grid.Columns.DataColumns[columnName].FilterColumnSettings.FilteringOperand = f;
                    break;
                }
            }
        }

        private void ActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this._IsLoaded) return;
            ActionType actionType = (ActionType)Enum.ToObject(typeof(ActionType), this.ActionTypeComboBox.SelectedIndex);

            if (actionType == ActionType.Daily)
            {
                this.DailyTypeBorder.Visibility = Visibility.Visible;
                this.WeeklyTypeBorder.Visibility = Visibility.Collapsed;
            }
            else if (actionType == ActionType.Weekly)
            {
                this.DailyTypeBorder.Visibility = Visibility.Collapsed;
                this.WeeklyTypeBorder.Visibility = Visibility.Visible;
            }
            else
            {
                this.DailyTypeBorder.Visibility = Visibility.Collapsed;
                this.WeeklyTypeBorder.Visibility = Visibility.Collapsed;
            }
        }

    }
}
