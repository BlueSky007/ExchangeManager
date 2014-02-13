using System;
using System.Collections;
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

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for AbnormalQuotationProcessControl.xaml
    /// </summary>
    public partial class AbnormalQuotationProcessControl : UserControl
    {
        public AbnormalQuotationProcessControl()
        {
            InitializeComponent();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            VmAbnormalQuotation abnormalQuotation = VmQuotationManager.Instance.AbnormalQuotationManager.FirstItem;
            if (abnormalQuotation != null)
            {
                bool accepted = ((Button)sender).Name.Equals("AcceptButton");
                ConsoleClient.Instance.ConfirmAbnormalQuotation(abnormalQuotation.InstrumentId, abnormalQuotation.ConfirmId, accepted);
                VmQuotationManager.Instance.AbnormalQuotationManager.RemoveFirstItem();
                if(VmQuotationManager.Instance.AbnormalQuotationManager.AbnormalQuotations.Count == 0)
                {
                    App.MainFrameWindow.HideAbnormalQuotationPane();
                }
            }
        }
    }
}
