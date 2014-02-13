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
using System.Collections.ObjectModel;
using ManagerConsole.ViewModel;
using ManagerConsole.Model;

namespace ManagerConsole.UI
{
    /// <summary>
    /// NewRelationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewRelationWindow : XamDialogWindow
    {
        private Action<AdjustRelationViewModel> _AddNewSuccess;
        private Guid _NewId;
        private string _NewCode;
        private List<int> _instrumentIds;

        public NewRelationWindow(Action<AdjustRelationViewModel> addNewSuccess)
        {
            InitializeComponent();
            this.SourceInstrument.ItemsSource = VmQuotationManager.Instance.Instruments;
            this._AddNewSuccess = addNewSuccess;
        }

        public NewRelationWindow(List<int> instrumentIds,string code, Action<AdjustRelationViewModel> editSuccess)
        {
            InitializeComponent();
            this.SourceInstrument.ItemsSource = VmQuotationManager.Instance.Instruments;
            this._AddNewSuccess = editSuccess;
            this.RelationCode.Text = code;
            foreach (VmInstrument item in VmQuotationManager.Instance.Instruments)
            {
                if (instrumentIds.Contains(item.Id))
                {
                    this.SourceInstrument.SelectedItems.Add(item);
                }
            }
            
            
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Guid id = Guid.NewGuid();
            string code = this.RelationCode.Text;
            List<int> instruments = new List<int>();
            foreach (VmInstrument instrument in this.SourceInstrument.SelectedItems)
            {
                instruments.Add(instrument.Id);
            }
            this._NewId = id;
            this._NewCode = code;
            this._instrumentIds = instruments;
            ConsoleClient.Instance.AddNewRelation(id,code,instruments,CallBack);
        }

        public void CallBack(bool result)
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                if (result)
                {
                    AdjustRelationViewModel relation = new AdjustRelationViewModel(this._NewId, this._NewCode,this._instrumentIds);
                    this._AddNewSuccess(relation);
                    this.Message.Foreground = Brushes.Green;
                    this.Message.Text = "Success";
                }
                else
                {
                    this.Message.Foreground = Brushes.Red;
                    this.Message.Text = "Add New Failed";
                }

            }, null);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
