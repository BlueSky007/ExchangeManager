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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommonAccountGroupGNP = iExchange.Common.Manager.AccountGroupGNP;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for OpenInterestControl.xaml
    /// </summary>
    public partial class OpenInterestControl : UserControl
    {
        private ObservableCollection<RootGNP> _RootGNP = new ObservableCollection<RootGNP>();
        private static IComparer<InstrumentGNP> _InstrumentGNPSummaryCompare = new InstrumentGNPSummaryCompare();
        private static IComparer<InstrumentGNP> _InstrumentGNPCompare = new InstrumentGNPCompare();
        private Hashtable _AllColumns = new Hashtable();
        private Hashtable _SummaryColumns = new Hashtable();
        private List<InstrumentGNP> _ColumnsList;
        private MainWindow _App;
        private Style _BuyCellStyle;
        private Style _SellCellStyle;
        private Style _BuySummaryGroupStyle;
        private Style _SellSummaryGroupStyle;
        public OpenInterestControl()
        {
            InitializeComponent();
            this.InitializeData();
        }

        private void InitializeData()
        {
            this._App = (MainWindow)Application.Current.MainWindow;
            this._BuyCellStyle = this.Resources["BuyCellStyle"] as Style;
            this._SellCellStyle = this.Resources["SellCellStyle"] as Style;
            this._BuySummaryGroupStyle = this.Resources["BuySummaryGroupCellStyle"] as Style;
            this._SellSummaryGroupStyle = this.Resources["SellSummaryGroupStyle"] as Style;
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

                if (obj.Columns[index] == null) continue;
                if ((decimal)obj.Columns[index] > 0)
                {
                    Column column = row.Columns[key] as Column;
                    row.Cells[column].Style = instrumentGNP.IsSummaryGroup ? this._BuySummaryGroupStyle : this._BuyCellStyle;
                }
                else if ((decimal)obj.Columns[index] < 0)
                {
                    Column column = row.Columns[key] as Column;
                    row.Cells[column].Style = instrumentGNP.IsSummaryGroup ? this._SellSummaryGroupStyle : this._SellCellStyle;
                }
            } 
        }

        #region Grid Event
        private void GroupNetPositionGrid_ColumnLayoutAssigned(object sender, ColumnLayoutAssignedEventArgs e)
        {
            if (e.ColumnLayout.Columns.Count == 4 && e.DataType == typeof(RootGNP))
            {
                this._ColumnsList = this.GetSortedInstrumentGNPs(this._AllColumns);

                foreach (InstrumentGNP de in this._ColumnsList)
                {
                    string headText = de.IsSummaryGroup ? de.SummaryGroupCode : de.InstrumentCode;
                    //summaryColumnStyle = de.IsSummaryGroup ? this._SummaryGroupStyle : null;
                    Debug.WriteLine(headText);
                    string myKey = "Columns[MyColumn" + de.ColumnIndex + "]";
                    e.ColumnLayout.Columns.Add(new TextColumn
                    {
                        HeaderText = headText,
                        Key = myKey,
                        Width = new ColumnWidth(100, false),
                        IsReadOnly = true,
                        //CellStyle = summaryColumnStyle,
                    });

                    e.ColumnLayout.Columns.ColumnLayouts["AccountGroupGNPs"].Columns.Add(new TextColumn
                    {
                        HeaderText = headText,
                        Key = myKey,
                        Width = new ColumnWidth(100, false),
                        IsReadOnly = true,
                        //CellStyle = summaryColumnStyle,
                    });

                    e.ColumnLayout.Columns.ColumnLayouts["AccountGroupGNPs"].Columns.ColumnLayouts["AccountGNPs"].Columns.Add(new TextColumn
                    {
                        HeaderText = headText,
                        Key = myKey,
                        Width = new ColumnWidth(100, false),
                        IsReadOnly = true,
                        //CellStyle = summaryColumnStyle,
                    });
                    e.ColumnLayout.Columns.ColumnLayouts["AccountGroupGNPs"].Columns.ColumnLayouts["AccountGNPs"].Columns.ColumnLayouts["DetailGNPs"].Columns.Add(new TextColumn
                    {
                        HeaderText = headText,
                        Key = myKey,
                        Width = new ColumnWidth(100, false),
                        IsReadOnly = true,
                       // CellStyle = summaryColumnStyle,
                    });
                }
            }
        }

        private void GroupNetPositionGrid_InitializeRow(object sender, InitializeRowEventArgs e)
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
        }

        private void GroupNetPositionGrid_CellExitedEditMode(object sender, CellExitedEditingEventArgs e)
        {
            bool isNumber = false;
            switch (e.Cell.Column.Key)
            {
                case "OiPercent":
                    AccountGroupGNP accountGroupGNP = e.Cell.Row.Data as AccountGroupGNP;
                    isNumber = Toolkit.IsNumber(accountGroupGNP.OiPercent.ToString());
                    if (!isNumber || accountGroupGNP.OiPercent > 100 || accountGroupGNP.OiPercent < 0)
                    {
                        accountGroupGNP.OiPercent = 100;
                        return;
                    }
                    var ss = e.Cell.Value;
                    this.CalculateOIPercentQuantity(accountGroupGNP,false);
                    break;
            }
        }

       

        #endregion
        private void tabControl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            int selectedIndex = this._OpenInterestTab.TabIndex;

            Infragistics.Windows.Controls.TabItemEx selectedTab = this._OpenInterestTab.SelectedItem as Infragistics.Windows.Controls.TabItemEx;
            if (selectedTab.Name == "NetPositionItem")
            {
                this._SummaryToolbar.Visibility = System.Windows.Visibility.Collapsed;
                this._NetPositionToolbar.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this._SummaryToolbar.Visibility = System.Windows.Visibility.Visible;
                this._NetPositionToolbar.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        int index = 0;
        void AddTable_Click(object sender, RoutedEventArgs e)
        {
            index++;
            Infragistics.Windows.Controls.TabItemEx newTabItemEx = new Infragistics.Windows.Controls.TabItemEx();

            newTabItemEx.CloseButtonVisibility = Infragistics.Windows.Controls.TabItemCloseButtonVisibility.Visible;
            newTabItemEx.Header = string.Format("Net Postion", index.ToString());
            newTabItemEx.Content = new OpenInterestControl();

            this._OpenInterestTab.Items.Add(newTabItemEx);
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

            this.CalculateOIPercentQuantity(accountGroupGNP,true);
        }

        private void CalculateOIPercentQuantity(AccountGroupGNP accountGroupGNP,bool isCheckBoxColumn)
        {
            bool isCheck = accountGroupGNP.IsSelected;
            decimal oIPercent = accountGroupGNP.OiPercent;

            RootGNP rootGNP = this._RootGNP[0];
            foreach (InstrumentGNP de in this._AllColumns.Values)
            {
                string key = "MyColumn" + de.ColumnIndex;

                if (rootGNP.Columns[key] == null || accountGroupGNP.Columns[key] == null) continue;
                if (isCheck)
                {
                    if (isCheckBoxColumn)
                    {
                        rootGNP.Columns[key] = (decimal)rootGNP.Columns[key] + (decimal)accountGroupGNP.Columns[key] * oIPercent / 100;
                    }
                    else
                    {
                        rootGNP.Columns[key] = (decimal)rootGNP.Columns[key] - (decimal)accountGroupGNP.Columns[key] * (1 - oIPercent / 100);
                    }
                }
                else
                {
                    rootGNP.Columns[key] = (decimal)rootGNP.Columns[key] - (decimal)accountGroupGNP.Columns[key] * oIPercent;
                }
            }
            this._GroupNetPositionGrid.ItemsSource = null;
            this._GroupNetPositionGrid.ItemsSource = this._RootGNP;
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
            ConsoleClient.Instance.GetGroupNetPosition(this.GetGroupNetPositionCallback);
        }

        private void GetGroupNetPositionCallback(List<CommonAccountGroupGNP> accountGroupGNPs)
        {
            this.Dispatcher.BeginInvoke((Action)delegate() 
            {
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
            this._GroupNetPositionGrid.ItemsSource = this._RootGNP; ;
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
                        InstrumentClient instrument = this._App.InitDataManager.GetInstruments().SingleOrDefault(P => P.Id == instrumentGNP.Id);
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




    }
}
