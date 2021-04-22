using System;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Cqrs
{
	public interface IDomainEvent : IRequest
	{
		Guid Id { get; set; }
	}

	public abstract class BaseDomainEvent<TEntity> : IDomainEvent
    {
        public Guid Id { get; set; }

		public BaseDomainEvent()
		{
			Id = Guid.NewGuid();
		}

		public override bool Equals(object obj)
		{
			return obj is BaseDomainEvent<TEntity> value && Id == value.Id;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
}
