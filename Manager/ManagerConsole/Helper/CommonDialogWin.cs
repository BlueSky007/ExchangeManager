using Infragistics.Controls.Interactions;
using ManagerConsole.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AccountInfor = Manager.Common.AccountInformation;

namespace ManagerConsole.Helper
{
    public class CommonDialogWin
    {
        private Grid _LayoutContainer;
        //private XamDialogWindow _CommonDialogWin = null;
        private List<XamDialogWindow> _CommonDialogWins = new List<XamDialogWindow>();

        public CommonDialogWin(Grid layoutContainer)
        {
            this._LayoutContainer = layoutContainer;
        }
        public CommonDialogWin(Grid layoutContainer, double left, double top)
            : this(layoutContainer)
        {
            //this._CommonDialogWin = new XamDialogWindow
            //{
            //    StartupPosition = Infragistics.Controls.Interactions.StartupPosition.Manual,
            //    IsModal = false,
            //    CloseButtonVisibility = Visibility.Collapsed,
            //    MinimizeButtonVisibility=Visibility.Collapsed,
            //    MaximizeButtonVisibility=Visibility.Collapsed,
            //    Left=left,
            //    Top=top,
            //    Name = "_CommonDialogWin"
            //};			
        }

        public void ShowDialogWin(string message, string caption, double width, double height, double left, double top, bool isManualStartupPosition)
        {
            XamDialogWindow commonDialogWin = new XamDialogWindow()
            {
                Width = width,
                Height = height,
                Header = caption,
                StartupPosition = isManualStartupPosition ? Infragistics.Controls.Interactions.StartupPosition.Manual : Infragistics.Controls.Interactions.StartupPosition.Center,
                Left = left,
                Top = top,
                IsModal = true,
                CloseButtonVisibility = Visibility.Collapsed,
                MinimizeButtonVisibility = Visibility.Collapsed,
                MaximizeButtonVisibility = Visibility.Collapsed,
                Name = "_CommonDialogWin"
            };

            TextBlock msgLabel = new TextBlock() { Foreground = new SolidColorBrush(Colors.Blue), Text = message, Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap };
            Button okBtn = new Button() { Content = "OK", Width = 80, Height = 25, VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(5), HorizontalAlignment = HorizontalAlignment.Center, Tag = commonDialogWin, TabIndex = 1 };
            okBtn.Click += new RoutedEventHandler(okBtn_Click);

            StackPanel msgPanel = new StackPanel();
            msgPanel.Margin = new Thickness(5);
            msgPanel.HorizontalAlignment = HorizontalAlignment.Center;
            msgPanel.VerticalAlignment = VerticalAlignment.Center;
            msgPanel.Children.Add(msgLabel);
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalContentAlignment = VerticalAlignment.Stretch;
            scrollViewer.Content = msgPanel;
            Grid.SetRow(scrollViewer, 0);

            StackPanel btnPanel = new StackPanel();
            btnPanel.Margin = new Thickness(5);
            btnPanel.HorizontalAlignment = HorizontalAlignment.Center;
            btnPanel.VerticalAlignment = VerticalAlignment.Bottom;
            btnPanel.Children.Add(okBtn);
            Grid.SetRow(btnPanel, 1);

            Grid layoutGrid = new Grid();
            layoutGrid.RowDefinitions.Add(new RowDefinition());
            layoutGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            layoutGrid.Children.Add(scrollViewer);
            layoutGrid.Children.Add(btnPanel);

            commonDialogWin.Content = layoutGrid;
            int columnSpan = this._LayoutContainer.ColumnDefinitions.Count;
            int rowSpan = this._LayoutContainer.RowDefinitions.Count;
            if (columnSpan > 0) Grid.SetColumnSpan(commonDialogWin, columnSpan);
            if (rowSpan > 0) Grid.SetRowSpan(commonDialogWin, rowSpan);

            if (!this._LayoutContainer.Children.Contains(commonDialogWin))
            {
                this._LayoutContainer.Children.Add(commonDialogWin);
            }
            this._CommonDialogWins.Add(commonDialogWin);
            commonDialogWin.Show();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            Button clickBtn = (Button)sender;
            XamDialogWindow commonDialogWin = clickBtn.Tag as XamDialogWindow;
            this.Close(commonDialogWin);
        }
        public void ShowDialogWin(string message, string caption)
        {
            this.ShowDialogWin(message, caption, 350, 150, 0, 0, false);
        }
        public void ShowDialogWin(string message, string caption, double width, double height)
        {
            this.ShowDialogWin(message, caption, width, height, 0, 0, false);
        }
        private void Close(XamDialogWindow commonDialogWin)
        {
            if (commonDialogWin != null)
            {
                commonDialogWin.WindowState = Infragistics.Controls.Interactions.WindowState.Hidden;
                commonDialogWin.Close();
                this._LayoutContainer.Children.Remove(commonDialogWin);
                commonDialogWin = null;
            }
        }
        public void Close()
        {
            foreach (XamDialogWindow win in this._CommonDialogWins)
            {
                this.Close(win);
            }
        }
    }

