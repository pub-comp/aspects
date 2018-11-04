using PostSharp.Extensibility;

// Includes assembly self created aspects, since cant be set in the new csProj,
// because the supports only string values
using PubComp.Aspects.Monitoring;

[assembly: Log(AttributeExclude = true, AttributeTargetMembers = @"regex:get_.*|set_.*")]

[assembly: Log(AttributeExclude = true, AttributeTargetElements = MulticastTargets.InstanceConstructor)]

[assembly: Log(
    AttributeExclude = false,
    AttributeTargetTypes = "PubComp.Aspects.Monitoring.UnitTests.LogMocks.Mocks2.*",
    AttributeTargetElements = MulticastTargets.Method,
    AttributeTargetParameterAttributes =
        MulticastAttributes.Instance | MulticastAttributes.Public
    )]

