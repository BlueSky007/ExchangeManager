using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public static readonly SolidColorBrush LightBlue = new SolidColorBrush(Color.FromArgb(0xFF, 0xBB, 0xBB, 0xF4));
        public static readonly SolidColorBrush Red = new SolidColorBrush(Colors.Red);
        public static readonly SolidColorBrush LightRed = new SolidColorBrush(Color.FromArgb(0xFF, 0xF5, 0xC1, 0xC0));
        public static readonly SolidColorBrush Transparent = new SolidColorBrush(Colors.Transparent);
        public static readonly SolidColorBrush LightGray = new SolidColorBrush(Colors.LightGray);
        public static readonly SolidColorBrush DarkGray = new SolidColorBrush(Colors.DarkGray);
    }
}
