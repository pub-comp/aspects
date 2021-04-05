namespace PubComp.Aspects.Monitoring.UnitTests.Objects
{
    public class LoggableObject
    {
        [LogIgnore]
        public string ShouldBeIgnored { get; set; }

        public string ShouldBeLogged { get; set; }
    }
}
