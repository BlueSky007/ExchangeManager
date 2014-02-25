using Manager.Common;
using Manager.Common.LogEntities;
using ManagerService.Console;
using ManagerService.Quotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerService.Audit
{
    public class LogManager
    {
        private LogSourceChange _LogSourceChange;

        private static LogManager _Instance = new LogManager();

        public static LogManager Instance
        {
            get { return LogManager._Instance; }
        }

        private LogManager()
        {
            // audit log default properties
            this._LogSourceChange = new LogSourceChange()
            {
                UserId = Guid.Empty,
                UserName = "System",
                Event = "SourceChange"
            };
        }

        #region Quotation Log
        public LogPrice GetLogPriceEntity(Client client,double ask, double bid, int instrumentId)
        {
            LogEntity logEntity = new LogEntity() { Id = Guid.NewGuid(), UserId = client.userId, UserName = client.user.UserName, IP = client._IP, ExchangeCode = string.Empty, Event = "SendQuotation", Timestamp = DateTime.Now };
            LogPrice logPrice = new LogPrice(logEntity);
            logPrice.InstrumentId = instrumentId;

            logPrice.InstrumentCode = MainService.QuotationManager.ConfigMetadata.Instruments[instrumentId].Code;
            logPrice.OperationType = PriceOperationType.SendPrice;
            string format = string.Format("F{0}", MainService.QuotationManager.ConfigMetadata.Instruments[instrumentId].DecimalPlace);
            logPrice.Ask = ask.ToString(format);
            logPrice.Bid = bid.ToString(format);
            return logPrice;
        }

        public LogPrice GetLogPriceEntity(Client client,int instrumentId, int confirmId, bool accepted, ConfirmResult confirmResult)
        {
            LogEntity logEntity = new LogEntity() { Id = Guid.NewGuid(), UserId = client.userId, UserName = client.user.UserName, IP = client._IP, ExchangeCode = string.Empty, Event = "SendQuotation", Timestamp = DateTime.Now };
            LogPrice logPrice = new LogPrice(logEntity);
            logPrice.InstrumentId = confirmResult.SourceQuotation.InstrumentId;
            logPrice.InstrumentCode = confirmResult.SourceQuotation.InstrumentCode;
            logPrice.OperationType = accepted ? PriceOperationType.OutOfRangeAccept : PriceOperationType.OutOfRangeReject;
            logPrice.OutOfRangeType = confirmResult.SourceQuotation.OutOfRangeType;
            logPrice.Bid = confirmResult.SourceQuotation.PrimitiveQuotation.Ask;
            logPrice.Ask = confirmResult.SourceQuotation.PrimitiveQuotation.Bid;
            logPrice.Diff = confirmResult.SourceQuotation.DiffPoints.ToString();

            return logPrice;
        }

        public LogSourceChange GetLogSourceChangeEntity(int oldSourceId, int newSourceId)
        {
            this._LogSourceChange.Id = Guid.NewGuid();
            this._LogSourceChange.Timestamp = DateTime.Now;
            this._LogSourceChange.FromSourceId = oldSourceId;
            this._LogSourceChange.ToSourceId = newSourceId;
            return this._LogSourceChange;
        }
        #endregion
    }
}
