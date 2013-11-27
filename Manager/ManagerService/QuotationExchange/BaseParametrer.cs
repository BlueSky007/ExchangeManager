using System;
using System.Data;

namespace ManagerService.QuotationExchange
{
    public class SystemParameter
    {
        private DateTime tradeDayBeginTime;
        private int mooMocAcceptDuration;
        private int mooMocCancelDuration;

        #region Common public properties definition
        public DateTime TradeDayBeginTime
        {
            get { return this.tradeDayBeginTime; }
        }
        public int MooMocAcceptDuration
        {
            get { return this.mooMocAcceptDuration; }
        }
        public int MooMocCancelDuration
        {
            get { return this.mooMocCancelDuration; }
        }
        #endregion Common public properties definition

        public SystemParameter(DataRow systemParameterRow)
        {
            this.tradeDayBeginTime = (DateTime)systemParameterRow["TradeDayBeginTime"];
            this.mooMocAcceptDuration = (int)systemParameterRow["MooMocAcceptDuration"];
            this.mooMocCancelDuration = (int)systemParameterRow["MooMocCancelDuration"];
        }
    }

    public class TradeDay
    {
        private DateTime day;
        private DateTime beginTime;
        private DateTime endTime;
        private bool isTrading;

        #region Common public properties definition
        public DateTime Day
        {
            get { return this.day; }
        }
        public DateTime BeginTime
        {
            get { return this.beginTime; }
        }
        public DateTime EndTime
        {
            get { return this.endTime; }
        }
        public bool IsTrading
        {
            get { return this.IsTrading; }
        }
        #endregion Common public properties definition

        public TradeDay(DataRow tradeDayRow)
        {
            this.day = (DateTime)tradeDayRow["TradeDay"];
            this.beginTime = (DateTime)tradeDayRow["BeginTime"];
            this.endTime = (DateTime)tradeDayRow["EndTime"];
            this.isTrading = (bool)tradeDayRow["IsTrading"];
        }
    }
}
