using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonSystemParameter = Manager.Common.SystemParameter;

namespace ManagerConsole.Model
{
    public class SystemParameter
    {
        public SystemParameter(CommonSystemParameter systemParameter)
        {
            this.Update(systemParameter);
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

        

        internal void Update(CommonSystemParameter systemParameter)
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
    }

}
