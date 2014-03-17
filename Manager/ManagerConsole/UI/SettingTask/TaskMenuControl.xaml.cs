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
    /// Interaction logic for TaskMenuControl.xaml
    /// </summary>
    public partial class TaskMenuControl : UserControl
    {
        //public delegate void TaskActionEventHandler(TaskEventType eventType);
        //public event TaskActionEventHandler OnTaskActionEvent;
        public event RoutedEventHandler ItemClicked;
        public TaskMenuControl()
        {
            InitializeComponent();
        }

        public void SetBinding(TaskMenuItemEntity taskEntity)
        {
            this.DataContext = taskEntity;
            this.TaskMenuItem.SetEventItemBinding(taskEntity);

            this.TaskMenuItem.TaskMenuContentCanva.ItemClicked += new RoutedEventHandler(this.TaskMenuControlEvent);
            this.TaskMenuItem.ExpandEvent += new RoutedEventHandler(this.ExpandItemEvent);
        }

        void TaskMenuControlEvent(object sender, RoutedEventArgs e)
        {
            this.ItemClicked(sender, e);
        }

        void ExpandItemEvent(object sender, RoutedEventArgs e)
        {
            bool isExpand = (bool)sender;

            if (isExpand)
            {
                this.TaskMenuItem.Height = 300;
            }
            else
            {
                this.TaskMenuItem.Height = 25;
            }
        }
    }
}
