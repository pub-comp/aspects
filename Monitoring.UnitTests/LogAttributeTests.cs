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
        private static CapturingLoggerFactoryAdapter logAdapter;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            logAdapter = new CapturingLoggerFactoryAdapter();
            Common.Logging.LogManager.Adapter = logAdapter;
        }

        [TestInitialize]
        public void TestInitialize()
        {
            logAdapter.Clear();
        }

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

        [TestMethod]
        public void TestLogAutoName()
        {
            const string expectedLogName = "PubComp.Aspects.Monitoring.UnitTests.LogMocks.LoggedMockA";

            var target = new LoggedMockA();

            try
            {
                target.ThrowSomething();
            }
            catch (ApplicationException)
            {
            }

            Assert.AreEqual(2, logAdapter.LoggerEvents.Count);
            Assert.AreEqual(expectedLogName, logAdapter.LoggerEvents[0].Source.Name);
            Assert.AreEqual(expectedLogName, logAdapter.LoggerEvents[1].Source.Name);
        }

        [TestMethod]
        public void TestLogExplicitName()
        {
            const string expectedLogName = "MyLog";

            var target = new LoggedMockA();

            target.Other();

            Assert.AreEqual(2, logAdapter.LoggerEvents.Count);
            Assert.AreEqual(expectedLogName, logAdapter.LoggerEvents[0].Source.Name);
            Assert.AreEqual(expectedLogName, logAdapter.LoggerEvents[1].Source.Name);
        }

        private void TestLogEntryExit(ILoggedMock target)
        {
            target.Short();

            Assert.AreEqual(2, logAdapter.LoggerEvents.Count);
            Assert.AreEqual(Common.Logging.LogLevel.Trace, logAdapter.LoggerEvents[0].Level);
            Assert.AreEqual(Common.Logging.LogLevel.Trace, logAdapter.LoggerEvents[1].Level);
        }

        private void TestLogEntryException(ILoggedMock target)
        {
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
