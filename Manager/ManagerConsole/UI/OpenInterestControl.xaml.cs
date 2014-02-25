using Infragistics.Controls.Grids;
using Infragistics.Windows.Reporting;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using CommonAccountGroupGNP = iExchange.Common.Manager.AccountGroupGNP;
using CommonOpenInterestSummary = iExchange.Common.Manager.OpenInterestSummary;
using Logger = Manager.Common.Logger;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for OpenInterestControl.xaml
    /// </summary>
    public partial class OpenInterestControl : UserControl, IControlLayout
    {
        private ObservableCollection<RootGNP> _RootGNP = new ObservableCollection<RootGNP>();
        private ObservableCollection<OpenInterestSummary> _OpenInterestSummarys = new ObservableCollection<OpenInterestSummary>();
        private static IComparer<InstrumentGNP> _InstrumentGNPSummaryCompare = new InstrumentGNPSummaryCompare();
        private static IComparer<InstrumentGNP> _InstrumentGNPCompare = new InstrumentGNPCompare();
        private Hashtable _AllColumns = new Hashtable();
        private Hashtable _SummaryColumns = new Hashtable();
        private List<Guid> _InstrumentSummaryLoadings = new List<Guid>();
        private List<InstrumentGNP> _ColumnsList;
        private MainWindow _App;
        private Style _BuyCellStyle;
        private Style _SellCellStyle;
        private Style _BuySummaryGroupStyle;
        private Style _SellSummaryGroupStyle;
        private Style _GroupHeadStyle;
        private Style _SummaryGroupHeaderStyle;
        private Style _NormalHeaderStyle;
        public OpenInterestControl()
        {
            InitializeComponent();
            this._App = (MainWindow)Application.Current.MainWindow;
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
                    this.InitializeData();
                });
                return true;
            }
            else
            {
                return false;
            }
        }

        private void InitializeData()
        {
            this._BuyCellStyle = this.Resources["BuyCellStyle"] as Style;
            this._SellCellStyle = this.Resources["SellCellStyle"] as Style;
            this._BuySummaryGroupStyle = this.Resources["BuySummaryGroupCellStyle"] as Style;
            this._SellSummaryGroupStyle = this.Resources["SellSummaryGroupStyle"] as Style;
            this._GroupHeadStyle = this.Resources["GroupHeaderStyle"] as Style;
            this._SummaryGroupHeaderStyle = this.Resources["SummaryGroupHeaderStyle"] as Style;
            this._NormalHeaderStyle = this.Resources["NormalHeaderStyle"] as Style;

            this.ExchangeComboBox.ItemsSource = this._App.ExchangeDataManager.ExchangeCodes;
            this.ExchangeComboBox.SelectedItem = this._App.ExchangeDataManager.ExchangeCodes[0];
            this.ExchangeComboBox2.ItemsSource = this._App.ExchangeDataManager.ExchangeCodes;
            this.ExchangeComboBox2.SelectedItem = this._App.ExchangeDataManager.ExchangeCodes[0];
            this.QueryGroupNetPosition();
        }

        private List<InstrumentGNP> GetSortedInstrumentGNPs(Hashtable allColumns)
        {
            List<InstrumentGNP> columnsList = new List<InstrumentGNP>();

            foreach (InstrumentGNP de in this._AllColumns.Values)
            {
                columnsList.Add(de);
            }
            columnsList.Sort(_InstrumentGNPSummaryCompare);
            int i = 0;
            int GroupIndex = 0;
            string lastSummaryCode = string.Empty;
            foreach (InstrumentGNP de in columnsList)
            {
                if (lastSummaryCode != de.SummaryGroupCode)
                {
                    lastSummaryCode = de.SummaryGroupCode;
                    i = GroupIndex * 1000;
                    GroupIndex++;
                }
                de.SortIndex = de.IsSummaryGroup ? 99 + i : i;
                i++;
            }
            columnsList.Sort(_InstrumentGNPCompare);
            return columnsList;
        }

        private void SettingGridStyle(Row row, dynamic obj)
        {
            foreach (InstrumentGNP instrumentGNP in this._ColumnsList)
            {
                string key = "Columns[MyColumn" + instrumentGNP.ColumnIndex + "]";
                string index = "MyColumn" + instrumentGNP.ColumnIndex;

                Column column = row.Columns[key] as Column;

                if (instrumentGNP.IsSummaryGroup)
                {
                    row.Cells[column].Style = this._BuySummaryGroupStyle;
                }

                if (obj.Columns[index] == null) continue;

                if (obj is DetailGNP)
                {
                    if (instrumentGNP.IsSummaryGroup)
                    {
                        row.Cells[column].Style = this._BuySummaryGroupStyle;
                        column.HeaderText = "555";
                    }
                    continue;
                }
                if ((decimal)obj.Columns[index] >= 0)
                {
                    row.Cells[column].Style = instrumentGNP.IsSummaryGroup ? this._BuySummaryGroupStyle : this._BuyCellStyle;
                    row.Cells[column].Column.HeaderStyle = instrumentGNP.IsSummaryGroup ? this._SummaryGroupHeaderStyle : this._NormalHeaderStyle;
                }
                else if ((decimal)obj.Columns[index] < 0)
                {
                    row.Cells[column].Style = instrumentGNP.IsSummaryGroup ? this._SellSummaryGroupStyle : this._SellCellStyle;
                    row.Cells[column].Column.HeaderStyle = instrumentGNP.IsSummaryGroup ? this._SummaryGroupHeaderStyle : this._NormalHeaderStyle;
                }
                else
                {
                    row.Cells[column].Style = this._BuySummaryGroupStyle;
                   
                }
            } 
        }

        #region Grid Event
        private void GroupNetPositionGrid_ColumnLayoutAssigned(object sender, ColumnLayoutAssignedEventArgs e)
        {
            try
            {
                if (e.ColumnLayout.Columns.Count == 4 && e.DataType == typeof(RootGNP))
                {
                    this._ColumnsList = this.GetSortedInstrumentGNPs(this._AllColumns);

                    foreach (InstrumentGNP de in this._ColumnsList)
                    {
                        string headText = de.IsSummaryGroup ? de.SummaryGroupCode : de.InstrumentCode;
                        Debug.WriteLine(headText);
                        string myKey = "Columns[MyColumn" + de.ColumnIndex + "]";
                        e.ColumnLayout.Columns.Add(new TextColumn
                        {
                            HeaderText = headText,
                            Key = myKey,
                            Width = new ColumnWidth(100, false),
                            IsReadOnly = true,
                            IsResizable = true,
                            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right,
                        });

                        e.ColumnLayout.Columns.ColumnLayouts["AccountGroupGNPs"].Columns.Add(new TextColumn
                        {
                            HeaderText = headText,
                            Key = myKey,
                            Width = new ColumnWidth(100, false),
                            IsReadOnly = true,
                            IsResizable = true,
                            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right,
                        });

                        e.ColumnLayout.Columns.ColumnLayouts["AccountGroupGNPs"].Columns.ColumnLayouts["AccountGNPs"].Columns.Add(new TextColumn
                        {
                            HeaderText = headText,
                            Key = myKey,
                            Width = new ColumnWidth(100, false),
                            IsReadOnly = true,
                            IsResizable = true,
                            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right,
                        });
                        e.ColumnLayout.Columns.ColumnLayouts["AccountGroupGNPs"].Columns.ColumnLayouts["AccountGNPs"].Columns.ColumnLayouts["DetailGNPs"].Columns.Add(new TextColumn
                        {
                            HeaderText = headText,
                            Key = myKey,
                            Width = new ColumnWidth(100, false),
                            IsReadOnly = true,
                            IsResizable = true,
                            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "OpenInterestControl.GroupNetPositionGrid_ColumnLayoutAssigned Error\r\n{0}", ex.ToString());
            }
        }

        private void GroupNetPositionGrid_InitializeRow(object sender, InitializeRowEventArgs e)
        {
            try
            {
                RootGNP rootGNP = e.Row.Data as RootGNP;
                if (rootGNP != null)
                {
                    this.SettingGridStyle(e.Row, rootGNP);
                }

                AccountGroupGNP accountGroupGNP = e.Row.Data as AccountGroupGNP;
                if (accountGroupGNP != null)
                {
                    this.SettingGridStyle(e.Row, accountGroupGNP);
                }

                AccountGNP accountGNP = e.Row.Data as AccountGNP;
                if (accountGNP != null)
                {
                    this.SettingGridStyle(e.Row, accountGNP);
                }

                DetailGNP detailGNP = e.Row.Data as DetailGNP;
                if (detailGNP != null)
                {
                    this.SettingGridStyle(e.Row, detailGNP);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "OpenInterestControl.GroupNetPositionGrid_InitializeRow Error\r\n{0}", ex.ToString());
            }
        }

        private void GroupNetPositionGrid_CellExitingEditMode(object sender, ExitEditingCellEventArgs e)
        {
            AccountGroupGNP accountGroupGNP = e.Cell.Row.Data as AccountGroupGNP;
            if (accountGroupGNP != null)
            {
                switch (e.Cell.Column.Key)
                {
                    case "OIPercent":
                        decimal oldOiPercent = accountGroupGNP.OIPercent;
                        bool isNumber = Toolkit.IsNumber(e.NewValue.ToString());
                        if (!isNumber || decimal.Parse(e.NewValue.ToString()) > 100 || decimal.Parse(e.NewValue.ToString()) < 0)
                        {
                            accountGroupGNP.OIPercent = oldOiPercent;
                            accountGroupGNP.OldOIPercent = oldOiPercent;
                            e.Cancel = true;
                            return;
                        }
                        //TextBox oiPercentEditor = e.Editor as TextBox;
                        accountGroupGNP.OldOIPercent = oldOiPercent;
                        break;
                    default:
                        break;
                }
            }
        }

        private void GroupNetPositionGrid_CellExitedEditMode(object sender, CellExitedEditingEventArgs e)
        {
            AccountGroupGNP accountGroupGNP = e.Cell.Row.Data as AccountGroupGNP;
            if (accountGroupGNP != null)
            {
                switch (e.Cell.Column.Key)
                {
                    case "OIPercent":
                        decimal oldOiPercent = accountGroupGNP.OIPercent;
                        bool isNumber = Toolkit.IsNumber(accountGroupGNP.OIPercent.ToString());
                        if (!isNumber || accountGroupGNP.OIPercent > 100 || accountGroupGNP.OIPercent < 0)
                        {
                            accountGroupGNP.OIPercent = oldOiPercent;
                            accountGroupGNP.OldOIPercent = oldOiPercent;
                            return;
                        }
                        this.CalculateOIPercentQuantity(accountGroupGNP, false, accountGroupGNP.OldOIPercent);
                        break;
                    default:
                        break;
                }  
            }
        }

        #endregion

        private void _OpenInterestTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;

            if (this.NetPositionItem.IsSelected)
            {
                this._SummaryToolbar.Visibility = System.Windows.Visibility.Collapsed;
                this._NetPositionToolbar.Visibility = System.Windows.Visibility.Visible;
            }
            if (this.SummaryItem.IsSelected)
            {
                this._SummaryToolbar.Visibility = System.Windows.Visibility.Visible;
                this._NetPositionToolbar.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void SelectChk_Click(object sender, RoutedEventArgs e)
        {
            //this.CalculateLotBalance();
            CheckBox chk = (CheckBox)sender;
            AccountGroupGNP accountGroupGNP = chk.DataContext as AccountGroupGNP;

            this.CalculateOIPercentQuantity(accountGroupGNP,true,decimal.Zero);
        }

        private void CalculateOIPercentQuantity(AccountGroupGNP accountGroupGNP, bool isCheckBoxColumn, decimal oldOiPercent)
        {
            try
            {
                bool isCheck = accountGroupGNP.IsSelected;
                decimal newOIPercent = accountGroupGNP.OIPercent;

                RootGNP rootGNP = this._RootGNP[0];
                foreach (InstrumentGNP de in this._AllColumns.Values)
                {
                    string key = "MyColumn" + de.ColumnIndex;

                    if (rootGNP.Columns[key] == null || accountGroupGNP.Columns[key] == null) continue;

                    decimal totalSum = (decimal)rootGNP.Columns[key];
                    decimal groupSum = (decimal)accountGroupGNP.Columns[key];
                    if (isCheck)
                    {
                        if (isCheckBoxColumn)
                        {
                            rootGNP.Columns[key] = totalSum + groupSum * newOIPercent / 100;
                        }
                        else
                        {
                            ColumnKeys keys = rootGNP.Columns;
                            keys[key] = totalSum + groupSum * (newOIPercent - oldOiPercent) / 100;
                            rootGNP.Columns = keys;
                        }
                    }
                    else
                    {
                        rootGNP.Columns[key] = totalSum - groupSum * newOIPercent;
                    }
                }

                //this._GroupNetPositionGrid.ItemsSource = null;
                //this._GroupNetPositionGrid.ItemsSource = this._RootGNP;
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "OpenInterestControl.CalculateOIPercentQuantity Error\r\n{0}", ex.ToString());
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            switch (_OpenInterestTab.SelectedIndex)
            {
                case 0:
                    this.PrintGrid(this._GroupNetPositionGrid);
                    break;
                case 1:
                    //this.PrintGrid();
                    break;
            }
        }

        private void PrintGrid(Infragistics.Controls.Grids.XamGrid printGrid)
        {
            Report reportObj = new Report();
            EmbeddedVisualReportSection section = new EmbeddedVisualReportSection(printGrid);
            reportObj.Sections.Add(section);
            reportObj.Print(true, false);
        }

        void QueryData(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Button btn = (Button)sender;
            switch (btn.Name)
            {
                case "_QueryNetPostionBtn":
                    this.Reset();
                    this.QueryGroupNetPosition();
                    break;
                case "_QuerySummaryBtn":
                    this.QueryGroupNetPosition();
                    break;
            }
        }

        private void Reset()
        {
            this._RootGNP = new ObservableCollection<RootGNP>();
            this._SummaryColumns.Clear();
            this._AllColumns.Clear();
        }

        private void QueryGroupNetPosition()
        {
            bool showActualQuantity = true;
            //string[] blotterCodeSelecteds
            string exchangeCode = (string)this.ExchangeComboBox.SelectedItem;
            ConsoleClient.Instance.GetGroupNetPosition(exchangeCode,showActualQuantity, null, this.GetGroupNetPositionCallback);
        }

        private void GetGroupNetPositionCallback(List<CommonAccountGroupGNP> accountGroupGNPs)
        {
            this.Dispatcher.BeginInvoke((Action)delegate() 
            {
                if (accountGroupGNPs == null) return;
                RootGNP rootGNP = new RootGNP();
                foreach (CommonAccountGroupGNP group in accountGroupGNPs)
                {
                    AccountGroupGNP accountGroupGNP = new AccountGroupGNP(group);
                    rootGNP.AccountGroupGNPs.Add(accountGroupGNP);
                }
                this._RootGNP.Add(rootGNP);
                this.CalculateLotBalance();
                this.BindingData();
            });
        }

        private void BindingData()
        {
            this._GroupNetPositionGrid.ItemsSource = this._RootGNP;
        }

        private void CalculateLotBalance()
        {
            int i = 0;
            RootGNP rootGNP = this._RootGNP[0];
            foreach (AccountGroupGNP groupGNP in this._RootGNP[0].AccountGroupGNPs)
            {
                foreach (AccountGNP accountGNP in groupGNP.AccountGNPs)
                {
                    DetailGNP detailGNP = new DetailGNP();
                    foreach (InstrumentGNP instrumentGNP in accountGNP.InstrumentGNPs)
                    {
                        InstrumentClient instrument = this._App.ExchangeDataManager.GetExchangeSetting("CHUNG").Instruments.Values.SingleOrDefault(P => P.Id == instrumentGNP.Id);
                        if (instrument == null) continue;
                        instrumentGNP.Instrument = instrument;

                        string summaryGroupCode = instrument.SummaryGroupId == null ? "Other":instrument.SummaryGroupCode;

                        if (!this._AllColumns.Contains(instrumentGNP.InstrumentCode))
                        {
                            string key = "MyColumn" + i;
                            decimal totalSum = this._RootGNP[0].Columns[key] == null ? decimal.Zero : (decimal)this._RootGNP[0].Columns[key];
                            decimal groupSum = groupGNP.Columns[key] == null ? decimal.Zero : (decimal)groupGNP.Columns[key];

                            rootGNP.Columns[key] = totalSum + instrumentGNP.LotBalance;
                            groupGNP.Columns[key] = groupSum + instrumentGNP.LotBalance;
                            accountGNP.Columns[key] = instrumentGNP.LotBalance;
                            detailGNP.Columns[key] = instrumentGNP.Detail;
                            instrumentGNP.ColumnIndex = i;
                            instrumentGNP.SummaryGroupCode = summaryGroupCode;
                            
                            this._AllColumns.Add(instrumentGNP.InstrumentCode, instrumentGNP);
                            i++;
                        }
                        else
                        {
                            InstrumentGNP item = (InstrumentGNP)this._AllColumns[instrumentGNP.InstrumentCode];
                            string key = "MyColumn" + item.ColumnIndex;
                            decimal totalSum = rootGNP.Columns[key] == null ? decimal.Zero : (decimal)rootGNP.Columns[key];
                            decimal groupSum = groupGNP.Columns[key] == null ? decimal.Zero : (decimal)groupGNP.Columns[key];

                            rootGNP.Columns[key] = totalSum + instrumentGNP.LotBalance;
                            groupGNP.Columns[key] = groupSum + instrumentGNP.LotBalance;
                            accountGNP.Columns[key] = instrumentGNP.LotBalance;
                            detailGNP.Columns[key] = instrumentGNP.Detail;
                        }

                        //Summary Group LotBalance
                        if (!this._SummaryColumns.Contains(summaryGroupCode))
                        {
                            InstrumentGNP summaryGNP = new InstrumentGNP();
                            summaryGNP.Instrument = instrument;
                            summaryGNP.IsSummaryGroup = true;
                            summaryGNP.ColumnIndex = i;
                            summaryGNP.SummaryGroupCode = summaryGroupCode;

                            string key = "MyColumn" + i;
                            decimal totalSum = rootGNP.Columns[key] == null ? decimal.Zero : (decimal)rootGNP.Columns[key];
                            decimal groupSum = groupGNP.Columns[key] == null ? decimal.Zero : (decimal)groupGNP.Columns[key];

                            rootGNP.Columns[key] = totalSum + instrumentGNP.LotBalance;
                            groupGNP.Columns[key] = groupSum + instrumentGNP.LotBalance;
                            accountGNP.Columns[key] = instrumentGNP.LotBalance;

                            this._SummaryColumns.Add(summaryGroupCode, summaryGNP);
                            this._AllColumns.Add(summaryGroupCode, summaryGNP);
                            i++;
                        }
                        else
                        {
                            InstrumentGNP summaryGNP = (InstrumentGNP)this._SummaryColumns[summaryGroupCode];
                            string key = "MyColumn" + summaryGNP.ColumnIndex;
                            decimal totalSum = rootGNP.Columns[key] == null ? decimal.Zero : (decimal)rootGNP.Columns[key];
                            decimal groupSum = groupGNP.Columns[key] == null ? decimal.Zero : (decimal)groupGNP.Columns[key];

                            rootGNP.Columns[key] = totalSum + instrumentGNP.LotBalance;
                            groupGNP.Columns[key] = groupSum + instrumentGNP.LotBalance;
                            accountGNP.Columns[key] = instrumentGNP.LotBalance;
                        }
                    }
                    accountGNP.DetailGNPs.Add(detailGNP);
                }  
            }
        }

        #region OpenInsterest SummaryItem

        private void AttachEvent()
        {
            this._SummaryItemGrid.RowSelectorClicked += new EventHandler<RowSelectorClickedEventArgs>(SummaryItemGrid_RowSelectorClicked);
        }

        private void SummaryItemGrid_RowSelectorClicked(object sender, RowSelectorClickedEventArgs e)
        {
            var ss = e.Row.Cells[1].Value;
            OpenInterestSummary sss = e.Row.Data as OpenInterestSummary;
        }

        private void SummaryItemGrid_RowExpansionChanging(object sender, RowExpansionChangedEventArgs e)
        {
            try
            {
                OpenInterestSummary openInterestSummary = e.Row.Data as OpenInterestSummary;

                string[] blotterCodes = null;
                if (openInterestSummary.Type == OpenInterestSummaryType.Instrument)
                {
                    Guid instrumentId = openInterestSummary.Id;
                    if (this._InstrumentSummaryLoadings.Contains(instrumentId)) return;
                    this.QueryAccountSummary(instrumentId, blotterCodes);
                }
                else if (openInterestSummary.Type == OpenInterestSummaryType.Account)
                {
                    Guid accountId = openInterestSummary.Id;
                    Guid instrumentId = openInterestSummary.InstrumentId;
                    this.QueryOrderSummary(openInterestSummary, null);
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "OpenInterestControl.SummaryItemGrid_RowExpansionChanging Error\r\n{0}", ex.ToString());
            }
        }

        private void QuerySummaryBtn_Click(object sender, RoutedEventArgs e)
        {
            this._OpenInterestSummarys.Clear();
            bool isGroupByOriginCode = this._OriginCodeRadio.IsChecked.Value;
            string[] blotterCodes = new string[] { "123"};
            this.QueryInstrumentSummary(isGroupByOriginCode, null);
        }
        private void QueryInstrumentSummary(bool isGroupByOriginCode, string[] blotterCodes)
        {
            string exchangeCode = (string)this.ExchangeComboBox.SelectedItem;
            ConsoleClient.Instance.GetOpenInterestInstrumentSummary(exchangeCode,isGroupByOriginCode, blotterCodes, this.GetInstrumentSummaryCallback);
        }
        private void QueryAccountSummary(Guid instrumentId, string[] blotterCodes)
        {
            string exchangeCode = (string)this.ExchangeComboBox.SelectedItem;
            ConsoleClient.Instance.GetOpenInterestAccountSummary(exchangeCode,instrumentId, blotterCodes, this.GetAccountSummaryCallback);
        }
        private void QueryOrderSummary(OpenInterestSummary accountSumamry, string[] blotterCodes)
        {
            string exchangeCode = (string)this.ExchangeComboBox.SelectedItem;
            ConsoleClient.Instance.GetOpenInterestOrderSummary(exchangeCode,accountSumamry, blotterCodes, this.GetOrderSummaryCallback);
        }

        private void GetInstrumentSummaryCallback(List<CommonOpenInterestSummary> openInterestSummarys)
        {
            this.Dispatcher.BeginInvoke((Action)delegate() 
            {
                foreach (CommonOpenInterestSummary openInterestSummary in openInterestSummarys)
                {
                    OpenInterestSummary entity = new OpenInterestSummary(openInterestSummary,OpenInterestSummaryType.Instrument);
                    this._OpenInterestSummarys.Add(entity);
                }
                this.BindingSummaryGridData();
            });
        }

        private void GetAccountSummaryCallback(Guid instrumentId, List<CommonOpenInterestSummary> openInterestSummarys)
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                string exchangeCode = (string)this.ExchangeComboBox.SelectedItem;
                ExchangeSettingManager settingManager = this._App.ExchangeDataManager.GetExchangeSetting(exchangeCode);
                ObservableCollection<OpenInterestSummary> accountGroupSummarys = new ObservableCollection<OpenInterestSummary>();
                
                foreach (CommonOpenInterestSummary openInterestSummary in openInterestSummarys)
                {
                    OpenInterestSummary entity = new OpenInterestSummary(openInterestSummary, OpenInterestSummaryType.Account);
                    entity.InstrumentId = instrumentId;
                    Guid accountId = entity.Id;

                    Account account = settingManager.GetAccount(accountId);
                    if (account != null)
                    {
                        entity.Code = account.Code;
                    }

                    accountGroupSummarys.Add(entity);
                }

                OpenInterestSummary  instrumentSummary = this._OpenInterestSummarys.SingleOrDefault(P => P.Id == instrumentId);
                instrumentSummary.ChildSummaryItems.Clear();

                IEnumerable<IGrouping<string, OpenInterestSummary>> query = accountGroupSummarys.GroupBy(P => P.GroupCode, P => P);
                foreach (IGrouping<string, OpenInterestSummary> group in query)
                {
                   OpenInterestSummary groupSummary = new OpenInterestSummary(OpenInterestSummaryType.Group);

                   Guid accountId = accountGroupSummarys[0].Id;
                   Guid accountGroupId = Guid.Empty;

                   Account account = settingManager.GetAccount(accountId);
                   if (account != null)
                   {
                       accountGroupId = account.GroupId;
                   }

                   AccountGroup accountGroup = settingManager.GetAccountGroup(accountGroupId);
                   if (accountGroup != null)
                   {
                       groupSummary.Code = accountGroup.Code;
                   }

                   groupSummary.Id = accountGroupId;

                   List<OpenInterestSummary> accountSummarys = group.ToList<OpenInterestSummary>();
                   foreach (OpenInterestSummary item in accountGroupSummarys)
                   {
                       groupSummary.SetItem(item,true);
                       groupSummary.ChildSummaryItems.Add(item);
                   }
                   groupSummary.SetAvgPrice();
                   instrumentSummary.ChildSummaryItems.Add(groupSummary);
               }
            });
        }

        private void GetOrderSummaryCallback(OpenInterestSummary accountSumamry, List<CommonOpenInterestSummary> openInterestSummarys)
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                accountSumamry.ChildSummaryItems.Clear();
                foreach (CommonOpenInterestSummary openInterestSummary in openInterestSummarys)
                {
                    OpenInterestSummary entity = new OpenInterestSummary(openInterestSummary, OpenInterestSummaryType.Order);
                    accountSumamry.ChildSummaryItems.Add(entity);
                }
            });
        }

        private void BindingSummaryGridData()
        {
            this._SummaryItemGrid.ItemsSource = this._OpenInterestSummarys;
        }
        #endregion

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
            layoutBuilder.Append(ColumnWidthPersistence.GetPersistentColumnsWidthString(this._SummaryItemGrid));
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
                        ColumnWidthPersistence.LoadColumnsWidth(this._SummaryItemGrid, columnWidthElement);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(System.Diagnostics.TraceEventType.Error, "OpenInterestControl.SetLayout\r\n{0}", ex.ToString());
            }
        }
        #endregion

        
    }
}
