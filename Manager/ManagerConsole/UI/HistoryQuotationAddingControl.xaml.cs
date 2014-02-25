using Infragistics.Controls.Interactions;
using Manager.Common.QuotationEntities;
using ManagerConsole.ViewModel;
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

namespace ManagerConsole.UI
{
    /// <summary>
    /// HistoryQuotationAddingControl.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryQuotationAddingControl : XamDialogWindow
    {
        private HistoryQuotationData _HistoryQuotationData;
        private Action<HistoryQuotationData> _AddHistoryQuotation;

        public HistoryQuotationAddingControl(Action<HistoryQuotationData> addHistoryQuotation)
        {
            InitializeComponent();
            foreach (InstrumentQuotation instrument in ExchangeQuotationViewModel.Instance.Exchanges)
            {

                if (!this.Exchange.Items.Contains(instrument.ExchangeCode))
                {
                    this.Exchange.Items.Add(instrument.ExchangeCode);
                }
            }
            this.Exchange.SelectedIndex = -1;
            this._HistoryQuotationData = new HistoryQuotationData();
            this._AddHistoryQuotation = addHistoryQuotation;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this._AddHistoryQuotation(this._HistoryQuotationData);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Exchange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Exchange.SelectedIndex != -1)
            {
                List<Guid> strs = new List<Guid>();
                foreach (InstrumentQuotation instrument in ExchangeQuotationViewModel.Instance.Exchanges)
                {

                    if (!strs.Contains(instrument.InstruemtnId))
                    {
                        strs.Add(instrument.InstruemtnId);
                        this.Instument.Items.Add(instrument);
                    }
                }
            }
        }

        private void Instument_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
