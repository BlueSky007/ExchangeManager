using System;
using System.Collections.ObjectModel;
using SoundType = Manager.Common.SoundType;
using CommonSettingsParameter = Manager.Common.Settings.SettingsParameter;
using CommonDealingOrderParameter = Manager.Common.Settings.DealingOrderParameter;
using CommonSetValueSetting = Manager.Common.Settings.SetValueSetting;
using CommonSoundSetting = Manager.Common.Settings.SoundSetting;
using FieldSetting = Manager.Common.Settings.FieldSetting;
using ManagerConsole.Helper;
using System.Collections.Generic;

namespace ManagerConsole.ViewModel
{
    public class SettingsParameterManager
    {
        public Guid SettingId { get; set; }
        public DealingOrderParameter DealingOrderParameter { get; set; }
        public SetValueSetting SetValueSetting { get; set; }
        public ObservableCollection<SoundSetting> SoundSettings { get; set; }

        public Dictionary<string, object> UpdateSoundPathFileds { get; set; }

        public SettingsParameterManager() { }
        public SettingsParameterManager(CommonSettingsParameter commonSettingsParameter)
        {
            this.SettingId = commonSettingsParameter.SettingId;
            this.SoundSettings = new ObservableCollection<SoundSetting>();
            this.UpdateSoundPathFileds = new Dictionary<string, object>();

            this.Initailize(commonSettingsParameter);
        }

        internal  SettingsParameterManager Clone()
        {
            return new SettingsParameterManager()
            {
                DealingOrderParameter = this.DealingOrderParameter.Clone(),
                SetValueSetting = this.SetValueSetting.Clone(),
                SoundSettings = this.GetCloneSoundSettings(),
            };
        }

        private ObservableCollection<SoundSetting> GetCloneSoundSettings()
        {
            ObservableCollection<SoundSetting> cloneSoundSettings = new ObservableCollection<SoundSetting>();
            foreach (SoundSetting setting in this.SoundSettings)
            {
                cloneSoundSettings.Add(setting.Clone());
            }
            return cloneSoundSettings;
        }

        internal void Initailize(CommonSettingsParameter commonSettingsParameter)
        {
            this.DealingOrderParameter = new DealingOrderParameter(commonSettingsParameter.DealingOrderParameter);
            this.SetValueSetting = new SetValueSetting(commonSettingsParameter.SetValueSetting);

            foreach (CommonSoundSetting comonSoundSetting in commonSettingsParameter.SoundSettings)
            {
                SoundSetting soundSetting = new SoundSetting(comonSoundSetting);
                this.SoundSettings.Add(soundSetting);
            }
        }

        public void GetUpdateSoundPathFileds()
        {
            this.UpdateSoundPathFileds.Clear();
            foreach (SoundSetting setting in this.SoundSettings)
            {
                if (setting.SoundPath != setting.OriginSoundSetting.SoundPath)
                {
                    this.UpdateSoundPathFileds.Add(setting.SoundKey, setting.SoundPath);
                }
            }
        }
    }

    public class DealingOrderParameter : PropertyChangedNotifier
    {
        private DealingOrderParameter _OriginDealingOrderParameter;
        public Dictionary<string, object> UpdateDealingParameterFileds { get; set; }

        public DealingOrderParameter() { }

        public DealingOrderParameter(CommonDealingOrderParameter commonTradingParameter)
        {
            this.Initailize(commonTradingParameter);
            this.UpdateDealingParameterFileds = new Dictionary<string, object>();
            this._OriginDealingOrderParameter = this.Clone();
        }

        public DealingOrderParameter OriginDealingOrderParameter
        {
            get
            {
                if (this._OriginDealingOrderParameter == null)
                {
                    this._OriginDealingOrderParameter = this.Clone();
                }

                return this._OriginDealingOrderParameter;
            }
        }

