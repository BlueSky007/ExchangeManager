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
using Infragistics.Controls.Interactions;
using ManagerConsole.ViewModel;
using ManagerConsole.Model;
using Manager.Common.QuotationEntities;

namespace ManagerConsole.UI
{
    /// <summary>
    /// UpdateExchangeHistoryQuotationControl.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateExchangeHistoryQuotationControl : XamDialogWindow
    {
        private HistoryQuotationInfo _HistoryQuotation;
        private decimal _OriginValue;
        private decimal _EditValue;


        public UpdateExchangeHistoryQuotationControl()
        {
            InitializeComponent();
            foreach (InstrumentQuotation instrument in ExchangeQuotationViewModel.Instance.Exchanges)
            {

                if (!this.Exchange.Items.Contains(instrument.ExchangeCode))
                {
                    this.Exchange.Items.Add(instrument.ExchangeCode);
                }
            }
            this.Exchange.SelectedIndex = -1;
            this._HistoryQuotation = new HistoryQuotationInfo();
            this.HistotyQuotation.ItemsSource = this._HistoryQuotation.HistoryQuotationGridData;
        }

        private void Instrument_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Instrument.SelectedIndex >= 0)
            {
                InstrumentQuotation instrumentQuotation = this.Instrument.SelectedItem as InstrumentQuotation;
                if (instrumentQuotation != null)
                {
                    InstrumentClient instrument = App.MainFrameWindow.ExchangeDataManager.ExchangeSettingManagers[instrumentQuotation.ExchangeCode].Instruments[instrumentQuotation.InstruemtnId];
                    this._HistoryQuotation.InstrumentId = instrumentQuotation.InstruemtnId;
                    this._HistoryQuotation.ExchangeCode = instrumentQuotation.ExchangeCode;
                    this._HistoryQuotation.NumeratorUnit = instrument.NumeratorUnit;
                    this._HistoryQuotation.Denominator = instrument.Denominator;
                    this.Origin.Mask = this._HistoryQuotation.OriginMask;
                }
            }
        }

        private void Exchange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Exchange.SelectedIndex >= 0)
            {
                List<Guid> temp = new List<Guid>();
                this.Instrument.Items.Clear();
                foreach (InstrumentQuotation iq in ExchangeQuotationViewModel.Instance.Exchanges.Where(eq=>eq.ExchangeCode==this.Exchange.SelectedItem.ToString()))
                {
                    if (!temp.Contains(iq.InstruemtnId))
                    {
                        temp.Add(iq.InstruemtnId);
                        this.Instrument.Items.Add(iq);
                    }
                }
                this.Instrument.SelectedIndex = -1;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._HistoryQuotation.IsHistoryQuotationEdited())
            {
                if (MessageBox.Show(App.MainFrameWindow, "Is abandon the changes", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            InstrumentQuotation instrument = this.Instrument.SelectedItem as InstrumentQuotation;
            if (instrument == null) return;
            if (string.IsNullOrEmpty(this.BeginTime.Text)) return;
            DateTime beginTime = new DateTime();
            if (!DateTime.TryParse(this.BeginTime.Text, out beginTime)) return;
            if (beginTime > DateTime.Now) return;
            ConsoleClient.Instance.GetOriginQuotationForModifyAskBidHistory(instrument.ExchangeCode, instrument.InstruemtnId, beginTime, this.Origin.Text, this.GetOriginQuotationForModifyAskBidHistoryCallback);

        }

        private void GetOriginQuotationForModifyAskBidHistoryCallback(List<HistoryQuotationData> historyQuotations)
        {
            if (historyQuotations.Count > 0)
            {
                foreach (HistoryQuotationData item in historyQuotations)
                {
                    this._HistoryQuotation.HistoryQuotationGridData.Add(item);
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryQuotationAddingControl addControl = new HistoryQuotationAddingControl(AddHistoryQuotationSuccess);
            HistoryFrame.Children.Add(addControl);
            addControl.Show();
            addControl.BringToFront();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddHistoryQuotationSuccess(HistoryQuotationData data)
        {
            this._HistoryQuotation.HistoryQuotationGridData.Add(data);
        }

        private void HistotyQuotation_CellEnteredEditMode(object sender, Infragistics.Controls.Grids.EditingCellEventArgs e)
        {
            this._OriginValue = decimal.Parse(e.Cell.Value.ToString());
        }

        private void HistotyQuotation_CellExitedEditMode(object sender, Infragistics.Controls.Grids.CellExitedEditingEventArgs e)
        {
            HistoryQuotationData data = e.Cell.Row.Data as HistoryQuotationData;
            if (decimal.TryParse(e.Cell.Value.ToString(), out this._EditValue))
            {
                if (this._OriginValue != this._EditValue)
                {
                    this._HistoryQuotation.HistoryQuotationGridData.Single(h => h.InstrumentId == data.InstrumentId && h.Time.CompareTo(data.Time) == 0).IsEdit = true;
                }
            }
            else
            {
                this._HistoryQuotation.HistoryQuotationGridData.Single(h => h.InstrumentId == data.InstrumentId && h.Time.CompareTo(data.Time) == 0).Origin = this._OriginValue.ToString(); ;
            }
        }
    }
}
