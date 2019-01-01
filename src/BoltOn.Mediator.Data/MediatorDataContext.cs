namespace BoltOn.Mediator.Data
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
