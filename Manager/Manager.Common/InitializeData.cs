using Manager.Common.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class InitializeData
    {
        public Dictionary<string, List<Guid>> ValidAccounts = new Dictionary<string, List<Guid>>();
        public Dictionary<string, List<Guid>> ValidInstruments = new Dictionary<string, List<Guid>>();
        public Dictionary<string, ConfigParameters> SettingParameters = new Dictionary<string, ConfigParameters>();
        public InitializeData()
        { 
        }

        public string ExchangeCode { get; set; }

        public string OrganizationName
        {
            get;
            set;
        }

        public SettingSet SettingSet
        {
            get;
            set;
        }
    }
}
