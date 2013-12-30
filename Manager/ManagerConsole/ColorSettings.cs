using Infragistics.Controls.Menus;
using ManagerConsole.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ManagerConsole
{
    public enum BlackOrWhite
    {
        Black,
        White
    }
    public class ColorSettings : INotifyPropertyChanged
    {
        private static readonly ColorTemplate _BlackColorTemplate = new BlackColorTemplate();
        private static readonly ColorTemplate _WhiteColorTemplate = new WhiteColorTemplate();

        private static ColorTemplate _CurrentColorTemplate;
        private static ColorSettings _CurrentColorSettings;
        private static string _ColorSettingsFromServer;
        public static bool _UseInstrumentStyleForWF;
        public static int _FrontNumber = 0;
        public static int _BackNumber = 0;
        public event PropertyChangedEventHandler PropertyChanged;

        static ColorSettings()
        {
            ColorSettings._CurrentColorTemplate = ColorSettings._BlackColorTemplate;
        }

        public static void Initailize(string defualtBackground, string colorSettings, bool useInstrumentStyleForWF)
        {
            BlackOrWhite blackOrWhite = BlackOrWhite.White;
            _ColorSettingsFromServer = colorSettings;

            if (Enum.TryParse<BlackOrWhite>(defualtBackground, true, out blackOrWhite))
            {
                if (blackOrWhite == BlackOrWhite.Black && ColorSettings._CurrentColorTemplate != ColorSettings._BlackColorTemplate)
                {
                    ColorSettings._CurrentColorTemplate = ColorSettings._BlackColorTemplate;
                }
                else if (blackOrWhite == BlackOrWhite.White && ColorSettings._CurrentColorTemplate != ColorSettings._WhiteColorTemplate)
                {
                    ColorSettings._CurrentColorTemplate = ColorSettings._WhiteColorTemplate;
                }
            }

            ApplyColorSettingsFromServer();
        }

        internal static void ApplyColorSettingsFromServer()
        {
            try
            {
                string colorSettings = _ColorSettingsFromServer;
                string[] colors = colorSettings.Split('&');

                if (colors.Length == 10)
                {
                    string[] toolbarBackground1 = colors[5].Split(' ');
                    string[] toolbarBackground2 = colors[6].Split(' ');

                    Color color1 = Color.FromArgb(byte.Parse(toolbarBackground1[0]), byte.Parse(toolbarBackground1[1]), byte.Parse(toolbarBackground1[2]), byte.Parse(toolbarBackground1[3]));
                    Color color2 = Color.FromArgb(byte.Parse(toolbarBackground2[0]), byte.Parse(toolbarBackground2[1]), byte.Parse(toolbarBackground2[2]), byte.Parse(toolbarBackground2[3]));
                    LinearGradientBrush brush = new LinearGradientBrush();
                    brush.StartPoint = new Point(0.5, 0);
                    brush.EndPoint = new Point(0.5, 1);
                    GradientStop stop = new GradientStop();
                    stop.Color = color1;
                    stop.Offset = 0;
                    brush.GradientStops.Add(stop);

                    stop = new GradientStop();
                    stop.Color = color2;
                    stop.Offset = 0.45;
                    brush.GradientStops.Add(stop);

                    stop = new GradientStop();
                    stop.Color = color2;
                    stop.Offset = 0.55;
                    brush.GradientStops.Add(stop);

                    stop = new GradientStop();
                    stop.Color = color1;
                    stop.Offset = 1;
                    brush.GradientStops.Add(stop);
                    ColorSettings.CurrentColorSettings.ColorTemplate.MainControlHeaderBackground = brush;

                    string[] toolbarForeground = colors[7].Split(' ');
                    Color color = Color.FromArgb(byte.Parse(toolbarForeground[0]), byte.Parse(toolbarForeground[1]), byte.Parse(toolbarForeground[2]), byte.Parse(toolbarForeground[3]));
                    ColorSettings.CurrentColorSettings.ColorTemplate.MainMenuForeground = new SolidColorBrush(color);

                    Style style = ColorSettings.CurrentColorSettings.ColorTemplate.XamGridGroupByAreaBackgroundStyle;
                    style.Setters.Clear();
                    style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(color1)));
                    style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(color)));

                    if (ColorSettings.CurrentColorSettings.CurrentBackground == BlackOrWhite.Black)
                    {
                        string[] positiveNumberColorForBalckBackground = colors[8].Split(' ');
                        ColorSettings.CurrentColorSettings.ColorTemplate.PositiveNumberColor
                            = new SolidColorBrush(Color.FromArgb(byte.Parse(positiveNumberColorForBalckBackground[0]), byte.Parse(positiveNumberColorForBalckBackground[1]), byte.Parse(positiveNumberColorForBalckBackground[2]), byte.Parse(positiveNumberColorForBalckBackground[3])));
                    }
                    else
                    {
                        string[] positiveNumberColorForWhiteBackground = colors[9].Split(' ');
                        ColorSettings.CurrentColorSettings.ColorTemplate.PositiveNumberColor
                            = new SolidColorBrush(Color.FromArgb(byte.Parse(positiveNumberColorForWhiteBackground[0]), byte.Parse(positiveNumberColorForWhiteBackground[1]), byte.Parse(positiveNumberColorForWhiteBackground[2]), byte.Parse(positiveNumberColorForWhiteBackground[3])));
                    }
                }
                else
                {
                    Style style = ColorSettings.CurrentColorSettings.ColorTemplate.XamGridGroupByAreaBackgroundStyle;
                    style.Setters.Clear();
                    style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Colors.Gray)));
                    style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(Colors.White)));
                }
            }
            catch (Exception exception)
            {
                Manager.Common.Logger.TraceEvent(TraceEventType.Error, exception.ToString());
            }
        }

        public ColorTemplate ColorTemplate
        {
            get { return ColorSettings._CurrentColorTemplate; }
        }

        public BlackOrWhite CurrentBackground
        {
            get { return ColorSettings._CurrentColorTemplate == ColorSettings._BlackColorTemplate ? BlackOrWhite.Black : BlackOrWhite.White; }
        }

        public static ColorSettings CurrentColorSettings
        {
            get
            {
                if (_CurrentColorSettings == null)
                {
                    _CurrentColorSettings = Application.Current.Resources["ColorSettings"] as ColorSettings;
                }
                return _CurrentColorSettings;
            }
        }

        public void ChangeColor(BlackOrWhite blackOrWhite)
        {
            this.ChangeColor(blackOrWhite, true);
        }

        private void ChangeColor(BlackOrWhite blackOrWhite, bool saveSettings)
        {
            if (blackOrWhite == BlackOrWhite.Black && ColorSettings._CurrentColorTemplate != ColorSettings._BlackColorTemplate)
            {
                ColorSettings._CurrentColorTemplate = ColorSettings._BlackColorTemplate;
                ApplyColorSettingsFromServer();
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ColorTemplate"));
                }
            }
            else if (blackOrWhite == BlackOrWhite.White && ColorSettings._CurrentColorTemplate != ColorSettings._WhiteColorTemplate)
            {
                ColorSettings._CurrentColorTemplate = ColorSettings._WhiteColorTemplate;
                ApplyColorSettingsFromServer();
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ColorTemplate"));
                }
            }
        }

    }

    public abstract class ColorTemplate
    {
        public abstract SolidColorBrush Background
        {
            get;
        }

        public abstract SolidColorBrush Foreground
        {
            get;
        }

        public abstract SolidColorBrush BorderBrush
        {
            get;
        }

        public abstract SolidColorBrush CompanyForeground
        {
            get;
        }

        public abstract SolidColorBrush CompanyBackground
        {
            get;
        }

        public abstract Style XamMenuSeparatorStyle
        {
            get;
        }

        private Style _XamGridGroupByAreaBackgroundStyle = null;
        public Style XamGridGroupByAreaBackgroundStyle
        {
            get { return this._XamGridGroupByAreaBackgroundStyle == null ? (Application.Current.Resources["GroupByAreaCellControlStyle"] as Style) : this._XamGridGroupByAreaBackgroundStyle; }
            set { this._XamGridGroupByAreaBackgroundStyle = value; }
        }

        private Brush _MainControlHeaderBackground = null;
        public virtual Brush MainControlHeaderBackground
        {
            get { return this._MainControlHeaderBackground == null ? this.GetMainControlHeaderBackground() : this._MainControlHeaderBackground; }
            set { this._MainControlHeaderBackground = value; }
        }

        protected abstract Brush GetMainControlHeaderBackground();

        public abstract SolidColorBrush MainControlHeaderForeground
        {
            get;
        }

        private Brush _MainMenuForeground = null;
        public virtual Brush MainMenuForeground
        {
            get { return this._MainMenuForeground == null ? this.MainControlHeaderForeground : this._MainMenuForeground; }
            set { this._MainMenuForeground = value; }
        }

        public virtual Brush PositiveNumberColor
        {
            get;
            set;
        }

        public abstract Style XamGridHeaderStyle
        {
            get;
        }

        public abstract Style XamGridCellStyle
        {
            get;
        }

        public abstract Color TurnoverInstrumentBackControlButtonBackground1
        {
            get;
        }

        public abstract Color TurnoverInstrumentBackControlButtonBackground2
        {
            get;
        }

        public abstract Color TurnoverInstrumentBackControlButtonBackground3
        {
            get;
        }
        public abstract Color TurnoverInstrumentBackControlButtonBackground4
        {
            get;
        }
        public abstract Color TurnoverInstrumentBackControlButtonMouseOverColor1
        {
            get;
        }

        public abstract Color TurnoverInstrumentBackControlButtonMouseOverColor2
        {
            get;
        }

        public abstract Color TurnoverInstrumentBackControlButtonMouseOverColor3
        {
            get;
        }

        public abstract Style TabItemStyle
        {
            get;
        }
    }

    public class WhiteColorTemplate : ColorTemplate
    {
        public override SolidColorBrush Background
        {
            get { return SolidColorBrushes.White; }
        }

        public override SolidColorBrush Foreground
        {
            get { return SolidColorBrushes.Black; }
        }

        public override SolidColorBrush BorderBrush
        {
            get { return SolidColorBrushes.BorderBrush; }
        }

        public override SolidColorBrush CompanyForeground
        {
            get { return new SolidColorBrush(Color.FromArgb(0XFF, 0X00, 0X00, 0XFF)); }
        }

        public override SolidColorBrush CompanyBackground
        {
            get { return new SolidColorBrush(Colors.LightGray); }
        }

        protected override Brush GetMainControlHeaderBackground()
        {
            return new SolidColorBrush(Color.FromArgb(0XFF, 0XF0, 0XF0, 0XF0));
        }

        public override Style XamMenuSeparatorStyle
        {
            get { return new Style(typeof(XamMenuSeparator)); }
        }

        public override SolidColorBrush MainControlHeaderForeground
        {
            get { return SolidColorBrushes.Black; }
        }

        public override Style XamGridHeaderStyle
        {
            get { return Application.Current.Resources["GridHeaderCellStyle"] as Style; }
        }

        public override Style XamGridCellStyle
        {
            get { return Application.Current.Resources["XamGridBlackCellStyle"] as Style; }
        }

        public override Color TurnoverInstrumentBackControlButtonBackground1
        {
            get { return (Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)); }
        }

        public override Color TurnoverInstrumentBackControlButtonBackground2
        {
            get { return (Color.FromArgb(0xF9, 0xFF, 0xFF, 0xFF)); }
        }

        public override Color TurnoverInstrumentBackControlButtonBackground3
        {
            get { return (Color.FromArgb(0xE5, 0xFF, 0xFF, 0xFF)); }
        }

        public override Color TurnoverInstrumentBackControlButtonBackground4
        {
            get { return (Color.FromArgb(0xC6, 0xFF, 0xFF, 0xFF)); }
        }

        public override Color TurnoverInstrumentBackControlButtonMouseOverColor1
        {
            get { return (Color.FromArgb(0xF2, 0xFF, 0xFF, 0xFF)); }
        }

        public override Color TurnoverInstrumentBackControlButtonMouseOverColor2
        {
            get { return (Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF)); }
        }

        public override Color TurnoverInstrumentBackControlButtonMouseOverColor3
        {
            get { return (Color.FromArgb(0x7F, 0xFF, 0xFF, 0xFF)); }
        }

        public override Style TabItemStyle
        {
            get { return Application.Current.Resources["TabItemStyle"] as Style; }
        }
    }

    public class BlackColorTemplate : ColorTemplate
    {
        public override SolidColorBrush Background
        {
            get { return SolidColorBrushes.Black; }
        }

        public override SolidColorBrush Foreground
        {
            get { return SolidColorBrushes.White; }
        }

        public override SolidColorBrush BorderBrush
        {
            get { return SolidColorBrushes.BorderBrush; }
        }

        public override SolidColorBrush CompanyForeground
        {
            get { return new SolidColorBrush(Color.FromArgb(0XFF, 0XFF, 0XFF, 0XFF)); }
        }

        protected override Brush GetMainControlHeaderBackground()
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0.5, 0);
            brush.EndPoint = new Point(0.5, 1);
            GradientStop stop = new GradientStop();
            stop.Color = Colors.Black;
            stop.Offset = 0;
            brush.GradientStops.Add(stop);

            stop = new GradientStop();
            stop.Color = Color.FromArgb(0x80, 0x5F, 0x5F, 0x5F);
            stop.Offset = 0.45;
            brush.GradientStops.Add(stop);

            stop = new GradientStop();
            stop.Color = Color.FromArgb(0x80, 0x5F, 0x5F, 0x5F);
            stop.Offset = 0.55;
            brush.GradientStops.Add(stop);

            stop = new GradientStop();
            stop.Color = Colors.Black;
            stop.Offset = 1;
            brush.GradientStops.Add(stop);

            return brush;
        }        

        public override SolidColorBrush CompanyBackground
        {
            get { return new SolidColorBrush(Color.FromArgb(0XFF, 0X00, 0X00, 0X00)); }
        }

        public override Style XamMenuSeparatorStyle
        {
            get { return Application.Current.Resources["XamMenuSeparatorBackgroundStyle"] as Style; }
        }

        public override SolidColorBrush MainControlHeaderForeground
        {
            get { return SolidColorBrushes.White; }
        }

        public override Style XamGridHeaderStyle
        {
            get { return Application.Current.Resources["GridHeaderCellStyle"] as Style; }
        }

        public override Style XamGridCellStyle
        {
            get { return Application.Current.Resources["XamGridBlackCellStyle"] as Style; }
        }

        public override Color TurnoverInstrumentBackControlButtonBackground1
        {
            get { return (Color.FromArgb(0xFF, 0X40, 0X40, 0X40)); }
        }

        public override Color TurnoverInstrumentBackControlButtonBackground2
        {
            get { return (Color.FromArgb(0xF9, 0X40, 0X40, 0X40)); }
        }

        public override Color TurnoverInstrumentBackControlButtonBackground3
        {
            get { return (Color.FromArgb(0xE5, 0X40, 0X40, 0X40)); }
        }
        public override Color TurnoverInstrumentBackControlButtonBackground4
        {
            get { return (Color.FromArgb(0xC6, 0X40, 0X40, 0X40)); }
        }
        public override Color TurnoverInstrumentBackControlButtonMouseOverColor1
        {
            get { return (Color.FromArgb(0xFF, 0X40, 0X40, 0X40)); }
        }

        public override Color TurnoverInstrumentBackControlButtonMouseOverColor2
        {
            get { return (Color.FromArgb(0xFF, 0X40, 0X40, 0X50)); }
        }

        public override Color TurnoverInstrumentBackControlButtonMouseOverColor3
        {
            get { return (Color.FromArgb(0xFF, 0X40, 0X40, 0X60)); }
        }

        public override Style TabItemStyle
        {
            get { return Application.Current.Resources["TabItemStyle"] as Style; }
        }
    }
}
