using System;
using Microsoft.Extensions.Logging;

namespace BoltOn.Logging
{
	public interface IBoltOnLogger<TType>
    {
		void Debug(string message);
		void Info(string message);
		void Error(string message);
		void Error(Exception exception);
		void Warn(string message);
    }

	public class NetStandardLoggerAdapter<TType> : IBoltOnLogger<TType>
	{
		readonly ILogger<TType> _logger;

		public NetStandardLoggerAdapter(ILogger<TType> logger)
		{
			_logger = logger;
		}

		public void Debug(string message)
		{
			_logger.LogDebug(message);
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
