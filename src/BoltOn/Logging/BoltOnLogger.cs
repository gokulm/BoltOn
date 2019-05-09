using System;
using Microsoft.Extensions.Logging;

namespace BoltOn.Logging
{
	public interface IBoltOnLogger
	{
		void Debug(string message);
		void Info(string message);
		void Error(string message);
		void Error(Exception exception);
		void Warn(string message);
	}

	public interface IBoltOnLogger<TType> : IBoltOnLogger
	{
	}

	public class BoltOnLogger<TType> : IBoltOnLogger<TType>
	{
		readonly ILogger<TType> _logger;

		public BoltOnLogger(IServiceProvider serviceProvider)
		{
			_logger = serviceProvider.GetService(typeof(ILogger<TType>)) as ILogger<TType>;
		}

		public virtual void Debug(string message)
		{
			_logger?.LogDebug(message);
		}

		public virtual void Info(string message)
		{
			_logger?.LogInformation(message);
		}

		public virtual void Warn(string message)
		{
			_logger?.LogWarning(message);
		}

		public virtual void Error(string message)
		{
			_logger?.LogError(message);
		}

		public virtual void Error(Exception exception)
		{
			_logger?.LogError(null, exception);
		}
	}

	public class BoltOnNetStandarLoggerAdaper<TType> : IBoltOnLogger<TType>
	{
		readonly ILogger<TType> _logger;

		public BoltOnNetStandarLoggerAdaper(BoltOnNetStandardLogger<TType> boltOnNetStandardLogger)
		{
			_logger = boltOnNetStandardLogger;
		}

		public virtual void Debug(string message)
		{
			_logger?.LogDebug(message);
		}

		public virtual void Info(string message)
		{
			_logger?.LogInformation(message);
		}

		public virtual void Warn(string message)
		{
			_logger?.LogWarning(message);
		}

		public virtual void Error(string message)
		{
			_logger?.LogError(message);
		}

		public virtual void Error(Exception exception)
		{
			_logger?.LogError(null, exception);
		}
	}

	public class BoltOnNetStandardLogger : ILogger
	{
		private readonly ILogger _logger;

		public BoltOnNetStandardLogger(ILogger logger)
		{
			_logger = logger;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return _logger.BeginScope(state);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return _logger.IsEnabled(logLevel);
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			_logger.Log(logLevel, eventId, state, exception, formatter);
		}
	}

	public class BoltOnNetStandardLogger<TType> : ILogger<TType>
	{
		private readonly ILogger<TType> _logger;

		public BoltOnNetStandardLogger(ILogger<TType> logger)
		{
			_logger = logger;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return _logger.BeginScope(state);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return _logger.IsEnabled(logLevel);
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			_logger.Log(logLevel, eventId, state, exception, formatter);
		}
	}

	public class BoltOnLoggerProvider : ILoggerProvider
	{
		private readonly ILoggerFactory _loggerFactory;

		public BoltOnLoggerProvider(ILoggerFactory loggerFactory)
		{
			this._loggerFactory = loggerFactory;
		}

		public ILogger CreateLogger(string categoryName)
		{
			return _loggerFactory.CreateLogger(categoryName);
		}

		public void Dispose()
		{
		}
	}
}
