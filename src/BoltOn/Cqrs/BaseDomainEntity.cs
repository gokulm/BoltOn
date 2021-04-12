using System;
using System.Collections.Generic;
using System.Linq;

namespace BoltOn.Cqrs
{
	public abstract class BaseDomainEntity
	{
		public abstract string DomainEntityId { get; }

		private HashSet<IDomainEvent> _eventsToBeProcessed = new HashSet<IDomainEvent>();

		private HashSet<IDomainEvent> _processedEvents = new HashSet<IDomainEvent>();

		public virtual IEnumerable<IDomainEvent> EventsToBeProcessed
		{
			get => _eventsToBeProcessed;
			internal set => _eventsToBeProcessed = value == null
				? new HashSet<IDomainEvent>()
				: new HashSet<IDomainEvent>(value);
		}

		public virtual IEnumerable<IDomainEvent> ProcessedEvents
		{
			get => _processedEvents;
			internal set => _processedEvents = value == null
				? new HashSet<IDomainEvent>()
				: new HashSet<IDomainEvent>(value);
		}

		protected bool RaiseEvent<TEvent>(TEvent @event)
			where TEvent : IDomainEvent
		{
			if (_eventsToBeProcessed.Any(c => c.Id == @event.Id))
				return false;

			if (@event.Id == Guid.Empty)
				@event.Id = Guid.NewGuid();

			@event.EntityId = DomainEntityId;
			@event.EntityType = GetType().AssemblyQualifiedName;
			if (!@event.CreatedDate.HasValue)
				@event.CreatedDate = DateTime.Now;
			_eventsToBeProcessed.Add(@event);
			return true;
		}

		protected bool ProcessEvent<TEvent>(TEvent @event, Action<TEvent> action)
			where TEvent : IDomainEvent
		{
			if (_processedEvents.Any(c => c.Id == @event.Id))
				return false;

			action(@event);
			if (!@event.ProcessedDate.HasValue)
				@event.ProcessedDate = DateTime.Now;
			_processedEvents.Add(@event);
			return true;
		}

		public void RemoveEventToBeProcessed<TEvent>(TEvent @event)
			where TEvent : IDomainEvent
		{
			_eventsToBeProcessed.Remove(@event);
		}

		public void RemoveProcessedEvent<TEvent>(TEvent @event)
			where TEvent : IDomainEvent
		{
			_processedEvents.Remove(@event);
		}
	}
}
