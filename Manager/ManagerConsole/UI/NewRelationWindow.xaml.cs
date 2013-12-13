﻿using Infragistics.Controls.Interactions;
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

        public NewRelationWindow(Action<AdjustRelationViewModel> addNewSuccess)
        {
            InitializeComponent();
            this.SourceInstrument.ItemsSource = VmQuotationManager.Instance.Instruments;
            this._AddNewSuccess = addNewSuccess;
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
            ConsoleClient.Instance.AddNewRelation(id,code,instruments,CallBack);
        }

        public void CallBack(bool result)
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                if (result)
                {
                    AdjustRelationViewModel relation = new AdjustRelationViewModel(this._NewId, this._NewCode);
                    this._AddNewSuccess(relation);
                    this.Message.Foreground = Brushes.Green;
                    this.Message.Content = "Success";
                }
                else
                {
                    this.Message.Foreground = Brushes.Red;
                    this.Message.Content = "Add New Failed";
                }

            }, null);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}