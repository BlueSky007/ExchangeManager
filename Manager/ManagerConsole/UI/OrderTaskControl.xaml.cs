using ManagerCommon = Manager.Common;
using ManagerConsole.Helper;
using ManagerConsole.Model;
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
using ManagerConsole.ViewModel;
using System.Collections.ObjectModel;
using CommonTransactionError = Manager.Common.TransactionError;
using CommonParameter = Manager.Common.SystemParameter;
using Infragistics.Controls.Interactions;

namespace ManagerConsole.UI
{
    /// <summary>
    /// Interaction logic for OrderTaskControl.xaml
    /// </summary>
    public partial class OrderTaskControl : UserControl
    {
        private ObservableCollection<OrderTask> _OrderTaskEntity = new ObservableCollection<OrderTask>();

        private CommonDialogWin _CommonDialogWin;
        private ConfirmDialogWin _ConfirmDialogWin;
        private ManagerConsole.MainWindow _App;
        private ExcuteOrderConfirm _AccountInfoConfirm = null;
        public OrderTaskControl()
        {
            InitializeComponent();

            this._App = ((ManagerConsole.MainWindow)Application.Current.MainWindow);
            this._AccountInfoConfirm = new ExcuteOrderConfirm();
            this._AccountInfoConfirm.Closed +=_AccountInfoConfirm_Closed;
            this._CommonDialogWin = this._App.CommonDialogWin;
            this._ConfirmDialogWin = this._App.ConfirmDialogWin;
            this.BindGridData();
        }
        void _AccountInfoConfirm_Closed(object sender, EventArgs e)
        {
            if ((bool)this._AccountInfoConfirm.DialogResult)
            {
                MessageBox.Show("Do Accept");
            }
        }

        private void BindGridData()
        {
            this.OrderTaskGrid.ItemsSource = this._App.InitDataManager.OrderTaskEntities;//this._OrderTaskEntity;
            this._ConfirmDialogWin.OnConfirmDialogResult += new ConfirmDialogWin.ConfirmDialogResultHandle(ExcuteOrder);
        }

        void ExcuteOrder(bool yesOrNo, UIElement uIElement)
        {
            if (yesOrNo)
            {
                MessageBox.Show("你点击了确定按钮");
            }
        }

        private void OrderHandlerBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            OrderTask orderTask = btn.DataContext as OrderTask;
            CellDataDefine currentCellData = null;

            if (orderTask != null)
            {
                switch (btn.Name)
                {
                    case "UpdateBtn":
                        this._CommonDialogWin.ShowDialogWin("Update Now?", "Information");
                        currentCellData = orderTask.CellDataDefine1;
                        break;
                    case "ModiFyBtn":
                        currentCellData = orderTask.CellDataDefine2;
                        break;
                    case "CancelBtn":
                        currentCellData = orderTask.CellDataDefine3;
                        break;
                    case "ExcuteBtn":
                        currentCellData = orderTask.CellDataDefine4;
                        break;
                }
                this.ProcessPendingOrder(orderTask, currentCellData);
            }
        }

        private void ProcessPendingOrder(OrderTask orderTask, CellDataDefine currentCellData)
        {
            HandleAction actionType = currentCellData.Action;
            bool isEnabled;
            switch (actionType)
            {
                case HandleAction.None:
                    break;
                case HandleAction.OnOrderAccept:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        this.OnOrderAccept(orderTask); // DQ Order Accept
                    }
                    break;
                case HandleAction.OnOrderReject:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        //OnOrderReject(row);
                    }
                    break;
                case HandleAction.OnOrderDetail:
                    //OnOrderDetail(row);
                    break;
                case HandleAction.OnOrderAcceptPlace:
                    Transaction tran = orderTask.Transaction;
                    foreach (Order orderEntity in tran.Orders)
                    {
                        orderEntity.Phase = ManagerCommon.Phase.Placed;
                    }
                    orderTask.DoAcceptPlace();
                    break;
                case HandleAction.OnOrderRejectPlace:
                    //OnOrderRejectPlace(row);
                    break;
                case HandleAction.OnOrderAcceptCancel:
                    //OnOrderAcceptCancel(row);
                    break;
                case HandleAction.OnOrderRejectCancel:
                    //OnOrderRejectCancel(row);
                    break;
                case HandleAction.OnOrderUpdate:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        //OnOrderUpdate(row);
                    }
                    break;
                case HandleAction.OnOrderModify:
                    isEnabled = currentCellData.IsEnable;
                    if (isEnabled)
                    {
                        // OnOrderModify(row);
                    }
                    break;
                case HandleAction.OnOrderWait:
                    //order.DoWait();
                    break;
                case HandleAction.OnOrderExecute:
                    //OnOrderExecute(row);
                    break;
                case HandleAction.OnOrderCancel:
                    //this.OnOrderCancel(order);
                    break;
            }
        }

        #region Quote Order
        private void OnOrderAccept(OrderTask order)
        {
            CommonParameter systemParameter = this._App.InitDataManager.SystemParameter;
            systemParameter.CanDealerViewAccountInfo = false;
            bool isOK = OrderTaskManager.CheckDQOrder(order, systemParameter);
            isOK = false;

            if (!isOK)
            {
                if (systemParameter.CanDealerViewAccountInfo)
                {
                    //Show Account Information
                }
                else
                {
                    if (MessageBox.Show("Accept the order?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        this.AcceptPlace(order.Transaction.Id);
                    }
                }
            }
        }
        private void AcceptPlace(Guid transactionId)
        {
            ConsoleClient.Instance.AcceptPlace(transactionId, AcceptPlaceCallback);
        }
        //Call Back
        private void AcceptPlaceCallback(CommonTransactionError transactionError)
        {
            this.Dispatcher.BeginInvoke((Action<CommonTransactionError>)delegate(CommonTransactionError result)
            {
                if (result == CommonTransactionError.OK)
                {
                    this._CommonDialogWin.ShowDialogWin("Accept Place Succeed","Infromation");
                }
            }, transactionError);
        }
        #endregion



    }
}
