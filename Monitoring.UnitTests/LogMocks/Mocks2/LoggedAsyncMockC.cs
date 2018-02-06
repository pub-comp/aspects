using System;
using System.Threading.Tasks;

namespace PubComp.Aspects.Monitoring.UnitTests.LogMocks.Mocks2
{
    public class LoggedAsyncMockC : ILoggedAsyncMock
    {
        public async Task ThrowSomethingAsync()
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