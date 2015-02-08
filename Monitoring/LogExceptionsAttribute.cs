using System;
using System.Linq;
using System.Threading;
using Common.Logging;
using Newtonsoft.Json;
using PostSharp.Aspects;

namespace PubComp.Aspects.Monitoring
{
    [Serializable]
    public class LogExceptionsAttribute : OnMethodBoundaryAspect
    {
        private string className;
        private string fullMethodName;
        private string logName;
        private ILog log;
        private readonly bool doLogValuesOnException;
        private long initialized = 0L;
        [NonSerialized]
        private Action<string, Exception> logException;
        private readonly LogLevel exceptionLogLevel;

        /// <summary>
        /// Creates a new LogExceptionsAttribute
        /// </summary>
        /// <param name="logName">Name of logger (from Common.Logging) to use, defaults to full class name of decorated class</param>
        /// <param name="exceptionLogLevel">Log level to use in case of exception, defaults to Error</param>
        /// <param name="doLogValuesOnException">Do log values of parameters passed to method in case of exception, defaults to true</param>
        /// <remarks>
        /// Exceptions are rethrown (using throw;)
        /// </remarks>
        public LogExceptionsAttribute(string logName = null,
            LogLevel exceptionLogLevel = LogLevel.Error, bool doLogValuesOnException = true)
        {
            this.logName = logName;
            this.doLogValuesOnException = doLogValuesOnException;
            this.exceptionLogLevel = exceptionLogLevel;
        }

        public override void CompileTimeInitialize(System.Reflection.MethodBase method, AspectInfo aspectInfo)
        {
            // ReSharper disable once PossibleNullReferenceException
            this.className = method.DeclaringType.FullName;
            
            if (string.IsNullOrEmpty(this.logName))
                this.logName = this.className;

            var parameterTypes = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.FullName).ToArray());

            this.fullMethodName = string.Concat(className, '.', method.Name, '(', parameterTypes, ')');
        }

        private void InitializeLogger()
        {
            this.log = LogManager.GetLogger(this.logName);

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

        public override void OnException(MethodExecutionArgs args)
        {
            if (Interlocked.Read(ref initialized) == 0L)
            {
                InitializeLogger();
                Interlocked.Exchange(ref initialized, 1L);
            }

            if (this.log != null)
            {
                string message = doLogValuesOnException
                    ? string.Concat("Exception in method: ", this.fullMethodName, ", values: ",
                            JsonConvert.SerializeObject(args.Arguments.ToArray()))
                    : string.Concat("Exception in method: ", this.fullMethodName);

                this.logException(message, args.Exception);
            }
        }
    }
}
