using Manager.Common.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
{
    public class SettingsManager
    {
        public SettingsParameter SettingsParameter = new SettingsParameter();

        public void InitializeSettingParameter(SettingsParameter settingsParameter)
        {
            this.SettingsParameter = settingsParameter;
        }
    }
}
