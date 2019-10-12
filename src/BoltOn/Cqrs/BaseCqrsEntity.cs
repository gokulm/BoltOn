using System;
using System.Collections.Generic;
using System.Linq;
using BoltOn.Data;

namespace BoltOn.Cqrs
{
	public interface ICqrsEntity
	{
		HashSet<EventToBeProcessed> EventsToBeProcessed { get; set; }
		HashSet<ProcessedEvent> ProcessedEvents { get; set; }
	}

	public abstract class BaseCqrsEntity : BaseEntity<Guid>, ICqrsEntity
	{
		public HashSet<EventToBeProcessed> EventsToBeProcessed { get; set; } = new HashSet<EventToBeProcessed>();

		public HashSet<ProcessedEvent> ProcessedEvents { get; set; } = new HashSet<ProcessedEvent>();

		protected bool RaiseEvent<TEvent>(TEvent @event) 
			where TEvent : ICqrsEvent
		{
			if (EventsToBeProcessed.FirstOrDefault(c => c.Id == @event.Id) != null)
				return false;

			if (@event.Id == Guid.Empty)
				@event.Id = Guid.NewGuid();

			@event.SourceTypeName = GetType().AssemblyQualifiedName;

			EventsToBeProcessed.Add(@event as EventToBeProcessed);
			return true;
		}

		protected bool ProcessEvent<TEvent>(TEvent @event, Action<TEvent> action) 
			where TEvent : ICqrsEvent
		{
			if (ProcessedEvents.FirstOrDefault(c => c.Id == @event.Id) != null)
				return false;

			action(@event);

			var processedEvent = new ProcessedEvent
			{
				Id = @event.Id,
				SourceId = @event.SourceId,
				SourceTypeName = @event.SourceTypeName,
				CreatedDate = @event.CreatedDate
			};

			ProcessedEvents.Add(processedEvent);
			return true;
		}
	}
}
