using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace ManagerService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if(DEBUG)
            ManagerService managerService = new ManagerService();
            managerService.Start();
            System.Threading.Thread.Sleep(TimeSpan.FromDays(1));
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new ManagerService() 
			};
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