        public Guid SettingDetailId { get; set; }
        public Guid SettingId { get; set; }
        public int InactiveWaitTime { get; set; }
        public int EnquiryWaitTime { get; set; }
        public int PriceOrderSetting { get; set; }
        public bool DisablePopup { get; set; }
        public bool AutoConfirm { get; set; }
        public bool ConfirmRejectDQOrder { get; set; }
        public bool LimitStopSummaryIsTimeRange { get; set; }
        public int LimitStopSummaryTimeRangeValue { get; set; }
        public int LimitStopSummaryPriceRangeValue { get; set; }

        internal void Initailize(CommonDealingOrderParameter commonTradingParameter)
        {
            this.InactiveWaitTime = commonTradingParameter.InactiveWaitTime;
            this.EnquiryWaitTime = commonTradingParameter.EnquiryWaitTime;
            this.PriceOrderSetting = commonTradingParameter.PriceOrderSetting;
            this.DisablePopup = commonTradingParameter.DisablePopup;
            this.AutoConfirm = commonTradingParameter.AutoConfirm;
            this.ConfirmRejectDQOrder = commonTradingParameter.ConfirmRejectDQOrder;
            this.LimitStopSummaryIsTimeRange = commonTradingParameter.LimitStopSummaryIsTimeRange;
            this.LimitStopSummaryTimeRangeValue = commonTradingParameter.LimitStopSummaryTimeRangeValue;
            this.LimitStopSummaryPriceRangeValue = commonTradingParameter.LimitStopSummaryPriceRangeValue;
        }

        public DealingOrderParameter Clone()
        {
            return new DealingOrderParameter()
            {
                SettingDetailId = this.SettingDetailId,
                SettingId = this.SettingId,
                InactiveWaitTime = this.InactiveWaitTime,
                EnquiryWaitTime = this.EnquiryWaitTime,
                PriceOrderSetting = this.PriceOrderSetting,
                DisablePopup = this.DisablePopup,
                AutoConfirm = this.AutoConfirm,
                ConfirmRejectDQOrder = this.ConfirmRejectDQOrder,
                LimitStopSummaryIsTimeRange = this.LimitStopSummaryIsTimeRange,
                LimitStopSummaryTimeRangeValue = this.LimitStopSummaryTimeRangeValue,
                LimitStopSummaryPriceRangeValue = this.LimitStopSummaryPriceRangeValue,
            };
        }

        public void UpdateOrigin()
        {
            this._OriginDealingOrderParameter.Clone();
        }

        public void GetUpdateDealingOrderParameters()
        {
            this.UpdateDealingParameterFileds.Clear();
            if (this.InactiveWaitTime != this.OriginDealingOrderParameter.InactiveWaitTime)
            {
                this.UpdateDealingParameterFileds.Add(FieldSetting.InactiveWaitTime, this.InactiveWaitTime);
            }
            if (this.EnquiryWaitTime != this.OriginDealingOrderParameter.EnquiryWaitTime)
            {
                this.UpdateDealingParameterFileds.Add(FieldSetting.EnquiryWaitTime, this.EnquiryWaitTime);
            }
            if (this.PriceOrderSetting != this.OriginDealingOrderParameter.PriceOrderSetting)
            {
                this.UpdateDealingParameterFileds.Add(FieldSetting.PriceOrderSetting, this.PriceOrderSetting);
            }
            if (this.DisablePopup != this.OriginDealingOrderParameter.DisablePopup)
            {
                this.UpdateDealingParameterFileds.Add(FieldSetting.DisablePopup, this.DisablePopup);
            }
            if (this.AutoConfirm != this.OriginDealingOrderParameter.AutoConfirm)
            {
                this.UpdateDealingParameterFileds.Add(FieldSetting.AutoConfirm, this.AutoConfirm);
            }
            if (this.ConfirmRejectDQOrder != this.OriginDealingOrderParameter.ConfirmRejectDQOrder)
            {
                this.UpdateDealingParameterFileds.Add(FieldSetting.ConfirmRejectDQOrder, this.ConfirmRejectDQOrder);
            }
            if (this.LimitStopSummaryIsTimeRange != this.OriginDealingOrderParameter.LimitStopSummaryIsTimeRange)
            {
                this.UpdateDealingParameterFileds.Add(FieldSetting.LimitStopSummaryIsTimeRange, this.LimitStopSummaryIsTimeRange);
            }
            if (this.LimitStopSummaryTimeRangeValue != this.OriginDealingOrderParameter.LimitStopSummaryTimeRangeValue)
            {
                this.UpdateDealingParameterFileds.Add(FieldSetting.LimitStopSummaryTimeRangeValue, this.LimitStopSummaryTimeRangeValue);
            }
            if (this.LimitStopSummaryPriceRangeValue != this.OriginDealingOrderParameter.LimitStopSummaryPriceRangeValue)
            {
                this.UpdateDealingParameterFileds.Add(FieldSetting.LimitStopSummaryPriceRangeValue, this.LimitStopSummaryPriceRangeValue);
            }
        }

