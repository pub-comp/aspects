using System;
using System.Threading.Tasks;
using PostSharp.Extensibility;

namespace PubComp.Aspects.Monitoring.UnitTests.LogMocks
{
    [Log(AttributeExclude = true, AttributeTargetMembers = @"regex:get_.*|set_.*")]
    [Log(AttributeExclude = true, AttributeTargetElements = MulticastTargets.InstanceConstructor)]
    [Log(
        AttributeExclude = false,
        AttributeTargetTypes = "PubComp.Aspects.Monitoring.UnitTests.Mocks2.*",
        AttributeTargetElements = MulticastTargets.Method,
        AttributeTargetParameterAttributes =
            MulticastAttributes.Instance | MulticastAttributes.Public)]
    public class LoggedAsyncMockB : ILoggedAsyncMock
    {
        public async Task ThrowSomethingAsync()
        {
            await Task.Delay(10);
            throw new ApplicationException("Something");
        }

        public async Task ShortAsync()
        {
            await Task.Delay(10);
        }
    }
}
