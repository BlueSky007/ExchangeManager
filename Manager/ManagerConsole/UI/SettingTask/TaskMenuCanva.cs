using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ManagerConsole.UI.SettingTask
{
    [TemplatePart(Name = "TaskMenuCanva", Type = typeof(ItemsControl))]
    public class TaskMenuCanva:ItemsControl
    {
        public event RoutedEventHandler ItemClicked;
        public TaskMenuCanva()
        { 
            this.DefaultStyleKey = typeof(TaskMenuCanva);
            this.Loaded += new RoutedEventHandler(ExpandMenuCanva_Loaded);
        }

        private StackPanel TaskMenus
        {
            get;
            set;
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (e.NewItems != null && e.NewItems.Count > 0)
            {

            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.TaskMenus = this.GetTemplateChild("ExpandMenuWraPanel") as StackPanel;
        }
        void ExpandMenuCanva_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<TaskMenuItemEntity> taskMenuItems = this.ItemsSource as ObservableCollection<TaskMenuItemEntity>;
            if (taskMenuItems == null) return;
            foreach (TaskMenuItemEntity menuItem in taskMenuItems)
            {
                TaskMenuControl item = new TaskMenuControl();
                int childCount = menuItem.TaskMenuEventItems.Count;
                //item.Height = 25 * childCount + 100;
                item.Height = double.NaN;
                item.Margin = new Thickness(-1);
                item.ItemClicked += new RoutedEventHandler(this.TaskItemEvent);
                item.SetBinding(menuItem);
                this.TaskMenus.Children.Add(item);
            }
        }

        void TaskItemEvent(object sender, RoutedEventArgs e)
        {
            this.ItemClicked(sender, e);
        }
    }
}
