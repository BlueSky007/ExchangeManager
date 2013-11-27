using ManagerService.QuotationExchange;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ManagerService
{
    public class ExchangeSystemSetting
    {
        public string Code;
        public string DbConnectionString;
        public string QuotationConnectionString;
        public string StateServerUrl;
        public bool ConfirmRejectDQOrder;
        public bool AllowModifyOrderLot;
        public string TraceOriginCode;
        public bool AcceptZeroPricep;
        public string OriginPriceValidTime;
        public string CacheType;
        public string CatchSize ;
        public string QuotationFileCacheDirectory;
        public string CommitFrequency;
        public string MinIntervalToGetPricesOfHiLo;
        public string ChartCommandTimeout;
        public bool TraceOpenPrice;
    }

    public class ManagerSettings
    {
        public string ServiceAddressForConsole;
        public string ServiceAddressForExchange;
        public int QuotationListenPort;
        public ExchangeSystemSetting[] ExchangeSystems;

        public static ManagerSettings Load()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ManagerSettings));
            ManagerSettings settings = (ManagerSettings)serializer.Deserialize(new FileStream(@"Configuration\Manager.config", FileMode.Open));
            return settings;
        }

        public static void Save(string fileName, ManagerSettings settings)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ManagerSettings));
            serializer.Serialize(new FileStream(fileName, FileMode.Create), settings);
        }
    }
}
