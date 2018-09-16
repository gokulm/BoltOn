using System;
using Microsoft.Extensions.Logging;

namespace BoltOn.Logging.NetStandard
{
	public class NetStandardLoggerAdapter<TType> : IBoltOnLogger<TType>
	{
		readonly ILogger<TType> _logger;

		public NetStandardLoggerAdapter(ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<TType>();
		}

		public void Debug(string message)
		{
			// just to keep it unit testable instead of using LogDebug, used Log method which is part of the
			// ILogger and other methods are extension methods
			//_logger.LogDebug(message);
			_logger.Log(LogLevel.Debug, 0, message, null,  null);

		}

		public void Info(string message)
		{
			_logger.LogInformation(message);
		}

		public void Warn(string message)
		{
			_logger.LogWarning(message);
		}

		public void Error(string message)
		{
			_logger.LogError(message);
		}

		public void Error(Exception exception)
		{
			_logger.LogError(null, exception);
		}
	}
}
