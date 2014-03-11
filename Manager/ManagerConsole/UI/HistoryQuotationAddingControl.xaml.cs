using Infragistics.Controls.Interactions;
using Manager.Common.QuotationEntities;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
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

namespace ManagerConsole.UI
{
    /// <summary>
    /// HistoryQuotationAddingControl.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryQuotationAddingControl : XamDialogWindow
    {
        private HistoryQuotationData _HistoryQuotationData;
        private Action<HistoryQuotationData> _AddHistoryQuotation;

        public HistoryQuotationAddingControl(InstrumentQuotation instrumentQuotation,Action<HistoryQuotationData> addHistoryQuotation)
        {
            InitializeComponent();
            this._HistoryQuotationData = new HistoryQuotationData();
            this._AddHistoryQuotation = addHistoryQuotation;

            this._HistoryQuotationData.InstrumentId = instrumentQuotation.InstruemtnId;
            this._HistoryQuotationData.InstrumentCode = instrumentQuotation.InstrumentCode;
            this._HistoryQuotationData.ExchangeCode = instrumentQuotation.ExchangeCode;
            this._HistoryQuotationData.Time = instrumentQuotation.TimeSpan;
            InstrumentClient instrument = App.MainFrameWindow.ExchangeDataManager.ExchangeSettingManagers[instrumentQuotation.ExchangeCode].Instruments[instrumentQuotation.InstruemtnId];
            HistoryQuotationInfo info = new HistoryQuotationInfo();
            info.NumeratorUnit = instrument.NumeratorUnit;
            info.Denominator = instrument.Denominator;
            this.Origin.Mask = info.GetMask();
            this.TimeSpan.Value = instrumentQuotation.TimeSpan;
            this.Origin.Value = instrumentQuotation.Origin;
            this.TimeMessage.Text = string.Format("Value must less than {0}", instrumentQuotation.TimeSpan);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (DateTime.Parse(this.TimeSpan.Value.ToString()) > DateTime.Parse(this._HistoryQuotationData.Time)) return;
            this._HistoryQuotationData.Time = this.TimeSpan.Value.ToString();
            this._HistoryQuotationData.Origin = this.Origin.Value.ToString();
            this._AddHistoryQuotation(this._HistoryQuotationData);
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
