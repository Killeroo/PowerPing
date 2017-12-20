using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PowerPing;

namespace PowerPing.Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestAttributes()
        {
            PingAttributes testAttrs = new PingAttributes();

            testAttrs.Address = "127.0.0.1";
            testAttrs.Count = 1;
            testAttrs.Interval = 100;
            testAttrs.Message = "testest";

            Ping p = new Ping();

            PingResults results = p.SendICMP(testAttrs);

            Assert.AreEqual(testAttrs, p.Attributes);
        }

        [TestMethod]
        public void TestLocalhostPing()
        {
            PingAttributes testAttrs = new PingAttributes();

            testAttrs.Address = "127.0.0.1";
            testAttrs.Count = 3;
            testAttrs.Interval = 100;

            Ping p = new Ping();

            PingResults results = p.SendICMP(testAttrs);

            Assert.AreEqual(results.Received, 3);
        }
        
    }
}
