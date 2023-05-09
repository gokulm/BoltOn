using Microsoft.Extensions.Logging;

namespace BoltOn.Logger
{
    public class AppLogger<TType> : IAppLogger<TType>
	{
		readonly ILogger<TType> _logger;

		public AppLogger(ILogger<TType> logger)
		{
			_logger = logger;
		}

		public virtual void Debug(string message, params object?[] args)
		{
			_logger?.LogDebug(message, args);
		}

		public virtual void Info(string message, params object?[] args)
		{
			_logger?.LogInformation(message, args);
		}

		public virtual void Warn(string message, params object?[] args)
		{
			_logger?.LogWarning(message, args);
		}

		public virtual void Error(string message, params object?[] args)
		{
			_logger?.LogError(message, args);
		}

		public virtual void Error(Exception exception, params object?[] args)
		{
			_logger?.LogError(exception, exception.Message, args);
		}
	}
}
