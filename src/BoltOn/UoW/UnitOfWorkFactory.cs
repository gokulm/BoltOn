using BoltOn.Logging;

namespace BoltOn.UoW
{
	// this is mainly used to help unit testing UnitOfWorkManager
	internal interface IUnitOfWorkFactory
	{
		IUnitOfWork Create(UnitOfWorkOptions options);
	}

	internal class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IBoltOnLoggerFactory _boltOnLoggerFactory;
		private readonly IBoltOnLogger<UnitOfWorkFactory> _uowFactory;

        public UnitOfWorkFactory(IBoltOnLoggerFactory boltOnLoggerFactory)
        {
            _boltOnLoggerFactory = boltOnLoggerFactory;
			_uowFactory = _boltOnLoggerFactory.Create<UnitOfWorkFactory>();
        }

        public IUnitOfWork Create(UnitOfWorkOptions options)
        {
			_uowFactory.Debug("Creating new UoW");
            return new UnitOfWork(_boltOnLoggerFactory, options);
        }
    }
}
