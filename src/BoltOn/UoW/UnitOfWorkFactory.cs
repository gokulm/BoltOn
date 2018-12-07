using BoltOn.Logging;

namespace BoltOn.UoW
{
	public interface IUnitOfWorkFactory
	{
		IUnitOfWork Get(UnitOfWorkOptions options);
	}

	public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IBoltOnLoggerFactory _boltOnLoggerFactory;

        public UnitOfWorkFactory(IBoltOnLoggerFactory boltOnLoggerFactory)
        {
            _boltOnLoggerFactory = boltOnLoggerFactory;
        }

        public IUnitOfWork Get(UnitOfWorkOptions options)
        {
            return new UnitOfWork(_boltOnLoggerFactory, options);
        }
    }
}
