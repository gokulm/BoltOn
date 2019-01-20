namespace BoltOn.Mediator.Data.EF
{
	public interface IMediatorDataContext
	{
		bool IsQueryRequest { get; set; }
	}

	public class MediatorDataContext : IMediatorDataContext
	{
		public bool IsQueryRequest { get; set; }
	}
}
