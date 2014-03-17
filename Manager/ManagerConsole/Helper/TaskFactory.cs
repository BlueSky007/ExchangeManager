using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagerConsole.Helper
{
    public class TaskFactory
    {
        private bool IsInitializeCompleted;
        public TaskFactory(ExchangeDataManager exchangeDataManager)
        {
            this.IsInitializeCompleted = exchangeDataManager.IsInitializeCompleted;
        }
        public void TaskWait()
        {
            Task task = new Task(() =>
            {
                
            });

            task.Start();
            task.Wait();
        }
    }
}
