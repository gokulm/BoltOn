using Microsoft.Extensions.Logging;

namespace BoltOn.Logger
{
	public sealed class AppLoggerFactory : IAppLoggerFactory
	{
		private readonly ILoggerFactory _loggerFactory;

		public AppLoggerFactory(ILoggerFactory loggerFactory)
		{
			_loggerFactory = loggerFactory;
		}

		public IAppLogger<TType> Create<TType>()
		{
			return new AppLogger<TType>(_loggerFactory.CreateLogger<TType>());
		}
	}
}
