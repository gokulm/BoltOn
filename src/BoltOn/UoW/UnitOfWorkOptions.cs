using System;
using System.Transactions;

namespace BoltOn.UoW
{
	public class UnitOfWorkOptions
    {
        public virtual IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;
		public TimeSpan TransactionTimeout { get; set; } = TransactionManager.DefaultTimeout;
		internal TransactionScopeOption TransactionScopeOption { get; set; } = TransactionScopeOption.RequiresNew;
	}
}
