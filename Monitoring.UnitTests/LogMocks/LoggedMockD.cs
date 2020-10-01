using System;

namespace PubComp.Aspects.Monitoring.UnitTests.LogMocks
{
    public class LoggedMockD : ILoggedMock
    {
        [LogExceptions(typeof(ApplicationException))]
        public LoggedMockD(bool doThrow = true)
        {
            if (doThrow)
                throw new ApplicationException();
        }

        [Log]
        public void ThrowSomething()
        {
            throw new ApplicationException("Something");
        }

        [Log]
        public void Short()
        {
        }

        [Log("MyLog")]
        public void Other()
        {
        }
    }
}
