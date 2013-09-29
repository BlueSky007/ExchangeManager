using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
{
    public class SystemParameterClient : PropertyChangedNotifier
    {
        private Guid? _Id;
        private bool _IsCustomerVisibleToDealer;
        private bool _CanDealerViewAccountInfo;
        private bool _DealerUsingAccountPermission;
        private double _MooMocAcceptDuration;
        private double _MooMocCancelDuration;
        private Guid _QuotePolicyDetailID;
        private int _LotDecimal;

        private System.Int32? _EnquiryOutTimeSeconds;

        #region Public Property 
        public Guid? SystemParameterId
        {
            get { return this._Id; }
            set { this._Id = value; }
        }
        public bool IsCustomerVisibleToDealer
        {
            get { return this._IsCustomerVisibleToDealer; }
            set
            {
                this._IsCustomerVisibleToDealer = value;
                this.OnPropertyChanged("IsCustomerVisibleToDealer");
            }
        }
        public bool CanDealerViewAccountInfo
        {
            get { return this._CanDealerViewAccountInfo; }
            set
            {
                this._CanDealerViewAccountInfo = value;
                this.OnPropertyChanged("IsCustomerVisibleToDealer");
            }
        }
        public bool DealerUsingAccountPermission
        {
            get { return this._DealerUsingAccountPermission; }
            set
            {
                this._DealerUsingAccountPermission = value;
                this.OnPropertyChanged("DealerUsingAccountPermission");
            }
        }
        public double MooMocAcceptDuration
        {
            get { return this._MooMocAcceptDuration; }
            set
            {
                this._MooMocAcceptDuration = value;
                this.OnPropertyChanged("MooMocAcceptDuration");
            }
        }
        public double MooMocCancelDuration
        {
            get { return this._MooMocCancelDuration; }
            set
            {
                this._MooMocCancelDuration = value;
                this.OnPropertyChanged("MooMocCancelDuration");
            }
        }
        public int LotDecimal
        {
            get { return this._LotDecimal; }
            set
            {
                this._LotDecimal = value;
                this.OnPropertyChanged("LotDecimal");
            }
        }
        public System.Int32? EnquiryOutTimeSeconds
        {
            get { return this._EnquiryOutTimeSeconds; }
            set
            {
                if (value < 10 || value > 60)
                {
                    throw (new Exception("invalid range(10~60)"));
                }
                this._EnquiryOutTimeSeconds = value;
                this.OnPropertyChanged("EnquiryOutTimeSeconds");
            }
        }
        #endregion
    }
}
