using Infragistics.Controls.Grids;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Logger = Manager.Common.Logger;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for DealingInstanceOrder.xaml
    /// </summary>
    public partial class DealingInstanceOrder : UserControl
    {
        private ManagerConsole.MainWindow _App;
        private ObservableCollection<InstrumentClient> _InstrumentList = new ObservableCollection<InstrumentClient>();
        private Style _ExecuteStatusStyle;
        private ProcessInstantOrder _ProcessInstantOrder;
        public DealingInstanceOrder()
        {
            InitializeComponent();
            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._ProcessInstantOrder = this._App.InitDataManager.ProcessInstantOrder;
            this.InitializeData();
            this.BindGridData();
            this.GetComboBoxData();
            this.AttachEvent();
        }

        private void InitializeData()
        {
            Color bgColor = Colors.Transparent;
            Style style = new Style(typeof(Infragistics.Controls.Grids.CellControl));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(bgColor)));
            this._ExecuteStatusStyle = App.Current.Resources["ExecuteSatusCellStyle"] as Style;
        }

        private void AttachEvent()
        {
            this._ProcessInstantOrder.OnSettingFirstRowStyleEvent += new ProcessInstantOrder.SettingFirstRowStyleHandle(SettingFirstRowBackGround);
        }

        private void GetComboBoxData()
        {
            InstrumentClient allInstrument = new InstrumentClient();
            allInstrument.Code = "All";

            foreach (string exchangeCode in this._App.InitDataManager.ExchangeCodes)
            {
                ExchangeSettingManager settingManager = this._App.InitDataManager.GetExchangeSetting(exchangeCode);

                foreach (InstrumentClient instrument in settingManager.Instruments.Values)
                {
                    this._InstrumentList.Add(instrument);
                }
            }
            
            this._InstrumentList.Insert(0, allInstrument);
            this._InstrumentCombo.ItemsSource = this._InstrumentList;
            this._InstrumentCombo.DisplayMemberPath = "Code";
            this._InstrumentCombo.SelectedIndex = 0;
            this._InstrumentCombo.SelectedValuePath = "Id";
            this._InstrumentCombo.SelectedItem = allInstrument;
        }

        private void BindGridData()
        {
            this.LayRootGrid.DataContext = this._ProcessInstantOrder.InstantOrderForInstrument;
            this._OrderTaskGrid.ItemsSource = this._ProcessInstantOrder.OrderTasks;
        }

        private void FilterOrderByInstrument(InstrumentClient instrument)
        {
            if (instrument.Id == Guid.Empty) //All
            {
                this._OrderTaskGrid.ItemsSource = this._ProcessInstantOrder.OrderTasks;
                return;
            }
            this._OrderTaskGrid.ItemsSource = this._ProcessInstantOrder.OrderTasks.Where(P => P.InstrumentId == instrument.Id);
            this._ProcessInstantOrder.InitializeBinding(instrument.Id);
            this.LayRootGrid.DataContext = this._ProcessInstantOrder.InstantOrderForInstrument;
        }

        #region Grid Event
        #endregion

        #region Dealing Order Event
        private void AdjustBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this._ProcessInstantOrder.OrderTasks.Count == 0) return;
            e.Handled = true;
            Button btn = sender as Button;
            
            switch (btn.Name)
            {
                case "_UpPriceButton":
                    this._ProcessInstantOrder.AdjustPrice(true);
                    break;
                case "_DownPriceButton":
                    this._ProcessInstantOrder.AdjustPrice(false);
                    break;
                case "_IncreaseSellAutoPointsButton":
                    this._ProcessInstantOrder.AdjustAutoPointVariation(false, true);
                    break;
                case "_DecreaseSellAutoPointsButton":
                    this._ProcessInstantOrder.AdjustAutoPointVariation(false, false);
                    break;
                case "_IncreaseBuyAutoPointsButton":
                    this._ProcessInstantOrder.AdjustAutoPointVariation(true, true);
                    break;
                case "_DecreaseBuyAutoPointsButton":
                    this._ProcessInstantOrder.AdjustAutoPointVariation(true, false);
                    break;
                default: break;
            }
        }

        private void OrderHandlerBtn_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (this._ProcessInstantOrder.OrderTasks.Count == 0) return;
            Button btn = sender as Button;
            OrderTask orderTask = null;
            CellDataDefine currentCellData = null;
            Guid orderId = this._ProcessInstantOrder.InstantOrderForInstrument.OrderId;
            switch (btn.Name)
            {
                case "_ExecutedButton":
                    orderTask = this._ProcessInstantOrder.OrderTasks.First(P => P.OrderId == orderId);
                    currentCellData = orderTask.DQCellDataDefine1;
                    break;
                case "_RejectButton":
                    orderTask = this._ProcessInstantOrder.OrderTasks.First(P => P.OrderId == orderId);
                    currentCellData = orderTask.DQCellDataDefine2;
                    break;
                case "DQAcceptBtn":
                    orderTask = btn.DataContext as OrderTask;
                    currentCellData = orderTask.DQCellDataDefine1;
                    break;
                case "DQRejectBtn":
                    orderTask = btn.DataContext as OrderTask;
                    currentCellData = orderTask.DQCellDataDefine2;
                    break;
                case "_ExecuteRejectSellButton":
                    this.ExcuteAllDQOrder("WF01",false);
                    return;
                case "_ExecuteRejectBuyButton":
                    this.ExcuteAllDQOrder("WF01",true);
                    return;
                default:
                    break;
            }
            try
            {
                this.ProcessPendingOrder(orderTask, currentCellData);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "DealingInstanceOrder.OrderHandlerBtn_Click Error\r\n{0}", ex.ToString());
                this._App._CommonDialogWin.ShowDialogWin("Dealing Instance Order Error", "Error");
            }
        }

        private void ExcuteAllDQOrder(string exchangeCode,bool isBuy)
        {
            ExchangeSettingManager settingManager = this._App.InitDataManager.GetExchangeSetting(exchangeCode);
            
            for (int i = 0; i < this._OrderTaskGrid.Rows.Count; i++)
            {
                OrderTask order = this._OrderTaskGrid.Rows[i].Data as OrderTask;
                bool buySell = (order.IsBuy == BuySell.Buy);
                if (buySell != isBuy) continue;

                int currentDQVarition = isBuy ? this._ProcessInstantOrder.InstantOrderForInstrument.BuyVariation:this._ProcessInstantOrder.InstantOrderForInstrument.SellVariation;
                string marketPrice = isBuy ? order.Instrument.Ask : order.Instrument.Bid;
                InstrumentClient instrument = order.Transaction.Instrument;
                int acceptDQVarition = this._ProcessInstantOrder.CheckDQVariation(instrument, currentDQVarition);

                //just test
                Customer customer = settingManager.GetCustomer(new Guid("4556F2F0-3C1B-4C27-AA67-03DE2D0C1E0C"));
                QuotePolicyDetail quotePolicyDetail = settingManager.GetQuotePolicyDetail(order.InstrumentId.Value, customer);

                bool isAllowed = this._ProcessInstantOrder.AllowAccept(order, quotePolicyDetail, isBuy, marketPrice, currentDQVarition);
                if (isAllowed)
                {
                    this._App.OrderHandle.OnOrderAccept(order);
                }
                else
                {
                    this._App.OrderHandle.OnOrderReject(order);
                }
            }
        }

        private void ProcessPendingOrder(OrderTask orderTask, CellDataDefine currentCellData)
        {
            HandleAction actionType = currentCellData.Action;
            bool isEnabled;
            switch (actionType)
            {
                case HandleAction.None:
                    break;
                case HandleAction.OnOrderAccept:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        this._App.OrderHandle.OnOrderAccept(orderTask);
                    }
                    break;
                case HandleAction.OnOrderReject:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        this._App.OrderHandle.OnOrderReject(orderTask);
                    }
                    break;
                default: break;
            }
        }

        #endregion

        private void ToolBar_Click(object sender, RoutedEventArgs e)
        {
            this.ShowGroupPanel();
        }

        private void InstrumentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InstrumentClient instrument = (InstrumentClient)this._InstrumentCombo.SelectedItem;
            if (instrument == null) return;
            this.FilterOrderByInstrument(instrument);
        }

        private void SettingFirstRowBackGround()
        {
            this._OrderTaskGrid.Rows[0].CellStyle = this._ExecuteStatusStyle;
        }

        private void ShowGroupPanel()
        {
            if (this._OrderTaskGrid.GroupBySettings.AllowGroupByArea == GroupByAreaLocation.Hidden)
            {
                this._OrderTaskGrid.GroupBySettings.AllowGroupByArea = Infragistics.Controls.Grids.GroupByAreaLocation.Top;
                this._OrderTaskGrid.GroupBySettings.EmptyGroupByAreaContent = "拖动列在此分组";
            }
            else
            {
                this._OrderTaskGrid.GroupBySettings.AllowGroupByArea = Infragistics.Controls.Grids.GroupByAreaLocation.Hidden;
            }
        }

        
    }
}
