using System.Linq;
using NLog;

namespace PubComp.Aspects.Monitoring.UnitTests
{
    public static class TestExtensions
    {
        private const string LevelPrefix = "Level=";
        private const string SourcePrefix = "Source=";
        private const string MessagePrefix = "Message=";
        private const string CallSitePrefix = "CallSite=";

        public static LogLevel GetLevel(this string logMessage)
        {
            return LogLevel.FromString(logMessage?.Split('|')
                .Where(p => p.StartsWith(LevelPrefix))
                .Select(p => p.Substring(LevelPrefix.Length))
                .FirstOrDefault());
        }

        public static string GetLogName(this string logMessage)
        {
            return logMessage?.Split('|')
                .Where(p => p.StartsWith(SourcePrefix))
                .Select(p => p.Substring(SourcePrefix.Length))
                .FirstOrDefault();
        }

        public static string GetMessage(this string logMessage)
        {
            return logMessage?.Split('|')
                .Where(p => p.StartsWith(MessagePrefix))
                .Select(p => p.Substring(MessagePrefix.Length))
                .FirstOrDefault();
        }

        public static string GetCallSite(this string logMessage)
        {
            return logMessage?.Split('|')
                .Where(p => p.StartsWith(CallSitePrefix))
                .Select(p => p.Substring(CallSitePrefix.Length))
                .FirstOrDefault();
        }
    }
}
