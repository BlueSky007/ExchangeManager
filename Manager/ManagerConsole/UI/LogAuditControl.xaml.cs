using Infragistics.Windows.Ribbon;
using Manager.Common;
using ManagerConsole.FramePages;
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
    /// Interaction logic for LogAuditControl.xaml
    /// </summary>
    public partial class LogAuditControl : UserControl
    {
        public LogAuditControl()
        {
            InitializeComponent();
            this.InitUI();
        }

        private void InitUI()
        {
            this._LogRibbon.Theme = "IGTheme";
        }

        private void ButtonTool_Click(object sender, RoutedEventArgs e)
        {
            ButtonTool clickBtn = (ButtonTool)e.OriginalSource;
            UserControl currentLogPage = this.LogContentFrame.Content as UserControl;
            switch (clickBtn.Id)
            {
                case "_QuotePriceLog":
                    this.LogContentFrame.Content = new DealingLogControl(LogType.QuotePrice);
                    break;
                case "_QuoteOrderLog":
                    this.LogContentFrame.Content = new DealingLogControl(LogType.QuoteOrder);
                    break;
                case "_SettingChangeLog":
                    this.LogContentFrame.Content = new DealingLogControl(LogType.SettingChange);
                    break;
                case "_AdjustPriceLog":
                    this.LogContentFrame.Content = new SourceManagerLogControl(LogType.AdjustPrice);
                    break;
                case "_SourceChangeLog":
                    this.LogContentFrame.Content = new SourceManagerLogControl(LogType.SourceChange);
                    break;

            }
        }
    }
}
