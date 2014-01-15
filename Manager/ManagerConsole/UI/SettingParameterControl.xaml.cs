using Manager.Common.Settings;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for SettingParameterControl.xaml
    /// </summary>
    public partial class SettingParameterControl : UserControl
    {
        private MainWindow _App;
        private SettingsParameter _SettingsParameter;
        public SettingParameterControl()
        {
            InitializeComponent();
            this._App = (MainWindow)Application.Current.MainWindow;

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

        private bool InilizeUI()
        {
            if (this._App.InitDataManager.IsInitializeCompleted)
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
            this._SettingsParameter = this._App.InitDataManager.SettingsManager.SettingsParameter;
            this.SettingLayOutGrid.DataContext = this._SettingsParameter.ParameterSetting;
        }

        private void OpenFileDialogBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
