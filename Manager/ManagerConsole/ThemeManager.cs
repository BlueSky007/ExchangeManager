using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ManagerConsole
{
    public class ThemeManager
    {
        private ResourceDictionary _BlackThemeResourceDictionary;
        private ResourceDictionary _WhiteThemeResourceDictionary = new ResourceDictionary() { Source = new Uri("ManagerConsole;component/Asset/Theme_White.xaml", UriKind.Relative) };

        private bool _IsWhite = false;
        private ResourceDictionary _AppResourceDictionary;

        public ThemeManager(bool isWhite)
        {
            this._AppResourceDictionary = Application.Current.Resources;
            this._BlackThemeResourceDictionary = this._AppResourceDictionary.MergedDictionaries.Single(d => d.Source.OriginalString.EndsWith("Black.xaml"));
            if (isWhite) this.SwitchTheme();
        }

        public bool IsWhite { get { return this._IsWhite; } }

        public void SwitchTheme()
        {
            this._AppResourceDictionary.MergedDictionaries.Remove(this._IsWhite ? this._WhiteThemeResourceDictionary : this._BlackThemeResourceDictionary);
            this._IsWhite = !this._IsWhite;
            this._AppResourceDictionary.MergedDictionaries.Add(this._IsWhite ? this._WhiteThemeResourceDictionary : this._BlackThemeResourceDictionary);
        }
    }
}
