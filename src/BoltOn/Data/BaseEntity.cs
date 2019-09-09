using System;

namespace BoltOn.Data
{
	public abstract class BaseEntity<TIdType>
	{
		public virtual TIdType Id { get; set; }
	}
}
