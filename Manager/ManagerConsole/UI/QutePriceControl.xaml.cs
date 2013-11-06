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
        private ObservableCollection<QuotePriceClient> _EnquiryTimeOutQuotePriceClients = new ObservableCollection<QuotePriceClient>();
        private DispatcherTimer _QuoteTimer;
        private CommonDialogWin _CommonDialogWin;
        private ConfirmDialogWin _ConfirmDialogWin;
       
        
        public QutePriceControl()
        {
            InitializeComponent();
        }

        void onLoad(object sender, RoutedEventArgs e)
        {
            this._CommonDialogWin = new CommonDialogWin(this.LayoutRoot);
            this._ConfirmDialogWin = new ConfirmDialogWin(this.LayoutRoot);
            this.AttachEvent();
            this.InitData();
            this.BindGridData();
            this.TimerHandle();
        }
        private void InitData()
        { 

        }
        private void BindGridData()
        {
            this.QuotePriceGrid.ItemsSource = this._ClientQuotePriceForInstrument;
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
            this._ConfirmDialogWin.OnConfirmDialogResult += new ConfirmDialogWin.ConfirmDialogResultHandle(ExcuteOrder);
        }
        #endregion

        void ExcuteOrder(bool yesOrNo, UIElement uIElement)
        {
            if (yesOrNo)
            {
            }
        }

        void Quote_Tick(object sender, EventArgs e)
        {
            if (this._ClientQuotePriceForInstrument != null && this._ClientQuotePriceForInstrument.Count > 0)
            {
                for(int i=0;i<this._ClientQuotePriceForInstrument.Count;i++)
                {
                    var quotePriceClients = this._ClientQuotePriceForInstrument[i].QuotePriceClients;
                    for(int j=0;j<quotePriceClients.Count;j++)
                    {
                        QuotePriceClient quotePriceEntity = quotePriceClients[j];
                         quotePriceEntity.WaitTimes = quotePriceEntity.WaitTimes > 0 ? quotePriceEntity.WaitTimes - 1 : quotePriceEntity.WaitTimes;
                        if (quotePriceEntity.WaitTimes == 0)
                        {
                            this._ClientQuotePriceForInstrument[i].RemoveSendQuotePrice(quotePriceEntity);
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
            List<Manager.Common.QuoteQuotation> quoteQuotations = new List<Manager.Common.QuoteQuotation>();

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
                string exchangeCode = string.Empty;
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
            this.Dispatcher.BeginInvoke((Action<QuoteMessage>)delegate(QuoteMessage result)
            {
                //声音处理
                MediaManager.PlayMedia(this._Media, MediaManager._EnquiryMediaSource);

                int waiteTime = 50;     //取初始化数据系统参数
                Guid customerId = result.CustomerID;
                //通过CustomerId获取Customer对象
                //var customer = this._Customers.SingleOrDefault(P => P.id == customerId);
                var customer = new Customer();
                customer.Id = result.CustomerID;
                customer.Code = "WF007";
                QuotePriceClient quotePriceClient = new QuotePriceClient(result, waiteTime, customer);
                QuotePriceForInstrument clientQuotePriceForInstrument = null;
                clientQuotePriceForInstrument = this._ClientQuotePriceForInstrument.SingleOrDefault(P => P.InstrumentClient.Id == quotePriceClient.InstrumentId);
                if (clientQuotePriceForInstrument == null)
                {
                    //从内存中获取Instrument
                    //var instrumentEntity = this._Instruments.SingleOrDefault(P => P.InstrumentId == clientQuotePriceForAccount.InstrumentId);
                    clientQuotePriceForInstrument = new QuotePriceForInstrument();
                    var instrument = this.GetInstrument(quotePriceClient);
                    clientQuotePriceForInstrument.InstrumentClient = instrument;
                    clientQuotePriceForInstrument.Origin = instrument.Origin;
                    clientQuotePriceForInstrument.Adjust = decimal.Parse(instrument.Origin);
                    clientQuotePriceForInstrument.AdjustLot = quotePriceClient.QuoteLot;
                    this._ClientQuotePriceForInstrument.Add(clientQuotePriceForInstrument);
                }
                clientQuotePriceForInstrument.OnEmptyQuotePriceClient += new QuotePriceForInstrument.EmptyQuotePriceHandle(ClientQuotePriceForInstrument_OnEmptyClientQuotePriceClient);
                //clientQuotePriceForInstrument.OnEmptyCheckBoxClient += new QuotePriceForInstrument.EmptyCheckBoxHandle(ClientQuotePriceForInstrument_OnEmptyCheckBoxClient);
                clientQuotePriceForInstrument.AddNewQuotePrice(quotePriceClient);

                if (!this._QuoteTimer.IsEnabled)
                {
                    this._QuoteTimer.Start();
                }

            }, quoteMessage);
        }

        void ClientQuotePriceForInstrument_OnEmptyClientQuotePriceClient(QuotePriceForInstrument clientQuotePriceForInstrument)
        {
            this._ClientQuotePriceForInstrument.Remove(clientQuotePriceForInstrument);
        }


        #endregion

        #region 测试数据

        private string GetCode() 
        { 
            int number; 
            char code; 
            string checkCode = String.Empty; 
            Random random = new Random(); 
            for (int i = 0; i < 4; i++) { number = random.Next(); 
                if (number % 2 == 0)               
                code = (char)('0' + (char)(number % 10)); 
            else                
                code = (char)('A' + (char)(number % 26)); 
                checkCode += code.ToString(); } return checkCode; 
        }
        private InstrumentClient GetInstrument(QuotePriceClient quotePriceClient)
        {
            //Property not empty
            InstrumentClient instrument = new InstrumentClient();
            instrument.Id = quotePriceClient.InstrumentId;
            instrument.Code = "GBP" + GetCode();
            instrument.Origin = "1.1";
            instrument.Ask = "1.5";
            instrument.Bid = "1.2";
            instrument.Denominator = 10;
            instrument.NumeratorUnit = 1;
            instrument.MaxAutoPoint = 100;
            instrument.MaxSpread = 100;
            //instrument.AlertPoint = 10;
            instrument.AutoPoint = 1;
            instrument.Spread = 3;
            
            return instrument;
        }
        #endregion

        #region 异步回调事件
        private void AbandonQuotePriceCallBack(int abandonResult)
        {
            try
            {
                this.Dispatcher.BeginInvoke((Action<int>)delegate(int result)
                {
                    MessageBox.Show(result.ToString());
                }, abandonResult
            );
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "AbandonQuotePriceCallBack.\r\n{0}", ex.ToString());
            }
        }

        private void ConfirmQuotePriceCallBack(int confirmResult)
        {
            try
            {
                this.Dispatcher.BeginInvoke((Action<int>)delegate(int result)
                {
                    MessageBox.Show(result.ToString());
                }, confirmResult
            );
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ConfirmQuotePriceCallBack.\r\n{0}", ex.ToString());
            }
        }
        #endregion

    }
}
