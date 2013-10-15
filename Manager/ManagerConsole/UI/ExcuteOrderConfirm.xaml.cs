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
using System.Windows.Shapes;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for ExcuteOrderConfirm.xaml
    /// </summary>
    public partial class ExcuteOrderConfirm :Window
    {
        public ExcuteOrderConfirm()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ExcuteOrderConfirm_Loaded); ;
        }
        void ExcuteOrderConfirm_Loaded(object sender, RoutedEventArgs e)
        {


        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        //return this.StateServer.GetAcountInfo(token, new Guid(tranID));
        /// <returns>		
        ///		<Account ID="" Balance="" Equity="" Necessary="">
        ///			<Instrument ID="" BuyLotBalanceSum="" SellLotBalanceSum="" />
        ///		</Account>
        /// </returns>
    }
}
