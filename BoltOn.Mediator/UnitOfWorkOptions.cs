using System;
using System.Transactions;

namespace BoltOn.Mediator
{
	public class UnitOfWorkOptions
	{
		public IsolationLevel DefaultIsolationLevel { get; set; }
		public IsolationLevel DefaultCommandIsolationLevel { get; set; }
		public IsolationLevel DefaultQueryIsolationLevel { get; set; }
		public TimeSpan? DefaultTransactionTimeout { get; set; }
    }
}
