using System;
using Common.Logging.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubComp.Aspects.Monitoring.UnitTests.LogMocks;
using PubComp.Aspects.Monitoring.UnitTests.LogMocks.Mocks2;

namespace PubComp.Aspects.Monitoring.UnitTests
{
    [TestClass]
    public class LogAttributeTests
    {
        [TestMethod]
        public void TestLogEntryExit_AspectOnMethod()
        {
            var target = new LoggedMockA();
            TestLogEntryExit(target);
        }

        [TestMethod]
        public void TestLogEntryException_AspectOnMethod()
        {
            var target = new LoggedMockA();
            TestLogEntryException(target);
        }

        [TestMethod]
        public void TestLogEntryExit_AspectOnClass()
        {
            var target = new LoggedMockB();
            TestLogEntryExit(target);
        }

        [TestMethod]
        public void TestLogEntryException_AspectOnClass()
        {
            var target = new LoggedMockB();
            TestLogEntryException(target);
        }

        [TestMethod]
        public void TestLogEntryExit_AspectOnAssembly()
        {
            var target = new LoggedMockC();
            TestLogEntryExit(target);
        }

        [TestMethod]
        public void TestLogEntryException_AspectOnAssembly()
        {
            var target = new LoggedMockC();
            TestLogEntryException(target);
        }

        private void TestLogEntryExit(ILoggedMock target)
        {
            var logAdapter = new CapturingLoggerFactoryAdapter();
            Common.Logging.LogManager.Adapter = logAdapter;
            logAdapter.Clear();

            target.Short();

            Assert.AreEqual(2, logAdapter.LoggerEvents.Count);
            Assert.AreEqual(Common.Logging.LogLevel.Trace, logAdapter.LoggerEvents[0].Level);
            Assert.AreEqual(Common.Logging.LogLevel.Trace, logAdapter.LoggerEvents[1].Level);
        }

        private void TestLogEntryException(ILoggedMock target)
        {
            var logAdapter = new CapturingLoggerFactoryAdapter();
            Common.Logging.LogManager.Adapter = logAdapter;
            logAdapter.Clear();

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

            Assert.AreEqual(2, logAdapter.LoggerEvents.Count);
            Assert.AreEqual(Common.Logging.LogLevel.Trace, logAdapter.LoggerEvents[0].Level);
            Assert.AreEqual(Common.Logging.LogLevel.Error, logAdapter.LoggerEvents[1].Level);
        }
    }
}
