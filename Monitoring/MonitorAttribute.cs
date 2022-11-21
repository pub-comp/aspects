using System;
using System.Linq;
using Newtonsoft.Json;
using PostSharp.Aspects;
using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using PostSharp.Serialization;

namespace PubComp.Aspects.Monitoring
{
    [PSerializable]
    public class MonitorAttribute : MethodInterceptionAspect
    {
        private string fullMethodName;
        private string className;

        #region Logging

        private string logName;
        [NonSerialized]
        private Logger log;
        
        private  bool doLogValuesOnException;
        private  bool doLogValuesOnEnterExit;
        
        private long initialized = 0L;
        [NonSerialized]
        private Action<string> logEnterExit;
        [NonSerialized]
        private Action<string, Exception> logException;
        private string enterMessage;
        private string exitMessage;
        private   LogLevelValue exceptionLogLevel;
        private   LogLevelValue enterExistLogLevel;
        private static readonly JsonSerializerSettings LogSerializerSettings = new JsonSerializerSettings { ContractResolver = new LoggableContractResolver() };

        #endregion

        #region Performance Monitoring

        private static readonly ConcurrentDictionary<string, MonitorState> Monitors
            = new ConcurrentDictionary<string, MonitorState>();

        /// <summary>
        /// The monitor names are the full name of the type, method and parameters
        /// e.g. PubComp.Aspects.Monitoring.GetMonitorNames(string)
        /// </summary>
        /// <param name="monitorName"></param>
        /// <returns></returns>
        public static MonitorStatistics GetStatistics(string monitorName)
        {
            MonitorState instance;
            if (!Monitors.TryGetValue(monitorName, out instance))
                return default(MonitorStatistics);

            return instance.CreateStatistics(monitorName);
        }

        /// <summary>
        /// The monitor names are the full name of the type, method and parameters
        /// e.g. PubComp.Aspects.Monitoring.GetMonitorNames()
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetMonitorNames()
        {
            return Monitors.Keys.ToList();
        }

        #endregion

        /// <summary>
        /// Creates a new MonitorAttribute
        /// </summary>
        /// <param name="logName">Name of logger (from Common.Logging) to use, defaults to full class name of decorated class</param>
        /// <param name="exceptionLogLevel">Log level to use in case of exception, defaults to Error</param>
        /// <param name="doLogValuesOnException">Do log values of parameters passed to method in case of exception, defaults to true</param>
        /// <param name="enterExistLogLevel">Log level to use for method enter/exit, defaults to Trace</param>
        /// <param name="doLogValuesOnEnterExit">Do log values of parameters passed to method on enter/exit, defaults to false></param>
        /// <remarks>
        /// Entries and exists are logged with Trace log level.
        /// Exceptions are rethrown (using throw;)
        /// </remarks>
        public MonitorAttribute(string logName = null,
            LogLevelValue exceptionLogLevel = LogLevelValue.Error, bool doLogValuesOnException = true,
            LogLevelValue enterExistLogLevel = LogLevelValue.Trace, bool doLogValuesOnEnterExit = false)
        {
            this.logName = logName;
            this.doLogValuesOnException = doLogValuesOnException;
            this.doLogValuesOnEnterExit = doLogValuesOnEnterExit;
            this.exceptionLogLevel = exceptionLogLevel;
            this.enterExistLogLevel = enterExistLogLevel;
        }

        public override void CompileTimeInitialize(System.Reflection.MethodBase method, AspectInfo aspectInfo)
        {
            // ReSharper disable once PossibleNullReferenceException
            this.className = method.DeclaringType.FullName;
            
            if (string.IsNullOrEmpty(this.logName))
                this.logName = this.className;

            var parameterTypes = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.FullName).ToArray());

            this.fullMethodName = string.Concat(className, '.', method.Name, '(', parameterTypes, ')');

