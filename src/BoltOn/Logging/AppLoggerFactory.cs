using System;

namespace BoltOn.Logging
{
	public interface IAppLoggerFactory
	{
		IAppLogger<TType> Create<TType>();
	}

	public sealed class AppLoggerFactory : IAppLoggerFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public AppLoggerFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public IAppLogger<TType> Create<TType>()
		{
			return new AppLogger<TType>(_serviceProvider);
		}
	}
}
