using System;
using System.Threading;

namespace PubComp.Aspects.Monitoring.UnitTests.LogMocks
{
    public class LoggedMockA : ILoggedMock
    {
        [Log]
        public void ThrowSomething()
        {
            throw new ApplicationException("Something");
        }

        [Log]
        public void Short()
        {
        }
    }
}
