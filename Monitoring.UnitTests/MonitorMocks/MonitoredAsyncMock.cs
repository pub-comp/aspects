using System;
using System.Threading.Tasks;

namespace PubComp.Aspects.Monitoring.UnitTests.MonitorMocks
{
    public class MonitoredAsyncMock
    {
        [Monitor]
        public async Task ThrowSomethingAsync()
        {
            await Task.Delay(1);
            throw new ApplicationException("Something");
        }

        [Monitor]
        public async Task LongAsync()
        {
            await Task.Delay(100);
        }

        [Monitor]
        public async Task ShortAsync()
        {
            await Task.Delay(1);
        }
    }
}