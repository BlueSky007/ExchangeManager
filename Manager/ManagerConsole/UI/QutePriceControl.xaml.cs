using Infragistics.Controls.Grids;
using ManagerConsole.Helper;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Threading;
using Logger = Manager.Common.Logger;
using QuoteMessage = Manager.Common.QuoteMessage;
using Answer = Manager.Common.Answer;


namespace ManagerConsole
{
    /// <summary>
    /// Interaction logic for QutePriceControl.xaml
    /// </summary>
    public partial class QutePriceControl : UserControl
    {
        private ObservableCollection<QuotePriceForInstrument> _ClientQuotePriceForInstrument = new ObservableCollection<QuotePriceForInstrument>();
        private DispatcherTimer _QuoteTimer;
        private CommonDialogWin _CommonDialogWin;
        private ConfirmDialogWin _ConfirmDialogWin;
        private ManagerConsole.MainWindow _App;
        
        public QutePriceControl()
        {
            InitializeComponent();
        }

        void onLoad(object sender, RoutedEventArgs e)
        {
            this.InitData();
            this.AttachEvent();
            this.BindGridData();
            this.TimerHandle();
        }
        private void InitData()
        {
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._CommonDialogWin = this._App.CommonDialogWin;
            this._ConfirmDialogWin = this._App.ConfirmDialogWin;
            this._ClientQuotePriceForInstrument = this._App.InitDataManager.ClientQuotePriceForInstrument;
        }
        private void BindGridData()
        {
            this.QuotePriceGrid.ItemsSource = this._App.InitDataManager.ClientQuotePriceForInstrument;// this._ClientQuotePriceForInstrument;
        }
        private void TimerHandle()
        {
            this._QuoteTimer = new DispatcherTimer();
            this._QuoteTimer.Interval = new TimeSpan(0, 0, 1);
            this._QuoteTimer.Tick += new EventHandler(Quote_Tick);
        }
        
        #region 页面初始化过程
        private void AttachEvent()
        { 
            ConsoleClient.Instance.MessageClient.QuotePriceToDealerEvent += this.MessageClient_QuotePriceReceived;
        }
        #endregion

        void Quote_Tick(object sender, EventArgs e)
        {
            if (this._App.InitDataManager.ClientQuotePriceForInstrument != null && this._App.InitDataManager.ClientQuotePriceForInstrument.Count > 0)
            {
                for (int i = 0; i < this._App.InitDataManager.ClientQuotePriceForInstrument.Count; i++)
                {
                    var quotePriceClients = this._App.InitDataManager.ClientQuotePriceForInstrument[i].QuotePriceClients;
                    for (int j = 0; j < quotePriceClients.Count; j++)
                    {
                        QuotePriceClient quotePriceEntity = quotePriceClients[j];
                        quotePriceEntity.WaitTimes = quotePriceEntity.WaitTimes > 0 ? quotePriceEntity.WaitTimes - 1 : quotePriceEntity.WaitTimes;
                        if (quotePriceEntity.WaitTimes == 0)
                        {
                            this._App.InitDataManager.ClientQuotePriceForInstrument[i].RemoveSendQuotePrice(quotePriceEntity);
                            j--;
                        }
                    }
                }
            }
            else
            {
                if (this._QuoteTimer.IsEnabled)
                {
                    this._QuoteTimer.Stop();
                }
            }
        }

        #region Grid Event Hander
        void QuotePriceGrid_CellControlAttached(object sender, CellControlAttachedEventArgs e)
        {
            if (e.Cell.Column.Key == "QuotePriceClients")
            {
                return;
            }
            Row row = (Row)e.Cell.Row;
            if (row.HasChildren)
            {
                row.IsExpanded = true;
            }
        }

