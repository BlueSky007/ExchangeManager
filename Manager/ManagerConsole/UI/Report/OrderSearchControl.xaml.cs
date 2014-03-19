using Infragistics.Windows.Reporting;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using OrderType = iExchange.Common.OrderType;
using CommonOrderQueryEntity = Manager.Common.ReportEntities.OrderQueryEntity;
using System.Windows.Media.Animation;
namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for OrderSearchControl.xaml
    /// </summary>
    public partial class OrderSearchControl : UserControl,IControlLayout
    {
        private ManagerConsole.MainWindow _App;
        private Storyboard _RetrievalCircleAnimation;
        private ObservableCollection<OrderQueryEntity> _OrderQueryEntities = new ObservableCollection<OrderQueryEntity>();
        private ObservableCollection<InstrumentClient> _InstrumentList = new ObservableCollection<InstrumentClient>();
        private ObservableCollection<AccountGroup> _AccountGroups = new ObservableCollection<AccountGroup>();
        public OrderSearchControl()
        {
            InitializeComponent();

            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            Thread thread = new Thread(new ThreadStart(delegate()
            {
                while (!this.InilizeUI())
                {
                    Thread.Sleep(800);
                }
            }));
            thread.IsBackground = true;
            thread.Start();
        }

        private bool InilizeUI()
        {
            if (this._App.ExchangeDataManager.IsInitializeCompleted)
            {
                this.Dispatcher.BeginInvoke((Action)delegate()
                {
                    this._FromDatePicker.Value = DateTime.Now;
                    this._ToDatePicker.Value = DateTime.Now + TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59) + TimeSpan.FromSeconds(59);
                    this._OrderTypeCombo.ItemsSource = System.Enum.GetNames(typeof(OrderType));
                    this._OrderTypeCombo.SelectedIndex = 0;
                    this.InitilizeComboListData();

                    this._OrderSerchGrid.ItemsSource = this._OrderQueryEntities;
                    this._RetrievalCircleAnimation = this.Resources["RetrievalCircleAnimation"] as Storyboard;
                });
                return true;
            }
            else
            {
                return false;
            }
        }

        private void LoadComboListData(string exchangeCode)
        {
            this._InstrumentList.Clear();
            this._AccountGroups.Clear();
            ExchangeSettingManager settingManager = this._App.ExchangeDataManager.GetExchangeSetting(exchangeCode);

            foreach (InstrumentClient instrument in settingManager.GetInstruments())
            {
                this._InstrumentList.Add(instrument);
            }
            foreach (AccountGroup group in settingManager.GetAccountGroups())
            {
                this._AccountGroups.Add(group);
            }
            InstrumentClient allInstrument = new InstrumentClient() { Code = "All" };
            AccountGroup allAccountGroup = new AccountGroup(){Code = "All"};
            this._InstrumentList.Insert(0, allInstrument);
            this._AccountGroups.Insert(0, allAccountGroup);

            this._InstrumentCombo.ItemsSource = this._InstrumentList;
            this._InstrumentCombo.DisplayMemberPath = "Code";
            this._InstrumentCombo.SelectedValuePath = "Id";
            this._InstrumentCombo.SelectedItem = allInstrument;

            this._AccountGroupCombo.ItemsSource = this._AccountGroups;
            this._AccountGroupCombo.DisplayMemberPath = "Code";
            this._AccountGroupCombo.SelectedValuePath = "Id";
            this._AccountGroupCombo.SelectedItem = allAccountGroup;
        }

        private void InitilizeComboListData()
        {
            this.ExchangeComboBox.ItemsSource = this._App.ExchangeDataManager.ExchangeCodes;
            this.ExchangeComboBox.SelectedItem = this._App.ExchangeDataManager.ExchangeCodes[0];

            this.LoadComboListData(this._App.ExchangeDataManager.ExchangeCodes[0]);
        }

        private void ToolBar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            switch (btn.Name)
            {
                case "_QueryButton":
                    this.QueryOrder();
                    break;
                case "_PrintBtn":
                    this.PrintGrid();
                    break;
            }
        }

        private void ExchangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string exchangeCode = this.ExchangeComboBox.SelectedItem.ToString();
            this.LoadComboListData(exchangeCode);
        }

        private void PrintGrid()
        {
            Report reportObj = new Report();
            EmbeddedVisualReportSection section = new EmbeddedVisualReportSection(this._OrderSerchGrid);
            reportObj.Sections.Add(section);

            this._OrderReportPreview.GeneratePreview(reportObj, false, false);
            tbiPreview.IsSelected = true;
        }

        private void QueryOrder()
        {
            try
            {
                bool isExecute = this._QueryTypeCombo.SelectedIndex == 0 ? true : false;
                InstrumentClient instrument = (InstrumentClient)this._InstrumentCombo.SelectedItem;
                AccountGroup group = (AccountGroup)this._AccountGroupCombo.SelectedItem;
                DateTime fromDate = DateTime.Parse(this._FromDatePicker.Text);
                DateTime toDate = DateTime.Parse(this._ToDatePicker.Text);
                string exchangeCode = this.ExchangeComboBox.SelectedItem.ToString();

                OrderType orderType = (OrderType)Enum.ToObject(typeof(OrderType), this._OrderTypeCombo.SelectedIndex);

                this.RetrieveStoredLinksMask.Visibility = Visibility.Visible;
                this._RetrievalCircleAnimation.Begin();

                ConsoleClient.Instance.GetOrderByInstrument(exchangeCode, instrument.Id, group.Id, orderType, isExecute, fromDate, toDate, GetOrderByInstrumentCallback);
            }
            catch (Exception ex)
            {
                Manager.Common.Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "OrderSearchControl.QueryOrder.\r\n{0}", ex.ToString());
            }
        }

        private void GetOrderByInstrumentCallback(List<CommonOrderQueryEntity> queryOrders)
        {
            this.Dispatcher.BeginInvoke((Action<List<CommonOrderQueryEntity>>)delegate(List<CommonOrderQueryEntity> result)
            {
                foreach (CommonOrderQueryEntity entity in result)
                {
                    OrderQueryEntity orderEntity = new OrderQueryEntity(entity);
                    this._OrderQueryEntities.Add(orderEntity);
                    this.RetrieveStoredLinksMask.Visibility = Visibility.Collapsed;
                    this._RetrievalCircleAnimation.Stop();
                }
            }, queryOrders);
        }

        #region 布局
        /// <summary>
        /// Layout format:
        /// <GridSettings>
        ///    <ColumnsWidth Data="53,0,194,70,222,60,89,60,80,80,80,70,80,80,80,60,60,59,80,80,80,100,80,150,80,"/>
        /// </GridSettings>

        public string GetLayout()
        {
            //InstrumentCode
            StringBuilder layoutBuilder = new StringBuilder();
            layoutBuilder.Append("<GridSettings>");
            layoutBuilder.Append(ColumnWidthPersistence.GetPersistentColumnsWidthString(this._OrderSerchGrid));
            layoutBuilder.Append("</GridSettings>");
            return layoutBuilder.ToString();
        }

        public void SetLayout(XElement layout)
        {
            try
            {
                if (layout.HasElements)
                {
                    XElement columnWidthElement = layout.Element("GridSettings").Element("ColumnsWidth");
                    if (columnWidthElement != null)
                    {
                        ColumnWidthPersistence.LoadColumnsWidth(this._OrderSerchGrid, columnWidthElement);
                    }
                }
            }
            catch (Exception ex)
            {
                Manager.Common.Logger.AddEvent(System.Diagnostics.TraceEventType.Error, "OrderSearchControl.SetLayout\r\n{0}", ex.ToString());
            }
        }
        #endregion

    }
}
