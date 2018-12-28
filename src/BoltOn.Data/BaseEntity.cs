namespace BoltOn.Data
{
	public class BaseEntity<TIdType>
	{
		public virtual TIdType Id { get; protected set; }
	}
}
