using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ManagerService.QuotationExchange;
using System.Collections.Generic;
using Manager.Common.QuotationEntities;
using ManagerService;
using System.Xml.Serialization;
using System.IO;
using iExchange.Common;
using System.Threading;

namespace Manager.UnitTest
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestMethod1()
        {
            ExchangeSystemSetting setting = new ExchangeSystemSetting();
            XmlSerializer serializer = new XmlSerializer(typeof(ManagerSettings));
            ManagerSettings settings = (ManagerSettings)serializer.Deserialize(new FileStream(@"D:\Teams\iExchangeCollection\iExchange3 Team\Manager\Manager.UnitTest\bin\Debug\Configuration\Manager.config", FileMode.Open));
            setting = settings.ExchangeSystems[0];
            QuotationServer quotation = new QuotationServer(setting);
            List<GeneralQuotation> quotations = new List<GeneralQuotation>();
            GeneralQuotation gene = new GeneralQuotation();
            gene.Ask = 10;
            gene.Bid = 10;
            gene.High = 20;
            gene.Low = 8;
            gene.Volume = "12";
            gene.TotalVolume = "13";
            gene.OriginCode = "XAUUSD";
            quotations.Add(gene);
            Token token = new Token(Guid.Empty, UserType.System, AppType.RiskMonitor);
            iExchange.Common.OriginQuotation[] originQs = null;                                                                                                                                                                                                                                                                                                                                                                                 
            iExchange.Common.OverridedQuotation[] overridedQs = null;
            quotation.SetQuotation(token, quotations, out originQs, out overridedQs);
            
        }

        [TestMethod]
        public void Flush()
        {
            ExchangeSystemSetting setting = new ExchangeSystemSetting();
            XmlSerializer serializer = new XmlSerializer(typeof(ManagerSettings));
            ManagerSettings settings = (ManagerSettings)serializer.Deserialize(new FileStream(@"D:\Teams\iExchangeCollection\iExchange3 Team\Manager\Manager.UnitTest\bin\Debug\Configuration\Manager.config", FileMode.Open));
            setting = settings.ExchangeSystems[0];
            QuotationServer quotation = new QuotationServer(setting);
            quotation.Flush();
        }

        [TestMethod]
        public void SetLimit()
        {
            ExchangeSystemSetting setting = new ExchangeSystemSetting();
            XmlSerializer serializer = new XmlSerializer(typeof(ManagerSettings));
            ManagerSettings settings = (ManagerSettings)serializer.Deserialize(new FileStream(@"D:\Teams\iExchangeCollection\iExchange3 Team\Manager\Manager.UnitTest\bin\Debug\Configuration\Manager.config", FileMode.Open));
            setting = settings.ExchangeSystems[0];
            QuotationServer quotation = new QuotationServer(setting);
        }
    }
}
