using System;
using BoltOn.Requestor.Pipeline;

namespace BoltOn.Cqrs
{
	public interface ICqrsEvent : IRequest
	{
		Guid Id { get; set; }
		string EntityType { get; set; }
		string EntityId { get; set; }
		DateTimeOffset? CreatedDate { get; set; }
		DateTimeOffset? ProcessedDate { get; set; }
	}

	public class CqrsEvent : ICqrsEvent
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; }
		public string EntityId { get; set; }
		public DateTimeOffset? CreatedDate { get; set; }
		public DateTimeOffset? ProcessedDate { get; set; }

		public CqrsEvent()
		{
			Id = Guid.NewGuid();
			CreatedDate = DateTime.Now;
		}

		public override bool Equals(object obj)
		{
			return obj is CqrsEvent value && Id == value.Id;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}

	public class EventStore<TData> where TData: ICqrsEvent
	{
		public Guid EventId { get; set; }
		public TData Data { get; set; }
	}
}
