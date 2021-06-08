using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using PostSharp.Aspects;

namespace PubComp.Aspects.Monitoring
{
    [Serializable]
    public class LogAttribute : MethodInterceptionAspect
    {
        private string className;
        private string fullMethodName;
        private string logName;
        [NonSerialized]
        private Logger log;
        private readonly bool doLogValuesOnException;
        private readonly bool doLogValuesOnEnterExit;
        private readonly bool doLogResultsOnExit;
        private long initialized = 0L;
        [NonSerialized]
        private Action<string> logEnterExit;
        [NonSerialized]
        private Action<string, Exception> logException;
        private string enterMessage;
        private string exitMessage;
        private readonly LogLevelValue exceptionLogLevel;
        private readonly LogLevelValue enterExistLogLevel;
        private static readonly JsonSerializerSettings LogSerializerSettings = new JsonSerializerSettings { ContractResolver = new LoggableContractResolver() };

        /// <summary>
        /// Creates a new LogAttribute
        /// </summary>
        /// <param name="logName">Name of logger (from Common.Logging) to use, defaults to full class name of decorated class</param>
        /// <param name="exceptionLogLevel">Log level to use in case of exception, defaults to Error</param>
        /// <param name="doLogValuesOnException">Do log values of parameters passed to method in case of exception, defaults to true</param>
        /// <param name="enterExistLogLevel">Log level to use for method enter/exit, defaults to Trace</param>
        /// <param name="doLogValuesOnEnterExit">Do log values of parameters passed to method on enter/exit, defaults to false></param>
        /// <param name="doLogResultsOnExit">Do log results of function's output parameters on exit, defaults to false></param>
        /// <remarks>
        /// Entries and exists are logged with Trace log level.
        /// Exceptions are rethrown (using throw;)
        /// </remarks>
        public LogAttribute(string logName = null,
            LogLevelValue exceptionLogLevel = LogLevelValue.Error, bool doLogValuesOnException = true,
            LogLevelValue enterExistLogLevel = LogLevelValue.Trace, bool doLogValuesOnEnterExit = false,
            bool doLogResultsOnExit = false)
        {
            this.logName = logName;
            this.doLogValuesOnException = doLogValuesOnException;
            this.doLogValuesOnEnterExit = doLogValuesOnEnterExit;
            this.exceptionLogLevel = exceptionLogLevel;
            this.enterExistLogLevel = enterExistLogLevel;
            this.doLogResultsOnExit = doLogResultsOnExit;
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
            this.logEnterExit = msg => this.log.Log(typeof(LogAttribute),
                new LogEventInfo
                {
                    LoggerName = this.logName,
                    Level = enterExistLogLevel.ToNLog(),
                    Message = msg
                });
            this.logException = (msg, ex) => this.log.Log(typeof(LogAttribute),
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
            if (Interlocked.Read(ref initialized) == 0L)
            {
                InitializeLogger();
                Interlocked.Exchange(ref initialized, 1L);
            }

            if (this.log == null)
            {
                base.OnInvoke(args);
                return;
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

            try
            {
                base.OnInvoke(args);
                exit = AddResultsToExitLog(args, exit);
                this.logEnterExit(exit);
            }
            catch (Exception ex)
            {
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
            if (Interlocked.Read(ref initialized) == 0L)
            {
                InitializeLogger();
                Interlocked.Exchange(ref initialized, 1L);
            }

            if (this.log == null)
            {
                await base.OnInvokeAsync(args);
                return;
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

            try
            {
                await base.OnInvokeAsync(args);
                exit = AddResultsToExitLog(args, exit);
                this.logEnterExit(exit);
            }
            catch (Exception ex)
            {
                string message = doLogValuesOnException
                    ? string.Concat("Exception in method: ", this.fullMethodName, ", values: ",
                        JsonConvert.SerializeObject(args.Arguments.ToArray(), LogSerializerSettings))
                    : string.Concat("Exception in method: ", this.fullMethodName);

                this.logException(message, ex);

                throw;
            }
        }

        private string AddResultsToExitLog(MethodInterceptionArgs args, string exitLog)
        {
            if (this.doLogResultsOnExit)
            {
                var values = JsonConvert.SerializeObject(args.ReturnValue, LogSerializerSettings);
                exitLog = string.Concat(exitLog, ", results: ", values);
            }

            return exitLog;
        }
    }
}
