using System.Threading.Tasks;

namespace PubComp.Aspects.Monitoring.UnitTests.LogMocks
{
    public interface ILoggedAsyncMock
    {
        Task ThrowSomethingAsync();

        Task ShortAsync();
    }
}