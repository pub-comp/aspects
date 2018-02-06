using System;
using System.Threading.Tasks;
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

        [TestMethod]
        public async Task TestLogEntryExit_Async_AspectOnMethod()
        {
            var target = new LoggedAsyncMockA();
            await TestLogEntryExitAsync(target);
        }

        [TestMethod]
        public async Task TestLogEntryException_Async_AspectOnMethod()
        {
            var target = new LoggedAsyncMockA();
            await TestLogEntryExceptionAsync(target);
        }

        [TestMethod]
        public async Task TestLogAutoName_Async()
        {
            const string expectedLogName = "PubComp.Aspects.Monitoring.UnitTests.LogMocks.LoggedAsyncMockA";

            var target = new LoggedAsyncMockA();

            try
            {
                await target.ThrowSomethingAsync();
            }
            catch (ApplicationException)
            {
            }

            Assert.AreEqual(2, logList.Logs.Count);
            Assert.AreEqual(expectedLogName, logList.Logs[0].GetLogName());
            Assert.AreEqual(expectedLogName, logList.Logs[1].GetLogName());
        }

        [TestMethod]
        public async Task TestLogExplicitName_Async()
        {
            const string expectedLogName = "MyLog";

            var target = new LoggedAsyncMockA();

            await target.OtherAsync();

            Assert.AreEqual(2, logList.Logs.Count);
            Assert.AreEqual(expectedLogName, logList.Logs[0].GetLogName());
            Assert.AreEqual(expectedLogName, logList.Logs[1].GetLogName());
        }

        [TestMethod]
        public async Task TestLogEntryExit_Async_AspectOnClass()
        {
            var target = new LoggedAsyncMockB();
            await TestLogEntryExitAsync(target);
        }

        [TestMethod]
        public async Task TestLogEntryException_Async_AspectOnClass()
        {
            var target = new LoggedAsyncMockB();
            await TestLogEntryExceptionAsync(target);
        }

        [TestMethod]
        public async Task TestLogEntryExit_Async_AspectOnAssembly()
        {
            var target = new LoggedAsyncMockC();
            await TestLogEntryExitAsync(target);
        }

        [TestMethod]
        public async Task TestLogEntryException_Async_AspectOnAssembly()
        {
            var target = new LoggedAsyncMockC();
            await TestLogEntryExceptionAsync(target);
        }

        private async Task TestLogEntryExitAsync(ILoggedAsyncMock target)
        {
            await target.ShortAsync();

            Assert.AreEqual(2, logList.Logs.Count);
            Assert.AreEqual(LogLevel.Trace, logList.Logs[0].GetLevel());
            Assert.AreEqual(LogLevel.Trace, logList.Logs[1].GetLevel());
        }

        private async Task TestLogEntryExceptionAsync(ILoggedAsyncMock target)
        {
            bool caughtException = false;

            try
            {
                await target.ThrowSomethingAsync();
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
