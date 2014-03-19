using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Model
{
    public class InitDataForChart
    {
        public List<InstrumentClient> Instruments;
        public bool EnableTrendSheetChart;
        public bool HighBid;
        public bool LowBid;
        public DateTime CurrentDay;
        public DateTime BeginTime;
        public DateTime EndTime;
        public string Language;
        public string UserCode { get { return Principal.Instance.User.UserName; } }

        public string ConvertToXml()
        {
            StringBuilder str = new StringBuilder();
            str.AppendFormat("<InitDataForChart EnableTrendSheetChart=\"{0}\" HighBid=\"{1}\" LowBid=\"{2}\" CurrentDay=\"{3}\" BeginTime=\"{4}\" EndTime=\"{5}\" Language=\"{6}\" UserCode=\"{7}\">", this.EnableTrendSheetChart, this.HighBid, this.LowBid, this.CurrentDay, this.BeginTime, this.EndTime, this.Language, this.UserCode);
            str.Append("<Instruments>");
            foreach (InstrumentClient item in this.Instruments)
            {
                str.AppendFormat("<Instrument Id=\"{0}\" Description=\"{1}\" Decimals=\"{2}\" HasVolume=\"{3}\"/>", item.Id, string.IsNullOrEmpty(item.Description) ? item.Code : item.Description, item.GetDecimalsForChart(), false);
            }
            str.Append("</Instruments>");
            str.Append("</InitDataForChart>");
            return str.ToString();
        }
    }
}
