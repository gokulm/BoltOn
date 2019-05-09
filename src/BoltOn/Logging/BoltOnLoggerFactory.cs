using System;

namespace BoltOn.Logging
{
	public interface IBoltOnLoggerFactory
    {
        IBoltOnLogger<TType> Create<TType>();
		IBoltOnLogger GetLogger(string categoryName);
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

		public IBoltOnLogger GetLogger(string categoryName)
		{
			throw new NotImplementedException();
		}
	}
}
