using System;
using BoltOn.Logging;
using BoltOn.Other;

namespace BoltOn.UoW
{
	public interface IUnitOfWorkManager
	{
		IUnitOfWork Get(UnitOfWorkOptions unitOfWorkOptions = null);
		IUnitOfWork Get(Action<UnitOfWorkOptions> action);
	}

	[ExcludeFromRegistration]
	public sealed class UnitOfWorkManager : IUnitOfWorkManager
	{
		private readonly IBoltOnLogger<UnitOfWorkManager> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private bool _isStarted;

		internal UnitOfWorkManager(IBoltOnLogger<UnitOfWorkManager> logger, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_logger = logger;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public IUnitOfWork Get(UnitOfWorkOptions unitOfWorkOptions = null)
		{
			if (_isStarted)
				throw new Exception("Unit of Work already started");

			if (unitOfWorkOptions == null)
				unitOfWorkOptions = new UnitOfWorkOptions();

			_logger.Debug($"About to start UoW. IsolationLevel: {unitOfWorkOptions.IsolationLevel} " +
						  $"TransactionTimeOut: {unitOfWorkOptions.TransactionTimeout}" +
						  $"TransactionScopeOption: {unitOfWorkOptions.TransactionScopeOption}");
			var unitOfWork = _unitOfWorkFactory.Create(unitOfWorkOptions);
			_isStarted = true;

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