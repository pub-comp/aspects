using PubComp.Aspects.Monitoring.UnitTests.Objects;

namespace PubComp.Aspects.Monitoring.UnitTests.LogMocks
{
    public interface ILoggedMock
    {
        void ThrowSomething(LoggableObject obj);

        void Short();
    }
}
