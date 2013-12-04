using Infragistics.Controls.Interactions;
using System;
using System.Collections.Generic;
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
using ManagerConsole.Model;
using ManagerConsole.ViewModel;
using Manager.Common.QuotationEntities;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for InstrumentWindow.xaml
    /// </summary>
    public partial class InstrumentWindow : XamDialogWindow
    {
        private VmInstrument _vmInstrument;

        public InstrumentWindow()
        {
            InitializeComponent();
            this._vmInstrument = new VmInstrument(new Instrument() { IsDerivative = false });
            this._vmInstrument.VmPriceRangeCheckRule = new VmPriceRangeCheckRule(new PriceRangeCheckRule());
            this._vmInstrument.VmWeightedPriceRule = new VmWeightedPriceRule(new WeightedPriceRule());
            this.DataContext = this._vmInstrument;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            HintTextBlock.Text = string.Empty;
            InstrumentData instrumentData = new InstrumentData();
            instrumentData.Instrument = this._vmInstrument.Instrument;
            instrumentData.PriceRangeCheckRule = this._vmInstrument.VmPriceRangeCheckRule.PriceRangeCheckRule;
            if (this._vmInstrument.UseWeightedPrice.HasValue && this._vmInstrument.UseWeightedPrice.Value)
            {
                instrumentData.WeightedPriceRule = this._vmInstrument.VmWeightedPriceRule.WeightedPriceRule;
            }
            ConsoleClient.Instance.AddInstrument(instrumentData, delegate(int instrumentId)
            {
                if (instrumentId != 0)
                {
                    this.Dispatcher.BeginInvoke((Action<int>)delegate(int ids)
                    {
                        instrumentData.Instrument.Id = instrumentId;
                        VmQuotationManager.Instance.Add(instrumentData.Instrument);

                        instrumentData.PriceRangeCheckRule.Id = instrumentId;
                        VmQuotationManager.Instance.Add(instrumentData.PriceRangeCheckRule);

                        if(instrumentData.WeightedPriceRule != null)
                        {
                            instrumentData.WeightedPriceRule.Id = instrumentId;
                            VmQuotationManager.Instance.Add(instrumentData.WeightedPriceRule);
                        }

                        HintTextBlock.Foreground = Brushes.Green;
                        HintTextBlock.Text = "Add Instrument successfully";
                    }, instrumentId);
                }
            });
        }
    }
}
