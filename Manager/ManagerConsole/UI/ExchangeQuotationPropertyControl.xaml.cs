using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ManagerConsole.ViewModel;
using Manager.Common;
using Manager.Common.QuotationEntities;
using ManagerConsole.Model;
using PriceType = iExchange.Common.PriceType;

namespace ManagerConsole.UI
{
    /// <summary>
    /// ExchangeQuotationPropertyControl.xaml 的交互逻辑
    /// </summary>
    public partial class ExchangeQuotationPropertyControl : UserControl
    {
        private InstrumentQuotation _Source = null;
        private bool _IsInit = false;
        private Action<InstrumentQuotation> _SetInstrumentQuotation;

        public ExchangeQuotationPropertyControl()
        {
            InitializeComponent();
            this.priceType.ItemsSource = Enum.GetNames(typeof(PriceType));
        }

        public ExchangeQuotationPropertyControl( Action<InstrumentQuotation> setInstrumentQuotation)
        {
            InitializeComponent();
            this._SetInstrumentQuotation = setInstrumentQuotation;
        }

        public void SetSource(InstrumentQuotation source)
        {
            this._IsInit = false;
            this._Source = source;
            var model = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.AutoAdjustPoints.Value = model.AutoAdjustPoints;
            this.AutoAdjustPoints2.Value = model.AutoAdjustPoints2;
            this.AutoAdjustPoints3.Value = model.AutoAdjustPoints3;
            this.AutoAdjustPoints4.Value = model.AutoAdjustPoints4;
            this.SpreadPoints.Value = model.SpreadPoints;
            this.SpreadPoints2.Value = model.SpreadPoints2;
            this.SpreadPoints3.Value = model.SpreadPoints3;
            this.SpreadPoints4.Value = model.SpreadPoints4;
            this.priceType.SelectedItem = model.PriceType.ToString();
            this.IsOriginHiLo.IsChecked = model.IsOriginHiLo;
            this.MaxAuotAdjustPoints.Value = model.MaxAuotAdjustPoints;
            this.MaxSpreadPoints.Value = model.MaxSpreadPoints;
            this.IsAutoFill.IsChecked = model.IsAutoFill;
            this.IsEnablePrice.IsChecked = model.IsEnablePrice;
            this.IsAutoEnablePrice.IsChecked = model.IsAutoEnablePrice;
            this._IsInit = true;
        }

        private void AutoAdjustPoints_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).AutoAdjustPoints = int.Parse(this.AutoAdjustPoints.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.AutoAdjustPoints, int.Parse(this.AutoAdjustPoints.Value.ToString()));
            }
        }

        private void SpreadPoints_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).SpreadPoints = int.Parse(this.SpreadPoints.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.SpreadPoints, int.Parse(this.SpreadPoints.Value.ToString()));
            }
        }

        private void AutoAdjustPoints2_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).AutoAdjustPoints2 = int.Parse(this.AutoAdjustPoints2.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.AutoAdjustPoints2, int.Parse(this.AutoAdjustPoints2.Value.ToString()));
            }
        }

        private void AutoAdjustPoints3_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).AutoAdjustPoints3 = int.Parse(this.AutoAdjustPoints3.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.AutoAdjustPoints3, int.Parse(this.AutoAdjustPoints3.Value.ToString()));
            }
        }

        private void AutoAdjustPoints4_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).AutoAdjustPoints4 = int.Parse(this.AutoAdjustPoints4.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.AutoAdjustPoints4, int.Parse(this.AutoAdjustPoints4.Value.ToString()));
            }
        }

        private void SpreadPoints2_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).SpreadPoints2 = int.Parse(this.SpreadPoints2.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.SpreadPoints2, int.Parse(this.SpreadPoints2.Value.ToString()));
            }

        }

        private void SpreadPoints3_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).SpreadPoints3 = int.Parse(this.SpreadPoints3.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.SpreadPoints3, int.Parse(this.SpreadPoints3.Value.ToString()));
            }
        }

        private void SpreadPoints4_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).SpreadPoints4 = int.Parse(this.SpreadPoints4.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.SpreadPoints4, int.Parse(this.SpreadPoints4.Value.ToString()));
            }
        }

        private void MaxAuotAdjustPoints_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).MaxAuotAdjustPoints = int.Parse(this.MaxAuotAdjustPoints.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.MaxAuotAutoAdjustPointsPoints, int.Parse(this.MaxAuotAdjustPoints.Value.ToString()));
            }
        }

        private void MaxSpreadPoints_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).MaxSpreadPoints = int.Parse(this.MaxSpreadPoints.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.MaxSpreadPointsPoints, int.Parse(this.MaxSpreadPoints.Value.ToString()));
            }
        }

        private void IsOriginHiLo_Click(object sender, RoutedEventArgs e)
        {
            if (this._IsInit)
            {
                int value;
                if (this.IsOriginHiLo.IsChecked == true)
                {
                    value = 1;
                }
                else
                {
                    value = 0;
                }
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).IsOriginHiLo = (this.IsOriginHiLo.IsChecked == true);
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.IsOriginHiLo, value);
            }
        }

        private void priceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._IsInit)
            {
                PriceType type = (PriceType)Enum.Parse(typeof(PriceType),this.priceType.SelectedItem.ToString());
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).PriceType = type;
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.PriceType, (int)type);
            }
        }

        private void SetQuotePolicyDetail(InstrumentQuotationEditType type, int value)
        {
            InstrumentQuotationSet set = new InstrumentQuotationSet();
            set.ExchangeCode = this._Source.ExchangeCode;
            set.QoutePolicyId = this._Source.QuotationPolicyId;
            set.InstrumentId = this._Source.InstruemtnId;
            set.Value = value;
            set.type = type;
            ConsoleClient.Instance.UpdateExchangeQuotation(set);
        }

        private void IsAutoFill_Click(object sender, RoutedEventArgs e)
        {
            if (this._IsInit)
            {
                int value;
                if (this.IsAutoFill.IsChecked == true)
                {
                    value = 1;
                }
                else
                {
                    value = 0;
                }
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).IsOriginHiLo = (this.IsOriginHiLo.IsChecked == true);
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.IsAutoFill, value);
            }
        }

        private void IsEnablePrice_Click(object sender, RoutedEventArgs e)
        {
            if (this._IsInit)
            {
                int value;
                if (this.IsEnablePrice.IsChecked == true)
                {
                    value = 1;
                }
                else
                {
                    value = 0;
                }
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).IsOriginHiLo = (this.IsOriginHiLo.IsChecked == true);
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.IsEnablePrice, value);
            }
        }

        private void IsAutoEnablePrice_Click(object sender, RoutedEventArgs e)
        {
            if (this._IsInit)
            {
                int value;
                if (this.IsAutoEnablePrice.IsChecked == true)
                {
                    value = 1;
                }
                else
                {
                    value = 0;
                }
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).IsOriginHiLo = (this.IsOriginHiLo.IsChecked == true);
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.IsAutoEnablePrice, value);
            }
        }
    }
}
