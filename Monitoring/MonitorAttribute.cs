using System;
using System.Linq;
using Common.Logging;
using Newtonsoft.Json;
using PostSharp.Aspects;
using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PubComp.Aspects.Monitoring
{
    [Serializable]
    public class MonitorAttribute : MethodInterceptionAspect
    {
        private string fullMethodName;
        private string className;

        #region Logging

        private readonly LogLevel exceptionLogLevel;
        private ILog log;
        private readonly bool doLogValuesOnException;

        #endregion

        #region Performance Monitoring

        private long entries;
        private long exits;
        private long failures;
        private double totalDuration;
        private double maxDuration;
        private double lastDuration;
        private double weighedAverage;
        private readonly double Factor = 0.25;
        private readonly SpinMonitor spinMonitor = new SpinMonitor();
        private int hasBeenInitialized = 0;
        private static readonly ConcurrentDictionary<string, MonitorAttribute> monitors
            = new ConcurrentDictionary<string, MonitorAttribute>();

        private double UpdateStatistics(double elaspedMilliseconds, bool failure)
        {
            exits++;
            if (failure) failures++;
            var duration = elaspedMilliseconds;
            totalDuration += duration;
            maxDuration = Math.Max(maxDuration, duration);
            lastDuration = duration;
            weighedAverage = exits == 1 ? duration : weighedAverage + (duration - weighedAverage) * Factor;
                
            return duration;
        }

        /// <summary>
        /// The monitor names are the full name of the type, method and parameters
        /// e.g. PubComp.Aspects.Monitoring.GetMonitorNames(string)
        /// </summary>
        /// <param name="monitorName"></param>
        /// <returns></returns>
        public static MonitorStatistics GetStatistics(string monitorName)
        {
            MonitorAttribute instance;
            if (!monitors.TryGetValue(monitorName, out instance))
                return default(MonitorStatistics);

            return instance.GetStatistics();
        }

        /// <summary>
        /// The monitor names are the full name of the type, method and parameters
        /// e.g. PubComp.Aspects.Monitoring.GetMonitorNames()
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetMonitorNames()
        {
            return monitors.Keys.ToList();
        }

        private MonitorStatistics GetStatistics()
        {
            var averageDuration = totalDuration / exits;

            return new MonitorStatistics(
                this.fullMethodName,
                Interlocked.Read(ref this.entries),
                Interlocked.Read(ref this.exits),
                Interlocked.Read(ref this.failures),
                this.totalDuration, averageDuration, this.maxDuration, this.lastDuration, this.weighedAverage);
        }

        #endregion

        /// <summary>
        /// Creates a new MonitorAttribute
        /// </summary>
        /// <param name="logName">Name of logger (from Common.Logging) to use, defaults to full class name of decorated class</param>
        /// <param name="exceptionLogLevel">Log level to use in case of exception, defaults to Error</param>
        /// <param name="doLogValuesOnException">Do log values of parameters passed to method in case of exception, defaults to true</param>
        /// <remarks>
        /// Entries and exists are logged with Trace log level.
        /// Exceptions are rethrown (using throw;)
        /// </remarks>
        public MonitorAttribute(string logName = null, LogLevel exceptionLogLevel = LogLevel.Error, bool doLogValuesOnException = true)
        {
            if (!string.IsNullOrEmpty(logName))
                this.log = LogManager.GetLogger(logName);

            this.exceptionLogLevel = exceptionLogLevel;
            this.doLogValuesOnException = doLogValuesOnException;
        }

        public override void CompileTimeInitialize(System.Reflection.MethodBase method, AspectInfo aspectInfo)
        {
            // ReSharper disable once PossibleNullReferenceException
            className = method.DeclaringType.FullName;

            var parameterTypes = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.FullName).ToArray());

            this.fullMethodName = string.Concat(className, '.', method.Name, '(', parameterTypes, ')');
        }

        public override void OnInvoke(MethodInterceptionArgs args)
        {
            if (Interlocked.CompareExchange(ref hasBeenInitialized, 1, 0) == 0)
                monitors.GetOrAdd(this.fullMethodName, this);

            var stopwatch = new Stopwatch();

            Interlocked.Increment(ref entries);

            if (this.log == null)
                this.log = LogManager.GetLogger(className);

            if (this.log == null)
            {
                try
                {
                    stopwatch.Start();

                    base.OnInvoke(args);

                    var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                    stopwatch.Stop();
                    spinMonitor.InMonitor(() => UpdateStatistics(elapsedMilliseconds, false));

                    return;
                }
                catch (Exception)
                {
                    var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                    stopwatch.Stop();
                    spinMonitor.InMonitor(() => UpdateStatistics(elapsedMilliseconds, true));
                    
                    throw;
                }
            }

            log.Trace(string.Concat("Entering method: ", this.fullMethodName));

            stopwatch.Start();

            try
            {
                base.OnInvoke(args);

                var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                stopwatch.Stop();
                spinMonitor.InMonitor(() => UpdateStatistics(elapsedMilliseconds, false));

                log.Trace(string.Concat("Exiting method: ", this.fullMethodName));
            }
            catch (Exception ex)
            {
                var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                stopwatch.Stop();
                spinMonitor.InMonitor(() => UpdateStatistics(elapsedMilliseconds, true));

                string message = doLogValuesOnException
                    ? string.Concat("Exception in method: ", this.fullMethodName, ", values: ",
                            JsonConvert.SerializeObject(args.Arguments.ToArray()))
                    : string.Concat("Exception in method: ", this.fullMethodName);

                switch (this.exceptionLogLevel)
                {
                    case LogLevel.Fatal:
                        log.Fatal(message, ex);
                        break;
                    case LogLevel.Error:
                        log.Error(message, ex);
                        break;
                    case LogLevel.Warn:
                        log.Warn(message, ex);
                        break;
                    case LogLevel.Info:
                        log.Info(message, ex);
                        break;
                    case LogLevel.Debug:
                        log.Debug(message, ex);
                        break;
                    default:
                        log.Trace(message, ex);
                        break;
                }

                throw;
            }
        }
    }
}
