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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Infragistics.Controls.Interactions;
using ManagerConsole.Model;
using System.Configuration;
using Manager.Common;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace ManagerConsole
{
    /// <summary>
    /// SaveLayoutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SaveLayoutWindow : XamDialogWindow
    {
        private ObservableCollection<string> _Layouts;
        private Action<string, Action> _SaveLayout;
        public SaveLayoutWindow()
        {
            InitializeComponent();
            this.layoutName.ItemsSource = _Layouts;
            this.Delete.IsEnabled = false;
        }

        public SaveLayoutWindow(ObservableCollection<string> layouts, Action<string, Action> SaveLayout)
        {
            InitializeComponent();
            this._Layouts = layouts;
            this._SaveLayout = SaveLayout;
            this.layoutName.ItemsSource = _Layouts;
            this.Delete.IsEnabled = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CloseDialog()
        {
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (this.layoutName.Items.Contains(this.layoutName.Text))
            {
                if (MessageBox.Show("是否覆盖所选布局","",MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    this._SaveLayout(this.layoutName.Text, this.CloseDialog);
                }
            }
            else
            {
                this._SaveLayout(this.layoutName.Text, this.CloseDialog);
            }
        }

        private void Deltet_Click(object sender, RoutedEventArgs e)
        {
            if (this.layoutName.SelectedIndex != -1)
            {
                this.layoutName.Items.RemoveAt(this.layoutName.SelectedIndex);
            }
        }

        private void layoutName_Selected(object sender, RoutedEventArgs e)
        {
            this.Delete.IsEnabled = true;
        }
    }
}