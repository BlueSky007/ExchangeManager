using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Manager.Common;

namespace ManagerService
{
    public partial class ManagerService : ServiceBase
    {
        private MainService _Manager;

        public ManagerService()
        {
            InitializeComponent();
            this._Manager = new MainService();
        }

#if(DEBUG)
        public void Start()
        {
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
