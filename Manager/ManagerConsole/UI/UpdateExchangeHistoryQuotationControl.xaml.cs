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
using Manager.Common;

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
        private List<string> _EditExchangeCodes;
        private UpdateHighLowBatchProcessInfo _RestoreInfo;


        public UpdateExchangeHistoryQuotationControl()
        {
            try
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
                this.RestoreProcess.ItemsSource = ExchangeQuotationViewModel.Instance.HighLowBatchProcessInfos;
                this._EditExchangeCodes = new List<string>();
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "UpdateExchangeHistoryQuotationControl.\r\n{0}", ex.ToString());   
            }
        }

        private void Instrument_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
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
                        this.Origin.Mask = this._HistoryQuotation.GetMask();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "Instrument_SelectionChanged.\r\n{0}", ex.ToString());
            }
        }

        private void Exchange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (this.Exchange.SelectedIndex >= 0)
                {
                    List<Guid> temp = new List<Guid>();
                    this.Instrument.Items.Clear();
                    foreach (InstrumentQuotation iq in ExchangeQuotationViewModel.Instance.Exchanges.Where(eq => eq.ExchangeCode == this.Exchange.SelectedItem.ToString()))
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
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "Exchange_SelectionChanged.\r\n{0}", ex.ToString());
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "SearchButton_Click.\r\n{0}", ex.ToString());
            }
        }

        private void GetOriginQuotationForModifyAskBidHistoryCallback(List<HistoryQuotationData> historyQuotations)
        {
            this._HistoryQuotation.HistoryQuotationGridData.Clear();
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
            InstrumentQuotation instrument = this.Instrument.SelectedItem as InstrumentQuotation;
            if (instrument == null) return;
            HistoryQuotationAddingControl addControl = new HistoryQuotationAddingControl(instrument, AddHistoryQuotationSuccess);
            App.MainFrameWindow.MainFrame.Children.Add(addControl);
            addControl.Show();
            addControl.BringToFront();
        }

        private void AddHistoryQuotationSuccess(HistoryQuotationData data)
        {
            data.Status = "Inserted";
            if (!this._EditExchangeCodes.Contains(data.ExchangeCode))
            {
                this._EditExchangeCodes.Add(data.ExchangeCode);
            }
            this._HistoryQuotation.HistoryQuotationGridData.Add(data);
        }

        private void HistotyQuotation_CellEnteredEditMode(object sender, Infragistics.Controls.Grids.EditingCellEventArgs e)
        {
            this._OriginValue = decimal.Parse(e.Cell.Value.ToString());
        }

        private void HistotyQuotation_CellExitedEditMode(object sender, Infragistics.Controls.Grids.CellExitedEditingEventArgs e)
        {
            try
            {
                HistoryQuotationData data = e.Cell.Row.Data as HistoryQuotationData;
                if (decimal.TryParse(e.Cell.Value.ToString(), out this._EditValue))
                {
                    if (this._OriginValue != this._EditValue)
                    {
                        if (string.IsNullOrEmpty(data.Status))
                        {
                            this._HistoryQuotation.HistoryQuotationGridData.Single(h => h.InstrumentId == data.InstrumentId && h.Time.CompareTo(data.Time) == 0).Status = "Modified";
                        }
                        if (!this._EditExchangeCodes.Contains(data.ExchangeCode))
                        {
                            this._EditExchangeCodes.Add(data.ExchangeCode);
                        }
                    }
                }
                else
                {
                    this._HistoryQuotation.HistoryQuotationGridData.Single(h => h.InstrumentId == data.InstrumentId && h.Time.CompareTo(data.Time) == 0).Origin = this._OriginValue.ToString(); ;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "HistotyQuotation_CellExitedEditMode.\r\n{0}", ex.ToString());
            }
        }

        private void ResoreButton_Click(object sender, RoutedEventArgs e)
        {
            this._RestoreInfo = this.RestoreProcess.SelectedItem as UpdateHighLowBatchProcessInfo;
            ConsoleClient.Instance.RestoreHighLow(this._RestoreInfo.ExchangeCode, this._RestoreInfo.BatchProcessId, this.ResoreHiLoCallBack);
        }

        private void ResoreHiLoCallBack(bool result)
        {
            if (result)
            {
                ExchangeQuotationViewModel.Instance.HighLowBatchProcessInfos.Remove(this._RestoreInfo);
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<string, string> quotations = new Dictionary<string, string>();
                foreach (string exchange in this._EditExchangeCodes)
                {
                    string quotation = string.Empty;
                    if (this._HistoryQuotation.ConvertEditDataForExchangeToXml(exchange, out quotation))
                    {
                        quotations.Add(exchange, quotation);
                    }
                }
                if (quotations.Count > 0)
                {
                    ConsoleClient.Instance.FixOverridedQuotationHistory(quotations, true, this.CallBack);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "Confirm_Click.\r\n{0}", ex.ToString());
            }
        }

        private void CallBack(bool result)
        {
            if (result)
            {
                this.Close();
            }
        }

        private void RestoreProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateHighLowBatchProcessInfo info = this.RestoreProcess.SelectedItem as UpdateHighLowBatchProcessInfo;
            if (info != null)
            {
                foreach (UpdateHighLowBatchProcessInfo item in ExchangeQuotationViewModel.Instance.HighLowBatchProcessInfos.Where(h => h.ExchangeCode == info.ExchangeCode && h.InstrumentId == info.InstrumentId))
                {
                    if (info.BatchProcessId < item.BatchProcessId)
                    {
                        info = item;
                    }
                }
                this.RestoreProcess.SelectedItem = info;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this._HistoryQuotation.IsHistoryQuotationEdited())
            {
                if (MessageBox.Show(App.MainFrameWindow, "Is abandon the changes", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            this.Close();
        }
    }
}
