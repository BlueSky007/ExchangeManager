using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ManagerService.DataAccess;
using Manager.Common.QuotationEntities;
using ManagerConsole.UI;

namespace Manager.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            double bid = 2323.12345;
            string sbid = bid.ToString("F3");
            Assert.IsTrue(sbid == "2323.123");

            object o1 = new string('a', 10);
            object o2 = new string('a', 10);
            Assert.IsTrue(o1.Equals(o2));

            int i=1, j=1;

            object o3 = i;
            object o4 = j;
            Assert.IsTrue(o3.Equals(o4));
        }

        [TestMethod]
        public void TestMethod2()
        {
            PriceRangeCheckRule entity = new PriceRangeCheckRule()
            {
                Id = 1,
                DiscardOutOfRangePrice = true,
                OutOfRangeCount = 0,
                OutOfRangeType = OutOfRangeType.Bid,
                OutOfRangeWaitTime = 0,
                ValidVariation = 0
            };
            QuotationData.AddMetadataObject(entity);
        }

        [TestMethod]
        public void TestPre3()
        {
            string value = "121.12345";
            int decimalPlace = 4;
            string output = PriceHelper.Cut(value, decimalPlace);
            Assert.IsTrue(output == "121.1234");

            value = "121.12";
            output = PriceHelper.Cut(value, decimalPlace);
            Assert.IsTrue(output == "121.12");

            value = "121";
            output = PriceHelper.Cut(value, decimalPlace);
            Assert.IsTrue(output == "121");

            value = ".121";
            output = PriceHelper.Cut(value, decimalPlace);
            Assert.IsTrue(output == ".121");
        }        

        [TestMethod]
        public void Test3()
        {
            string adjustPriceText = "1.12";
            int decimalPlace = 3;
            string bid = "102.726";
            string ask = "102.826";
            decimal sendAsk;
            decimal sendBid;
            PriceHelper.GetSendPrice(adjustPriceText, decimalPlace, ask, bid, out sendAsk, out sendBid);
            Assert.IsTrue(sendBid == decimal.Parse("101.126"));
            Assert.IsTrue(sendAsk == decimal.Parse("101.226"));

            adjustPriceText = "9555";
            PriceHelper.GetSendPrice(adjustPriceText, decimalPlace, ask, bid, out sendAsk, out sendBid);
            Assert.IsTrue(sendBid == decimal.Parse("102.955"));
            Assert.IsTrue(sendAsk == decimal.Parse("103.055"));

            adjustPriceText = "95";
            PriceHelper.GetSendPrice(adjustPriceText, decimalPlace, ask, bid, out sendAsk, out sendBid);
            Assert.IsTrue(sendBid == decimal.Parse("102.795"));
            Assert.IsTrue(sendAsk == decimal.Parse("102.895"));

            adjustPriceText = "0.22";
            PriceHelper.GetSendPrice(adjustPriceText, decimalPlace, ask, bid, out sendAsk, out sendBid);
            Assert.IsTrue(sendBid == decimal.Parse("100.226"));
            Assert.IsTrue(sendAsk == decimal.Parse("100.326"));

            adjustPriceText = "131.8438";
            PriceHelper.GetSendPrice(adjustPriceText, decimalPlace, ask, bid, out sendAsk, out sendBid);
            Assert.IsTrue(sendBid == decimal.Parse("131.843"));
            Assert.IsTrue(sendAsk == decimal.Parse("131.943"));

            decimalPlace = 0;
            bid = "102726";
            ask = "102826";
            adjustPriceText = "131.8438";
            PriceHelper.GetSendPrice(adjustPriceText, decimalPlace, ask, bid, out sendAsk, out sendBid);
            Assert.IsTrue(sendBid == decimal.Parse("102131"));
            Assert.IsTrue(sendAsk == decimal.Parse("102231"));
        }
    }
}
