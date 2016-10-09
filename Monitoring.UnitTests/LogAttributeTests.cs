using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Targets;
using PubComp.Aspects.Monitoring.UnitTests.LogMocks;
using PubComp.Aspects.Monitoring.UnitTests.LogMocks.Mocks2;

namespace PubComp.Aspects.Monitoring.UnitTests
{
    [TestClass]
    public class LogAttributeTests
    {
        private static MemoryTarget logList;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            logList = ((MemoryTarget)LogManager.Configuration.FindTargetByName("testTarget"));
        }

        [TestInitialize]
        public void TestInitialize()
        {
            logList.Logs.Clear();
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
            
            Assert.AreEqual(2, logList.Logs.Count);
            Assert.AreEqual(expectedLogName, logList.Logs[0].GetLogName());
            Assert.AreEqual(expectedLogName, logList.Logs[1].GetLogName());
        }

        [TestMethod]
        public void TestLogExplicitName()
        {
            const string expectedLogName = "MyLog";

            var target = new LoggedMockA();

            target.Other();

            Assert.AreEqual(2, logList.Logs.Count);
            Assert.AreEqual(expectedLogName, logList.Logs[0].GetLogName());
            Assert.AreEqual(expectedLogName, logList.Logs[1].GetLogName());
        }

        [TestMethod]
        public void TestLogConstructorExceptions_NoExeption()
        {
            var target = new LoggedMockA(false);
            Assert.AreEqual(0, logList.Logs.Count);
        }

        [TestMethod]
        public void TestLogConstructorExceptions_Exception()
        {
            bool didCatchException = false;

            try
            {
                var target = new LoggedMockA(true);
            }
            catch (ApplicationException)
            {
                didCatchException = true;
            }

            Assert.AreEqual(1, logList.Logs.Count, "No log written");
            Assert.AreEqual(LogLevel.Error, logList.Logs[0].GetLevel(), "Log level is not Error");
            Assert.IsTrue(didCatchException, "Exception was not rethrown");
        }

        private void TestLogEntryExit(ILoggedMock target)
        {
            target.Short();

            Assert.AreEqual(2, logList.Logs.Count);
            Assert.AreEqual(LogLevel.Trace, logList.Logs[0].GetLevel());
            Assert.AreEqual(LogLevel.Trace, logList.Logs[1].GetLevel());
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

            Assert.AreEqual(2, logList.Logs.Count);
            Assert.AreEqual(LogLevel.Trace, logList.Logs[0].GetLevel());
            Assert.AreEqual(LogLevel.Error, logList.Logs[1].GetLevel());
        }
    }
}
