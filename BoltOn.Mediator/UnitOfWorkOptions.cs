using System.Transactions;

namespace BoltOn.Mediator
{
	public class UnitOfWorkOptions
	{
		public IsolationLevel DefaultIsolationLevel { get; set; }
		public IsolationLevel DefaultCommandIsolationLevel { get; set; }
		public IsolationLevel DefaultQueryIsolationLevel { get; set; }

		public UnitOfWorkOptions()
		{
			DefaultIsolationLevel = IsolationLevel.ReadCommitted;
			DefaultCommandIsolationLevel = IsolationLevel.ReadCommitted;
			DefaultQueryIsolationLevel = IsolationLevel.ReadUncommitted;
		}
    }

	public class MediatorContext
	{
		public IsolationLevel DefaultIsolationLevel { get; set; }
		public IsolationLevel DefaultCommandIsolationLevel { get; set; }
		public IsolationLevel DefaultQueryIsolationLevel { get; set; }
	}
}
