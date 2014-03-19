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
                        this.InitExchangeSystemComboBox();
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
            layoutBuilder.AppendFormat("<Spliter Width=\"{0}\"/>", this.SplitGrid.ColumnDefinitions[0].ActualWidth);
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
                        this.SplitGrid.ColumnDefinitions[0].Width = new GridLength(double.Parse(spliterElement.Attribute("Width").Value));
                    }
                    XElement columnWidthElement = layout.Element("ExchangeQuotationFilter");
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
            InstrumentQuotation instrumentQuotation = button.Tag as InstrumentQuotation;
            Dictionary<string ,List<Guid>> instruments = new Dictionary<string,List<Guid>>();
            List<Guid> ids = new List<Guid>();
            ids.Add(instrumentQuotation.InstruemtnId);
            instruments.Add(instrumentQuotation.ExchangeCode,ids);
            IEnumerable<InstrumentQuotation> instrumentQuotations = ExchangeQuotationViewModel.Instance.Exchanges.Where(i => i.ExchangeCode == instrumentQuotation.ExchangeCode && i.InstruemtnId == instrumentQuotation.InstruemtnId);
            foreach (InstrumentQuotation item in instrumentQuotations)
            {
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == item.ExchangeCode && i.QuotationPolicyId == item.QuotationPolicyId && i.InstruemtnId == item.InstruemtnId).IsPriceEnabled = resume;
                ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == item.ExchangeCode && i.QuotationPolicyId == item.QuotationPolicyId && i.InstruemtnId == item.InstruemtnId).IsAutoEnablePrice = resume;
            }
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

        private void QuotePolicyApply_Click(object sender, RoutedEventArgs e)
        {
            if (this.ExchangeSystem.SelectedIndex != -1 && this.QuotePolicyCode.SelectedIndex != -1 && this.QuoteParameter.SelectedIndex != -1)
            {
                string exchangeCode = this.ExchangeSystem.SelectedItem.ToString();
                string quotePolicyCode = this.QuotePolicyCode.SelectedItem.ToString();
                ComboBoxItem combox = this.QuoteParameter.SelectedItem as ComboBoxItem;
                if (combox != null)
                {
                    int parameter = int.Parse(combox.Content.ToString());
                    switch (parameter)
                    {
                        case 1:
                            foreach (InstrumentQuotation item in ExchangeQuotationViewModel.Instance.Exchanges.Where(ec => ec.ExchangeCode == exchangeCode && ec.QuotationPolicyCode == quotePolicyCode))
                            {
                                item.AutoAdjustPoints = item.AutoAdjustPoints2;
                                item.SpreadPoints = item.SpreadPoints2;
                                ConsoleClient.Instance.UpdateExchangeQuotation(new InstrumentQuotationSet { ExchangeCode = item.ExchangeCode, InstrumentId = item.InstruemtnId, QoutePolicyId = item.QuotationPolicyId, type = InstrumentQuotationEditType.AutoAdjustPoints, Value = item.AutoAdjustPoints2 });
                                ConsoleClient.Instance.UpdateExchangeQuotation(new InstrumentQuotationSet { ExchangeCode = item.ExchangeCode, InstrumentId = item.InstruemtnId, QoutePolicyId = item.QuotationPolicyId, type = InstrumentQuotationEditType.SpreadPoints, Value = item.SpreadPoints2 });
                            }
                            break;
                        case 2:
                            foreach (InstrumentQuotation item in ExchangeQuotationViewModel.Instance.Exchanges.Where(ec => ec.ExchangeCode == exchangeCode && ec.QuotationPolicyCode == quotePolicyCode))
                            {
                                item.AutoAdjustPoints = item.AutoAdjustPoints3;
                                item.SpreadPoints = item.SpreadPoints3;
                                ConsoleClient.Instance.UpdateExchangeQuotation(new InstrumentQuotationSet { ExchangeCode = item.ExchangeCode, InstrumentId = item.InstruemtnId, QoutePolicyId = item.QuotationPolicyId, type = InstrumentQuotationEditType.AutoAdjustPoints, Value = item.AutoAdjustPoints3 });
                                ConsoleClient.Instance.UpdateExchangeQuotation(new InstrumentQuotationSet { ExchangeCode = item.ExchangeCode, InstrumentId = item.InstruemtnId, QoutePolicyId = item.QuotationPolicyId, type = InstrumentQuotationEditType.SpreadPoints, Value = item.SpreadPoints3 });
                            }
                            break;
                        case 3:
                            foreach (InstrumentQuotation item in ExchangeQuotationViewModel.Instance.Exchanges.Where(ec => ec.ExchangeCode == exchangeCode && ec.QuotationPolicyCode == quotePolicyCode))
                            {
                                item.AutoAdjustPoints = item.AutoAdjustPoints4;
                                item.SpreadPoints = item.SpreadPoints4;
                                ConsoleClient.Instance.UpdateExchangeQuotation(new InstrumentQuotationSet { ExchangeCode = item.ExchangeCode, InstrumentId = item.InstruemtnId, QoutePolicyId = item.QuotationPolicyId, type = InstrumentQuotationEditType.AutoAdjustPoints, Value = item.AutoAdjustPoints4 });
                                ConsoleClient.Instance.UpdateExchangeQuotation(new InstrumentQuotationSet { ExchangeCode = item.ExchangeCode, InstrumentId = item.InstruemtnId, QoutePolicyId = item.QuotationPolicyId, type = InstrumentQuotationEditType.SpreadPoints, Value = item.SpreadPoints4 });
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                MessageBox.Show(App.MainFrameWindow, "部分参数未选择", "Warning", MessageBoxButton.OK);
            }
        }

        private void ExchangeSystem_Selected(object sender, RoutedEventArgs e)
        {
            if (this.ExchangeSystem.SelectedIndex == -1)
            {
                return;
            }
            this.InitQuotePolicyCodeComboBox(this.ExchangeSystem.Items[this.ExchangeSystem.SelectedIndex].ToString());
        }

        private void InitExchangeSystemComboBox()
        {
            this.ExchangeSystem.Items.Clear();
            foreach (InstrumentQuotation instrument in ExchangeQuotationViewModel.Instance.Exchanges)
            {

                if (!this.ExchangeSystem.Items.Contains(instrument.ExchangeCode))
                {
                    this.ExchangeSystem.Items.Add(instrument.ExchangeCode);
                }
            }
            this.ExchangeSystem.SelectedIndex = -1;
        }

        private void InitQuotePolicyCodeComboBox(string exchangeCode)
        {
            List<string> strs = new List<string>();
            this.QuotePolicyCode.Items.Clear();
            IEnumerable<InstrumentQuotation> exchanges ;
            exchanges = ExchangeQuotationViewModel.Instance.Exchanges.Where(e => e.ExchangeCode == exchangeCode);
            foreach (InstrumentQuotation instrument in exchanges)
            {
                if (!strs.Contains(instrument.QuotationPolicyCode))
                {
                    strs.Add(instrument.QuotationPolicyCode);
                    this.QuotePolicyCode.Items.Add(instrument);
                }
            }
        }

        private void ModifyHistoty_Click(object sender, RoutedEventArgs e)
        {
            //ExchangeQuotationViewModel.Instance.Exchanges[0].High = "10.000";
            UpdateExchangeHistoryQuotationControl dialog = new UpdateExchangeHistoryQuotationControl();
            this.MainGrid.Children.Add(dialog);
            dialog.IsModal = true;
            dialog.Show();
            dialog.BringToFront();
        }

        private void HighTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            InstrumentQuotation iq = textBox.Tag as InstrumentQuotation;
            if (iq == null) return;
            decimal editValue;
            if (!decimal.TryParse(textBox.Text, out editValue))
            {
                textBox.Text = iq.High;
                return;
            }
            if (iq.IsDealerInput(InstrumentQuotationEditType.High, editValue))
            {
                if (!iq.IsOriginHiLo)
                {
                    decimal high = decimal.Parse(iq.High);
                    if (high > editValue)
                    {
                        ConsoleClient.Instance.UpdateHighLow(iq.ExchangeCode, iq.InstruemtnId, iq.IsOriginHiLo, textBox.Text, true, this.UpdateHighLowCallBack);
                    }
                }
            }
        }

        private void LowTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            InstrumentQuotation iq = textBox.Tag as InstrumentQuotation;
            if (iq == null) return;
            decimal editValue;
            if (!decimal.TryParse(textBox.Text, out editValue)) return;
            if (iq.IsDealerInput(InstrumentQuotationEditType.Low, editValue))
            {
                if (!iq.IsOriginHiLo)
                {
                    decimal low = decimal.Parse(iq.Low);
                    if (low < editValue)
                    {
                        ConsoleClient.Instance.UpdateHighLow(iq.ExchangeCode, iq.InstruemtnId, iq.IsOriginHiLo, textBox.Text, false, this.UpdateHighLowCallBack);
                    }
                }
            }
        }

        private void UpdateHighLowCallBack(UpdateHighLowBatchProcessInfo info)
        {
            
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                if (info.StateCode == 0)
                {
                    ExchangeQuotationViewModel.Instance.HighLowBatchProcessInfos.Add(info);
                    string message = string.Format("Success to update {0} at {1}.\r\nBatchProcessId:{2}",info.IsHigh? "High":"Low",info.UpdateTime,info.BatchProcessId);
                    MessageBox.Show(App.MainFrameWindow, message, "", MessageBoxButton.OK);
                }
                else
                {
                    string message = string.Format("Update {0} failed.\r\n ErrorMessage:{1}", info.IsHigh ? "High" : "Low", info.ErrorMessage);
                    MessageBox.Show(App.MainFrameWindow, message, "", MessageBoxButton.OK);
                }
            }, null);
            
        }

        private void QuotationGrid_SelectedCellsCollectionChanged(object sender, SelectionCollectionChangedEventArgs<SelectedCellsCollection> e)
        {
            
        }
    }
}
