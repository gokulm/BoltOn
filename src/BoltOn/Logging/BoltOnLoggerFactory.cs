using System;

namespace BoltOn.Logging
{
	public interface IBoltOnLoggerFactory
    {
        IBoltOnLogger<TType> Create<TType>();
    }

	public sealed class BoltOnLoggerFactory : IBoltOnLoggerFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public BoltOnLoggerFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public IBoltOnLogger<TType> Create<TType>()
		{
			return new BoltOnLogger<TType>(_serviceProvider);
		}
	}
}
