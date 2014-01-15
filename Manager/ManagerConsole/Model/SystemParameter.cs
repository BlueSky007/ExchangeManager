using Manager.Common.ExchangeEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonSystemParameter = Manager.Common.Settings.SystemParameter;

namespace ManagerConsole.Model
{
    public class SystemParameter
    {
        public SystemParameter(CommonSystemParameter systemParameter)
        {
            this.Initialize(systemParameter);
        }

        public bool? IsCustomerVisibleToDealer { get; set; }

        public bool CanDealerViewAccountInfo { get; set; }

        public bool? DealerUsingAccountPermission { get; set; }

        public TimeSpan MooMocAcceptDuration { get; set; }

        public TimeSpan MooMocCancelDuration { get; set; }

        public Guid? QuotePolicyDetailID { get; set; }

        public System.Int32 LotDecimal { get; set; }

        public System.Int32 _EnquiryOutTimeSeconds;

        public bool AutoConfirmOrder { get; set; }

        public bool ConfirmRejectDQOrder { get; set; } //WebConfig

        #region Update
        public void Update(Dictionary<string, string> fieldAndValues)
        {
            foreach (string key in fieldAndValues.Keys)
            {
                this.Update(key, fieldAndValues[key]);
            }
        }

        public void Update(string field, string value)
        {
            if (field == ExchangeFieldSR.IsCustomerVisibleToDealer)
            {
                this.IsCustomerVisibleToDealer = bool.Parse(value);
            }
            else if (field == ExchangeFieldSR.CanDealerViewAccountInfo)
            {
                this.CanDealerViewAccountInfo = bool.Parse(value);
            }
            else if (field == ExchangeFieldSR.DealerUsingAccountPermission)
            {
                this.DealerUsingAccountPermission = bool.Parse(value);
            }
            else if (field == ExchangeFieldSR.MooMocAcceptDuration)
            {
                this.MooMocAcceptDuration = TimeSpan.Parse(value);
            }
            else if (field == ExchangeFieldSR.LotDecimal)
            {
                this.LotDecimal = int.Parse(value);
            }
            else if (field == ExchangeFieldSR.MooMocCancelDuration)
            {
                this.MooMocCancelDuration = TimeSpan.Parse(value);
            }
            else if (field == ExchangeFieldSR.QuotePolicyDetailID)
            {
                this.QuotePolicyDetailID = Guid.Parse(value);
            }
        }

        internal void Initialize(CommonSystemParameter systemParameter)
        {
            this.IsCustomerVisibleToDealer = systemParameter.IsCustomerVisibleToDealer;
            this.CanDealerViewAccountInfo = systemParameter.CanDealerViewAccountInfo;
            this.DealerUsingAccountPermission = systemParameter.DealerUsingAccountPermission;
            this.MooMocAcceptDuration = TimeSpan.FromMinutes(systemParameter.MooMocAcceptDuration);
            this.MooMocCancelDuration = TimeSpan.FromMinutes(systemParameter.MooMocCancelDuration);
            this.QuotePolicyDetailID = systemParameter.QuotePolicyDetailID;
            this.LotDecimal = systemParameter.LotDecimal;
            this.AutoConfirmOrder = systemParameter.AutoConfirmOrder;
            this.ConfirmRejectDQOrder = systemParameter.ConfirmRejectDQOrder;
        }
        #endregion
    }

}
