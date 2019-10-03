using System;
using System.Collections.Generic;
using BoltOn.Data;

namespace BoltOn.Cqrs
{
	public interface ICqrsEntity
	{
		HashSet<EventToBeProcessed> EventsToBeProcessed { get; set; }
		HashSet<ProcessedEvent> ProcessedEvents { get; set; }
	}

	public abstract class BaseCqrsEntity : BaseEntity<string>, ICqrsEntity
	{
		public HashSet<EventToBeProcessed> EventsToBeProcessed { get; set; } = new HashSet<EventToBeProcessed>();

		public HashSet<ProcessedEvent> ProcessedEvents { get; set; } = new HashSet<ProcessedEvent>();

		protected void RaiseEvent(EventToBeProcessed @event)
		{
			if (@event.Id == Guid.Empty)
				@event.Id = Guid.NewGuid();

			@event.SourceTypeName = GetType().AssemblyQualifiedName;
			if (!@event.CreatedDate.HasValue)
				@event.CreatedDate = DateTime.Now;
			EventsToBeProcessed.Add(@event);
		}

		protected void MarkEventAsProcessed(EventToBeProcessed @event)
		{
			var processedEvent = new ProcessedEvent
			{
				Id = @event.Id,
				SourceId = @event.SourceId,
				SourceTypeName = @event.SourceTypeName,
				CreatedDate = @event.CreatedDate
			};

			if (!processedEvent.CreatedDate.HasValue)
				processedEvent.CreatedDate = DateTime.Now;
			ProcessedEvents.Add(processedEvent);
		}
	}
}