    public class ConfirmDialogWin
    {
        public delegate void ConfirmDialogResultHandle(bool yesOrNo, UIElement uIElement);
        public event ConfirmDialogResultHandle OnConfirmDialogResult;

        private Grid _LayoutContainer;
        private XamDialogWindow _ConfirmDialogWin = null;
        private UIElement _ConfirmOptionElement = null;

        public ConfirmDialogWin(Grid layoutContainer)
        {
            this._LayoutContainer = layoutContainer;
        }
        public void ShowDialogWin(string message, string caption, double width, double height, UIElement uIElement)
        {
            this.ShowDialogWin(message, caption, width, height);

            var msgPanel = this._ConfirmDialogWin.FindName("msgPanel") as StackPanel;
            if (msgPanel != null)
            {
                msgPanel.Children.Add(uIElement);

                this._ConfirmOptionElement = uIElement;
            }
        }
        public void ShowDialogWin(string message, string caption, double width, double height)
        {
            if (this._ConfirmDialogWin == null)
            {
                this._ConfirmDialogWin = new XamDialogWindow()
                {
                    Width = width,
                    Height = height,
                    Header = caption,
                    StartupPosition = Infragistics.Controls.Interactions.StartupPosition.Center,
                    IsModal = true,
                    CloseButtonVisibility = Visibility.Collapsed,
                    MinimizeButtonVisibility = Visibility.Collapsed,
                    MaximizeButtonVisibility = Visibility.Collapsed,
                };
            }
            else
            {
                this._ConfirmDialogWin.Width = width;
                this._ConfirmDialogWin.Height = height;
                this._ConfirmDialogWin.Header = caption;
                this._ConfirmDialogWin.Content = null;
            }
            TextBlock msgLabel = new TextBlock() { Name = "msgLabel", Foreground = new SolidColorBrush(Colors.Blue), Text = message, Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, TextWrapping = TextWrapping.Wrap };

            Button ok = new Button() { Name = "okBtn", Content = "Yes", Width = 80, Height = 25, TabIndex = 1, Margin = new Thickness(8) };
            Button cancel = new Button() { Name = "cancelBtn", Content = "No", Width = 80, Height = 25 };
            ok.Click += new RoutedEventHandler(ok_Click);
            cancel.Click += new RoutedEventHandler(cancel_Click);

            StackPanel msgPanel = new StackPanel() { Name = "msgPanel" };
            msgPanel.Margin = new Thickness(5);
            msgPanel.HorizontalAlignment = HorizontalAlignment.Center;
            msgPanel.VerticalAlignment = VerticalAlignment.Center;
            msgPanel.Children.Add(msgLabel);
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalContentAlignment = VerticalAlignment.Stretch;
            scrollViewer.Content = msgPanel;
            Grid.SetRow(scrollViewer, 0);

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.Children.Add(ok);
            panel.Children.Add(cancel);
            panel.VerticalAlignment = VerticalAlignment.Bottom;
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.Margin = new Thickness(5);

            Grid layoutGrid = new Grid();
            layoutGrid.RowDefinitions.Add(new RowDefinition());
            layoutGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            layoutGrid.Children.Add(scrollViewer);
            layoutGrid.Children.Add(panel);

            this._ConfirmDialogWin.Content = layoutGrid;
            int columnSpan = this._LayoutContainer.ColumnDefinitions.Count;
            int rowSpan = this._LayoutContainer.RowDefinitions.Count;
            if (columnSpan > 0) Grid.SetColumnSpan(this._ConfirmDialogWin, columnSpan);
            if (rowSpan > 0) Grid.SetRowSpan(this._ConfirmDialogWin, rowSpan);

            if (!this._LayoutContainer.Children.Contains(this._ConfirmDialogWin))
            {
                this._LayoutContainer.Children.Add(this._ConfirmDialogWin);
            }

            this._ConfirmDialogWin.Show();
        }

