using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PubComp.Aspects.Monitoring
{
    internal class LoggableContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {

            var property = base.CreateProperty(member, memberSerialization);
            var excludeFromLog = member.CustomAttributes.Any(cad => cad.AttributeType == typeof(LogIgnoreAttribute));

            if (excludeFromLog)
            {
                property.ShouldSerialize = obj => false;
            }
            return property;
        }
    }
}
