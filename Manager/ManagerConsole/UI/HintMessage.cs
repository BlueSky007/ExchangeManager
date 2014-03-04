using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace ManagerConsole.UI
{
    public class HintMessage
    {
        private TextBlock _MessageBlock;
        public HintMessage(TextBlock hintTextBlock)
        {
            this._MessageBlock = hintTextBlock;
        }

        public void ShowError(string message)
        {
            this._MessageBlock.Foreground = Brushes.Red;
            this._MessageBlock.Text = message;
        }

        public void ShowMessage(string message)
        {
            this._MessageBlock.Foreground = Brushes.Green;
            this._MessageBlock.Text = message;
        }

        public void Clear()
        {
            this._MessageBlock.Text = string.Empty;
        }
    }

    public enum EditMode
    {
        AddNew,
        Modify
    }
}
