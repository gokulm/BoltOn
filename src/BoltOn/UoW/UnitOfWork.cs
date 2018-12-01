using System;
using System.Transactions;
using BoltOn.Logging;
using BoltOn.Other;

namespace BoltOn.UoW
{
	public interface IUnitOfWork : IDisposable
	{
		void Begin();
		void Commit();
	}

	[ExcludeFromRegistration]
	public sealed class UnitOfWork : IUnitOfWork
	{
		private TransactionScope _transactionScope;
		private bool _isStarted;
		private readonly IBoltOnLogger<UnitOfWork> _logger;
		private readonly IsolationLevel _isolationLevel;
		private readonly TimeSpan _transactionTimeOut;

		internal UnitOfWork(IBoltOnLoggerFactory loggerFactory, IsolationLevel isolationLevel, TimeSpan transactionTimeOut)
		{
			_logger = loggerFactory.Create<UnitOfWork>();
			_isolationLevel = isolationLevel;
			_transactionTimeOut = transactionTimeOut;
		}

		public void Dispose()
		{
			_logger.Debug("Disposing UoW...");
			_transactionScope.Dispose();
			_logger.Debug("Disposed UoW");
		}

		public void Begin()
		{
			if (_isStarted)
			{
				_logger.Debug("UoW already started");
				return;
			}

			_logger.Debug("Starting UoW...");
			_transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
			{
				IsolationLevel = _isolationLevel,
				Timeout = _transactionTimeOut
			}, TransactionScopeAsyncFlowOption.Enabled);

			_isStarted = true;
			_logger.Debug("Started UoW");
		}

		public void Commit()
		{
			_logger.Debug("Committing UoW...");
			_transactionScope.Complete();
			_logger.Debug("Committed UoW");
		}
	}
}
