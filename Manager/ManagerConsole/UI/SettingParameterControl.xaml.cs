using Infragistics.Controls.Grids;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using SettingParameterType = Manager.Common.SettingParameterType;
using UserData = Manager.Common.UserData;
using CommnSoundSetting = Manager.Common.Settings.SoundSetting;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for SettingParameterControl.xaml
    /// </summary>
    public partial class SettingParameterControl : UserControl
    {
        private MainWindow _App;
        private SettingsParameterManager _SettingsParameter;
        private MediaElement _MediaElement;
        private List<UserData> _UserList = new List<UserData>();
        public SettingParameterControl()
        {
            InitializeComponent();
            this._App = (MainWindow)Application.Current.MainWindow;
            this._MediaElement = this._App._Media;

            this.GetUserData();

            Thread thread = new Thread(new ThreadStart(delegate()
            {
                while (!this.InilizeUI())
                {
                    Thread.Sleep(800);
                }
            }));
            thread.IsBackground = true;
            thread.Start();
           
        }

        private void BindingUI()
        {
            this.UserListComboBox.ItemsSource = this._UserList;
            this.UserListComboBox.DisplayMemberPath = "UserName";
            this.UserListComboBox.SelectedIndex = 0;
        }

        private void GetUserData()
        {
            ConsoleClient.Instance.GetUserData(GetUserDataCallback);
        }

        

        private bool InilizeUI()
        {
            if (this._App.ExchangeDataManager.IsInitializeCompleted)
            {
                this.Dispatcher.BeginInvoke((Action)delegate()
                {
                    this.BindingData();
                });
                return true;
            }
            else
            {
                return false;
            }
        }

        private void BindingData()
        {
            this._SettingsParameter = this._App.ExchangeDataManager.SettingsParameterManager;
            this.DealingOrderParameterGroup.DataContext = this._SettingsParameter.DealingOrderParameter;
            this.SetValueParameterGroup.DataContext = this._SettingsParameter.SetValueSetting;
            this._SoundSettingGrid.ItemsSource = this._SettingsParameter.SoundSettings;
        }

        private void UpdateCopySoundSettings(List<CommnSoundSetting> newSoundSettings)
        {
            this._SoundSettingGrid.ItemsSource = null;
            this._App.ExchangeDataManager.SettingsParameterManager.UpdateCopySoundSettings(newSoundSettings);
            this.BindingData();
        }

        #region Update Event
        private void CopySoundSettingBtn_Click(object sender, RoutedEventArgs e)
        {
            UserData userData = this.UserListComboBox.SelectedItem as UserData;
            if (userData == null) return;
            Guid copyFromUserId = userData.UserId;

            ConsoleClient.Instance.CopyFromSetting(copyFromUserId, this.CopyFromSettingCallback);
        }

        private void CopyFromSettingCallback(List<CommnSoundSetting> newSoundSettings)
        {
            this.Dispatcher.BeginInvoke((Action<List<CommnSoundSetting>>)delegate(List<CommnSoundSetting> soundSettings) 
            {
                if (soundSettings.Count > 0)
                {
                    this._App._CommonDialogWin.ShowDialogWin("Copy settings successfully.", "Manager");
                    this.UpdateCopySoundSettings(soundSettings);
                }
            }, newSoundSettings);
        }

        private void GetUserDataCallback(List<UserData> userList)
        {
            this.Dispatcher.BeginInvoke((Action<List<UserData>>)delegate(List<UserData> userData)
            {
                this._UserList = userData;
                UserData currentUser = this._UserList.SingleOrDefault(P => P.UserId == ConsoleClient.Instance.user.UserId);
                this._UserList.Remove(currentUser);
                this.BindingUI();

            }, userList);
        }

        private void SetSoundPathButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Button btn = sender as Button;
            SoundSetting soundSetting = ((UnboundColumnDataContext)btn.DataContext).RowData as SoundSetting;
            //soundSetting.SoundPathFontColor = new SolidColorBrush(Colors.Black);

            if (soundSetting == null) return;

            var dialog = new OpenFileDialog();

            dialog.Title = "Sound Setting";
            dialog.Filter = "All Files|*.wav";

            bool? isOpen = dialog.ShowDialog();

            if (isOpen.HasValue && isOpen.Value)
            {
                soundSetting.SoundPath = dialog.FileName;
            }
        }

        private void TestSoundButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Button btn = sender as Button;
            SoundSetting soundSetting = ((UnboundColumnDataContext)btn.DataContext).RowData as SoundSetting;

            if (soundSetting == null) return;

            string path = soundSetting.SoundPath;
            MediaManager.PlayMedia(this._MediaElement, path);
        }

        private void ApplySoundButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this._SettingsParameter.GetUpdateSoundPathFileds();
            if (this._SettingsParameter.UpdateSoundPathFileds.Count == 0) return;
            ConsoleClient.Instance.UpdateManagerSettings(this._SettingsParameter.SettingId,SettingParameterType.SoundParameter, this._SettingsParameter.UpdateSoundPathFileds, delegate(bool success)
            {
                this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool updated)
                {
                    if (updated)
                    {
                        this._App._CommonDialogWin.ShowDialogWin("Update Sound parameter settings successfully.", "Manager");
                    }
                    else
                    {
                        this._App._CommonDialogWin.ShowDialogWin("Update Sound parameter settings failed.", "Manager");
                    }
                }, success);
            });
        }

        private void ApplyParameterSettingBtn_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this._SettingsParameter.DealingOrderParameter.GetUpdateDealingOrderParameters();
            if (this._SettingsParameter.DealingOrderParameter.UpdateDealingParameterFileds.Count == 0) return;
            ConsoleClient.Instance.UpdateManagerSettings(this._SettingsParameter.SettingId,SettingParameterType.DealingOrderParameter, this._SettingsParameter.DealingOrderParameter.UpdateDealingParameterFileds, delegate(bool success)
            {
                this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool updated)
                {
                    if (updated)
                    {
                        this._App._CommonDialogWin.ShowDialogWin("Update Dealing order parameter settings successfully.", "Manager");
                        this._SettingsParameter.DealingOrderParameter.UpdateOrigin();
                    }
                    else
                    {
                        this._App._CommonDialogWin.ShowDialogWin("Update Dealing order parameter settings failed.", "Manager");
                    }
                }, success);
            });
        }

        private void SetValueApplyButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this._SettingsParameter.SetValueSetting.GetUpdateSetValueSettingFileds();
            if (this._SettingsParameter.SetValueSetting.UpdateSetValueParameterFileds.Count == 0) return;
            ConsoleClient.Instance.UpdateManagerSettings(this._SettingsParameter.SettingId, SettingParameterType.SetValueParameter, this._SettingsParameter.SetValueSetting.UpdateSetValueParameterFileds, delegate(bool success)
            {
                this.Dispatcher.BeginInvoke((Action<bool>)delegate(bool updated)
                {
                    if (updated)
                    {
                        this._App._CommonDialogWin.ShowDialogWin("Update SetValue parameter settings successfully.", "Manager");
                        this._SettingsParameter.SetValueSetting.UpdateOrigin();
                    }
                    else
                    {
                        this._App._CommonDialogWin.ShowDialogWin("Update SetValue parameter settings failed.", "Manager");
                    }
                }, success);
            });
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this._SoundSettingGrid.ItemsSource = null;
            this._SoundSettingGrid.ItemsSource = this._SettingsParameter.SoundSettings;
        }
        #endregion


        #region 布局
        /// <summary>
        /// Layout format:
        /// <GridSettings>
        ///    <GridSetting Name="" ColumnWidth="53,0,194,70,222,60,89,60,80,80,80,70,80,80,80,60,60,59,80,80,80,100,80,150,80,"/>
        /// </GridSettings>

        public string GetLayout()
        {
            //InstrumentCode
            StringBuilder layoutBuilder = new StringBuilder();
            layoutBuilder.Append("<GridSettings>");
            layoutBuilder.Append(ColumnWidthPersistence.GetPersistentColumnsWidthString(this._SoundSettingGrid));
            layoutBuilder.Append("</GridSettings>");
            return layoutBuilder.ToString();
        }

        public void SetLayout(XElement layout)
        {
            try
            {
                if (layout.HasElements)
                {
                    XElement columnWidthElement = layout.Element("GridSettings").Element("ColumnsWidth");
                    if (columnWidthElement != null)
                    {
                        ColumnWidthPersistence.LoadColumnsWidth(this._SoundSettingGrid, columnWidthElement);
                    }
                }
            }
            catch (Exception ex)
            {
                Manager.Common.Logger.AddEvent(System.Diagnostics.TraceEventType.Error, "SettingParameterControl.SetLayout\r\n{0}", ex.ToString());
            }
        }
        #endregion
    }
}
