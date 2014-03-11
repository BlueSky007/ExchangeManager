using Manager.Common.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common
{
    public class InitializeData
    {
        public ConfigParameter ConfigParameter;
        public ExchangeInitializeData[] ExchangeInitializeDatas;
    }
    public class ExchangeInitializeData
    {
        //public Dictionary<string, List<Guid>> ValidAccounts = new Dictionary<string, List<Guid>>();
        //public Dictionary<string, List<Guid>> ValidInstruments = new Dictionary<string, List<Guid>>();
        //public Dictionary<string, ConfigParameters> SettingParameters = new Dictionary<string, ConfigParameters>();
        public string ExchangeCode;
        public SettingSet SettingSet;
    }
}
