using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ManagerConsole.ViewModel
{
    public class ReportDataManager
    {
        private ExchangeDataManager _ExchangeDataManager;

        public ReportDataManager(ExchangeDataManager exchangeDataManager)
        { 
            this._ExchangeDataManager = exchangeDataManager;
            this.GroupNetPositionModels = new Dictionary<string, GroupNetPositionModel>();
        }

        public Dictionary<string, GroupNetPositionModel> GroupNetPositionModels
        {
            get;
            set;
        }

        public void Add

        public GroupNetPositionModel GetGroupNetPositionModel(string exchangeCode)
        {
            return (this.GroupNetPositionModels.ContainsKey(exchangeCode) ? this.GroupNetPositionModels[exchangeCode] : null);
        }
    }
}
