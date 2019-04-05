using Microsoft.Extensions.Logging;

namespace BoltOn.Logging
{
    public interface IBoltOnLoggerFactory
    {
        IBoltOnLogger<TType> Create<TType>();
    }

	public sealed class BoltOnLoggerFactory : IBoltOnLoggerFactory
	{
		private readonly ILoggerFactory _loggerFactory;

		public BoltOnLoggerFactory(ILoggerFactory loggerFactory)
		{
			this._loggerFactory = loggerFactory;
		}

		public IBoltOnLogger<TType> Create<TType>()
		{
			return new BoltOnLogger<TType>(_loggerFactory.CreateLogger<TType>());
		}
	}
}
