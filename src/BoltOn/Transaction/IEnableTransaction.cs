using System;
using System.Transactions;

namespace BoltOn.Transaction
{
	public interface IEnableTransaction
	{
		public IsolationLevel IsolationLevel { get; }
		public TimeSpan TransactionTimeout => TransactionManager.DefaultTimeout;
		public TransactionScopeOption TransactionScopeOption => TransactionScopeOption.RequiresNew;
	}
}
