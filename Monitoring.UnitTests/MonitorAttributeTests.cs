using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubComp.Aspects.Monitoring.UnitTests.MonitorMocks;

namespace PubComp.Aspects.Monitoring.UnitTests
{
    [TestClass]
    public class MonitorAttributeTests
    {
        [TestMethod]
        public void TestLogEntryExit_AspectOnMethod()
        {
            const string expectedName = "PubComp.Aspects.Monitoring.UnitTests.MonitorMocks.MonitoredMock.Long()";

            var target = new MonitoredMock();
            
            target.Long();

            var names = MonitorAttribute.GetMonitorNames();
            Assert.IsTrue(names.Contains(expectedName));

            var stats = MonitorAttribute.GetStatistics(expectedName);

            Assert.AreEqual(1, stats.Entries);
            Assert.AreEqual(1, stats.Exits);
            Assert.AreEqual(0, stats.Failures);
            Assert.IsTrue(stats.AverageDuration >= 90.0);
        }

        [TestMethod]
        public void TestLogEntryException_AspectOnMethod()
        {
            const string expectedName = "PubComp.Aspects.Monitoring.UnitTests.MonitorMocks.MonitoredMock.ThrowSomething()";

            var target = new MonitoredMock();
            
            bool caughtException = false;

            try
            {
                target.ThrowSomething();
            }
            catch (ApplicationException)
            {
                caughtException = true;
            }

            Assert.IsTrue(caughtException);

            var stats = MonitorAttribute.GetStatistics(expectedName);

            Assert.AreEqual(1, stats.Entries);
            Assert.AreEqual(1, stats.Exits);
            Assert.AreEqual(1, stats.Failures);
        }
    }
}
