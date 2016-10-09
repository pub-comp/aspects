using System;

// ReSharper disable UnusedMember.Global
namespace PubComp.Aspects.Monitoring
{
    [Serializable]
    public enum LogLevelValue
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5,
    }
}