        void QuotePriceGrid_CellExitedEditMode(object sender, CellExitedEditingEventArgs e)
        {
            QuotePriceForInstrument quotePriceForInstrument;
            
            bool? isValidPrice = null;
            switch (e.Cell.Column.Key)
            {
                case "AdjustSingle":
                    QuotePriceClient quotePriceClient = e.Cell.Row.Data as QuotePriceClient;
                    quotePriceForInstrument = this._ClientQuotePriceForInstrument.SingleOrDefault(P => P.InstrumentClient.Id == quotePriceClient.InstrumentId);

                    isValidPrice = quotePriceForInstrument.IsValidPrice(quotePriceClient, quotePriceClient.AdjustSingle);

                    if (isValidPrice.HasValue)
                    {
                        if (isValidPrice.Value &&
                            MessageBox.Show("Invalid price,continue?", "Alert!", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                    }
                    else
                    {
                        this._CommonDialogWin.ShowDialogWin("Invalid price!", "Error");
                        return;
                    }
                    quotePriceForInstrument.AdjustCurrentPrice(quotePriceClient.AdjustSingle, quotePriceClient, false);
                    break;
                case "Adjust":
                    quotePriceForInstrument = e.Cell.Row.Data as QuotePriceForInstrument;

                    isValidPrice = quotePriceForInstrument.IsValidPrice(quotePriceForInstrument.Adjust);
                    if (isValidPrice.HasValue)
                    {
                        if (isValidPrice.Value
                        && MessageBox.Show("Invalid price,continue?", "Alert!", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        {
                            return;
                        }
                    }
                    else
                    {
                        this._CommonDialogWin.ShowDialogWin("Invalid price!", "Error");
                        return;
                    }
                    quotePriceForInstrument.AdjustCurrentPrice(quotePriceForInstrument.Adjust, true);
                    break;
                case "AdjustLot":
                    quotePriceForInstrument = e.Cell.Row.Data as QuotePriceForInstrument;
                    quotePriceForInstrument.AdjustCurrentLot(quotePriceForInstrument.AdjustLot, true);
                    break;
            }
        }

        void QuoteHandler_Click(object sender, RoutedEventArgs e)
        {
            this.QuotePriceGrid.ExitEditMode(true);

            Button btn = sender as Button;
            QuotePriceForInstrument quotePriceForInstrument;
            QuotePriceClient quotePriceClient;
            List<Manager.Common.Settings.QuoteQuotation> quoteQuotations = new List<Manager.Common.Settings.QuoteQuotation>();

            try
            {
                switch (btn.Name)
                {
                    case "AbandonSingleBtn":
                        quotePriceClient = ((UnboundColumnDataContext)btn.DataContext).RowData as QuotePriceClient;
                        quotePriceForInstrument = this._ClientQuotePriceForInstrument.SingleOrDefault(P => P.InstrumentClient.Id == quotePriceClient.InstrumentId);
                        btn.IsEnabled = false;
                        this.AbandonQuotePrice(quotePriceClient, quotePriceForInstrument, true, btn);
                        quotePriceForInstrument.RemoveSendQuotePrice(quotePriceClient);
                        break;
                    case "UpdateSingleBtn":
                        quotePriceClient = ((UnboundColumnDataContext)btn.DataContext).RowData as QuotePriceClient;
                        quotePriceForInstrument = this._ClientQuotePriceForInstrument.SingleOrDefault(P => P.InstrumentClient.Id == quotePriceClient.InstrumentId);
                        quotePriceForInstrument.UpdateCurrentPrice(quotePriceClient);
                        break;
                    case "SendSingleBtn":
                        quotePriceClient = ((UnboundColumnDataContext)btn.DataContext).RowData as QuotePriceClient;
                        quotePriceForInstrument = this._ClientQuotePriceForInstrument.SingleOrDefault(P => P.InstrumentClient.Id == quotePriceClient.InstrumentId);
                        btn.IsEnabled = false;
                        quotePriceForInstrument.QuotePriceClients.Remove(quotePriceClient);
                        this.SendQuotePrice(quotePriceClient, quotePriceForInstrument, true, btn);
                        quotePriceForInstrument.RemoveSendQuotePrice(quotePriceClient);
                        break;
                    case "AbandonBtn":
                        quotePriceForInstrument = ((UnboundColumnDataContext)btn.DataContext).RowData as QuotePriceForInstrument;
                        btn.IsEnabled = false;
                        this.AbandonQuotePrice(null, quotePriceForInstrument, false, btn);
                        quotePriceForInstrument.RemoveSendQuotePrice(quotePriceForInstrument.QuotePriceClients);
                        this._ClientQuotePriceForInstrument.Remove(quotePriceForInstrument);
                        break;
                    case "UpdateBtn":
                        quotePriceForInstrument = ((UnboundColumnDataContext)btn.DataContext).RowData as QuotePriceForInstrument;
                        quotePriceForInstrument.UpdateCurrentPrice();
                        break;
                    case "SendBtn":
                        quotePriceForInstrument = ((UnboundColumnDataContext)btn.DataContext).RowData as QuotePriceForInstrument;
                        btn.IsEnabled = false;
                        this._ClientQuotePriceForInstrument.Remove(quotePriceForInstrument);
                        this.SendQuotePrice(null, quotePriceForInstrument, false, btn);
                        btn.Width = 0;
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "QutePriceControl.QuoteHandler_Click error:\r\n{0}", ex.ToString());
            }
        }

        void QuantityHandler_Click(object sender, RoutedEventArgs e)
        {
            this.QuotePriceGrid.ExitEditMode(true);
            RadioButton radioBtn = sender as RadioButton;
            QuotePriceForInstrument quotePriceForInstrument;
            try
            {
                switch (radioBtn.Name)
                {
                    case "AboveRadio":
                        quotePriceForInstrument = ((UnboundColumnDataContext)radioBtn.DataContext).RowData as QuotePriceForInstrument;
                        quotePriceForInstrument.OnEnquiryQuantity(true);
                        break;
                    case "BelowRadio":
                        quotePriceForInstrument = ((UnboundColumnDataContext)radioBtn.DataContext).RowData as QuotePriceForInstrument;
                        quotePriceForInstrument.OnEnquiryQuantity(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "QutePriceControl.QuantityHandler_Click error:\r\n{0}", ex.ToString());
            }
        }

        void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            this.QuotePriceGrid.ExitEditMode(true);
            CheckBox chk = sender as CheckBox;
            QuotePriceForInstrument quotePriceForInstrument;
            try
            {
                switch (chk.Name)
                {
                    case "SelectedAllChk":
                        quotePriceForInstrument = (chk.DataContext) as QuotePriceForInstrument;
                        if (chk.IsChecked.HasValue)
                        {
                            quotePriceForInstrument.SelectAllQuotePrice(chk.IsChecked.Value);
                        }
                        break;
                    case "SelectSigleChk":
                        QuotePriceClient quotePriceClient = (chk.DataContext) as QuotePriceClient;
                        quotePriceForInstrument = this._ClientQuotePriceForInstrument.SingleOrDefault(P => P.InstrumentClient.Id == quotePriceClient.InstrumentId);
                        quotePriceForInstrument.OnEmptyCheckBoxEvent();
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "QutePriceControl.SelectedAll_Checked error:\r\n{0}", ex.ToString());
            }
        }

        void AdjustPrice_Click(object sender, RoutedEventArgs e)
        {
            this.QuotePriceGrid.ExitEditMode(true);
            Button btn = sender as Button;
            QuotePriceForInstrument quotePriceForInstrument;
            bool? isValidPrice = null;
            try
            {
                switch (btn.Name)
                {
                    case "UpButton":
                        quotePriceForInstrument = (btn.DataContext) as QuotePriceForInstrument;
                        isValidPrice = quotePriceForInstrument.IsValidPrice(1);

                        if (this.CheckPrice(isValidPrice))
                        {
                            quotePriceForInstrument.AdjustCurrentPrice(1, true);

                        }
                        break;
                    case "DownButton":
                        quotePriceForInstrument = (btn.DataContext) as QuotePriceForInstrument;
                         isValidPrice = quotePriceForInstrument.IsValidPrice(1);

                         if (this.CheckPrice(isValidPrice))
                         {
                             quotePriceForInstrument.AdjustCurrentPrice(-1, true);
                         }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(TraceEventType.Error, "QutePriceControl.AdjustPrice_Click error:\r\n{0}", ex.ToString());
            }
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

        void MainPage_KeyUp(object sender, KeyEventArgs e)
        {
            ModifierKeys modifierKeys = Keyboard.Modifiers;
            if ((int)modifierKeys == 3)
            {
                IEnumerable<Row> rows;
                switch (e.Key)
                {
                    case Key.A: //QuotePrice aBandon
                        rows = this.QuotePriceGrid.Rows.Where(P => P.IsSelected || P.IsActive);
                        if (rows != null && rows.Count() > 0)
                        {
                            foreach (Row row in rows)
                            {
                                QuotePriceForInstrument clientQuotePriceForInstrument;
                                QuotePriceClient quotePriceClient;
                                this.QuotePriceGrid.ExitEditMode(true);
                                var btn = new Button();
                                if (row.ColumnLayout.Key == "QuotePriceClients")
                                {
                                    //single
                                    quotePriceClient = row.Data as QuotePriceClient;
                                    clientQuotePriceForInstrument = this._ClientQuotePriceForInstrument.SingleOrDefault(P => P.InstrumentClient.Id == quotePriceClient.InstrumentId);
                                    this.AbandonQuotePrice(quotePriceClient, clientQuotePriceForInstrument, true, btn);
                                }
                                else
                                {
                                    clientQuotePriceForInstrument = row.Data as QuotePriceForInstrument;
                                    this.AbandonQuotePrice(null, clientQuotePriceForInstrument, false, btn);
                                }
                            }
                        }
                        break;
                    case Key.U: //QuotePrice Update
                        rows = this.QuotePriceGrid.Rows.Where(P => P.IsSelected || P.IsActive);
                        if (rows != null && rows.Count() > 0)
                        {
                            foreach (Row row in rows)
                            {
                                QuotePriceForInstrument clientQuotePriceForInstrument;
                                QuotePriceClient clientQuotePrice;
                                this.QuotePriceGrid.ExitEditMode(true);
                                if (row.ColumnLayout.Key == "QuotePriceClients")
                                {
                                    //single
                                    clientQuotePrice = row.Data as QuotePriceClient;
                                    clientQuotePriceForInstrument = this._ClientQuotePriceForInstrument.SingleOrDefault(P => P.InstrumentClient.Id == clientQuotePrice.InstrumentId);
                                    clientQuotePriceForInstrument.UpdateCurrentPrice(clientQuotePrice);
                                }
                                else
                                {
                                    clientQuotePriceForInstrument = row.Data as QuotePriceForInstrument;
                                    clientQuotePriceForInstrument.UpdateCurrentPrice();
                                }
                            }
                        }
                        break;
                    case Key.S: //QuotePrice Send
                        rows = this.QuotePriceGrid.Rows.Where(P => P.IsSelected || P.IsActive);
                        if (rows != null && rows.Count() > 0)
                        {
                            foreach (Row row in rows)
                            {
                                QuotePriceForInstrument clientQuotePriceForInstrument;
                                QuotePriceClient quotePriceClient;
                                this.QuotePriceGrid.ExitEditMode(true);
                                var btn = new Button();
                                if (row.ColumnLayout.Key == "QuotePriceClients")
                                {
                                    //single
                                    quotePriceClient = row.Data as QuotePriceClient;
                                    clientQuotePriceForInstrument = this._ClientQuotePriceForInstrument.SingleOrDefault(P => P.InstrumentClient.Id == quotePriceClient.InstrumentId);
                                    this.SendQuotePrice(quotePriceClient, clientQuotePriceForInstrument, true, btn);
                                }
                                else
                                {
                                    clientQuotePriceForInstrument = row.Data as QuotePriceForInstrument;
                                    this.SendQuotePrice(null, clientQuotePriceForInstrument, false, btn);
                                }
                            }
                        }
                        break;
                }
            }
        }
        #endregion

        #region Send Message
        public void AbandonQuotePrice(QuotePriceClient quotePrice, QuotePriceForInstrument quotePriceForInstrument, bool isSigle, Button btn)
        {
            if (quotePriceForInstrument != null)
            {
                List<Answer> quoteQuotations = new List<Answer>();
                if (isSigle)
                {
                    if (quotePrice != null)
                    {
                        quotePrice.InstrumentCode = quotePriceForInstrument.InstrumentClient.Code;
                        quoteQuotations.Add(quotePrice.ToSendQutoPrice());
                    }
                }
                else
                {
                    quoteQuotations = quotePriceForInstrument.GetSelectedQuotePriceForAccount();
                }
                if (quoteQuotations.Count > 0)
                {
                    var AbandonQuoteEventLog = new EventLogEntity(Guid.NewGuid())
                    {
                       //write log
                    };
                }
                //Notify service
                ConsoleClient.Instance.AbandonQuote(quoteQuotations);
            }
        }

        private void SendQuotePrice(QuotePriceClient quotePrice, QuotePriceForInstrument quotePriceForInstrument, bool isSingle, Button btn)
        {
            if (quotePriceForInstrument != null)
            {
                List<Answer> ToSendQutoPrices = new List<Answer>();
                if (isSingle)
                {
                    if (quotePrice != null)
                    {
                        quotePrice.InstrumentCode = quotePriceForInstrument.InstrumentClient.Code;
                        ToSendQutoPrices.Add(quotePrice.ToSendQutoPrice());
                    }
                }
                else
                {
                    ToSendQutoPrices = quotePriceForInstrument.GetSelectedQuotePriceForAccount();
                }
                if (ToSendQutoPrices.Count > 0)
                {
                    var ConfirmedQuoteEventLog = new EventLogEntity(Guid.NewGuid())
                    {
                        //Write Log
                    };
                    object[] abandonStatus = new object[] { ConfirmedQuoteEventLog, btn };
                    ConsoleClient.Instance.SendQuotePrice(ToSendQutoPrices);
                }
            }
        }
        #endregion

        #region 通知处理
        void MessageClient_QuotePriceReceived(QuoteMessage quoteMessage)
        {
            if (!this._QuoteTimer.IsEnabled)
            {
                this._QuoteTimer.Start();
            }
        }

        #endregion
    }
}
