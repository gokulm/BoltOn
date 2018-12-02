using BoltOn.Logging;

namespace BoltOn.UoW
{
	public interface IUnitOfWorkProvider
	{
		IUnitOfWork Get(UnitOfWorkOptions unitOfWorkOptions);
	}

	public sealed class UnitOfWorkProvider : IUnitOfWorkProvider
	{
		private readonly IBoltOnLogger<UnitOfWorkProvider> _logger;
		private readonly IBoltOnLoggerFactory _loggerFactory;
		private IUnitOfWork _unitOfWork;

		public UnitOfWorkProvider(IBoltOnLoggerFactory loggerFactory)
		{
			_logger = loggerFactory.Create<UnitOfWorkProvider>();
			_loggerFactory = loggerFactory;
		}

		public IUnitOfWork Get(UnitOfWorkOptions unitOfWorkOptions)
		{
			_logger.Debug($"Instantiating new UoW. IsolationLevel: {unitOfWorkOptions.IsolationLevel} " +
						  $"TransactionTimeOut: {unitOfWorkOptions.TransactionTimeout}");
			_unitOfWork = new UnitOfWork(_loggerFactory, unitOfWorkOptions.IsolationLevel, unitOfWorkOptions.TransactionTimeout);
			return _unitOfWork;
		}
	}
}