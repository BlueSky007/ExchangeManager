using iExchange4.Chart.SilverlightExtension;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ManagerConsole.Chart
{
    internal class SettingsProvider : iExchange4.Chart.SilverlightExtension.ISettingsProvider
    {
        public SettingsProvider(string xmlNodeStr)
        {
            this._instruments = new List<Instrument>();
            XDocument doc = XDocument.Parse(xmlNodeStr);
            foreach (XElement element in doc.Element("InitDataForChart").Element("Instruments").Elements())
            {
                Instrument instrument = new Instrument(Guid.Parse(element.Attribute("Id").Value), element.Attribute("Description").Value, short.Parse(element.Attribute("Decimals").Value), bool.Parse(element.Attribute("HasVolume").Value));
                this._instruments.Add(instrument);
            }
            this._EnableTrendSheetChart = bool.Parse(doc.Element("InitDataForChart").Attribute("EnableTrendSheetChart").Value);
            this._HighBid = bool.Parse(doc.Element("InitDataForChart").Attribute("HighBid").Value);
            this._LowBid = bool.Parse(doc.Element("InitDataForChart").Attribute("LowBid").Value);
            this._Language = doc.Element("InitDataForChart").Attribute("Language").Value;
            DateTime currentDay = DateTime.Parse(doc.Element("InitDataForChart").Attribute("CurrentDay").Value);
            DateTime BeginDay = DateTime.Parse(doc.Element("InitDataForChart").Attribute("BeginTime").Value);
            DateTime EndDay = DateTime.Parse(doc.Element("InitDataForChart").Attribute("EndTime").Value);
            this._TradeDay = new TradeDay(currentDay, BeginDay, EndDay);
            this._UserSettingsDirectory = System.IO.Path.Combine("Users",doc.Element("InitDataForChart").Attribute("UserCode").Value);
            this._Language = doc.Element("InitDataForChart").Attribute("Language").Value;
        }

        public SettingsProvider(ICollection<Instrument> instruments, bool enableTrendSheetChart, bool highBid, bool lowBid)
        {
            this._instruments = instruments;
            this._ColumnNumber = 30;
            this._EnableTrendSheetChart = enableTrendSheetChart;
            this._HighBid = highBid;
            this._LowBid = lowBid;
            this._UserSettingsDirectory = "D:/TXT";
        }

        private double _ColumnNumber;
        private bool _EnableTrendSheetChart;
        private bool _HighBid;
        private string _Language;
        private bool _LowBid;

        private TradeDay _TradeDay;
        private string _UserSettingsDirectory;


        private ICollection<Instrument> _instruments;
        public double ColumnNumber { get { return 30; } }
        public bool EnableTrendSheetChart { get { return this._EnableTrendSheetChart; } }
        public bool HighBid { get { return this._HighBid; } }
        public string Language { get { return this._Language; } }
        public bool LowBid { get { return this._LowBid; } }
        public DateTime Now { get { return DateTime.Now; } }
        public TradeDay TradeDay { get { return new TradeDay(DateTime.Now, DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1)); } }
        public string UserSettingsDirectory { get { return this._UserSettingsDirectory; } }

        public ICollection<Frequency> GetFrequecies()
        {
            return iExchange4.Chart.SilverlightExtension.Frequency.GetDefaultFrequencies();
        }
        public ICollection<Instrument> GetInstruments()
        {
            return this._instruments;
        }
        public string GetLanguage(string key)
        {
            Omnicare.StringLibrary _Strings = new Omnicare.StringLibrary();
            return _Strings[key].ToString();
        }
    }
}
