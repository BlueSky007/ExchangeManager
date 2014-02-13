using iExchange.Common.Manager;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Interactions;
using Manager.Common;
using ManagerConsole.Helper;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for QuoteControl.xaml
    /// </summary>
    public partial class QuotePriceWindow : XamDialogWindow
    {
        private ManagerConsole.MainWindow _App;
        private bool isBingdingGrid = false;
        private DispatcherTimer _QuoteTimer;
        private CommonDialogWin _CommonDialogWin;
        private ConfirmDialogWin _ConfirmDialogWin;
        private QuotePriceClientModel _QuotePriceClientModel;
        private Style _CurrentInstrumentStyle;
        private Style _NormalStyle;
        public QuotePriceWindow()
        {
            InitializeComponent();
            
            this.InitializeUI();
            this.TimerHandle();
            this.AttachEvent();
        }

        private void InitializeUI()
        {
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._CommonDialogWin = this._App._CommonDialogWin;
            this._ConfirmDialogWin = this._App._ConfirmDialogWin;
            this._QuotePriceClientModel = this._App.ExchangeDataManager.QuotePriceClientModel;
            this._CurrentInstrumentStyle = Application.Current.Resources["QuoteInstrumentStyle"] as Style;
            this._NormalStyle = Application.Current.Resources["CellControlStyle"] as Style;
        }

        private void TimerHandle()
        {
            this._QuoteTimer = new DispatcherTimer();
            this._QuoteTimer.Interval = new TimeSpan(0, 0, 1);
            this._QuoteTimer.Tick += new EventHandler(Quote_Tick);
        }

        private void Quote_Tick(object sender, EventArgs e)
        {
            ObservableCollection<QuotePriceClient> quotePriceClients = this._App.ExchangeDataManager.QuotePriceClientModel.QuotePriceClients;
            if (quotePriceClients.Count <= 0)
            {
                if (this._QuoteTimer.IsEnabled)
                {
                    this._QuoteTimer.Stop();
                }
                return;
            }
            for (int i = 0; i < quotePriceClients.Count; i++)
            {
                QuotePriceClient quotePriceEntity = quotePriceClients[i];
                quotePriceEntity.WaitTimes = quotePriceEntity.WaitTimes > 0 ? quotePriceEntity.WaitTimes - 1 : quotePriceEntity.WaitTimes;
                if (quotePriceEntity.WaitTimes == 0)
                {
                    this._QuotePriceClientModel.RemoveSendQuotePrice(quotePriceEntity);
                    this.SettingRowStyle();
                    i--;
                }
            }
        }

        private void AttachEvent()
        {
            ConsoleClient.Instance.MessageClient.QuotePriceToDealerEvent += this.MessageClient_QuotePriceReceived;
            this._QuotePriceClientModel.OnBindingQuotePriceUIEvent += this.BindingQuotePriceUI;
        }

        #region Grid Event
        private void QuotePriceGrid_InitializeRow(object sender, Infragistics.Controls.Grids.InitializeRowEventArgs e)
        {
            QuotePriceClient quotePriceClient = e.Row.Data as QuotePriceClient;
            
            Guid instrumentId = this._QuotePriceClientModel.QuotePriceForInstrument.Instrument.Id;
            if (instrumentId == quotePriceClient.InstrumentId)
            {
                e.Row.CellStyle = this._CurrentInstrumentStyle;
            }
            else
            {
                e.Row.CellStyle = this._NormalStyle;
            }
        }
        #endregion


        #region Event
        private void LotText_LostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (this._QuotePriceClientModel.QuotePriceClients.Count == 0) return;
            TextBox text = (TextBox)sender;

            string newLot = text.Text;
            if (!Toolkit.IsValidNumber(newLot)) return;
            decimal answerLot = decimal.Parse(newLot);
            decimal maxEnquireLot = this._QuotePriceClientModel.QuotePriceClients.Max(P => P.Lot);
            if (answerLot >= maxEnquireLot)
            {
                text.Text = maxEnquireLot.ToString();
                answerLot = maxEnquireLot;
                return;
            }
            this._QuotePriceClientModel.UpdateLot(answerLot);
        }

        private void AdjustHandle(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            
            Button btn = (Button)sender;
            switch (btn.Name)
            {
                case "_UpPriceButton":
                    this.AdjustPrice(true);
                    break;
                case "_DownPriceButton":
                    this.AdjustPrice(false);
                    break;
                case "_UpLotButton":
                    this._QuotePriceClientModel.AdjustLot(true);
                    break;
                case "_DownLotButton":
                    if (this._QuotePriceClientModel.QuotePriceForInstrument == null) return;
                    if (this._QuotePriceClientModel.QuotePriceForInstrument.AnswerLot < 1) return;
                    this._QuotePriceClientModel.AdjustLot(false);
                    break;
                default:
                    break;
            }
        }

        private void AdjustPrice(bool isAdd)
        {
            bool? isValidPrice = null;
            QuotePriceForInstrument quotePriceForInstrument = this.QuoteGrid.DataContext as QuotePriceForInstrument;

            if (this._QuotePriceClientModel.QuotePriceForInstrument == null) return;
            isValidPrice = quotePriceForInstrument.IsValidPrice(1);
            if (!this.CheckPrice(isValidPrice)) return;

            this._QuotePriceClientModel.AdjustPrice(isAdd); 
        }


        #endregion

        private void MessageClient_QuotePriceReceived(QuoteMessage quoteMessage)
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                this.WindowState = Infragistics.Controls.Interactions.WindowState.Normal;
                this.Height = 650;
                this.Width = 650;
                if (!this.isBingdingGrid)
                { 
                    this.BindGridData();
                    this.isBingdingGrid = true;
                }
                if (!this._QuoteTimer.IsEnabled)
                {
                    this._QuoteTimer.Start();
                }
            });
        }

        private void BindingQuotePriceUI()
        {
            this.QuoteGrid.DataContext = this._QuotePriceClientModel.QuotePriceForInstrument;
        }

        private void BindGridData()
        {
            this._QuotePriceGrid.ItemsSource = this._QuotePriceClientModel.QuotePriceClients; 
        }

        private bool CheckPrice(bool? isValidPrice)
        {
            bool result = true;
            if (!isValidPrice.HasValue)
            {
                this._CommonDialogWin.ShowDialogWin("Invalid price!", "Error");
                result = false;
            }
            return result;
        }

        private void SettingRowStyle()
        {
            if (this._QuotePriceClientModel.QuotePriceForInstrument == null) return;
            Guid instrumentId = this._QuotePriceClientModel.QuotePriceForInstrument.Instrument.Id;
            foreach (Row row in this._QuotePriceGrid.Rows)
            {
                QuotePriceClient quotePriceClient = row.Data as QuotePriceClient;
                if (instrumentId == quotePriceClient.InstrumentId)
                {
                    row.CellStyle = this._CurrentInstrumentStyle;
                }
                else
                {
                    row.CellStyle = this._NormalStyle;
                }
            }
        }

        private void QuoteHandler_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Button btn = (Button)sender;
            switch (btn.Name)
            {
                case "MultipleSendBtn":
                    this.SendMultipleQuotePrice();
                    break;
                case "SendSingleBtn":
                    this.SendQuotePrice();
                    break;
            }
        }

        private void SendMultipleQuotePrice()
        {
            if (this._QuotePriceClientModel.QuotePriceClients.Count == 0) return;
            List<Answer> ToSendQutoPrices = new List<Answer>();
            string bestBuy = this._QuotePriceClientModel.QuotePriceForInstrument.BestBuy;
            string bestSell = this._QuotePriceClientModel.QuotePriceForInstrument.BestSell;

            Guid currentInstrumentId = this._QuotePriceClientModel.QuotePriceForInstrument.Instrument.Id;
            IEnumerable<QuotePriceClient> quotePriceClients = this._QuotePriceClientModel.QuotePriceClients.Where((QuotePriceClient P) => P.InstrumentId == currentInstrumentId);
            QuotePriceClient[] quotePriceClientArray = quotePriceClients.ToArray();
            for (int i = 0; i < quotePriceClientArray.Count(); i++)
            {
                ToSendQutoPrices.Add(quotePriceClientArray[i].ToSendQutoPrice(bestBuy,bestSell));
                this._QuotePriceClientModel.RemoveSendQuotePrice(quotePriceClientArray[i]);
            }
            this.SettingRowStyle();
            ConsoleClient.Instance.SendQuotePrice(ToSendQutoPrices);
        }

        private void SendQuotePrice()
        {
            if (this._QuotePriceClientModel.QuotePriceClients.Count == 0) return;
            QuotePriceClient quotePriceClient = this._QuotePriceClientModel.QuotePriceClients[0];
            string bestBuy = this._QuotePriceClientModel.QuotePriceForInstrument.BestBuy;
            string bestSell = this._QuotePriceClientModel.QuotePriceForInstrument.BestSell;

            List<Answer> ToSendQutoPrices = new List<Answer>();
            ToSendQutoPrices.Add(quotePriceClient.ToSendQutoPrice(bestBuy,bestSell));

            ConsoleClient.Instance.SendQuotePrice(ToSendQutoPrices);

            this._QuotePriceClientModel.RemoveSendQuotePrice(quotePriceClient);
            this.SettingRowStyle();
        }
    }
}
