using Infragistics.Windows.Reporting;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
                    this._ToDatePicker.Value = DateTime.Now;
                    this._OrderTypeCombo.ItemsSource = System.Enum.GetNames(typeof(OrderType));
                    this._OrderTypeCombo.SelectedIndex = 0;
                    this.GetComboListData();

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

        private void GetComboListData()
        {
            InstrumentClient allInstrument = new InstrumentClient();
            allInstrument.Code = "All";

            foreach (string exchangeCode in this._App.ExchangeDataManager.ExchangeCodes)
            {
                ExchangeSettingManager settingManager = this._App.ExchangeDataManager.GetExchangeSetting(exchangeCode);

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


            AccountGroup allGroup = new AccountGroup();
            allGroup.Code = "All";
            foreach (string exchangeCode in this._App.ExchangeDataManager.ExchangeCodes)
            {
                ExchangeSettingManager settingManager = this._App.ExchangeDataManager.GetExchangeSetting(exchangeCode);

                foreach (AccountGroup group in settingManager.GetAccountGroups())
                {
                    this._AccountGroups.Add(group);
                }
            }
            this._AccountGroups.Insert(0, allGroup);
            this._AccountGroupCombo.ItemsSource = this._AccountGroups;
            this._AccountGroupCombo.DisplayMemberPath = "Code";
            this._AccountGroupCombo.SelectedIndex = 0;
            this._AccountGroupCombo.SelectedValuePath = "Id";
            this._AccountGroupCombo.SelectedItem = allGroup;

            this.ExchangeComboBox.ItemsSource = this._App.ExchangeDataManager.ExchangeCodes;
            this.ExchangeComboBox.SelectedItem = this._App.ExchangeDataManager.ExchangeCodes[0];
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
        private void PrintGrid()
        {
            Report reportObj = new Report();
            EmbeddedVisualReportSection section = new EmbeddedVisualReportSection(this._OrderSerchGrid);
            reportObj.Sections.Add(section);

            _OrderReportPreview.GeneratePreview(reportObj, false, false);
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
