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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
        private ReportDataManager _ReportDataManager;
        private GroupNetPositionModel _GroupNetPositionModel;
        //private ObservableCollection<RootGNP> _RootGNP;
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

        private BlotterSelectionControl _BlotterSelectionWind;
        private string _CurrentExchangeCode;
        private string[] _SelectedBlotterCodes;
        private Button _BlotterButton;
        private Button _PrintButton;
        private ComboBox _ExchangeComboBox;
        public OpenInterestControl()
        {
            InitializeComponent();
            this.InilizeContrl();
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

        private void InilizeContrl()
        {
            this._ExchangeComboBox = new ComboBox() 
            {
                Height = 23,
                Width = 100,
                Margin = new Thickness(8,0,0,0),
                VerticalAlignment = VerticalAlignment.Center, 
            };
            this._BlotterButton = new Button() 
            { 
                Content = "Blotter", 
                Width = 80, 
                Height = 25, 
                VerticalAlignment = VerticalAlignment.Center, 
                Margin = new Thickness(5), 
                HorizontalAlignment = HorizontalAlignment.Center
            };
            this._PrintButton = new Button()
            {
                Content = "Print",
                Width = 75,
                Height = 25,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15,0,0,0),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            this._BlotterButton.Click += new RoutedEventHandler(this.BlotterSelectBtn_Click);
            this._PrintButton.Click += new RoutedEventHandler(this.Print_Click);
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

            this._ExchangeComboBox.ItemsSource = this._App.ExchangeDataManager.ExchangeCodes;
            this._ExchangeComboBox.SelectedItem = this._App.ExchangeDataManager.ExchangeCodes[0];

            this._CurrentExchangeCode = (string)this._ExchangeComboBox.SelectedItem;

            this._ReportDataManager = this._App.ExchangeDataManager.ReportDataManager;
            this._GroupNetPositionModel = new GroupNetPositionModel();

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
                string columnCode = instrumentGNP.IsSummaryGroup ? instrumentGNP.SummaryGroupCode : instrumentGNP.InstrumentCode;
                string key = "Columns[Item" + columnCode + "]";
                string index = "Item" + columnCode;
                int rowIndex = row.Index;

                Column column = row.Columns[key] as Column;

                if (instrumentGNP.IsSummaryGroup)
                {
                    row.Cells[column].Style = this._BuySummaryGroupStyle;
                    row.Cells[column].Column.HeaderStyle = this._SummaryGroupHeaderStyle;
                }
                else
                {
                    row.Cells[column].Column.HeaderStyle = this._NormalHeaderStyle;
                }

                if (obj.Columns[index] == null) continue;

                if (obj is DetailGNP)
                {
                    if (instrumentGNP.IsSummaryGroup)
                    {
                        row.Cells[column].Style = this._BuySummaryGroupStyle;
                    }
                    continue;
                }
                if ((decimal)obj.Columns[index] >= 0)
                {
                    row.Cells[column].Style = instrumentGNP.IsSummaryGroup ? this._BuySummaryGroupStyle : this._BuyCellStyle;
                }
                else if ((decimal)obj.Columns[index] < 0)
                {
                    row.Cells[column].Style = instrumentGNP.IsSummaryGroup ? this._SellSummaryGroupStyle : this._SellCellStyle;
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
                        string myKey = "Columns[Item" + headText + "]";
                        e.ColumnLayout.Columns.Add(new TextColumn
                        {
                            HeaderText = headText,
                            Key = myKey,
                            Width = new ColumnWidth(150, false),
                            IsReadOnly = true,
                            IsResizable = true,
                            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right,
                        });

                        e.ColumnLayout.Columns.ColumnLayouts["AccountGroupGNPs"].Columns.Add(new TextColumn
                        {
                            HeaderText = headText,
                            Key = myKey,
                            Width = new ColumnWidth(150, false),
                            IsReadOnly = true,
                            IsResizable = true,
                            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right,
                        });

                        e.ColumnLayout.Columns.ColumnLayouts["AccountGroupGNPs"].Columns.ColumnLayouts["AccountGNPs"].Columns.Add(new TextColumn
                        {
                            HeaderText = headText,
                            Key = myKey,
                            Width = new ColumnWidth(150, false),
                            IsReadOnly = true,
                            IsResizable = true,
                            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right,
                        });
                        e.ColumnLayout.Columns.ColumnLayouts["AccountGroupGNPs"].Columns.ColumnLayouts["AccountGNPs"].Columns.ColumnLayouts["DetailGNPs"].Columns.Add(new TextColumn
                        {
                            HeaderText = headText,
                            Key = myKey,
                            Width = new ColumnWidth(150, false),
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

        #region Button Event
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            switch (_OpenInterestTab.SelectedIndex)
            {
                case 0:
                    this.PrintGrid(this._GroupNetPositionGrid);
                    break;
                case 1:
                    this.PrintGrid(this._SummaryItemGrid);
                    break;
            }
        }

        private void QueryData(object sender, RoutedEventArgs e)
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
            if (this._ReportDataManager.GroupNetPositionModels.ContainsKey(this._CurrentExchangeCode))
            {
                this._ReportDataManager.GroupNetPositionModels.Remove(this._CurrentExchangeCode);
            }
            this._SummaryColumns.Clear();
            this._AllColumns.Clear();
        }

        private void _OpenInterestTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;

            if (this.NetPositionItem.IsSelected)
            {
                this._SummaryToolbar.Visibility = System.Windows.Visibility.Collapsed;
                this._NetPositionToolbar.Visibility = System.Windows.Visibility.Visible;
                if (this.SummaryPanel.Children.Contains(this._BlotterButton))
                {
                    this.SummaryPanel.Children.Remove(this._BlotterButton);
                }

                if (this.SummaryPanel.Children.Contains(this._ExchangeComboBox))
                {
                    this.SummaryPanel.Children.Remove(this._ExchangeComboBox);
                }

                if (this.SummaryPanel.Children.Contains(this._PrintButton))
                {
                    this.SummaryPanel.Children.Remove(this._PrintButton);
                }

                this.NetPositionPanel.Children.Insert(3, this._BlotterButton);
                this.NetPositionPanel.Children.Add(this._PrintButton);
                this.NetPositionPanel.Children.Insert(1, this._ExchangeComboBox);
            }
            if (this.SummaryItem.IsSelected)
            {
                this._SummaryToolbar.Visibility = System.Windows.Visibility.Visible;
                this._NetPositionToolbar.Visibility = System.Windows.Visibility.Collapsed;

                this.NetPositionPanel.Children.Remove(this._BlotterButton);
                this.SummaryPanel.Children.Insert(6, this._BlotterButton);

                this.NetPositionPanel.Children.Remove(this._ExchangeComboBox);
                this.SummaryPanel.Children.Insert(1, this._ExchangeComboBox);

                this.NetPositionPanel.Children.Remove(this._PrintButton);
                this.SummaryPanel.Children.Add(this._PrintButton);
            }
        }

        private void SelectChk_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            AccountGroupGNP accountGroupGNP = chk.DataContext as AccountGroupGNP;

            this.CalculateOIPercentQuantity(accountGroupGNP, true, decimal.Zero);
        }

        private void BlotterSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            this._BlotterSelectionWind = new BlotterSelectionControl(this._CurrentExchangeCode);
            this._App.MainFrame.Children.Add(this._BlotterSelectionWind);

            this._BlotterSelectionWind.OnBlotterResultHandle += new BlotterSelectionControl.ConfirmBlotterResultHandle(this.BlotterSelectCompaleted);
            this._BlotterSelectionWind.IsModal = true;
            this._BlotterSelectionWind.StartupPosition = Infragistics.Controls.Interactions.StartupPosition.Center;
            this._BlotterSelectionWind.Show();
            this._BlotterSelectionWind.BringToFront();
        }

        private void BlotterSelectCompaleted(string[] blotterCodes)
        {
            this._SelectedBlotterCodes = blotterCodes;
        }

        private void _SetColumnWidthBtn_Click(object sender, RoutedEventArgs e)
        {
            string columnWidth = this._ColumnWidthTextBox.Text;
            if (Toolkit.IsValidNumber(columnWidth))
            {
                if (int.Parse(columnWidth) < 0)
                {
                    this.SetColumnWidth(100);
                    return;
                }
                this.SetColumnWidth(int.Parse(columnWidth));
            }
            else
            {
                this._App._CommonDialogWin.ShowDialogWin("Invalid input!", "Alert");
            }
        }

        private void SetColumnWidth(int columnWidth)
        {
            for (int i = 3; i < this._ColumnsList.Count + 3; i++)
            {
                this._GroupNetPositionGrid.Columns.DataColumns[i].Width = new ColumnWidth(columnWidth, false);
            }

            if (this._GroupNetPositionGrid.Columns.ColumnLayouts.Count > 0)
            {
                for (int i = 3; i < this._ColumnsList.Count + 3; i++)
                {
                    this._GroupNetPositionGrid.Columns.ColumnLayouts["AccountGroupGNPs"].Columns.DataColumns[i].Width = new ColumnWidth(columnWidth, false);
                }
            }

            if (this._GroupNetPositionGrid.Columns.ColumnLayouts["AccountGroupGNPs"].Columns.ColumnLayouts.Count > 0)
            {
                for (int i = 3; i < this._ColumnsList.Count + 3; i++)
                {
                    this._GroupNetPositionGrid.Columns.ColumnLayouts["AccountGroupGNPs"].Columns.ColumnLayouts["AccountGNPs"].Columns.DataColumns[i].Width = new ColumnWidth(columnWidth, false);
                }
            }

            if (this._GroupNetPositionGrid.Columns.ColumnLayouts["AccountGroupGNPs"].Columns.ColumnLayouts["AccountGNPs"].Columns.ColumnLayouts.Count > 0)
            {
                for (int i = 3; i < this._ColumnsList.Count + 3; i++)
                {
                    this._GroupNetPositionGrid.Columns.ColumnLayouts["AccountGroupGNPs"].Columns.ColumnLayouts["AccountGNPs"].Columns.ColumnLayouts["DetailGNPs"].Columns.DataColumns[i].Width = new ColumnWidth(columnWidth, false);
                }
            }
        }
        #endregion

        private void CalculateOIPercentQuantity(AccountGroupGNP accountGroupGNP, bool isCheckBoxColumn, decimal oldOiPercent)
        {
            try
            {
                bool isCheck = accountGroupGNP.IsSelected;
                decimal newOIPercent = accountGroupGNP.OIPercent;

                RootGNP rootGNP = this._GroupNetPositionModel.RootGNPs[0];
                foreach (InstrumentGNP de in this._AllColumns.Values)
                {
                    string columnCode = de.IsSummaryGroup ? de.SummaryGroupCode : de.InstrumentCode;
                    string key = "Item" + columnCode;

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
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "OpenInterestControl.CalculateOIPercentQuantity Error\r\n{0}", ex.ToString());
            }
        }

        private void PrintGrid(Infragistics.Controls.Grids.XamGrid printGrid)
        {
            Report reportObj = new Report();
            EmbeddedVisualReportSection section = new EmbeddedVisualReportSection(printGrid);
            reportObj.Sections.Add(section);
            reportObj.Print(true, false);
        }

        private void QueryGroupNetPosition()
        {
            try
            {
                bool showActualQuantity = true;
                ConsoleClient.Instance.GetGroupNetPosition(this._CurrentExchangeCode, showActualQuantity, this._SelectedBlotterCodes, this.GetGroupNetPositionCallback);
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "OpenInterestControl.QueryGroupNetPosition Error\r\n{0}", ex.ToString());
            }
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
                this._GroupNetPositionModel.RootGNPs.Add(rootGNP);
                string exchangeCode = this._ExchangeComboBox.SelectedItem.ToString();
                this.CalculateLotBalance(exchangeCode);
                this.BindingData();
                if (!this._ReportDataManager.GroupNetPositionModels.ContainsKey(exchangeCode))
                {
                    this._ReportDataManager.GroupNetPositionModels.Add(exchangeCode, this._GroupNetPositionModel);
                }
            });
        }

        private void BindingData()
        {
            this._GroupNetPositionGrid.ItemsSource = this._GroupNetPositionModel.RootGNPs;
        }

        private void CalculateLotBalance(string exchangeCode)
        {
            int i = 0;
            RootGNP rootGNP = this._GroupNetPositionModel.RootGNPs[0];
            foreach (AccountGroupGNP groupGNP in this._GroupNetPositionModel.RootGNPs[0].AccountGroupGNPs)
            {
                foreach (AccountGNP accountGNP in groupGNP.AccountGNPs)
                {
                    DetailGNP detailGNP = new DetailGNP();
                    detailGNP.AccountId = accountGNP.Id;
                    foreach (InstrumentGNP instrumentGNP in accountGNP.InstrumentGNPs)
                    {
                        InstrumentClient instrument = this._App.ExchangeDataManager.GetExchangeSetting(exchangeCode).Instruments.Values.SingleOrDefault(P => P.Id == instrumentGNP.Id);
                        if (instrument == null) continue;
                        instrumentGNP.Instrument = instrument;
                        detailGNP.InstrumentId = instrument.Id;

                        string summaryGroupCode = instrument.SummaryGroupId == null ? "Other":instrument.SummaryGroupCode;

                        if (!this._AllColumns.Contains(instrumentGNP.InstrumentCode))
                        {
                            string key = "Item" + instrumentGNP.InstrumentCode;
                            decimal totalSum = this._GroupNetPositionModel.RootGNPs[0].Columns[key] == null ? decimal.Zero : (decimal)this._GroupNetPositionModel.RootGNPs[0].Columns[key];
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
                            string key = "Item" + item.InstrumentCode;
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

                            string key = "Item" + summaryGNP.SummaryGroupCode;
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
                            string key = "Item" + summaryGroupCode;
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

        /// <summary>
        /// OpenInsterest SummaryItem汇总
        /// </summary>
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
            this.QueryInstrumentSummary(isGroupByOriginCode, this._SelectedBlotterCodes);
        }
        private void QueryInstrumentSummary(bool isGroupByOriginCode, string[] blotterCodes)
        {
            ConsoleClient.Instance.GetOpenInterestInstrumentSummary(this._CurrentExchangeCode, isGroupByOriginCode, blotterCodes, this.GetInstrumentSummaryCallback);
        }
        private void QueryAccountSummary(Guid instrumentId, string[] blotterCodes)
        {
            ConsoleClient.Instance.GetOpenInterestAccountSummary(this._CurrentExchangeCode, instrumentId, blotterCodes, this.GetAccountSummaryCallback);
        }
        private void QueryOrderSummary(OpenInterestSummary accountSumamry, string[] blotterCodes)
        {
            ConsoleClient.Instance.GetOpenInterestOrderSummary(this._CurrentExchangeCode, accountSumamry, blotterCodes, this.GetOrderSummaryCallback);
        }

        private void GetInstrumentSummaryCallback(List<CommonOpenInterestSummary> openInterestSummarys)
        {
            this.Dispatcher.BeginInvoke((Action)delegate() 
            {
                if (openInterestSummarys == null) return;
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
                if (openInterestSummarys == null) return;

                ExchangeSettingManager settingManager = this._App.ExchangeDataManager.GetExchangeSetting(this._CurrentExchangeCode);
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
                if (openInterestSummarys == null) return;

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
