using iExchange.Common;
using ManagerConsole.Helper;
using ManagerConsole.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using CommonOrderQueryEntity = Manager.Common.ReportEntities.OrderQueryEntity;

namespace ManagerConsole.ViewModel
{
    public class OrderQueryEntity
    {
        public OrderQueryEntity(CommonOrderQueryEntity commonEntity)
        {
            this.Intialize(commonEntity);
        }

        public string OrderCode { get; set; }
        public string InstrumentCode { get; set; }
        public string ExchangeCode { get; set; }
        public bool BuySell { get; set; }
        public string OpenClose { get; set; }
        public decimal Lot { get; set; }
        public string AccountCode { get; set; }
        public string SetPrice { get; set; }
        public OrderType OrderType { get; set; }
        public DateTime ExecuteTime { get; set; }
        public string Relation { get; set; }
        public string Dealer { get; set; }

        public string OrderTypeString
        {
            get
            {
                return OrderTypeHelper.GetCaption(this.OrderType);
            }
        }

        public SolidColorBrush IsOpenBrush
        {
            get
            {
                return this.OpenClose == "O" ? SolidColorBrushes.LightBlue : new SolidColorBrush(Colors.Red);
            }
        }
        public SolidColorBrush IsBuyBrush
        {
            get
            {
                return this.BuySell ? SolidColorBrushes.LightBlue : new SolidColorBrush(Colors.Red);
            }
        }

        public string IsBuyString
        {
            get { return this.BuySell ? "B" : "S"; }
        }

        public void Intialize(CommonOrderQueryEntity commonEntity)
        {
            this.OrderCode = commonEntity.OrderCode;
            this.InstrumentCode = commonEntity.InstrumentCode;
            this.ExchangeCode = commonEntity.ExchangeCode;
            this.BuySell = commonEntity.BuySell;
            this.OpenClose = commonEntity.OpenClose;
            this.Lot = commonEntity.Lot;
            this.AccountCode = commonEntity.AccountCode;
            this.SetPrice = commonEntity.SetPrice;
            this.OrderType = commonEntity.OrderType;
            this.ExecuteTime = commonEntity.ExecuteTime;
            this.Relation = commonEntity.Relation;
            this.Dealer = ConsoleClient.Instance.User.UserName;
        }
    }
}
