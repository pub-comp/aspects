using System;

namespace PubComp.Aspects.Monitoring
{
    /// <summary>
    /// Prevents this field or property from being serialized to the logs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class LogIgnoreAttribute : Attribute
    {
    }
}
