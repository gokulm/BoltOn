using System;
using System.Transactions;

namespace BoltOn.UoW
{
	public class UnitOfWorkOptions
	{
		public virtual IsolationLevel IsolationLevel { get; set; }
		public TimeSpan TransactionTimeout { get; set; } = TransactionManager.DefaultTimeout;
    }
}
