using System;
using NLog;

namespace PubComp.Aspects.Monitoring
{
    public static class LogLevelExtensions
    {
        public static LogLevel ToNLog(this LogLevelValue value)
        {
            if ((int)value < LogLevel.Trace.Ordinal || (int)value > LogLevel.Fatal.Ordinal)
            {
                throw new ArgumentOutOfRangeException(
                    $"{nameof(value)}",
                    $"{(int)value} is out of range [Trace={LogLevel.Trace.Ordinal}, Fatal={LogLevel.Fatal.Ordinal}]");
            }

            return LogLevel.FromOrdinal((int)value);
        }
    }
}
