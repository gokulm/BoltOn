using System;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Cqrs
{
	public interface ICqrsEvent : IRequest
	{
		Guid Id { get; set; }
		string SourceTypeName { get; set; }
		string DestinationTypeName { get; set; }
		Guid SourceId { get; set; }
		DateTime? CreatedDate { get; set; }
		DateTime? ProcessedDate { get; set; }
	}

	public class CqrsEvent : ICqrsEvent
    {
        public Guid Id { get; set; }
        public string SourceTypeName { get; set; }
		public string DestinationTypeName { get; set; }
		public Guid SourceId { get; set; }
		public DateTime? CreatedDate { get; set; }
		public DateTime? ProcessedDate { get; set; }

		public override bool Equals(object obj)
		{
			return obj is CqrsEvent value && Id == value.Id;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
}
