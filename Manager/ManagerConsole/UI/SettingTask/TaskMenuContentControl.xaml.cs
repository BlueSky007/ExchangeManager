using ManagerConsole.Model;
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

namespace ManagerConsole.UI.SettingTask
{
    /// <summary>
    /// Interaction logic for TaskMenuContentControl.xaml
    /// </summary>
    public partial class TaskMenuContentControl : UserControl
    {
        public event RoutedEventHandler ItemClicked;
        public TaskMenuContentControl()
        {
            InitializeComponent();
        }

        public void SetBinding(TaskMenuEventItem item)
        {
            this.DataContext = item;
        }

        public void ItemButton_Click(object sender, RoutedEventArgs e)
        {
            TaskMenuEventItem item = this.DataContext as TaskMenuEventItem;
            TaskEventType type = item.ActionType;
            //this.OnTaskActionEvent(type);
            this.ItemClicked(type, new RoutedEventArgs());
        }
    }
}
