using System;
using BoltOn.Logging;

namespace BoltOn.UoW
{
	public interface IUnitOfWorkManager
	{
		IUnitOfWork Get(UnitOfWorkOptions unitOfWorkOptions);
		IUnitOfWork Get(Action<UnitOfWorkOptions> action);
	}

	public sealed class UnitOfWorkManager : IUnitOfWorkManager
	{
		private readonly IBoltOnLogger<UnitOfWorkManager> _logger;
		private readonly IBoltOnLoggerFactory _loggerFactory;
		private bool _isUoWInstantiated;

		public UnitOfWorkManager(IBoltOnLoggerFactory loggerFactory)
		{
			_logger = loggerFactory.Create<UnitOfWorkManager>();
			_loggerFactory = loggerFactory;
		}

		public IUnitOfWork Get(UnitOfWorkOptions unitOfWorkOptions)
		{
			if (_isUoWInstantiated)
				unitOfWorkOptions.TransactionScopeOption = System.Transactions.TransactionScopeOption.RequiresNew;
			_logger.Debug($"Instantiating new UoW. IsolationLevel: {unitOfWorkOptions.IsolationLevel} " +
						  $"TransactionTimeOut: {unitOfWorkOptions.TransactionTimeout}" +
						  $"TransactionScopeOption: {unitOfWorkOptions.TransactionScopeOption}");
			var unitOfWork = new UnitOfWork(_loggerFactory, unitOfWorkOptions);
			_isUoWInstantiated = true;
			return unitOfWork;
		}

		public IUnitOfWork Get(Action<UnitOfWorkOptions> action)
		{
			var uowOptions = new UnitOfWorkOptions();
			action(uowOptions);
			return Get(uowOptions);
		}
	}
}