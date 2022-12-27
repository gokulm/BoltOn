using System;
using BoltOn.Requestor;

namespace BoltOn.Samples.Application.Entities
{
	public interface IDomainEvent : IRequest
	{
		Guid EventId { get; set; }
	}

	public abstract class BaseDomainEvent<TEntity> : IDomainEvent
	{
		public Guid EventId { get; set; }

		public BaseDomainEvent()
		{
			EventId = Guid.NewGuid();
		}

		public override bool Equals(object obj)
		{
			return obj is BaseDomainEvent<TEntity> value && EventId == value.EventId;
		}

		public override int GetHashCode()
		{
			return EventId.GetHashCode();
		}
	}
}

