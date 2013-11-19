using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ManagerService.DataAccess;
using Manager.Common.QuotationEntities;

namespace Manager.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            QuotationSource source = new QuotationSource{
                Id=0, Name="Source1", AuthName="aaa", Password="12345678"
            };
            //QuotationData.UpdateMetadata(source);
            Assert.IsTrue(source.Id > 0);
        }
    }
}