        void cancel_Click(object sender, RoutedEventArgs e)
        {
            Button cancelBtn = sender as Button;
            Button okBtn = this._ConfirmDialogWin.FindName("okBtn") as Button;
            if (okBtn != null) okBtn.IsEnabled = false;
            if (cancelBtn != null) cancelBtn.IsEnabled = false;
            if (this.OnConfirmDialogResult != null)
            {
                this.OnConfirmDialogResult(false, this._ConfirmOptionElement);
            }
            this.Close();
        }

        void ok_Click(object sender, RoutedEventArgs e)
        {
            Button okBtn = sender as Button;
            Button cancelBtn = this._ConfirmDialogWin.FindName("cancelBtn") as Button;
            if (okBtn != null) okBtn.IsEnabled = false;
            if (cancelBtn != null) cancelBtn.IsEnabled = false;
            TextBlock msgLabel = this._ConfirmDialogWin.FindName("msgLabel") as TextBlock;
            if (msgLabel != null)
            {
                msgLabel.Text = "Processing......";
            }

            if (this.OnConfirmDialogResult != null)
            {
                this.OnConfirmDialogResult(true, this._ConfirmOptionElement);
            }

        }
        public void Close()
        {
            if (this._ConfirmDialogWin != null)
            {
                this._LayoutContainer.Children.Remove(this._ConfirmDialogWin);
                this._ConfirmDialogWin.Close();
                this._ConfirmDialogWin = null;
            }
        }

    }

    public class ConfirmOrderDialogWin
    {
        public delegate void ConfirmDialogResultHandle(bool yesOrNo, OrderTask orderTask, HandleAction action);
        public event ConfirmDialogResultHandle OnConfirmDialogResult;

        public delegate void ConfirmModifyPriceResultHandle(bool yesOrNo, string newPrice,OrderTask orderTask, HandleAction action);
        public event ConfirmModifyPriceResultHandle OnModifyPriceDialogResult;

        public delegate void RejectOrderResultHandle(bool yesOrNo,OrderTask orderTask,HandleAction action);
        public event RejectOrderResultHandle OnRejectOrderDialogResult;

        private Grid _LayoutContainer;
        private XamDialogWindow _ConfirmOrderDialogWin = null;
        private XamDialogWindow _ModifyPriceDialogWin = null;
        private XamDialogWindow _RejectOrderDialogWin = null;
        private UIElement _ConfirmOptionElement = null;
        private OrderTask _OrderTask;
        private HandleAction _HandleAction;
        private TextBox _NewPriceElement;

        public ConfirmOrderDialogWin(Grid layoutContainer)
        {
            this._LayoutContainer = layoutContainer;
        }
        public void ShowDialogWin(AccountInfor accountInfor, string caption, OrderTask orderTask, UIElement uIElement, HandleAction action)
        {
            this.ShowDialogWin(accountInfor, caption, orderTask, action);

            var captionPanel = this._ConfirmOrderDialogWin.FindName("captionPanel") as StackPanel;
            if (captionPanel != null)
            {
                captionPanel.Children.Add(uIElement);

                this._ConfirmOptionElement = uIElement;
                this._OrderTask = orderTask;
                this._HandleAction = action;
            }
        }

