using System.Threading.Tasks;
using PubComp.Aspects.Monitoring.UnitTests.Objects;

namespace PubComp.Aspects.Monitoring.UnitTests.LogMocks
{
    public interface ILoggedAsyncMock
    {
        Task ThrowSomethingAsync(LoggableObject obj);

        Task ShortAsync();
    }
}