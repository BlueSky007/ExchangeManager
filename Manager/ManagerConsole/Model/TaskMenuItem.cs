using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace ManagerConsole.Model
{
    public class TaskMenuItemEntity
    {
        public TaskMenuItemEntity()
        {
            this.TaskMenuEventItems = new ObservableCollection<TaskMenuEventItem>();
        }

        public string MenuTitle { get; set; }

        public ObservableCollection<TaskMenuEventItem> TaskMenuEventItems { get; set; }
    }

    public class TaskMenuEventItem
    {
        public TaskEventType ActionType { get; set; }
        public string ActionName { get; set; }
        public BitmapImage ImageUri { get; set; }
    }
}
