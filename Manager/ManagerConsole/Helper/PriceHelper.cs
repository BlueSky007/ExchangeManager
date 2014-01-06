using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagerConsole.Helper
{
    public class PriceHelper
    {
        public static void GetSendPrice(string adjustPriceText, int decimalPlace, string ask, string bid, out decimal sendAsk, out decimal sendBid)
        {
            sendAsk = sendBid = 0;
            decimal spread = decimal.Parse(ask) - decimal.Parse(bid);
            bid = new string('0', adjustPriceText.Length) + bid;
            int offset = adjustPriceText.IndexOf('.');
            if (decimalPlace > 0)
            {
                if (bid.IndexOf('.') < 0) bid += '.';
                bid += new string('0', decimalPlace);
                bid = PriceHelper.Cut(bid, decimalPlace);

                if (offset < 0)
                {
                    if (adjustPriceText.Length > decimalPlace)
                    {
                        adjustPriceText = adjustPriceText.Substring(0, decimalPlace);
                    }
                    bid = bid.Substring(0, bid.Length - adjustPriceText.Length) + adjustPriceText;
                }
                else
                {
                    adjustPriceText = PriceHelper.Cut(adjustPriceText, decimalPlace);

                    int startIndex = bid.IndexOf('.') - offset;
                    bid = bid.Substring(0, startIndex) + adjustPriceText + bid.Substring(startIndex + adjustPriceText.Length);
                }
            }
            else
            {
                if (offset == 0) throw new ArgumentException();
                if (offset > 0) adjustPriceText = adjustPriceText.Substring(0, offset);
                if (bid.IndexOf('.') >= 0)
                {
                    bid = bid.Substring(0, bid.IndexOf('.'));
                }
                bid = bid.Substring(0, bid.Length - adjustPriceText.Length) + adjustPriceText;
            }
            sendBid = decimal.Parse(bid);
            sendAsk = sendBid + spread;
        }

        public static string Cut(string value, int decimalPlace)
        {
            int position = value.IndexOf('.') + 1;
            if (position > 0)
            {
                if (value.Length - position > decimalPlace)
                {
                    value = value.Substring(0, position + decimalPlace);
                }
            }
            return value;
        }
    }
}
