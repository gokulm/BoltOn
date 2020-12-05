using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace BoltOn.Logging.Serilog
{
    public class BoltOnSerilogLogger<TType> : IBoltOnLogger<TType>
    {
        private readonly string _typeName;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public BoltOnSerilogLogger(IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _typeName = typeof(TType).Name;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public void Debug(string message)
        {
            WriteToLog(LogEventLevel.Debug, message);
        }

        public void Error(string message)
        {
            WriteToLog(LogEventLevel.Error, message);
        }

        public void Error(Exception exception)
        {
            WriteToLog(LogEventLevel.Error, string.Empty, exception);
        }

        public void Info(string message)
        {
            WriteToLog(LogEventLevel.Information, message);
        }

        public void Warn(string message)
        {
            WriteToLog(LogEventLevel.Warning, message);
        }

        private void WriteToLog(LogEventLevel logLevel, string message, Exception exception = null)
        {
            var loggerContext = _serviceProvider.GetService<LoggerContext>();
            foreach (var key in loggerContext.Keys)
                LogContext.PushProperty(key, loggerContext.GetByKey<string>(key));

            if (exception != null)
                message = exception.Message;

            Log
                .ForContext("Type", _typeName)
                .ForContext("Module", _configuration.GetValue<string>("Module"))
                .Write(logLevel, exception, message);
        }
    }
}
