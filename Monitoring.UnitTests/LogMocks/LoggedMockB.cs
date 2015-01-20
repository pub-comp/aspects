using System;
using System.Threading;
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
    public class LoggedMockB : ILoggedMock
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
