using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ManagerConsole.UI.SettingTask
{
    [TemplatePart(Name = "TaskMenuContentCanva", Type = typeof(ItemsControl))]
    public class TaskMenuContentCanva:ItemsControl
    {
        public event RoutedEventHandler ItemClicked;
        public TaskMenuContentCanva()
        {
            this.DefaultStyleKey = typeof(TaskMenuContentCanva);
            this.Loaded += new System.Windows.RoutedEventHandler(TaskMenuContentCanva_OnLoad);
        }

        private StackPanel MenuEventItems
        {
            get;
            set;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.MenuEventItems = this.GetTemplateChild("ExpandMenuActionWraPanel") as StackPanel;
        }

        void TaskMenuContentCanva_OnLoad(object sender, RoutedEventArgs e)
        {
            ObservableCollection<TaskMenuEventItem> items = this.ItemsSource as ObservableCollection<TaskMenuEventItem>;
            if (items == null || this.MenuEventItems == null || this.MenuEventItems.Children.Count > 0) return;
            foreach (TaskMenuEventItem item in items)
            {
                TaskMenuContentControl control = new TaskMenuContentControl();
                control.Margin = new Thickness(1);
                control.SetBinding(item);
                control.ItemClicked += new RoutedEventHandler(this.OnTaskActionEvent);
               // control.OnTaskActionEvent += new TaskMenuContentControl.TaskActionEventHandler(this.OnTaskActionEvent);
                this.MenuEventItems.Children.Add(control);
            }
        }

        public void OnTaskActionEvent(object sender, RoutedEventArgs e)
        {
            this.ItemClicked(sender, e);
        }
    }
}
