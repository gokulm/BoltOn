using System;
using System.Collections.Generic;
using System.Linq;
using BoltOn.Data;

namespace BoltOn.Cqrs
{
	public interface ICqrsEntity
	{
		HashSet<CqrsEvent> EventsToBeProcessed { get; set; }
		HashSet<CqrsEvent> ProcessedEvents { get; set; }
	}

	public abstract class BaseCqrsEntity : BaseEntity<Guid>, ICqrsEntity
	{
		public HashSet<CqrsEvent> EventsToBeProcessed { get; set; } = new HashSet<CqrsEvent>();

		public HashSet<CqrsEvent> ProcessedEvents { get; set; } = new HashSet<CqrsEvent>();

		protected bool RaiseEvent<TEvent>(TEvent @event) 
			where TEvent : CqrsEvent
		{
			if (EventsToBeProcessed.FirstOrDefault(c => c.Id == @event.Id) != null)
				return false;

			if (@event.Id == Guid.Empty)
				@event.Id = Guid.NewGuid();

			if (@event.SourceId == Guid.Empty)
				@event.SourceId = Id;

			@event.SourceTypeName = GetType().AssemblyQualifiedName;

			EventsToBeProcessed.Add(@event);
			return true;
		}

		protected bool ProcessEvent<TEvent>(TEvent @event, Action<TEvent> action) 
			where TEvent : CqrsEvent
		{
			if (ProcessedEvents.FirstOrDefault(c => c.Id == @event.Id) != null)
				return false;

			action(@event);

			ProcessedEvents.Add(@event);
			return true;
		}
	}
}