        //Modify SetPrice
        public void ShowDialogWin(string caption, OrderTask orderTask, HandleAction action)
        {
            if (this._ModifyPriceDialogWin == null)
            {
                this._ModifyPriceDialogWin = new XamDialogWindow()
                {
                    Width = 250,
                    Height = 150,
                    Header = caption,
                    StartupPosition = Infragistics.Controls.Interactions.StartupPosition.Center,
                    IsModal = true,
                    CloseButtonVisibility = Visibility.Collapsed,
                    MinimizeButtonVisibility = Visibility.Collapsed,
                    MaximizeButtonVisibility = Visibility.Collapsed,
                };
            }
            this._HandleAction = action;
            this._OrderTask = orderTask;
            TextBlock ModifyPriceLable = new TextBlock()
            {
                Name = "ModifyPriceLable",
                Foreground = new SolidColorBrush(Colors.Blue),
                Text = "Input new Price:",
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            TextBox NewPriceText = new TextBox()
            {
                Name = "NewPriceText",
                Width = 100,
                Foreground = new SolidColorBrush(Colors.Blue),
                Text = orderTask.SetPrice,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                TextAlignment = TextAlignment.Right
            };
            this._NewPriceElement = NewPriceText;

            StackPanel ModifyPricePanle = new StackPanel() { Name = "captionPanel", Orientation = Orientation.Horizontal };
            ModifyPricePanle.Margin = new Thickness(15, 0, 0, 0);
            ModifyPricePanle.HorizontalAlignment = HorizontalAlignment.Left;
            ModifyPricePanle.VerticalAlignment = VerticalAlignment.Center;
            ModifyPricePanle.Children.Add(ModifyPriceLable);
            ModifyPricePanle.Children.Add(NewPriceText);

            Grid.SetRow(ModifyPricePanle, 0);

            //按钮
            Button ModifyPriceOk = new Button() { Name = "ModifyPriceOkBtn", Content = "OK", Width = 60, Height = 24, TabIndex = 1, Margin = new Thickness(25, 0, 0, 0) };
            Button ModifyPriceCancel = new Button() { Name = "ModifyPriceCancelBtn", Content = "Cancel", Width = 60, Height = 24, Margin = new Thickness(25, 0, 0, 0) };
            ModifyPriceOk.Click += new RoutedEventHandler(ModifyPriceOkBtn_Click);
            ModifyPriceCancel.Click += new RoutedEventHandler(ModifyPriceCancelBtn_Click);

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.Children.Add(ModifyPriceOk);
            panel.Children.Add(ModifyPriceCancel);
            panel.VerticalAlignment = VerticalAlignment.Bottom;
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.Margin = new Thickness(5);
            Grid.SetRow(panel, 1);

            Grid layoutGrid = new Grid();

            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0.5, 0);
            brush.EndPoint = new Point(0.5, 1);
            GradientStop stop = new GradientStop();

            layoutGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
            layoutGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
            layoutGrid.Children.Add(ModifyPricePanle);
            layoutGrid.Children.Add(panel);

            this._ModifyPriceDialogWin.Content = layoutGrid;
            int columnSpan = this._LayoutContainer.ColumnDefinitions.Count;
            int rowSpan = this._LayoutContainer.RowDefinitions.Count;
            if (columnSpan > 0) Grid.SetColumnSpan(this._ModifyPriceDialogWin, columnSpan);
            if (rowSpan > 0) Grid.SetRowSpan(this._ModifyPriceDialogWin, rowSpan);

            if (!this._LayoutContainer.Children.Contains(this._ModifyPriceDialogWin))
            {
                this._LayoutContainer.Children.Add(this._ModifyPriceDialogWin);
            }

            this._ModifyPriceDialogWin.Show();

        }