        public void Update(Dictionary<string, object> fieldAndValues)
        {
            foreach (string key in fieldAndValues.Keys)
            {
                this.Update(key, fieldAndValues[key]);
            }
        }

        public void Update(string field, object value)
        {
            if (field == FieldSetting.InactiveWaitTime)
            {
                this.InactiveWaitTime = (int)value;
            }
        }   
    }

    public class SoundSetting : PropertyChangedNotifier
    {
        private string _SoundPath;
        private SoundSetting _OriginSoundSetting;
        public Guid SettingDetailId { get; set; }
        public Guid SettingId { get; set; }
        public SoundType SoundType { get; set; }
        public string SoundKey { get; set; }
        public string SoundPath
        {
            get { return this._SoundPath; }
            set { this._SoundPath = value; this.OnPropertyChanged("SoundPath"); }
        }
        public Dictionary<string, object> UpdateFileds { get; set; }

        public SoundSetting() { }

        public SoundSetting(CommonSoundSetting comonSoundSetting)
        {
            this.SoundType = comonSoundSetting.SoundType;
            this.SoundKey = comonSoundSetting.SoundKey;
            this.SoundPath = comonSoundSetting.SoundPath;
            this._OriginSoundSetting = this.Clone();
        }

        public SoundSetting OriginSoundSetting
        {
            get
            {
                if (this._OriginSoundSetting == null)
                {
                    this._OriginSoundSetting = this.Clone();
                }

                return this._OriginSoundSetting;
            }
        }

        public SoundSetting Clone()
        {
            return new SoundSetting()
            {
                SoundPath = this.SoundPath,
            };
        }

    }

    public class SetValueSetting : PropertyChangedNotifier
    {
        private SetValueSetting _OriginSetValueSetting;
        public Dictionary<string, object> UpdateSetValueParameterFileds { get; set; }
        public SetValueSetting() { }

        public SetValueSetting(CommonSetValueSetting setValueSetting)
        {
            this.UpdateSetValueParameterFileds = new Dictionary<string, object>();
            this.Initailize(setValueSetting);
        }

        public SetValueSetting OriginSetValueSetting
        {
            get
            {
                if (this._OriginSetValueSetting == null)
                {
                    this._OriginSetValueSetting = this.Clone();
                }
                return this._OriginSetValueSetting;
            }
        }

        public int OriginInactiveTime { get; set; }
        public int AlertVariation { get; set; }
        public int NormalWaitTime { get; set; }
        public int AlertWaitTime { get; set; }
        public decimal MaxDQLot { get; set; }
        public decimal MaxOtherLot { get; set; }
        public decimal DQQuoteMinLot { get; set; }
        public decimal AutoDQMaxLot { get; set; }
        public decimal AutoLmtMktMaxLot { get; set; }
        public int AcceptDQVariation { get; set; }
        public int AcceptLmtVariation { get; set; }
        public int AcceptCloseLmtVariation { get; set; }
        public int CancelLmtVariation { get; set; }
        public int MaxMinAdjust { get; set; }
        public bool IsBetterPrice { get; set; }
        public decimal AutoAcceptMaxLot { get; set; }
        public decimal AutoCancelMaxLot { get; set; }
        public int AllowedNewTradeSides { get; set; }
        public int HitTimes { get; set; }
        public int PenetrationPoint { get; set; }
        public int PriceValidTime { get; set; }
        public int AutoDQDelay { get; set; }
        public int HitPriceVariationForSTP { get; set; }


