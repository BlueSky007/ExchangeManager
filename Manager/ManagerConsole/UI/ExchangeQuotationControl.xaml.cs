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
            string layout = "";
            return layout;
        }

        public void SetLayout(XElement layout)
        {
            
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
            InstrumentQuotation iq = ExchangeQuotationViewModel.Instance.Exchanges.SingleOrDefault(i => i.ExchangeCode == instrumentQuotations.ExchangeCode && i.QuotationPolicyId == instrumentQuotations.QuotationPolicyId && i.InstruemtnId == instrumentQuotations.InstruemtnId);
            Dictionary<string ,List<Guid>> instruments = new Dictionary<string,List<Guid>>();
            List<Guid> ids = new List<Guid>();
            ids.Add(instrumentQuotations.InstruemtnId);
            instruments.Add(instrumentQuotations.ExchangeCode,ids);
            ConsoleClient.Instance.ExchangeSuspendResume(instruments, resume);
        }

        //private void SRHeader_Click(object sender, RoutedEventArgs e)
        //{

        //}
    }
}
