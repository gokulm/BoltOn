using System;
namespace BoltOn.Mediator.Data.EF
{
	public interface IMediatorDataContext
	{
		bool IsAutoDetectChangesEnabled { get; set; }
	}

	public class MediatorDataContext : IMediatorDataContext
	{
		public bool IsAutoDetectChangesEnabled { get; set; }
	}
}
