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
        public delegate void ConfirmDialogResultHandle(bool yesOrNo, TaskScheduler taskScheduler);
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
            this._EditorTaskScheduler = taskScheduler;
            this.Loaded += this.NewTaskWindow_Loaded;
        }

        private void NewTaskWindow_Loaded(object sender,RoutedEventArgs e)
        {
            if (this._EditMode == EditMode.AddNew)
            {
                this._TaskScheduler = new TaskScheduler();
                this._TaskScheduler.IsEditorName = true;
                this.LoadParameterSetting();
            }
            else
            {
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

            ExchangInstrument allInstrument = new ExchangInstrument();
            allInstrument.InstrumentCode = "All";
            foreach (InstrumentClient instrument in this._App.InitDataManager.GetInstruments())
            {
                this._InstrumentList.Add(instrument.ToExchangeInstrument());
            }

            this._InstrumentList.Insert(0, allInstrument);
            this.InstrumentComboBox.ItemsSource = this._InstrumentList;
            this.InstrumentComboBox.DisplayMemberPath = "Code";
            this.InstrumentComboBox.SelectedIndex = 0;
            this.InstrumentComboBox.SelectedValuePath = "Id";
            this.InstrumentComboBox.SelectedItem = allInstrument;

            this._InstrumentComboBox.ItemsSource = this._InstrumentList;
            this._InstrumentComboBox.DisplayMemberPath = "InstrumentCode";

            this.ExchangeComboBox.ItemsSource = this._App.InitDataManager.ExchangeCodes;
            this.ExchangeComboBox.SelectedIndex = 0;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if(!this.CheckData())return;
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
            TaskScheduler taskEntity = this._EditMode == EditMode.AddNew ? this._TaskScheduler : this._EditorTaskScheduler;
            ObservableCollection<ExchangInstrument> instruments = this.GetExchangeInstruments();
            taskEntity.ExchangInstruments = instruments;
            taskEntity.CreateDateTime = DateTime.Now;
            CommonTaskScheduler taskScheduler = taskEntity.ToCommonTaskScheduler();
            ConsoleClient.Instance.CreateTaskScheduler(taskScheduler, this.CreateTaskSchedulerCallback);
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

        private void SettingsTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this._IsLoaded) return;
            SettingParameterType settingType = (SettingParameterType)Enum.ToObject(typeof(SettingParameterType), this.SettingsTypeComboBox.SelectedIndex);

            //if (settingType == SettingParameterType.All)
            //{
            //    this._ParameterSettingGrid.ItemsSource = this._TaskScheduler.ParameterSettings;
            //}
            //else
            //{
            //    this._ParameterSettingGrid.ItemsSource = this._TaskScheduler.ParameterSettings.Where(P => P.SettingParameterType == settingType);
            //}

            if (settingType == SettingParameterType.InstrumentParameter || settingType == SettingParameterType.All)
            {
                this.ExchangeCodeCaption.Visibility = Visibility.Visible;
                this.ExchangeComboBox.Visibility = Visibility.Visible;
            }
            else
            {
                this.ExchangeCodeCaption.Visibility = Visibility.Collapsed;
                this.ExchangeComboBox.Visibility = Visibility.Collapsed;
            }
            this.FilterSettingParameter(settingType);
        }


        private void InstrumentComboBox_SelectionChanged(object sender, EventArgs e)
        {
            ObservableCollection<ExchangInstrument> instruments = this.GetExchangeInstruments();
        }

        private void FilterSettingParameter(SettingParameterType settingType)
        {
            if (settingType == SettingParameterType.All)
            {
                this.FilterOrderTask(this._ParameterSettingGrid, "All", "SettingParameterType", ComparisonOperator.NotEquals);
            }
            else
            {
                this.FilterOrderTask(this._ParameterSettingGrid, settingType.ToString(), "SettingParameterType", ComparisonOperator.Equals);
            }
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
    }
}
