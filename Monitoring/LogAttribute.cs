using System;
using System.Linq;
using System.Threading;
using Common.Logging;
using Newtonsoft.Json;
using PostSharp.Aspects;

namespace PubComp.Aspects.Monitoring
{
    [Serializable]
    public class LogAttribute : MethodInterceptionAspect
    {
        private string className;
        private string fullMethodName;
        private string logName;
        private ILog log;
        private readonly bool doLogValuesOnException;
        private readonly bool doLogValuesOnEnterExit;
        private int initialized = 0;
        [NonSerialized]
        private Action<string> logEnterExit;
        [NonSerialized]
        private Action<string, Exception> logException;
        private string enterMessage;
        private string exitMessage;
        private readonly LogLevel exceptionLogLevel;
        private readonly LogLevel enterExistLogLevel;

        /// <summary>
        /// Creates a new LogAttribute
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
        public LogAttribute(string logName = null,
            LogLevel exceptionLogLevel = LogLevel.Error, bool doLogValuesOnException = true,
            LogLevel enterExistLogLevel = LogLevel.Trace, bool doLogValuesOnEnterExit = false)
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
            switch (enterExistLogLevel)
            {
                case LogLevel.Fatal:
                    logEnterExit = msg => this.log.Fatal(msg);
                    break;
                case LogLevel.Error:
                    logEnterExit = msg => this.log.Error(msg);
                    break;
                case LogLevel.Warn:
                    logEnterExit = msg => this.log.Warn(msg);
                    break;
                case LogLevel.Info:
                    logEnterExit = msg => this.log.Info(msg);
                    break;
                case LogLevel.Debug:
                    logEnterExit = msg => this.log.Debug(msg);
                    break;
                default:
                    logEnterExit = msg => this.log.Trace(msg);
                    break;
            }

            switch (exceptionLogLevel)
            {
                case LogLevel.Fatal:
                    logException = (msg, ex) => this.log.Fatal(msg, ex);
                    break;
                case LogLevel.Error:
                    logException = (msg, ex) => this.log.Error(msg, ex);
                    break;
                case LogLevel.Warn:
                    logException = (msg, ex) => this.log.Warn(msg, ex);
                    break;
                case LogLevel.Info:
                    logException = (msg, ex) => this.log.Info(msg, ex);
                    break;
                case LogLevel.Debug:
                    logException = (msg, ex) => this.log.Debug(msg, ex);
                    break;
                default:
                    logException = (msg, ex) => this.log.Trace(msg, ex);
                    break;
            }
        }

        public override void OnInvoke(MethodInterceptionArgs args)
        {
            if (Interlocked.CompareExchange(ref initialized, 1, 0) == 0)
            {
                InitializeLogger();
            }

            if (this.log == null)
            {
                base.OnInvoke(args);
                return;
            }

            string enter, exit;
            if (this.doLogValuesOnEnterExit)
            {
                var values = JsonConvert.SerializeObject(args.Arguments.ToArray());
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

                this.logEnterExit(exit);
            }
            catch (Exception ex)
            {
                string message = doLogValuesOnException
                    ? string.Concat("Exception in method: ", this.fullMethodName, ", values: ",
                            JsonConvert.SerializeObject(args.Arguments.ToArray()))
                    : string.Concat("Exception in method: ", this.fullMethodName);

                this.logException(message, ex);

                throw;
            }
        }
    }
}
