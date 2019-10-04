using System;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Cqrs
{
	public interface ICqrsEvent : IRequest
	{
		Guid Id { get; set; }
		string SourceTypeName { get; set; }
        string SourceId { get; set; }
		DateTime? CreatedDate { get; set; }
	}

	public class CqrsEvent : ICqrsEvent
    {
        public Guid Id { get; set; }
        public string SourceTypeName { get; set; }
		public string SourceId { get; set; }
		public DateTime? CreatedDate { get; set; }

		public override bool Equals(object obj)
		{
			return obj is CqrsEvent value && Id == value.Id;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}

	public class EventToBeProcessed : CqrsEvent
	{
	}

	public class ProcessedEvent : CqrsEvent
	{
	}
}
