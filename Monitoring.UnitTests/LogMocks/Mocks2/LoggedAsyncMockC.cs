using System;
using System.Threading.Tasks;
using PubComp.Aspects.Monitoring.UnitTests.Objects;

namespace PubComp.Aspects.Monitoring.UnitTests.LogMocks.Mocks2
{
    public class LoggedAsyncMockC : ILoggedAsyncMock
    {
        public async Task ThrowSomethingAsync(LoggableObject obj)
        {
            await Task.Delay(10);
            throw new ApplicationException("Something");
        }

        public async Task ShortAsync()
        {
            await Task.Delay(10);
        }
    }
}