        //Reject Order DialogWin
        public void ShowRejectOrderWin(string rejectMessage, OrderTask orderTask, HandleAction action)
        {
            if (this._RejectOrderDialogWin == null)
            {
                this._RejectOrderDialogWin = new XamDialogWindow()
                {
                    Width = 300,
                    Height = 140,
                    Header = "Confirm",
                    StartupPosition = Infragistics.Controls.Interactions.StartupPosition.Center,
                    IsModal = true,
                    CloseButtonVisibility = Visibility.Collapsed,
                    MinimizeButtonVisibility = Visibility.Collapsed,
                    MaximizeButtonVisibility = Visibility.Collapsed,
                };
            }
            this._OrderTask = orderTask;
            this._HandleAction = action;
            TextBlock alertLable = new TextBlock()
            {
                Name = "AlertLable",
                Foreground = new SolidColorBrush(Colors.Blue),
                Text = "Are you sure?",
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock RejectOrderMessage = new TextBlock()
            {
                Name = "RejectOrderMessage",
                Foreground = new SolidColorBrush(Colors.Blue),
                Text = rejectMessage,
                Margin = new Thickness(5,15,0,0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            StackPanel RejectOrderPanel = new StackPanel() { Name = "RejectOrderPanel", Orientation = Orientation.Vertical };
            RejectOrderPanel.Margin = new Thickness(15, 0, 0, 0);
            RejectOrderPanel.HorizontalAlignment = HorizontalAlignment.Left;
            RejectOrderPanel.VerticalAlignment = VerticalAlignment.Center;
            RejectOrderPanel.Children.Add(alertLable);
            RejectOrderPanel.Children.Add(RejectOrderMessage);

            Grid.SetRow(RejectOrderPanel, 0);

            //按钮
            Button RejectOrderOk = new Button() { Name = "RejectOrderOk", Content = "Yes", Width = 60, Height = 24, TabIndex = 1, Margin = new Thickness(25, 5, 0, 0) };
            Button RejectOrderCancel = new Button() { Name = "RejectOrderCancel", Content = "No", Width = 60, Height = 24, Margin = new Thickness(25, 5, 0, 0) };
            RejectOrderOk.Click += new RoutedEventHandler(RejectOrderOk_Click);
            RejectOrderCancel.Click += new RoutedEventHandler(RejectOrderCancel_Click);

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.Children.Add(RejectOrderOk);
            panel.Children.Add(RejectOrderCancel);
            panel.VerticalAlignment = VerticalAlignment.Bottom;
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.Margin = new Thickness(5);
            Grid.SetRow(panel, 1);

            Grid layoutGrid = new Grid();

            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0.5, 0);
            brush.EndPoint = new Point(0.5, 1);
            GradientStop stop = new GradientStop();

            layoutGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });
            layoutGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });
            layoutGrid.Children.Add(RejectOrderPanel);
            layoutGrid.Children.Add(panel);

            this._RejectOrderDialogWin.Content = layoutGrid;
            int columnSpan = this._LayoutContainer.ColumnDefinitions.Count;
            int rowSpan = this._LayoutContainer.RowDefinitions.Count;
            if (columnSpan > 0) Grid.SetColumnSpan(this._RejectOrderDialogWin, columnSpan);
            if (rowSpan > 0) Grid.SetRowSpan(this._RejectOrderDialogWin, rowSpan);

            if (!this._LayoutContainer.Children.Contains(this._RejectOrderDialogWin))
            {
                this._LayoutContainer.Children.Add(this._RejectOrderDialogWin);
            }

