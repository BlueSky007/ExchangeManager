using Infragistics.Controls.Grids;
using Manager.Common;
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Xml;
using Manager.Common.QuotationEntities;
using System.Xml.Linq;
using PriceType = iExchange.Common.PriceType;
using Infragistics;

namespace ManagerConsole.UI
{
    /// <summary>
    /// ExchangeQuotationControl.xaml 的交互逻辑
    /// </summary>
    public partial class ExchangeQuotationControl : UserControl, IControlLayout
    {
        //private bool _IsChange = false;
        //private string _Value;
        //private string _DisplayName;
        //private InstrumentQuotation _ChangeQuotation;

        //private Dictionary<string, ExchangeQuotationViewModel> _ExchangeQuotationViews;
        
        public ExchangeQuotationControl()
        {
            InitializeComponent();
            Thread thread = new Thread(new ThreadStart(delegate()
            {
                while (!this.Init())
                {
                    Thread.Sleep(800);
                }
            }));
            thread.IsBackground = true;
            thread.Start();
            
        }

        public bool Init()
        {
            try
            {
                ExchangeQuotationViewModel.Instance.InitData();
                if (ExchangeQuotationViewModel.Instance.IsInitData)
                {
                    this.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        this.QuotationGrid.ItemsSource = ExchangeQuotationViewModel.Instance.Exchanges;
                        InstrumentQuotation ins = new InstrumentQuotation();
                        this.QuotationProperty.SetSource(ExchangeQuotationViewModel.Instance.Exchanges[0]);
                    });
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "ExchangeQuotation.Init is abort.\r\n{0}", ex.ToString());
                return true;
            }
        }

        private void QuotationGrid_CellClicked(object sender, Infragistics.Controls.Grids.CellClickedEventArgs e)
        {
            this.QuotationProperty.SetSource((InstrumentQuotation)e.Cell.Row.Data);
        }

        //private void QuotationProperty_PropertyValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        //{
        //    if (e.NewValue.ToString() != e.OldValue.ToString())
        //    {
        //        this._IsChange = true;
        //        this._Value = e.NewValue.ToString();
        //        //this._DisplayName = this.QuotationProperty.SelectedProperty.ToString();
        //        //this._ChangeQuotation = this.QuotationProperty.SelectedObject as InstrumentQuotation;
        //        // ConsoleClient.Instance.UpdateExchangeQuotation(instrument.ExchangeCode, updateNode);
        //    }
            
        //}

        //private void QuotationProperty_SelectedPropertyItemChanged(object sender, RoutedPropertyChangedEventArgs<Xceed.Wpf.Toolkit.PropertyGrid.PropertyItemBase> e)
        //{
        //    if (this._IsChange)
        //    {
        //        InstrumentQuotation instrument = this._ChangeQuotation;
        //        QuotePolicyDetailSet set = new QuotePolicyDetailSet();
        //        set.ExchangeCode = instrument.ExchangeCode;
        //        set.QoutePolicyId = instrument.QuotationPolicyId;
        //        set.InstrumentId = instrument.InstruemtnId;
        //        set.type = (QuotePolicyEditType)Enum.Parse(typeof(QuotePolicyEditType), this._DisplayName);
        //        if (this._DisplayName == "PriceType")
        //        {
        //            set.Value = (int)Enum.Parse(typeof(PriceType), this._Value);
        //        }
        //        else if (this._DisplayName == "IsOriginHiLo")
        //        {
        //            if (bool.Parse(this._Value))
        //            {
        //                set.Value = 1;
        //            }
        //            else
        //            {
        //                set.Value = 0;
        //            }
        //        }
        //        else
        //        {
        //            set.Value = int.Parse(this._Value);
        //        }
        //        ConsoleClient.Instance.UpdateExchangeQuotation(set);
        //        this._IsChange = false;
        //    }
        //}

        private void New_Click(object sender, RoutedEventArgs e)
        {
            XamGrid newGrid = new XamGrid();
            newGrid = this.QuotationGrid;
            ExchangeQuotationViewModel FilterView = new ExchangeQuotationViewModel();
            
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {

        }

        public string GetLayout()
        {
           
            StringBuilder layoutBuilder = new StringBuilder();
            layoutBuilder.Append("<ExchangeQuotationFilter>");
            if (this.QuotationGrid.FilteringSettings.RowFiltersCollection.Count > 0)
            {
                foreach (RowsFilter rowsFilter in this.QuotationGrid.FilteringSettings.RowFiltersCollection)
                {
                    layoutBuilder.AppendFormat("<Fitler FieldName=\"{0}\" LogicalOperator=\"{1}\">",rowsFilter.FieldName, (int)rowsFilter.Conditions.LogicalOperator);
                    foreach (IFilterCondition condition in rowsFilter.Conditions)
                    {
                        ComparisonCondition comparisonCondition = condition as ComparisonCondition;
                        if (comparisonCondition != null)
                        {
                            layoutBuilder.AppendFormat("<Condition op=\"{0}\" val=\"{1}\"/>", (int)comparisonCondition.Operator, comparisonCondition.FilterValue);
                        }
                    }
                    layoutBuilder.Append("</Fitler>");
                }
            }
            layoutBuilder.AppendFormat("<Spliter Width=\"{0}\"/>", this.MainGrid.ColumnDefinitions[0].ActualWidth);
            layoutBuilder.Append(ColumnWidthPersistence.GetPersistentColumnsWidthString(this.QuotationGrid));
            layoutBuilder.Append("</ExchangeQuotationFilter>");
            //string layout = "";
            return layoutBuilder.ToString();
        }

        public void SetLayout(XElement layout)
        {
            try
            {
                
                if (layout.HasElements)
                {
                    foreach (XElement filterElement in layout.Element("ExchangeQuotationFilter").Elements("Fitler"))
                    {
                        if (filterElement != null)
                        {
                            RowsFilter rowsFilter = new RowsFilter(typeof(string), this.QuotationGrid.Columns.DataColumns[filterElement.Attribute("FieldName").Value]);
                            rowsFilter.Conditions.LogicalOperator = (LogicalOperator)int.Parse(filterElement.Attribute("LogicalOperator").Value);

                            foreach (XElement element in filterElement.Elements("Condition"))
                            {
                                rowsFilter.Conditions.Add(new ComparisonCondition() { FilterValue = element.Attribute("val").Value, Operator = (ComparisonOperator)int.Parse(element.Attribute("op").Value) });
                            }
                            this.QuotationGrid.FilteringSettings.RowFiltersCollection.Add(rowsFilter);
                        }
                    }
                    XElement spliterElement = layout.Element("ExchangeQuotationFilter").Element("Spliter");
                    if (spliterElement != null)
                    {
                        this.MainGrid.ColumnDefinitions[0].Width = new GridLength(double.Parse(spliterElement.Attribute("Width").Value));
                    }
                    XElement columnWidthElement = layout.Element("ExchangeQuotationFilter").Element("ColumnsWidth");
                    if (columnWidthElement !=null)
                    {
                        ColumnWidthPersistence.LoadColumnsWidth(this.QuotationGrid, columnWidthElement);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.AddEvent(System.Diagnostics.TraceEventType.Error, "ExchangeQuotationControl.SetLayout\r\n{0}", ex.ToString());
            }
        }

        private void SRButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            bool resume = false;
            if (button.Content.ToString() == "Resume")
            {
                button.Content = "Suspend";
                resume = true;
            }
            else
            {
                button.Content = "Resume";
                resume = false;
            }
            InstrumentQuotation instrumentQuotations = button.Tag as InstrumentQuotation;
            Dictionary<string ,List<Guid>> instruments = new Dictionary<string,List<Guid>>();
            List<Guid> ids = new List<Guid>();
            ids.Add(instrumentQuotations.InstruemtnId);
            instruments.Add(instrumentQuotations.ExchangeCode,ids);
            ConsoleClient.Instance.ExchangeSuspendResume(instruments, resume);
        }

        private void IsOriginHiLoCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            InstrumentQuotation instrumentQuotation = cb.Tag as InstrumentQuotation;
            if (instrumentQuotation != null)
            {
                InstrumentQuotationSet set = new InstrumentQuotationSet();
                set.ExchangeCode = instrumentQuotation.ExchangeCode;
                set.QoutePolicyId = instrumentQuotation.QuotationPolicyId;
                set.InstrumentId = instrumentQuotation.InstruemtnId;
                set.type = InstrumentQuotationEditType.IsOriginHiLo;
                set.Value = (bool)cb.IsChecked ? 1 : 0;
                ConsoleClient.Instance.UpdateExchangeQuotation(set);
            }
        }

        private void IsAutoFillCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            InstrumentQuotation instrumentQuotation = cb.Tag as InstrumentQuotation;
            if (instrumentQuotation != null)
            {
                IEnumerable<InstrumentQuotation> instruments = ExchangeQuotationViewModel.Instance.Exchanges.Where(i => i.ExchangeCode == instrumentQuotation.ExchangeCode && i.InstruemtnId == instrumentQuotation.InstruemtnId);
                foreach (InstrumentQuotation item in instruments)
                {
                    ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == item.ExchangeCode && i.QuotationPolicyId == item.QuotationPolicyId && i.InstruemtnId == item.InstruemtnId).IsAutoFill = (cb.IsChecked == true);
                }
                this.UpdateInstrument(InstrumentQuotationEditType.IsAutoFill, instrumentQuotation, (bool)cb.IsChecked ? 1 : 0);
            }
        }

        private void AllowLimitCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            InstrumentQuotation instrumentQuotation = cb.Tag as InstrumentQuotation;
            if (instrumentQuotation != null)
            {
                int value;
                IEnumerable<InstrumentQuotation> instruments = ExchangeQuotationViewModel.Instance.Exchanges.Where(i => i.ExchangeCode == instrumentQuotation.ExchangeCode && i.InstruemtnId == instrumentQuotation.InstruemtnId);
                foreach (InstrumentQuotation item in instruments)
                {
                    ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == item.ExchangeCode && i.QuotationPolicyId == item.QuotationPolicyId && i.InstruemtnId == item.InstruemtnId).AllowLimit = (cb.IsChecked == true);
                }
                value = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == instrumentQuotation.ExchangeCode && i.QuotationPolicyId == instrumentQuotation.QuotationPolicyId && i.InstruemtnId == instrumentQuotation.InstruemtnId).OrderTypeMask;
                this.UpdateInstrument(InstrumentQuotationEditType.OrderTypeMask, instrumentQuotation, value);
            }
        }

        private void IsPriceEnabledCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            InstrumentQuotation instrumentQuotation = cb.Tag as InstrumentQuotation;
            if (instrumentQuotation != null)
            {
                IEnumerable<InstrumentQuotation> instruments = ExchangeQuotationViewModel.Instance.Exchanges.Where(i => i.ExchangeCode == instrumentQuotation.ExchangeCode && i.InstruemtnId == instrumentQuotation.InstruemtnId);
                foreach (InstrumentQuotation item in instruments)
                {
                    ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == item.ExchangeCode && i.QuotationPolicyId == item.QuotationPolicyId && i.InstruemtnId == item.InstruemtnId).IsPriceEnabled = (cb.IsChecked == true);
                }
                this.UpdateInstrument(InstrumentQuotationEditType.IsPriceEnabled, instrumentQuotation, (bool)cb.IsChecked ? 1 : 0);
            }
        }

        private void IsAutoEnablePriceCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            InstrumentQuotation instrumentQuotation = cb.Tag as InstrumentQuotation;
            if (instrumentQuotation != null)
            {
                IEnumerable<InstrumentQuotation> instruments = ExchangeQuotationViewModel.Instance.Exchanges.Where(i => i.ExchangeCode == instrumentQuotation.ExchangeCode && i.InstruemtnId == instrumentQuotation.InstruemtnId);
                foreach (InstrumentQuotation item in instruments)
                {
                    ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == item.ExchangeCode && i.QuotationPolicyId == item.QuotationPolicyId && i.InstruemtnId == item.InstruemtnId).IsAutoEnablePrice = (cb.IsChecked == true);
                }
                this.UpdateInstrument(InstrumentQuotationEditType.IsAutoEnablePrice, instrumentQuotation, (bool)cb.IsChecked ? 1 : 0);
            }
        }

        private void UpdateInstrument(InstrumentQuotationEditType type, InstrumentQuotation instrumentQuotation,int value)
        {
            InstrumentQuotationSet set = new InstrumentQuotationSet();
            set.ExchangeCode = instrumentQuotation.ExchangeCode;
            set.QoutePolicyId = instrumentQuotation.QuotationPolicyId;
            set.InstrumentId = instrumentQuotation.InstruemtnId;
            set.type = type;
            set.Value = value;
            ConsoleClient.Instance.UpdateInstrument(set);
        }
        //private void SRHeader_Click(object sender, RoutedEventArgs e)
        //{

        //}
    }
}