        internal void Initailize(CommonSetValueSetting setValueSetting)
        { 
            this.OriginInactiveTime = setValueSetting.OriginInactiveTime;
            this.AlertVariation = setValueSetting.AlertVariation;
            this.NormalWaitTime = setValueSetting.NormalWaitTime;
            this.AlertWaitTime = setValueSetting.AlertWaitTime;
            this.MaxDQLot = setValueSetting.MaxDQLot;
            this.MaxOtherLot = setValueSetting.MaxOtherLot;
            this.DQQuoteMinLot = setValueSetting.DQQuoteMinLot;
            this.AutoDQMaxLot = setValueSetting.AutoDQMaxLot;
            this.AutoLmtMktMaxLot = setValueSetting.AutoLmtMktMaxLot;
            this.AcceptDQVariation = setValueSetting.AcceptDQVariation;
            this.AcceptLmtVariation = setValueSetting.AcceptLmtVariation;
            this.AcceptCloseLmtVariation = setValueSetting.AcceptCloseLmtVariation;
            this.CancelLmtVariation = setValueSetting.CancelLmtVariation;
            this.MaxMinAdjust = setValueSetting.MaxMinAdjust;
            this.IsBetterPrice = setValueSetting.IsBetterPrice;
            this.AutoAcceptMaxLot = setValueSetting.AutoAcceptMaxLot;
            this.AutoCancelMaxLot = setValueSetting.AutoCancelMaxLot;
            this.AllowedNewTradeSides = setValueSetting.AllowedNewTradeSides;
            this.HitTimes = setValueSetting.HitTimes;
            this.PenetrationPoint = setValueSetting.PenetrationPoint;
            this.PriceValidTime = setValueSetting.PriceValidTime;
            this.AutoDQDelay = setValueSetting.AutoDQDelay;
            this.HitPriceVariationForSTP = setValueSetting.HitPriceVariationForSTP;
        }

        public SetValueSetting Clone()
        {
            return new SetValueSetting()
            {
                OriginInactiveTime = this.OriginInactiveTime,
                AlertVariation=this.AlertVariation,
                NormalWaitTime=this.NormalWaitTime,
                AlertWaitTime=this.AlertWaitTime,
                MaxDQLot=this.MaxDQLot,
                MaxOtherLot=this.MaxOtherLot,
                DQQuoteMinLot=this.DQQuoteMinLot,
                AutoDQMaxLot=this.AutoDQMaxLot,
                AutoLmtMktMaxLot=this.AutoLmtMktMaxLot,
                AcceptDQVariation=this.AcceptDQVariation,
                AcceptLmtVariation=this.AcceptLmtVariation,
                AcceptCloseLmtVariation=this.AcceptCloseLmtVariation,
                CancelLmtVariation=this.CancelLmtVariation,
                MaxMinAdjust=this.MaxMinAdjust,
                IsBetterPrice=this.IsBetterPrice,
                AutoAcceptMaxLot=this.AutoAcceptMaxLot,
                AutoCancelMaxLot=this.AutoCancelMaxLot,
                AllowedNewTradeSides=this.AllowedNewTradeSides,
                HitTimes=this.HitTimes,
                PenetrationPoint=this.PenetrationPoint,
                PriceValidTime=this.PriceValidTime,
                AutoDQDelay=this.AutoDQDelay,
                HitPriceVariationForSTP=this.HitPriceVariationForSTP,
            };
        }

        public void UpdateOrigin()
        {
            this._OriginSetValueSetting = this.Clone();
        }

