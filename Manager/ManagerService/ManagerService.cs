using Manager.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace ManagerService
{
    public partial class ManagerService : ServiceBase
    {
        private MainService _Manager = new MainService();

        public ManagerService()
        {
            InitializeComponent();
        }

#if(DEBUG)
        public void Start()
        {
            Logger.AddEvent(TraceEventType.Information, "Manager started at:{0}, pid:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 10);
            //return;
            this.OnStart(null);
        }
#endif

        protected override void OnStart(string[] args)
        {
            this._Manager.Start();
        }

        protected override void OnStop()
        {
            this._Manager.Stop();
        }
    }
}
