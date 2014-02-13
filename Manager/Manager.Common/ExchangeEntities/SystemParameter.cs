using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manager.Common.Settings
{
    public class SystemParameter
    {
        public bool? IsCustomerVisibleToDealer { get; set; }

        public bool CanDealerViewAccountInfo { get; set; }

        public bool? DealerUsingAccountPermission { get; set; }

        public System.Int32 MooMocAcceptDuration { get; set; }

        public System.Int32 MooMocCancelDuration { get; set; }

        public Guid? QuotePolicyDetailID { get; set; }

        public System.Int32 LotDecimal { get; set; }

        public System.Int32 _EnquiryOutTimeSeconds;

        public bool AutoConfirmOrder { get; set; }

        public bool ConfirmRejectDQOrder { get; set; } //WebConfig
    }
}
