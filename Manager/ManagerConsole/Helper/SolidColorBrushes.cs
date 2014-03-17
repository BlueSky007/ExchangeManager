using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ManagerConsole.Helper
{
    public class SolidColorBrushes
    {
        public static readonly SolidColorBrush White = new SolidColorBrush(Colors.White);
        public static readonly SolidColorBrush Black = new SolidColorBrush(Colors.Black);
        public static readonly SolidColorBrush Gray = new SolidColorBrush(Colors.Gray);
        public static readonly SolidColorBrush Green = new SolidColorBrush(Colors.Green);
        public static readonly SolidColorBrush Blue = new SolidColorBrush(Colors.Blue);
        public static readonly SolidColorBrush LightBlue = new SolidColorBrush(Color.FromArgb(0xFF, 0x90, 0x90, 0xFF));
        public static readonly SolidColorBrush Red = new SolidColorBrush(Colors.Red);
        public static readonly SolidColorBrush LightRed = new SolidColorBrush(Color.FromArgb(0xFF, 0xF5, 0xC1, 0xC0));
        public static readonly SolidColorBrush Transparent = new SolidColorBrush(Colors.Transparent);
        public static readonly SolidColorBrush LightGray = new SolidColorBrush(Colors.LightGray);
        public static readonly SolidColorBrush DarkGray = new SolidColorBrush(Colors.DarkGray);
        public static readonly SolidColorBrush BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x28, 0x28, 0x28));
        public static readonly SolidColorBrush Gold = new SolidColorBrush(Colors.Gold);

        public static SolidColorBrush Sell = new SolidColorBrush(Color.FromArgb(0xFF, 0xDE, 0x1B, 0x1A));
        public static SolidColorBrush Buy = new SolidColorBrush(Color.FromArgb(0xFF, 0x1A, 0x1A, 0xDE));

        public static Brush BlueBrush
        {
            get
            {
                LinearGradientBrush brush = new LinearGradientBrush();
                brush.StartPoint = new Point(0.6, 0);
                brush.EndPoint = new Point(0.6, 1);
                GradientStop stop = new GradientStop();
                stop.Color = Colors.LightBlue;
                stop.Offset = 0;
                brush.GradientStops.Add(stop);

                stop = new GradientStop();
                stop.Color = Color.FromArgb(0xFF, 0x1A, 0x1A, 0xDE);
                stop.Offset = 0.48;
                brush.GradientStops.Add(stop);

                stop = new GradientStop();
                stop.Color = Color.FromArgb(0xFF, 0x1A, 0x1A, 0xDE);
                stop.Offset = 0.58;
                brush.GradientStops.Add(stop);

                stop = new GradientStop();
                stop.Color = Colors.LightBlue;
                stop.Offset = 1;
                brush.GradientStops.Add(stop);

                return brush;
            }
        }
    }
}
