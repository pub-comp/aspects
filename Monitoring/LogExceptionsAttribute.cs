using System;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using NLog;
using PostSharp.Aspects;

namespace PubComp.Aspects.Monitoring
{
    [Serializable]
    public class LogExceptionsAttribute : OnMethodBoundaryAspect
    {
        private string className;
        private string fullMethodName;
        private string logName;
        [NonSerialized]
        private Logger log;
        private readonly bool doLogValuesOnException;
        private long initialized = 0L;
        [NonSerialized]
        private Action<string, Exception> logException;
        private readonly LogLevelValue exceptionLogLevel;
        private static readonly JsonSerializerSettings LogSerializerSettings = new JsonSerializerSettings { ContractResolver = new LoggableContractResolver() };

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
            LogLevelValue exceptionLogLevel = LogLevelValue.Error, bool doLogValuesOnException = true)
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
            this.logException = (msg, ex) => this.log.Log(typeof(LogExceptionsAttribute),
                new LogEventInfo
                {
                    LoggerName = this.logName,
                    Level = exceptionLogLevel.ToNLog(),
                    Message = msg,
                    Exception = ex
                });
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
                            JsonConvert.SerializeObject(args.Arguments.ToArray(), LogSerializerSettings))
                    : string.Concat("Exception in method: ", this.fullMethodName);

                this.logException(message, args.Exception);
            }
        }
    }
}