        public void GetUpdateSetValueSettingFileds()
        {
            this.UpdateSetValueParameterFileds.Clear();
            if(this.OriginInactiveTime != this.OriginSetValueSetting.OriginInactiveTime)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.OriginInactiveTime, this.OriginInactiveTime);
            }
            if(this.AlertVariation != this.OriginSetValueSetting.AlertVariation)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.AlertVariation, this.AlertVariation);
            }
            if(this.NormalWaitTime != this.OriginSetValueSetting.NormalWaitTime)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.NormalWaitTime, this.NormalWaitTime);
            }
            if(this.AlertWaitTime != this.OriginSetValueSetting.AlertWaitTime)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.AlertWaitTime, this.AlertWaitTime);
            }
            if(this.MaxDQLot != this.OriginSetValueSetting.MaxDQLot)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.MaxDQLot, this.MaxDQLot);
            }
            if(this.MaxOtherLot != this.OriginSetValueSetting.MaxOtherLot)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.MaxOtherLot, this.MaxOtherLot);
            }
            if(this.DQQuoteMinLot != this.OriginSetValueSetting.DQQuoteMinLot)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.DQQuoteMinLot, this.DQQuoteMinLot);
            }
            if(this.AutoDQMaxLot != this.OriginSetValueSetting.AutoDQMaxLot)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.AutoDQMaxLot, this.AutoDQMaxLot);
            }
            if(this.AutoLmtMktMaxLot != this.OriginSetValueSetting.AutoLmtMktMaxLot)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.AutoLmtMktMaxLot, this.AutoLmtMktMaxLot);
            }
            if(this.AcceptDQVariation != this.OriginSetValueSetting.AcceptDQVariation)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.AcceptDQVariation, this.AcceptDQVariation);
            }
            if(this.AcceptLmtVariation != this.OriginSetValueSetting.AcceptLmtVariation)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.AcceptLmtVariation, this.AcceptLmtVariation);
            }
            if(this.AcceptCloseLmtVariation != this.OriginSetValueSetting.AcceptCloseLmtVariation)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.AcceptCloseLmtVariation, this.AcceptCloseLmtVariation);
            }
            if(this.CancelLmtVariation != this.OriginSetValueSetting.CancelLmtVariation)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.CancelLmtVariation, this.CancelLmtVariation);
            }
            if(this.MaxMinAdjust != this.OriginSetValueSetting.MaxMinAdjust)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.MaxMinAdjust, this.MaxMinAdjust);
            }
            if(this.IsBetterPrice != this.OriginSetValueSetting.IsBetterPrice)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.IsBetterPrice, this.IsBetterPrice);
            }
            if(this.AutoAcceptMaxLot != this.OriginSetValueSetting.AutoAcceptMaxLot)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.AutoAcceptMaxLot, this.AutoAcceptMaxLot);
            }
            if(this.AutoCancelMaxLot != this.OriginSetValueSetting.AutoCancelMaxLot)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.AutoCancelMaxLot, this.AutoCancelMaxLot);
            }
            if(this.AllowedNewTradeSides != this.OriginSetValueSetting.AllowedNewTradeSides)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.AllowedNewTradeSides, this.AllowedNewTradeSides);
            }
            if(this.HitTimes != this.OriginSetValueSetting.HitTimes)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.HitTimes, this.HitTimes);
            }
            if(this.PenetrationPoint != this.OriginSetValueSetting.PenetrationPoint)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.PenetrationPoint, this.PenetrationPoint);
            }
            if(this.PriceValidTime != this.OriginSetValueSetting.PriceValidTime)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.PriceValidTime, this.PriceValidTime);
            }
            if(this.AutoDQDelay != this.OriginSetValueSetting.AutoDQDelay)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.AutoDQDelay, this.AutoDQDelay);
            }
            if(this.HitPriceVariationForSTP != this.OriginSetValueSetting.HitPriceVariationForSTP)
            {
                this.UpdateSetValueParameterFileds.Add(FieldSetting.HitPriceVariationForSTP, this.HitPriceVariationForSTP);
            }
        }
    }
}
