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
using Infragistics.Controls.Editors;

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
            Binding bind = new Binding("AutoAdjustPoints");
            bind.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId); 
            this.AutoAdjustPoints.SetBinding(XamNumericInput.ValueProperty, bind);
            Binding autoAdjustPoints2 = new Binding("AutoAdjustPoints2");
            autoAdjustPoints2.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.AutoAdjustPoints2.SetBinding(XamNumericInput.ValueProperty, autoAdjustPoints2);
            Binding autoAdjustPoints3 = new Binding("AutoAdjustPoints3");
            autoAdjustPoints3.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.AutoAdjustPoints3.SetBinding(XamNumericInput.ValueProperty, autoAdjustPoints3);
            Binding autoAdjustPoints4 = new Binding("AutoAdjustPoints4");
            autoAdjustPoints4.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.AutoAdjustPoints4.SetBinding(XamNumericInput.ValueProperty, autoAdjustPoints4);

            Binding spreadPoints = new Binding("SpreadPoints");
            spreadPoints.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.SpreadPoints.SetBinding(XamNumericInput.ValueProperty, spreadPoints);
            Binding spreadPoints2 = new Binding("SpreadPoints2");
            spreadPoints2.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.SpreadPoints2.SetBinding(XamNumericInput.ValueProperty, spreadPoints2);
            Binding spreadPoints3 = new Binding("SpreadPoints3");
            spreadPoints3.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.SpreadPoints3.SetBinding(XamNumericInput.ValueProperty, spreadPoints3);
            Binding spreadPoints4 = new Binding("SpreadPoints4");
            spreadPoints4.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.SpreadPoints4.SetBinding(XamNumericInput.ValueProperty, spreadPoints4);

            Binding maxAuotAdjustPoints = new Binding("MaxAuotAdjustPoints");
            maxAuotAdjustPoints.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.MaxAuotAdjustPoints.SetBinding(XamNumericInput.ValueProperty, maxAuotAdjustPoints);
            Binding maxSpreadPoints = new Binding("MaxSpreadPoints");
            maxSpreadPoints.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.MaxSpreadPoints.SetBinding(XamNumericInput.ValueProperty, maxSpreadPoints);

            this.priceType.SelectedItem = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId).PriceType.ToString();

            Binding allowLimit = new Binding("AllowLimit");
            allowLimit.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.AllowLomit.SetBinding(CheckBox.IsCheckedProperty, allowLimit);
            Binding isOriginHiLo = new Binding("IsOriginHiLo");
            isOriginHiLo.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.IsOriginHiLo.SetBinding(CheckBox.IsCheckedProperty, isOriginHiLo);
            Binding isAutoFill = new Binding("IsAutoFill");
            isAutoFill.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.IsAutoFill.SetBinding(CheckBox.IsCheckedProperty, isAutoFill);
            Binding IsPriceEnabled = new Binding("IsPriceEnabled");
            IsPriceEnabled.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.IsEnablePrice.SetBinding(CheckBox.IsCheckedProperty, IsPriceEnabled);
            Binding isAutoEnablePrice = new Binding("IsAutoEnablePrice");
            isAutoEnablePrice.Source = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == source.ExchangeCode && i.QuotationPolicyId == source.QuotationPolicyId && i.InstruemtnId == source.InstruemtnId);
            this.IsAutoEnablePrice.SetBinding(CheckBox.IsCheckedProperty, isAutoEnablePrice);
            this._IsInit = true;
        }

        private void AutoAdjustPoints_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
               // ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).AutoAdjustPoints = int.Parse(this.AutoAdjustPoints.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.AutoAdjustPoints, int.Parse(this.AutoAdjustPoints.Value.ToString()));
            }
        }

        private void SpreadPoints_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                //ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).SpreadPoints = int.Parse(this.SpreadPoints.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.SpreadPoints, int.Parse(this.SpreadPoints.Value.ToString()));
            }
        }

        private void AutoAdjustPoints2_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                //ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).AutoAdjustPoints2 = int.Parse(this.AutoAdjustPoints2.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.AutoAdjustPoints2, int.Parse(this.AutoAdjustPoints2.Value.ToString()));
            }
        }

        private void AutoAdjustPoints3_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                //ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).AutoAdjustPoints3 = int.Parse(this.AutoAdjustPoints3.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.AutoAdjustPoints3, int.Parse(this.AutoAdjustPoints3.Value.ToString()));
            }
        }

        private void AutoAdjustPoints4_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                //ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).AutoAdjustPoints4 = int.Parse(this.AutoAdjustPoints4.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.AutoAdjustPoints4, int.Parse(this.AutoAdjustPoints4.Value.ToString()));
            }
        }

        private void SpreadPoints2_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                //ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).SpreadPoints2 = int.Parse(this.SpreadPoints2.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.SpreadPoints2, int.Parse(this.SpreadPoints2.Value.ToString()));
            }

        }

        private void SpreadPoints3_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                //ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).SpreadPoints3 = int.Parse(this.SpreadPoints3.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.SpreadPoints3, int.Parse(this.SpreadPoints3.Value.ToString()));
            }
        }

        private void SpreadPoints4_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                //ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).SpreadPoints4 = int.Parse(this.SpreadPoints4.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.SpreadPoints4, int.Parse(this.SpreadPoints4.Value.ToString()));
            }
        }

        private void MaxAuotAdjustPoints_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                //ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).MaxAuotAdjustPoints = int.Parse(this.MaxAuotAdjustPoints.Value.ToString());
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.MaxAuotAutoAdjustPointsPoints, int.Parse(this.MaxAuotAdjustPoints.Value.ToString()));
            }
        }

        private void MaxSpreadPoints_ValueChanged(object sender, EventArgs e)
        {
            if (this._IsInit)
            {
                //ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).MaxSpreadPoints = int.Parse(this.MaxSpreadPoints.Value.ToString());
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
                //ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).IsOriginHiLo = (this.IsOriginHiLo.IsChecked == true);
                this.SetQuotePolicyDetail(InstrumentQuotationEditType.IsOriginHiLo, value);
            }
        }

        private void priceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._IsInit)
            {
                PriceType type = (PriceType)Enum.Parse(typeof(PriceType),this.priceType.SelectedItem.ToString());
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault( i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).PriceType = type;
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
                IEnumerable<InstrumentQuotation> instruments = ExchangeQuotationViewModel.Instance.Exchanges.Where(i => i.ExchangeCode == this._Source.ExchangeCode && i.InstruemtnId == this._Source.InstruemtnId);
                foreach (InstrumentQuotation item in instruments)
                {
                    ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == item.ExchangeCode && i.QuotationPolicyId == item.QuotationPolicyId && i.InstruemtnId == item.InstruemtnId).IsAutoFill = (this.IsAutoFill.IsChecked == true);
                }
                this.SetInstrument(InstrumentQuotationEditType.IsAutoFill, value);
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
                IEnumerable<InstrumentQuotation> instruments = ExchangeQuotationViewModel.Instance.Exchanges.Where(i => i.ExchangeCode == this._Source.ExchangeCode && i.InstruemtnId == this._Source.InstruemtnId);
                foreach (InstrumentQuotation item in instruments)
                {
                    ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == item.ExchangeCode && i.QuotationPolicyId == item.QuotationPolicyId && i.InstruemtnId == item.InstruemtnId).IsPriceEnabled = (this.IsEnablePrice.IsChecked == true);
                }
               // ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).IsEnablePrice = (this.IsEnablePrice.IsChecked == true);
                this.SetInstrument(InstrumentQuotationEditType.IsPriceEnabled, value);
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
                IEnumerable<InstrumentQuotation> instruments = ExchangeQuotationViewModel.Instance.Exchanges.Where(i => i.ExchangeCode == this._Source.ExchangeCode && i.InstruemtnId == this._Source.InstruemtnId);
                foreach (InstrumentQuotation item in instruments)
                {
                    ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == item.ExchangeCode && i.QuotationPolicyId == item.QuotationPolicyId && i.InstruemtnId == item.InstruemtnId).IsAutoEnablePrice = (this.IsAutoEnablePrice.IsChecked == true);
                }
                //ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).IsAutoEnablePrice = (this.IsAutoEnablePrice.IsChecked == true);
                this.SetInstrument(InstrumentQuotationEditType.IsAutoEnablePrice, value);
            }
        }

        private void AllowLomit_Click(object sender, RoutedEventArgs e)
        {
            if (this._IsInit)
            {
                int value;
                IEnumerable<InstrumentQuotation> instruments = ExchangeQuotationViewModel.Instance.Exchanges.Where(i => i.ExchangeCode == this._Source.ExchangeCode && i.InstruemtnId == this._Source.InstruemtnId);
                foreach (InstrumentQuotation item in instruments)
                {
                    ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == item.ExchangeCode && i.QuotationPolicyId == item.QuotationPolicyId && i.InstruemtnId == item.InstruemtnId).AllowLimit = (this.AllowLomit.IsChecked == true);
                }
                value = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == this._Source.ExchangeCode && i.QuotationPolicyId == this._Source.QuotationPolicyId && i.InstruemtnId == this._Source.InstruemtnId).OrderTypeMask;
                this.SetInstrument(InstrumentQuotationEditType.OrderTypeMask, value);
            }
        }

        private void SetInstrument(InstrumentQuotationEditType type, int value)
        {
            InstrumentQuotationSet set = new InstrumentQuotationSet();
            set.ExchangeCode = this._Source.ExchangeCode;
            set.InstrumentId = this._Source.InstruemtnId;
            set.Value = value;
            set.type = type;
            ConsoleClient.Instance.UpdateInstrument(set);
        }

       
    }
}
