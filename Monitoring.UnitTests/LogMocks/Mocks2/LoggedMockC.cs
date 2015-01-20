using System;

namespace PubComp.Aspects.Monitoring.UnitTests.LogMocks.Mocks2
{
    public class LoggedMockC : ILoggedMock
    {
        public void ThrowSomething()
        {
            throw new ApplicationException("Something");
        }

        public void Short()
        {
        }
    }
}
