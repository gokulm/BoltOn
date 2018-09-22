using System;
using System.Transactions;
namespace BoltOn.UoW
{
	public interface IUnitOfWork : IDisposable
	{
		void Begin();

		void Commit();
	}

	public class TestUnitOfWork : IUnitOfWork
	{
		private TransactionScope _transactionScope;
		private bool _isStarted;
		private readonly IsolationLevel _isolationLevel;

		internal TestUnitOfWork(IsolationLevel isolationLevel)
		{
			_isolationLevel = isolationLevel;
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
				Timeout = TransactionManager.DefaultTimeout
			}, TransactionScopeAsyncFlowOption.Enabled);

			_isStarted = true;
		}

		public void Commit()
		{
			_transactionScope.Complete();
		}
	}
}
