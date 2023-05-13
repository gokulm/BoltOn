using System;
using Microsoft.Extensions.Logging;

namespace BoltOn.Logging
{
	public interface IAppLogger<TType>
	{
		void Debug(string message, params object[] args);
		void Info(string message, params object[] args);
		void Error(string message, params object[] args);
		void Error(Exception exception, params object[] args);
		void Warn(string message, params object[] args);
	}
	
	public class AppLogger<TType> : IAppLogger<TType>
	{
		readonly ILogger<TType> _logger;

		public AppLogger(ILogger<TType> logger)
		{
			_logger = logger;
		}

		public virtual void Debug(string message, params object[] args)
		{
			_logger?.LogDebug(message, args);
		}

		public virtual void Info(string message, params object[] args)
		{
			_logger?.LogInformation(message, args);
		}

		public virtual void Warn(string message, params object[] args)
		{
			_logger?.LogWarning(message, args);
		}

		public virtual void Error(string message, params object[] args)
		{
			_logger?.LogError(message, args);
		}

		public virtual void Error(Exception exception, params object[] args)
		{
			_logger?.LogError(exception, exception.Message, args);
		}
	}
}
