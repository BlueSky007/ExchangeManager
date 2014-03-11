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
            this.ExchangeBlotterDict = new Dictionary<string, ObservableCollection<BlotterSelection>>();
        }

        public Dictionary<string, GroupNetPositionModel> GroupNetPositionModels
        {
            get;
            set;
        }

        public Dictionary<string, ObservableCollection<BlotterSelection>> ExchangeBlotterDict
        {
            get;
            set;
        }

        public GroupNetPositionModel GetGroupNetPositionModel(string exchangeCode)
        {
            return (this.GroupNetPositionModels.ContainsKey(exchangeCode) ? this.GroupNetPositionModels[exchangeCode] : null);
        }

        public ObservableCollection<BlotterSelection> GetBlotterCodeList(string exchangeCode)
        {
            return (this.ExchangeBlotterDict.ContainsKey(exchangeCode) ? this.ExchangeBlotterDict[exchangeCode] : null);
        }
    }
}
