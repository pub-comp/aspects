using System;
using PubComp.Aspects.Monitoring.UnitTests.Objects;

namespace PubComp.Aspects.Monitoring.UnitTests.LogMocks.Mocks2
{
    public class LoggedMockC : ILoggedMock
    {
        public void ThrowSomething(LoggableObject obj)
        {
            throw new ApplicationException("Something");
        }

        public void Short()
        {
        }
    }
}