            this.enterMessage = string.Concat("Entering method: ", this.fullMethodName);
            this.exitMessage = string.Concat("Exiting method: ", this.fullMethodName);
        }

        private void InitializeLogger()
        {
            this.log = LogManager.GetLogger(this.logName);
            this.logEnterExit = msg => this.log.Log(typeof(MonitorAttribute),
                new LogEventInfo
                {
                    LoggerName = this.logName,
                    Level = enterExistLogLevel.ToNLog(),
                    Message = msg
                });
            this.logException = (msg, ex) => this.log.Log(typeof(MonitorAttribute),
                new LogEventInfo
                {
                    LoggerName = this.logName,
                    Level = exceptionLogLevel.ToNLog(),
                    Message = msg,
                    Exception = ex
                });
        }

        public override void OnInvoke(MethodInterceptionArgs args)
        {
            var state = Monitors.GetOrAdd(this.fullMethodName, x => new MonitorState());

            var stopwatch = new Stopwatch();

            state.IncrementEntries();

            if (Interlocked.Read(ref initialized) == 0L)
            {
                InitializeLogger();
                Interlocked.Exchange(ref initialized, 1L);
            }

            if (this.log == null)
            {
                try
                {
                    stopwatch.Start();

                    base.OnInvoke(args);

                    stopwatch.Stop();
                    var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                    state.UpdateStatistics(elapsedMilliseconds, false);

                    return;
                }
                catch (Exception)
                {
                    stopwatch.Stop();
                    var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                    state.UpdateStatistics(elapsedMilliseconds, true);
                    
                    throw;
                }
            }

            string enter, exit;
            if (this.doLogValuesOnEnterExit)
            {
                var values = JsonConvert.SerializeObject(args.Arguments.ToArray(), LogSerializerSettings);
                enter = string.Concat(this.enterMessage, ", values: ", values);
                exit = string.Concat(this.exitMessage, ", values: ", values);
            }
            else
            {
                enter = this.enterMessage;
                exit = this.exitMessage;
            }

            this.logEnterExit(enter);

            stopwatch.Start();

            try
            {
                base.OnInvoke(args);

                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                state.UpdateStatistics(elapsedMilliseconds, false);

                this.logEnterExit(exit);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                state.UpdateStatistics(elapsedMilliseconds, true);

                string message = doLogValuesOnException
                    ? string.Concat("Exception in method: ", this.fullMethodName, ", values: ",
                            JsonConvert.SerializeObject(args.Arguments.ToArray(), LogSerializerSettings))
                    : string.Concat("Exception in method: ", this.fullMethodName);

                this.logException(message, ex);

                throw;
            }
        }

        public override async Task OnInvokeAsync(MethodInterceptionArgs args)
        {
            var state = Monitors.GetOrAdd(this.fullMethodName, x => new MonitorState());

            var stopwatch = new Stopwatch();

            state.IncrementEntries();

            if (Interlocked.Read(ref initialized) == 0L)
            {
                InitializeLogger();
                Interlocked.Exchange(ref initialized, 1L);
            }

            if (this.log == null)
            {
                try
                {
                    stopwatch.Start();

                    await base.OnInvokeAsync(args);

                    stopwatch.Stop();
                    var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                    state.UpdateStatistics(elapsedMilliseconds, false);

                    return;
                }
                catch (Exception)
                {
                    stopwatch.Stop();
                    var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                    state.UpdateStatistics(elapsedMilliseconds, true);

                    throw;
                }
            }

            string enter, exit;
            if (this.doLogValuesOnEnterExit)
            {
                var values = JsonConvert.SerializeObject(args.Arguments.ToArray(), LogSerializerSettings);
                enter = string.Concat(this.enterMessage, ", values: ", values);
                exit = string.Concat(this.exitMessage, ", values: ", values);
            }
            else
            {
                enter = this.enterMessage;
                exit = this.exitMessage;
            }

            this.logEnterExit(enter);

            stopwatch.Start();

            try
            {
                await base.OnInvokeAsync(args);

                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                state.UpdateStatistics(elapsedMilliseconds, false);

                this.logEnterExit(exit);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
                state.UpdateStatistics(elapsedMilliseconds, true);

                string message = doLogValuesOnException
                    ? string.Concat("Exception in method: ", this.fullMethodName, ", values: ",
                            JsonConvert.SerializeObject(args.Arguments.ToArray(), LogSerializerSettings))
                    : string.Concat("Exception in method: ", this.fullMethodName);

                this.logException(message, ex);

                throw;
            }
        }
        #region Inner Types

        private class MonitorState
        {
            private long entries;
            private long exits;
            private long failures;
            private double totalDuration;
            private double maxDuration;
            private double lastDuration;
            private double weighedAverage;
            private readonly double Factor = 0.25;
            private readonly SpinMonitor spinMonitor = new SpinMonitor();

            public double UpdateStatistics(double elapsedMilliseconds, bool failure)
            {
                return spinMonitor.InMonitor(() => UpdateStatisticsInner(elapsedMilliseconds, failure));
            }

            private double UpdateStatisticsInner(double elapsedMilliseconds, bool failure)
            {
                exits++;
                if (failure) failures++;
                var duration = elapsedMilliseconds;
                totalDuration += duration;
                maxDuration = Math.Max(maxDuration, duration);
                lastDuration = duration;
                weighedAverage = exits == 1 ? duration : weighedAverage + (duration - weighedAverage) * Factor;

                return duration;
            }

            public MonitorStatistics CreateStatistics(string name)
            {
                var averageDuration = totalDuration / exits;

                return new MonitorStatistics(
                    name,
                    Interlocked.Read(ref this.entries),
                    Interlocked.Read(ref this.exits),
                    Interlocked.Read(ref this.failures),
                    this.totalDuration, averageDuration, this.maxDuration, this.lastDuration, this.weighedAverage);
            }

            public void IncrementEntries()
            {
                Interlocked.Increment(ref this.entries);
            }
        }

        #endregion
    }
}
