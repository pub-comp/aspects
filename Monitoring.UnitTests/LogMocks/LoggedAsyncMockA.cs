using System;
using System.Threading.Tasks;
using PubComp.Aspects.Monitoring.UnitTests.Objects;

namespace PubComp.Aspects.Monitoring.UnitTests.LogMocks
{
    public class LoggedAsyncMockA : ILoggedAsyncMock
    {
        [LogExceptions]
        public LoggedAsyncMockA(bool doThrow = false)
        {
            if (doThrow)
                throw new ApplicationException();
        }

        [Log]
        public async Task ThrowSomethingAsync(LoggableObject obj)
        {
            await Task.Delay(10);
            throw new ApplicationException("Something");
        }

        [Log]
        public async Task ShortAsync()
        {
            await Task.Delay(10);
        }

        [Log("MyLog")]
        public async Task OtherAsync()
        {
            await Task.Delay(10);
        }
    }
}