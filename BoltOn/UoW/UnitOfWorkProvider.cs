using System;
using System.Transactions;
using BoltOn.Logging;

namespace BoltOn.UoW
{
	public interface IUnitOfWorkProvider
	{
		IUnitOfWork Get(IsolationLevel isolationLevel, TimeSpan? transactionTimeOut = null);
	}

	public class UnitOfWorkProvider : IUnitOfWorkProvider
	{
		private readonly IBoltOnLogger<UnitOfWorkProvider> _logger;
		private readonly IBoltOnLoggerFactory _loggerFactory;
		private IUnitOfWork _unitOfWork;

		public UnitOfWorkProvider(IBoltOnLogger<UnitOfWorkProvider> boltOnLogger, IBoltOnLoggerFactory loggerFactory)
		{
			_logger = boltOnLogger;
			_loggerFactory = loggerFactory;
		}

		public IUnitOfWork Get(IsolationLevel isolationLevel, TimeSpan? transactionTimeOut = null)
		{
			if (_unitOfWork == null)
			{
				var timeOut = transactionTimeOut ?? TransactionManager.DefaultTimeout;
				_logger.Debug($"Instantiating new UoW. IsolationLevel: {isolationLevel} " +
				              $"TransactionTimeOut: {timeOut}");
				_unitOfWork = new UnitOfWork(_loggerFactory, isolationLevel, timeOut);
				return _unitOfWork;
			}
			_logger.Debug("Returning existing UoW");
			return _unitOfWork;
		}
	}
}
