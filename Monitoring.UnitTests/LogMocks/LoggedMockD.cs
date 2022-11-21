using System;
using PubComp.Aspects.Monitoring.UnitTests.Objects;

namespace PubComp.Aspects.Monitoring.UnitTests.LogMocks
{
    public class LoggedMockD : ILoggedMock
    {
        [LogExceptions]
        public LoggedMockD(bool doThrow = false)
        {
            if (doThrow)
                throw new ApplicationException();
        }

        [Log(exceptionLogLevel:LogLevelValue.Fatal)]
        public void ThrowSomething(LoggableObject obj)
        {
            throw new ApplicationException("Something");
        }

        [Log(enterExistLogLevel: LogLevelValue.Fatal)]
        public void Short()
        {
        }

        [Log("MyLog")]
        public void Other()
        {
        }
    }
}
