using ManagerService.QuotationExchange;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ManagerService;
using iExchange.Common;
using Manager.Common.QuotationEntities;
using System.Collections.Generic;
using System.Xml;
using System.Data;
using System.Xml.Serialization;
using System.IO;

namespace Manager.UnitTest
{
    
    
    /// <summary>
    ///这是 QuotationServerTest 的测试类，旨在
    ///包含所有 QuotationServerTest 单元测试
    ///</summary>
    [TestClass()]
    public class QuotationServerTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        // 
        //编写测试时，还可使用以下特性:
        //
        //使用 ClassInitialize 在运行类中的第一个测试前先运行代码
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //使用 ClassCleanup 在运行完类中的所有测试后再运行代码
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //使用 TestInitialize 在运行每个测试前先运行代码
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
            
        //}
        //
        //使用 TestCleanup 在运行完每个测试后运行代码
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///SetQuotation 的测试
        ///</summary>
        [TestMethod()]
        public void SetQuotationTest()
        {
            ExchangeSystemSetting setting = new ExchangeSystemSetting();
            XmlSerializer serializer = new XmlSerializer(typeof(ManagerSettings));
            ManagerSettings settings = (ManagerSettings)serializer.Deserialize(new FileStream(@"D:\Teams\iExchangeCollection\iExchange3 Team\Manager\Manager.UnitTest\bin\Debug\Configuration\Manager.config", FileMode.Open));
            setting = settings.ExchangeSystems[0]; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            Token token = new Token(Guid.Empty, UserType.System, AppType.QuotationCollector); // TODO: 初始化为适当的值
            List<GeneralQuotation> quotations = new List<GeneralQuotation>(); // TODO: 初始化为适当的值
            GeneralQuotation gene = new GeneralQuotation();
            gene.Ask = 10;
            gene.Bid = 10;
            gene.High = 20;
            gene.Low = 8;
            gene.Volume = "12";
            gene.TotalVolume = "13";
            gene.OriginCode = "XAUUSD";
            quotations.Add(gene);
            iExchange.Common.OriginQuotation[] originQs = null; // TODO: 初始化为适当的值
            //iExchange.Common.OriginQuotation[] originQsExpected = null; // TODO: 初始化为适当的值
            iExchange.Common.OverridedQuotation[] overridedQs = null; // TODO: 初始化为适当的值
            //iExchange.Common.OverridedQuotation[] overridedQsExpected = null; // TODO: 初始化为适当的值
            bool expected = true; // TODO: 初始化为适当的值
            bool actual;
            actual = target.SetQuotation(token, quotations, out originQs, out overridedQs);
            //Assert.AreEqual(originQsExpected, originQs);
            //Assert.AreEqual(overridedQsExpected, overridedQs);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Reset 的测试
        ///</summary>
        [TestMethod()]
        public void ResetTest()
        {
            ExchangeSystemSetting setting = null; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            target.Reset();
        }

        /// <summary>
        ///Flush 的测试
        ///</summary>
        [TestMethod()]
        public void FlushTest()
        {
            ExchangeSystemSetting setting = new ExchangeSystemSetting();
            XmlSerializer serializer = new XmlSerializer(typeof(ManagerSettings));
            ManagerSettings settings = (ManagerSettings)serializer.Deserialize(new FileStream(@"D:\Teams\iExchangeCollection\iExchange3 Team\Manager\Manager.UnitTest\bin\Debug\Configuration\Manager.config", FileMode.Open));
            setting = settings.ExchangeSystems[0]; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            bool expected = true; // TODO: 初始化为适当的值
            bool actual;
            actual = target.Flush();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///GetQuotation 的测试
        ///</summary>
        [TestMethod()]
        public void GetQuotationTest()
        {
            ExchangeSystemSetting setting = new ExchangeSystemSetting();
            XmlSerializer serializer = new XmlSerializer(typeof(ManagerSettings));
            ManagerSettings settings = (ManagerSettings)serializer.Deserialize(new FileStream(@"D:\Teams\iExchangeCollection\iExchange3 Team\Manager\Manager.UnitTest\bin\Debug\Configuration\Manager.config", FileMode.Open));
            setting = settings.ExchangeSystems[0]; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            Token token = new Token(Guid.Empty, UserType.System, AppType.RiskMonitor); // TODO: 初始化为适当的值
            string expected = string.Empty; // TODO: 初始化为适当的值
            string actual;
            actual = target.GetQuotation(token);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///ReplayQuotation 的测试
        ///</summary>
        [TestMethod()]
        public void ReplayQuotationTest()
        {
            ExchangeSystemSetting setting = new ExchangeSystemSetting();
            XmlSerializer serializer = new XmlSerializer(typeof(ManagerSettings));
            ManagerSettings settings = (ManagerSettings)serializer.Deserialize(new FileStream(@"D:\Teams\iExchangeCollection\iExchange3 Team\Manager\Manager.UnitTest\bin\Debug\Configuration\Manager.config", FileMode.Open));
            setting = settings.ExchangeSystems[0]; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            Token token = new Token(Guid.Empty, UserType.System, AppType.RiskMonitor); // TODO: 初始化为适当的值
            List<GeneralQuotation> quotations = null; // TODO: 初始化为适当的值
            iExchange.Common.OriginQuotation[] originQs = null; // TODO: 初始化为适当的值
           // iExchange.Common.OriginQuotation[] originQsExpected = null; // TODO: 初始化为适当的值
            iExchange.Common.OverridedQuotation[] overridedQs = null; // TODO: 初始化为适当的值
           // iExchange.Common.OverridedQuotation[] overridedQsExpected = null; // TODO: 初始化为适当的值
            bool expected = false; // TODO: 初始化为适当的值
            bool actual;
            actual = target.ReplayQuotation(token, quotations, out originQs, out overridedQs);
            //Assert.AreEqual(originQsExpected, originQs);
            //Assert.AreEqual(overridedQsExpected, overridedQs);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///RestoreHighLow 的测试
        ///</summary>
        [TestMethod()]
        public void RestoreHighLowTest()
        {
            ExchangeSystemSetting setting = null; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            Token token = new Token(Guid.Empty, UserType.System, AppType.RiskMonitor); // TODO: 初始化为适当的值
            string ip = string.Empty; // TODO: 初始化为适当的值
            int batchProcessId = 0; // TODO: 初始化为适当的值
            Guid instrumentId = new Guid(); // TODO: 初始化为适当的值
            Guid instrumentIdExpected = new Guid(); // TODO: 初始化为适当的值
            string instrumentCode = string.Empty; // TODO: 初始化为适当的值
            string instrumentCodeExpected = string.Empty; // TODO: 初始化为适当的值
            string newInput = string.Empty; // TODO: 初始化为适当的值
            string newInputExpected = string.Empty; // TODO: 初始化为适当的值
            bool isUpdateHigh = false; // TODO: 初始化为适当的值
            bool isUpdateHighExpected = false; // TODO: 初始化为适当的值
            bool highBid = false; // TODO: 初始化为适当的值
            bool highBidExpected = false; // TODO: 初始化为适当的值
            bool lowBid = false; // TODO: 初始化为适当的值
            bool lowBidExpected = false; // TODO: 初始化为适当的值
            DateTime minTimestamp = new DateTime(); // TODO: 初始化为适当的值
            DateTime minTimestampExpected = new DateTime(); // TODO: 初始化为适当的值
            iExchange.Common.OverridedQuotation[] overridedQs = null; // TODO: 初始化为适当的值
            iExchange.Common.OverridedQuotation[] overridedQsExpected = null; // TODO: 初始化为适当的值
            int returnValue = 0; // TODO: 初始化为适当的值
            int returnValueExpected = 0; // TODO: 初始化为适当的值
            string errorMessage = string.Empty; // TODO: 初始化为适当的值
            string errorMessageExpected = string.Empty; // TODO: 初始化为适当的值
            target.RestoreHighLow(token, ip, batchProcessId, out instrumentId, out instrumentCode, out newInput, out isUpdateHigh, out highBid, out lowBid, out minTimestamp, out overridedQs, out returnValue, out errorMessage);
            Assert.AreEqual(instrumentIdExpected, instrumentId);
            Assert.AreEqual(instrumentCodeExpected, instrumentCode);
            Assert.AreEqual(newInputExpected, newInput);
            Assert.AreEqual(isUpdateHighExpected, isUpdateHigh);
            Assert.AreEqual(highBidExpected, highBid);
            Assert.AreEqual(lowBidExpected, lowBid);
            Assert.AreEqual(minTimestampExpected, minTimestamp);
            Assert.AreEqual(overridedQsExpected, overridedQs);
            Assert.AreEqual(returnValueExpected, returnValue);
            Assert.AreEqual(errorMessageExpected, errorMessage);
        }

        /// <summary>
        ///SetHistoryQuotation 的测试
        ///</summary>
        [TestMethod()]
        public void SetHistoryQuotationTest()
        {
            ExchangeSystemSetting setting = new ExchangeSystemSetting();
            XmlSerializer serializer = new XmlSerializer(typeof(ManagerSettings));
            ManagerSettings settings = (ManagerSettings)serializer.Deserialize(new FileStream(@"D:\Teams\iExchangeCollection\iExchange3 Team\Manager\Manager.UnitTest\bin\Debug\Configuration\Manager.config", FileMode.Open));
            setting = settings.ExchangeSystems[0]; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            Token token = new Token(Guid.Empty, UserType.System, AppType.RiskMonitor); // TODO: 初始化为适当的值
            DateTime tradeDay = new DateTime(); // TODO: 初始化为适当的值
            string quotation = "<Local><Quotation InstrumentID='1DFB99D4-2B76-48B0-9109-0A67265F5B9F' Timestamp='2013-02-01 04:00:00.000' Origin='1.3559' Status='Modified'/></Local>"; // TODO: 初始化为适当的值
            bool needApplyAutoAdjustPoints = false; // TODO: 初始化为适当的值
            iExchange.Common.OriginQuotation[] originQs = null; // TODO: 初始化为适当的值
           // iExchange.Common.OriginQuotation[] originQsExpected = null; // TODO: 初始化为适当的值
            iExchange.Common.OverridedQuotation[] overridedQs = null; // TODO: 初始化为适当的值
           // iExchange.Common.OverridedQuotation[] overridedQsExpected = null; // TODO: 初始化为适当的值
            bool needBroadcastQuotation = false; // TODO: 初始化为适当的值
           // bool needBroadcastQuotationExpected = true; // TODO: 初始化为适当的值
            bool expected = true; // TODO: 初始化为适当的值
            bool actual;
            actual = target.SetHistoryQuotation(token, tradeDay, quotation, needApplyAutoAdjustPoints, out originQs, out overridedQs, out needBroadcastQuotation);
            //Assert.AreEqual(originQsExpected, originQs);
            //Assert.AreEqual(overridedQsExpected, overridedQs);
            //Assert.AreEqual(needBroadcastQuotationExpected, needBroadcastQuotation);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Update 的测试
        ///</summary>
        [TestMethod()]
        public void UpdateTest()
        {
            ExchangeSystemSetting setting = null; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            Token token = new Token(Guid.Empty, UserType.System, AppType.RiskMonitor); // TODO: 初始化为适当的值
            XmlNode update = null; // TODO: 初始化为适当的值
            bool expected = false; // TODO: 初始化为适当的值
            bool actual;
            actual = target.Update(token, update);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///UpdateHighLow 的测试
        ///</summary>
        [TestMethod()]
        public void UpdateHighLowTest()
        {
            ExchangeSystemSetting setting = null; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            Token token = new Token(Guid.Empty, UserType.System, AppType.RiskMonitor); // TODO: 初始化为适当的值
            string ip = string.Empty; // TODO: 初始化为适当的值
            Guid instrumentId = new Guid(); // TODO: 初始化为适当的值
            bool isOriginHiLo = false; // TODO: 初始化为适当的值
            string newInput = string.Empty; // TODO: 初始化为适当的值
            bool isUpdateHigh = false; // TODO: 初始化为适当的值
            int batchProcessId = 0; // TODO: 初始化为适当的值
            int batchProcessIdExpected = 0; // TODO: 初始化为适当的值
            string instrumentCode = string.Empty; // TODO: 初始化为适当的值
            string instrumentCodeExpected = string.Empty; // TODO: 初始化为适当的值
            bool highBid = false; // TODO: 初始化为适当的值
            bool highBidExpected = false; // TODO: 初始化为适当的值
            bool lowBid = false; // TODO: 初始化为适当的值
            bool lowBidExpected = false; // TODO: 初始化为适当的值
            DateTime updateTime = new DateTime(); // TODO: 初始化为适当的值
            DateTime updateTimeExpected = new DateTime(); // TODO: 初始化为适当的值
            DateTime minTimestamp = new DateTime(); // TODO: 初始化为适当的值
            DateTime minTimestampExpected = new DateTime(); // TODO: 初始化为适当的值
            iExchange.Common.OverridedQuotation[] overridedQs = null; // TODO: 初始化为适当的值
            iExchange.Common.OverridedQuotation[] overridedQsExpected = null; // TODO: 初始化为适当的值
            int returnValue = 0; // TODO: 初始化为适当的值
            int returnValueExpected = 0; // TODO: 初始化为适当的值
            string errorMessage = string.Empty; // TODO: 初始化为适当的值
            string errorMessageExpected = string.Empty; // TODO: 初始化为适当的值
            target.UpdateHighLow(token, ip, instrumentId, isOriginHiLo, newInput, isUpdateHigh, out batchProcessId, out instrumentCode, out highBid, out lowBid, out updateTime, out minTimestamp, out overridedQs, out returnValue, out errorMessage);
            Assert.AreEqual(batchProcessIdExpected, batchProcessId);
            Assert.AreEqual(instrumentCodeExpected, instrumentCode);
            Assert.AreEqual(highBidExpected, highBid);
            Assert.AreEqual(lowBidExpected, lowBid);
            Assert.AreEqual(updateTimeExpected, updateTime);
            Assert.AreEqual(minTimestampExpected, minTimestamp);
            Assert.AreEqual(overridedQsExpected, overridedQs);
            Assert.AreEqual(returnValueExpected, returnValue);
            Assert.AreEqual(errorMessageExpected, errorMessage);
        }

        /// <summary>
        ///UpdateInstrumentDealer 的测试
        ///</summary>
        [TestMethod()]
        public void UpdateInstrumentDealerTest()
        {
            ExchangeSystemSetting setting = null; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            Token token = new Token(Guid.Empty, UserType.System, AppType.RiskMonitor); // TODO: 初始化为适当的值
            DataSet instrumentSettingChanges = null; // TODO: 初始化为适当的值
            target.UpdateInstrumentDealer(token, instrumentSettingChanges);
        }

        /// <summary>
        ///UpdateOverridedQuotationHighLow 的测试
        ///</summary>
        [TestMethod()]
        public void UpdateOverridedQuotationHighLowTest()
        {
            ExchangeSystemSetting setting = null; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            Token token = new Token(Guid.Empty, UserType.System, AppType.RiskMonitor); // TODO: 初始化为适当的值
            Guid instrumentID = new Guid(); // TODO: 初始化为适当的值
            string quotation = string.Empty; // TODO: 初始化为适当的值
            iExchange.Common.OverridedQuotation[] overridedQs = null; // TODO: 初始化为适当的值
            iExchange.Common.OverridedQuotation[] overridedQsExpected = null; // TODO: 初始化为适当的值
            bool expected = false; // TODO: 初始化为适当的值
            bool actual;
            actual = target.UpdateOverridedQuotationHighLow(token, instrumentID, quotation, out overridedQs);
            Assert.AreEqual(overridedQsExpected, overridedQs);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///UpdateQuotePolicies 的测试
        ///</summary>
        [TestMethod()]
        public void UpdateQuotePoliciesTest()
        {
            ExchangeSystemSetting setting = null; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            Token token = new Token(Guid.Empty, UserType.System, AppType.RiskMonitor); // TODO: 初始化为适当的值
            XmlNode quotePolicies = null; // TODO: 初始化为适当的值
            int error = 0; // TODO: 初始化为适当的值
            int errorExpected = 0; // TODO: 初始化为适当的值
            bool expected = false; // TODO: 初始化为适当的值
            bool actual;
            actual = target.UpdateQuotePolicies(token, quotePolicies, out error);
            Assert.AreEqual(errorExpected, error);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///UpdateQuotePolicy 的测试
        ///</summary>
        [TestMethod()]
        public void UpdateQuotePolicyTest()
        {
            ExchangeSystemSetting setting = null; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            Token token = new Token(Guid.Empty, UserType.System, AppType.RiskMonitor); // TODO: 初始化为适当的值
            XmlNode quotePolicy = null; // TODO: 初始化为适当的值
            int error = 0; // TODO: 初始化为适当的值
            int errorExpected = 0; // TODO: 初始化为适当的值
            bool expected = false; // TODO: 初始化为适当的值
            bool actual;
            actual = target.UpdateQuotePolicy(token, quotePolicy, out error);
            Assert.AreEqual(errorExpected, error);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///FixOverridedQuotationHistory 的测试
        ///</summary>
        [TestMethod()]
        public void FixOverridedQuotationHistoryTest()
        {
            ExchangeSystemSetting setting = new ExchangeSystemSetting();
            XmlSerializer serializer = new XmlSerializer(typeof(ManagerSettings));
            ManagerSettings settings = (ManagerSettings)serializer.Deserialize(new FileStream(@"D:\Teams\iExchangeCollection\iExchange3 Team\Manager\Manager.UnitTest\bin\Debug\Configuration\Manager.config", FileMode.Open));
            setting = settings.ExchangeSystems[0];
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            Token token = new Token(Guid.Empty, UserType.System, AppType.RiskMonitor); // TODO: 初始化为适当的值
            string quotation = "<Local><Quotation InstrumentID='1DFB99D4-2B76-48B0-9109-0A67265F5B9F' Timestamp='2013-02-01 04:00:00.000' Origin='1.3559' Status='Modified'/></Local>"; // TODO: 初始化为适当的值
            bool needApplyAutoAdjustPoints = false; // TODO: 初始化为适当的值
            iExchange.Common.OriginQuotation[] originQs = null; // TODO: 初始化为适当的值
         //   iExchange.Common.OriginQuotation[] originQsExpected = null; // TODO: 初始化为适当的值
            iExchange.Common.OverridedQuotation[] overridedQs = null; // TODO: 初始化为适当的值
           // iExchange.Common.OverridedQuotation[] overridedQsExpected = null; // TODO: 初始化为适当的值
            bool needBroadcastQuotation = false; // TODO: 初始化为适当的值
           // bool needBroadcastQuotationExpected = true; // TODO: 初始化为适当的值
            XmlNode fixChartDatas = null; // TODO: 初始化为适当的值
           // XmlNode fixChartDatasExpected = null; // TODO: 初始化为适当的值
            bool expected = true; // TODO: 初始化为适当的值
            bool actual;
            actual = target.FixOverridedQuotationHistory(token, quotation, needApplyAutoAdjustPoints, out originQs, out overridedQs, out needBroadcastQuotation, out fixChartDatas);
            //Assert.AreEqual(originQsExpected, originQs);
            //Assert.AreEqual(overridedQsExpected, overridedQs);
            //Assert.AreEqual(needBroadcastQuotationExpected, needBroadcastQuotation);
            //Assert.AreEqual(fixChartDatasExpected, fixChartDatas);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///DiscardQuotation 的测试
        ///</summary>
        [TestMethod()]
        public void DiscardQuotationTest()
        {
            ExchangeSystemSetting setting = null; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting); // TODO: 初始化为适当的值
            Token token = new Token(Guid.Empty, UserType.System, AppType.RiskMonitor); // TODO: 初始化为适当的值
            Guid instrumentID = new Guid(); // TODO: 初始化为适当的值
            iExchange.Common.OriginQuotation[] originQs = null; // TODO: 初始化为适当的值
            iExchange.Common.OriginQuotation[] originQsExpected = null; // TODO: 初始化为适当的值
            iExchange.Common.OverridedQuotation[] overridedQs = null; // TODO: 初始化为适当的值
            iExchange.Common.OverridedQuotation[] overridedQsExpected = null; // TODO: 初始化为适当的值
            bool expected = false; // TODO: 初始化为适当的值
            bool actual;
            actual = target.DiscardQuotation(token, instrumentID, out originQs, out overridedQs);
            Assert.AreEqual(originQsExpected, originQs);
            Assert.AreEqual(overridedQsExpected, overridedQs);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///QuotationServer 构造函数 的测试
        ///</summary>
        [TestMethod()]
        public void QuotationServerConstructorTest()
        {
            ExchangeSystemSetting setting = null; // TODO: 初始化为适当的值
            QuotationServer target = new QuotationServer(setting);
        }
    }
}
