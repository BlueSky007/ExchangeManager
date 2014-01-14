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
using ManagerConsole.Model;
using Manager.Common.QuotationEntities;
using Manager.Common;
using System.Xml.Linq;

namespace ManagerConsole.UI
{
    /// <summary>
    /// AdjustAndSpreadSettingControl.xaml 的交互逻辑
    /// </summary>
    public partial class AdjustAndSpreadSettingControl : UserControl,IControlLayout 
    {
        private ObservableCollection<AdjustRelationViewModel> _ItemSource = new ObservableCollection<AdjustRelationViewModel>();
        private bool IsEditing = false;

        public AdjustAndSpreadSettingControl()
        {
            try
            {
                InitializeComponent();
                this.BeginInitData();
            }
            catch (Exception ex)
            {
                Logger.TraceEvent(System.Diagnostics.TraceEventType.Error, "AdjustAndSpreadSettingControl.Construct Error\r\n{0}", ex.ToString());
                MessageBox.Show("Open AdjustAndSpreadSettingControl Error");
            }
        }

        public void BeginInitData()
        {
            ConsoleClient.Instance.GetQuotePolicyRelation(this.EndInitData);
        }

        public void EndInitData(List<QuotePolicyRelation> relations)
        {
            this.Dispatcher.BeginInvoke((Action<List<QuotePolicyRelation>>)delegate(List<QuotePolicyRelation> result){
                foreach (QuotePolicyRelation rela in result)
                {
                    this._ItemSource.Add(new AdjustRelationViewModel(rela.RelationId,rela.RelationCode,rela.instrumentIds));
                }
                this.AdjustSettingGrid.ItemsSource = this._ItemSource;
            },relations);
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            NewRelationWindow newRelaWin = new NewRelationWindow(this.AddNewSuccess);
            App.MainFrameWindow.MainFrame.Children.Add(newRelaWin);
            newRelaWin.IsModal = true;
            newRelaWin.Show();
            newRelaWin.BringToFront();
        }

        public void AddNewSuccess(AdjustRelationViewModel view)
        {
            this._ItemSource.Add(view);
        }

        private void AdjustUp_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEditing)
            {
                Button btn = sender as Button;
                Guid id = (Guid)btn.Tag;
                AdjustRelationViewModel set = this._ItemSource.SingleOrDefault(a => a.Id == id);
                ConsoleClient.Instance.SetQuotePolicyDetail(set.Id, Manager.Common.QuotePolicyDetailsSetAction.AdjustUp, set.AdjustIncrement);
            }
        }

        private void AdjustDn_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEditing)
            {
                Button btn = sender as Button;
                Guid id = (Guid)btn.Tag;
                AdjustRelationViewModel set = this._ItemSource.SingleOrDefault(a => a.Id == id);
                ConsoleClient.Instance.SetQuotePolicyDetail(set.Id, Manager.Common.QuotePolicyDetailsSetAction.AdjustDn, set.AdjustIncrement);
            }
        }

        private void SpreadUp_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEditing)
            {
                Button btn = sender as Button;
                Guid id = (Guid)btn.Tag;
                AdjustRelationViewModel set = this._ItemSource.SingleOrDefault(a => a.Id == id);
                ConsoleClient.Instance.SetQuotePolicyDetail(set.Id, Manager.Common.QuotePolicyDetailsSetAction.SpreadUp, set.SpreadIncrement);
            }
        }

        private void SpreadDn_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEditing)
            {
                Button btn = sender as Button;
                Guid id = (Guid)btn.Tag;
                AdjustRelationViewModel set = this._ItemSource.SingleOrDefault(a => a.Id == id);
                ConsoleClient.Instance.SetQuotePolicyDetail(set.Id, Manager.Common.QuotePolicyDetailsSetAction.SpreadDn, set.SpreadIncrement);
            }
        }

        private void AdjustReplace_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEditing)
            {
                Button btn = sender as Button;
                Guid id = (Guid)btn.Tag;
                AdjustRelationViewModel set = this._ItemSource.SingleOrDefault(a => a.Id == id);
                ConsoleClient.Instance.SetQuotePolicyDetail(set.Id, Manager.Common.QuotePolicyDetailsSetAction.AdjustReplace, set.AdjustReplacement);
            }
        }

        private void SpreadReplace_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEditing)
            {
                Button btn = sender as Button;
                Guid id = (Guid)btn.Tag;
                AdjustRelationViewModel set = this._ItemSource.SingleOrDefault(a => a.Id == id);
                ConsoleClient.Instance.SetQuotePolicyDetail(set.Id, Manager.Common.QuotePolicyDetailsSetAction.SpreadReplace, set.SpreadReplacement);
            }
        }

        private void AdjustSettingGrid_CellEnteredEditMode(object sender, Infragistics.Controls.Grids.EditingCellEventArgs e)
        {
            this.IsEditing = true;
        }

        private void AdjustSettingGrid_CellExitedEditMode(object sender, Infragistics.Controls.Grids.CellExitedEditingEventArgs e)
        {
            this.IsEditing = false;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            List<int> instrumentIds = this._ItemSource.SingleOrDefault(i => i.Id == (Guid)btn.Tag).InstrumentIds;
            NewRelationWindow newRelaWin = new NewRelationWindow(instrumentIds, this.EditSuccess);
            App.MainFrameWindow.MainFrame.Children.Add(newRelaWin);
            newRelaWin.IsModal = true;
            newRelaWin.Show();
            newRelaWin.BringToFront();
        }

        private void EditSuccess(AdjustRelationViewModel result)
        {
            this._ItemSource.SingleOrDefault(i => i.Id == result.Id).InstrumentIds = result.InstrumentIds;
        }

        public string GetLayout()
        {
            string layout = "";
            return layout;
        }

        public void SetLayout(XElement layout)
        {
        }
    }
}
