using System;
using System.Transactions;
using BoltOn.Other;

namespace BoltOn.UoW
{
	public interface IUnitOfWork : IDisposable
	{
		void Begin();
		void Commit();
	}

	[ExcludeFromRegistration]
	public class UnitOfWork : IUnitOfWork
	{
		private TransactionScope _transactionScope;
		private bool _isStarted;
		private readonly IsolationLevel _isolationLevel;
		private readonly TimeSpan _transactionTimeOut;

		internal UnitOfWork(IsolationLevel isolationLevel, TimeSpan transactionTimeOut)
		{
			_isolationLevel = isolationLevel;
			_transactionTimeOut = transactionTimeOut;
		}

		public void Dispose()
		{
			_transactionScope.Dispose();
		}

		public void Begin()
		{
			if (_isStarted)
				return;

			_transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
			{
				IsolationLevel = _isolationLevel,
				Timeout = _transactionTimeOut
			}, TransactionScopeAsyncFlowOption.Enabled);

			_isStarted = true;
		}

		public void Commit()
		{
			_transactionScope.Complete();
		}
	}
}
