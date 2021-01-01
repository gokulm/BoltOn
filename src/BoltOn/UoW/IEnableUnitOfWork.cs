using System;
using System.Transactions;

namespace BoltOn.UoW
{
	public interface IEnableUnitOfWork 
	{
		public IsolationLevel IsolationLevel { get; }
		public TimeSpan TransactionTimeout => TransactionManager.DefaultTimeout;
		public TransactionScopeOption TransactionScopeOption => TransactionScopeOption.RequiresNew;
	}
}
