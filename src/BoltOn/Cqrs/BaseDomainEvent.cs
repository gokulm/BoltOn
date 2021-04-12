using System;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Cqrs
{
	public interface IDomainEvent : IRequest
	{
		Guid Id { get; set; }
		string EntityType { get; set; }
		string EntityId { get; set; }
		DateTimeOffset? CreatedDate { get; set; }
		DateTimeOffset? ProcessedDate { get; set; }
	}

	public abstract class BaseDomainEvent<TEntity> : IDomainEvent
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; }
		public string EntityId { get; set; }
		public DateTimeOffset? CreatedDate { get; set; }
		public DateTimeOffset? ProcessedDate { get; set; }

		public BaseDomainEvent()
		{
			Id = Guid.NewGuid();
			CreatedDate = DateTime.Now;
			EntityType = typeof(TEntity).FullName;
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
