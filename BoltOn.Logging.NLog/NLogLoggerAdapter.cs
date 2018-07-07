using System;
using NLog;

namespace BoltOn.Logging.NLog
{
	public sealed class NLogLoggerAdapter<TType> : IBoltOnLogger<TType>
	{
		private readonly ILogger _logger;

		public NLogLoggerAdapter(ILogger logger)
		{
			_logger = logger;
		}

		public void Debug(string message)
		{
			_logger.Debug(message);
		}

		public void Info(string message)
		{
			_logger.Info(message);
		}

		public void Warn(string message)
		{
			_logger.Warn(message);
		}

		public void Error(string message)
		{
			_logger.Error(message);
		}

		public void Error(Exception exception)
		{
			_logger.Error(exception);
		}
	}
}