            this._RejectOrderDialogWin.Show();
        }


        #region All Button Event
        //Account Information
        public void ShowDialogWin(AccountInfor accountInfor, string caption, OrderTask orderTask, HandleAction action)
        {
            if (this._ConfirmOrderDialogWin == null)
            {
                this._ConfirmOrderDialogWin = new XamDialogWindow()
                {
                    Width = 350,
                    Height = 450,
                    Header = caption,
                    StartupPosition = Infragistics.Controls.Interactions.StartupPosition.Center,
                    IsModal = true,
                    CloseButtonVisibility = Visibility.Collapsed,
                    MinimizeButtonVisibility = Visibility.Collapsed,
                    MaximizeButtonVisibility = Visibility.Collapsed,
                };
                this._OrderTask = orderTask;
                this._HandleAction = action;
            }
            else
            {
                this._ConfirmOrderDialogWin.Width = 350;
                this._ConfirmOrderDialogWin.Height = 450;
                this._ConfirmOrderDialogWin.Header = caption;
                this._ConfirmOrderDialogWin.Content = null;
            }
            TextBlock BLLable = new TextBlock()
            {
                Name = "BL",
                Foreground = new SolidColorBrush(Colors.White),
                Text = "BL",
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            TextBlock BalanceLable = new TextBlock()
            {
                Name = "Balance",
                Foreground = new SolidColorBrush(Colors.White),
                Text = "Balance",
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            TextBlock EquityLable = new TextBlock()
            {
                Name = "Equity",
                Foreground = new SolidColorBrush(Colors.White),
                Text = "Equity",
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            TextBlock NecessaryLable = new TextBlock()
            {
                Name = "Necessary",
                Foreground = new SolidColorBrush(Colors.White),
                Text = "Necessary",
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            TextBlock UsableLable = new TextBlock()
            {
                Name = "Usable",
                Foreground = new SolidColorBrush(Colors.White),
                Text = "Usable",
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            TextBlock TotalBuyLable = new TextBlock()
            {
                Name = "TotalBuy",
                Foreground = new SolidColorBrush(Colors.White),
                Text = "TotalBuy",
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            TextBlock TotalSellLable = new TextBlock()
            {
                Name = "TotalSell",
                Foreground = new SolidColorBrush(Colors.White),
                Text = "TotalSell",
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            TextBlock NetLable = new TextBlock()
            {
                Name = "Net",
                Foreground = new SolidColorBrush(Colors.Blue),
                Text = "Net",
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            TextBlock SetPriceLable = new TextBlock()
            {
                Name = "SetPrice",
                Foreground = new SolidColorBrush(Colors.White),
                Text = "SetPrice",
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            TextBlock ExecutePriceLable = new TextBlock()
            {
                Name = "ExecutePrice",
                Foreground = new SolidColorBrush(Colors.White),
                Text = "ExecutePrice",
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            TextBlock LotLable = new TextBlock()
            {
                Name = "Lot",
                Foreground = new SolidColorBrush(Colors.White),
                Text = "Lot",
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };



            StackPanel captionPanel = new StackPanel() { Name = "captionPanel", Orientation = Orientation.Vertical };
            captionPanel.Margin = new Thickness(0, 10, 0, 0);
            captionPanel.HorizontalAlignment = HorizontalAlignment.Center;
            captionPanel.VerticalAlignment = VerticalAlignment.Center;
            captionPanel.Children.Add(BLLable);
            captionPanel.Children.Add(BalanceLable);
            captionPanel.Children.Add(EquityLable);
            captionPanel.Children.Add(NecessaryLable);
            captionPanel.Children.Add(UsableLable);
            captionPanel.Children.Add(TotalBuyLable);
            captionPanel.Children.Add(TotalSellLable);
            captionPanel.Children.Add(NetLable);
            captionPanel.Children.Add(SetPriceLable);
            captionPanel.Children.Add(ExecutePriceLable);
            captionPanel.Children.Add(LotLable);

            Grid.SetRow(captionPanel, 0);
            Grid.SetColumn(captionPanel, 0);


            //Value Column
            CheckBox BLCheckBox = new CheckBox()
            {
                Name = "BLText",
                Foreground = new SolidColorBrush(Colors.White),
                IsChecked = true,
                Margin = new Thickness(8),
                IsEnabled = true,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            TextBlock BalanceText = new TextBlock()
            {
                Name = "BalanceText",
                Foreground = new SolidColorBrush(Colors.White),
                Text = accountInfor.Balance.ToString(),
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            TextBlock EquityText = new TextBlock()
            {
                Name = "EquityText",
                Foreground = new SolidColorBrush(Colors.White),
                Text = accountInfor.Equity.ToString(),
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            TextBlock NecessaryText = new TextBlock()
            {
                Name = "NecessaryText",
                Foreground = new SolidColorBrush(Colors.White),
                Text = accountInfor.Necessary.ToString(),
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            TextBlock UsableText = new TextBlock()
            {
                Name = "UsableText",
                Foreground = new SolidColorBrush(Colors.White),
                Text = accountInfor.Usable.ToString(),
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            TextBlock TotalBuyText = new TextBlock()
            {
                Name = "TotalBuyText",
                Foreground = new SolidColorBrush(Colors.White),
                Text = accountInfor.BuyLotBalanceSum.ToString(),
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            TextBlock TotalSellText = new TextBlock()
            {
                Name = "TotalSellText",
                Foreground = new SolidColorBrush(Colors.White),
                Text = accountInfor.SellLotBalanceSum.ToString(),
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            TextBlock NetText = new TextBlock()
            {
                Name = "NetText",
                Foreground = new SolidColorBrush(Colors.White),
                Text = (accountInfor.BuyLotBalanceSum - accountInfor.SellLotBalanceSum).ToString(),
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            TextBlock SetPriceText = new TextBlock()
            {
                Name = "SetPriceText",
                Foreground = new SolidColorBrush(Colors.White),
                Text = orderTask.SetPrice,
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            TextBlock ExecutePriceText = new TextBlock()
            {
                Name = "ExecutePriceText",
                Foreground = new SolidColorBrush(Colors.White),
                Text = orderTask.SetPrice,
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            TextBox LotText = new TextBox()
            {
                Name = "LotText",
                Width = 100,
                Foreground = new SolidColorBrush(Colors.Blue),
                Text = orderTask.Lot.ToString(),
                Margin = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                TextAlignment = TextAlignment.Right
            };



            StackPanel valuePanel = new StackPanel() { Name = "valuePanel", Orientation = Orientation.Vertical };
            valuePanel.Margin = new Thickness(0, 10, 0, 0);
            valuePanel.HorizontalAlignment = HorizontalAlignment.Right;
            valuePanel.VerticalAlignment = VerticalAlignment.Center;
            valuePanel.Children.Add(BLCheckBox);
            valuePanel.Children.Add(BalanceText);
            valuePanel.Children.Add(EquityText);
            valuePanel.Children.Add(NecessaryText);
            valuePanel.Children.Add(UsableText);
            valuePanel.Children.Add(TotalBuyText);
            valuePanel.Children.Add(TotalSellText);
            valuePanel.Children.Add(NetText);
            valuePanel.Children.Add(SetPriceText);
            valuePanel.Children.Add(ExecutePriceText);
            valuePanel.Children.Add(LotText);

            Grid.SetRow(valuePanel, 0);
            Grid.SetColumn(valuePanel, 1);

            //按钮
            Button ok = new Button() { Name = "okBtn", Content = "Yes", Width = 60, Height = 25, TabIndex = 1, Margin = new Thickness(25, 0, 0, 0) };
            Button cancel = new Button() { Name = "cancelBtn", Content = "No", Width = 60, Height = 25, Margin = new Thickness(25, 0, 0, 0) };
            ok.Click += new RoutedEventHandler(ok_Click);
            cancel.Click += new RoutedEventHandler(cancel_Click);

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.Children.Add(ok);
            panel.Children.Add(cancel);
            panel.VerticalAlignment = VerticalAlignment.Bottom;
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.Margin = new Thickness(5);
            Grid.SetRow(panel, 1);
            Grid.SetColumnSpan(panel, 2);

            Grid layoutGrid = new Grid();

            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0.5, 0);
            brush.EndPoint = new Point(0.5, 1);
            GradientStop stop = new GradientStop();


            stop = new GradientStop();
            stop.Color = Colors.LightGray;
            stop.Offset = 0.25;

            stop = new GradientStop();
            stop.Color = Color.FromArgb(0XFF, 0x4F, 0x4F, 0x4F);
            stop.Offset = 0.65;
            brush.GradientStops.Add(stop);



            layoutGrid.Background = brush;


            layoutGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(350) });
            layoutGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50) });
            layoutGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(160) });
            layoutGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(160) });
            layoutGrid.Children.Add(captionPanel);
            //layoutGrid.Children.Add(scrollViewer2);
            layoutGrid.Children.Add(valuePanel);
            layoutGrid.Children.Add(panel);

            this._ConfirmOrderDialogWin.Content = layoutGrid;
            int columnSpan = this._LayoutContainer.ColumnDefinitions.Count;
            int rowSpan = this._LayoutContainer.RowDefinitions.Count;
            if (columnSpan > 0) Grid.SetColumnSpan(this._ConfirmOrderDialogWin, columnSpan);
            if (rowSpan > 0) Grid.SetRowSpan(this._ConfirmOrderDialogWin, rowSpan);

            if (!this._LayoutContainer.Children.Contains(this._ConfirmOrderDialogWin))
            {
                this._LayoutContainer.Children.Add(this._ConfirmOrderDialogWin);
            }

            this._ConfirmOrderDialogWin.Show();
        }

        void cancel_Click(object sender, RoutedEventArgs e)
        {
            Button cancelBtn = sender as Button;
            Button okBtn = this._ConfirmOrderDialogWin.FindName("okBtn") as Button;
            if (okBtn != null) okBtn.IsEnabled = false;
            if (cancelBtn != null) cancelBtn.IsEnabled = false;
            if (this.OnConfirmDialogResult != null)
            {
                this.OnConfirmDialogResult(false,this._OrderTask,this._HandleAction);
            }
            this.Close();
        }

        void  ok_Click(object sender, RoutedEventArgs e)
        {
            Button okBtn = sender as Button;
            Button cancelBtn = this._ConfirmOrderDialogWin.FindName("cancelBtn") as Button;
            if (okBtn != null) okBtn.IsEnabled = false;
            if (cancelBtn != null) cancelBtn.IsEnabled = false;
            TextBlock msgLabel = this._ConfirmOrderDialogWin.FindName("msgLabel") as TextBlock;

            if (this.OnConfirmDialogResult != null)
            {
                this.OnConfirmDialogResult(true,this._OrderTask,this._HandleAction);
            }
            this.Close();
        }

        public void Close()
        {
            if (this._ConfirmOrderDialogWin != null)
            {
                this._LayoutContainer.Children.Remove(this._ConfirmOrderDialogWin);
                this._ConfirmOrderDialogWin.Close();
                this._ConfirmOrderDialogWin = null;
            }
            if (this._ModifyPriceDialogWin != null)
            {
                this._LayoutContainer.Children.Remove(this._ModifyPriceDialogWin);
                this._ModifyPriceDialogWin.Close();
                this._ModifyPriceDialogWin = null;
            }
            if (this._RejectOrderDialogWin != null)
            {
                this._LayoutContainer.Children.Remove(this._RejectOrderDialogWin);
                this._RejectOrderDialogWin.Close();
                this._RejectOrderDialogWin = null;
            }
        }

        //Modify Price Event
        void ModifyPriceOkBtn_Click(object sender, RoutedEventArgs e)
        {
            Button okBtn = sender as Button;
            Button cancelBtn = this._ModifyPriceDialogWin.FindName("ModifyPriceCancel") as Button;
            if (okBtn != null) okBtn.IsEnabled = false;
            if (cancelBtn != null) cancelBtn.IsEnabled = false;
            TextBox priceText = this._ModifyPriceDialogWin.FindName("NewPriceText") as TextBox;
            var newPrice = this._NewPriceElement.Text;

            if (this.OnModifyPriceDialogResult != null)
            {
                this.OnModifyPriceDialogResult(true, newPrice, this._OrderTask, this._HandleAction);
            }
            this.Close();
        }

        void ModifyPriceCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Button cancelBtn = sender as Button;
            Button okBtn = this._ModifyPriceDialogWin.FindName("ModifyPriceOkBtn") as Button;
            if (okBtn != null) okBtn.IsEnabled = false;
            if (cancelBtn != null) cancelBtn.IsEnabled = false;
            if (this.OnModifyPriceDialogResult != null)
            {
                this.OnModifyPriceDialogResult(false, "",null, this._HandleAction);
            }
            this.Close();
        }

        

        //Reject Order Event
        void RejectOrderOk_Click(object sender, RoutedEventArgs e)
        {
            Button okBtn = sender as Button;
            Button cancelBtn = this._RejectOrderDialogWin.FindName("RejectOrderCancel") as Button;
            if (okBtn != null) okBtn.IsEnabled = false;
            if (cancelBtn != null) cancelBtn.IsEnabled = false;

            if (this.OnRejectOrderDialogResult != null)
            {
                this.OnRejectOrderDialogResult(true, this._OrderTask,this._HandleAction);
            }
            this.Close();
        }

        void RejectOrderCancel_Click(object sender, RoutedEventArgs e)
        {
            Button cancelBtn = sender as Button;
            Button okBtn = this._RejectOrderDialogWin.FindName("RejectOrderOk") as Button;
            if (okBtn != null) okBtn.IsEnabled = false;
            if (cancelBtn != null) cancelBtn.IsEnabled = false;
            if (this.OnRejectOrderDialogResult != null)
            {
                this.OnRejectOrderDialogResult(false, null,this._HandleAction);
            }
            this.Close();
        }

        #endregion

    }
}
