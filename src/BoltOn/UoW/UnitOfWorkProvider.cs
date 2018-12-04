using System;
using BoltOn.Logging;

namespace BoltOn.UoW
{
	public interface IUnitOfWorkProvider
	{
		IUnitOfWork Start(UnitOfWorkOptions unitOfWorkOptions);
		IUnitOfWork Start(Action<UnitOfWorkOptions> action);
	}

	public sealed class UnitOfWorkProvider : IUnitOfWorkProvider
	{
		private readonly IBoltOnLogger<UnitOfWorkProvider> _logger;
		private readonly IBoltOnLoggerFactory _loggerFactory;
		private bool _isUoWStarted;

		public UnitOfWorkProvider(IBoltOnLoggerFactory loggerFactory)
		{
			_logger = loggerFactory.Create<UnitOfWorkProvider>();
			_loggerFactory = loggerFactory;
		}

		public IUnitOfWork Start(UnitOfWorkOptions unitOfWorkOptions)
		{
			if (_isUoWStarted)
				unitOfWorkOptions.TransactionScopeOption = System.Transactions.TransactionScopeOption.RequiresNew;
			_logger.Debug($"Instantiating new UoW. IsolationLevel: {unitOfWorkOptions.IsolationLevel} " +
						  $"TransactionTimeOut: {unitOfWorkOptions.TransactionTimeout}" +
						  $"TransactionScopeOption: {unitOfWorkOptions.TransactionScopeOption}");
			var unitOfWork = new UnitOfWork(_loggerFactory, unitOfWorkOptions);
			_isUoWStarted = true;
			return unitOfWork;
		}

		public IUnitOfWork Start(Action<UnitOfWorkOptions> action)
		{
			var uowOptions = new UnitOfWorkOptions();
			action(uowOptions);
			return Start(uowOptions);
		}
	}
}