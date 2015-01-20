namespace PubComp.Aspects.Monitoring
{
    public struct MonitorStatistics
    {
        public readonly string Method;
        public readonly long Entries;
        public readonly long Exits;
        public readonly long Failures;
        public readonly double TotalDuration;
        public readonly double AverageDuration;
        public readonly double MaxDuration;
        public readonly double LastDuration;
        public readonly double WeighedAverage;

        public MonitorStatistics(string method, long entries, long exits, long failures,
            double totalDuration, double averageDuration, double maxDuration, double lastDuration, double weighedAverage)
        {
            this.Method = method;
            this.Entries = entries;
            this.Exits = exits;
            this.Failures = failures;
            this.TotalDuration = totalDuration;
            this.AverageDuration = averageDuration;
            this.MaxDuration = maxDuration;
            this.LastDuration = lastDuration;
            this.WeighedAverage = weighedAverage;
        }

        public override string ToString()
        {
            return string.Concat(
                "Method: ", Method,
                ", Entries: ", Entries,
                ", Exits: ", Exits,
                ", Failures: ", Failures,
                ", TotalDuration: ", TotalDuration,
                ", AverageDuration: ", AverageDuration,
                ", MaxDuration: ", MaxDuration,
                ", LastDuration: ", LastDuration,
                ", WeighedAverage: ", WeighedAverage);
        }
    }
}
