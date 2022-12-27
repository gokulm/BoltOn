using System;
namespace BoltOn.Samples.Application.Entities
{
	public class EventStore
	{
		public Guid EventId { get; set; }
		public IDomainEvent Data { get; set; }
		public string EntityType { get; set; }
		public string EntityId { get; set; }
		public DateTimeOffset? CreatedDate { get; set; }
		public DateTimeOffset? ProcessedDate { get; set; }
	}
}

