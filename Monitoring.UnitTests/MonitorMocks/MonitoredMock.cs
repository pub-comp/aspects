using System;
using System.Threading;

namespace PubComp.Aspects.Monitoring.UnitTests.MonitorMocks
{
    public class MonitoredMock
    {
        [Monitor]
        public void ThrowSomething()
        {
            throw new ApplicationException("Something");
        }

        [Monitor]
        public void Long()
        {
            Thread.Sleep(100);
        }

        [Monitor]
        public void Short()
        {
        }
    }
}
