using System;
using System.Collections.Generic;
using System.Linq;

namespace BoltOn.Cqrs
{
	public abstract class BaseDomainEntity
	{
		public abstract string DomainEntityId { get; }

		public virtual bool PurgeEvents { get; set; } = true;

		private HashSet<IDomainEvent> _eventsToBeProcessed = new HashSet<IDomainEvent>();

		public virtual IEnumerable<IDomainEvent> EventsToBeProcessed
		{
			get => _eventsToBeProcessed;
			internal set => _eventsToBeProcessed = value == null
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

			_eventsToBeProcessed.Add(@event);
			return true;
		}

		public void RemoveEventToBeProcessed<TEvent>(TEvent @event)
		   where TEvent : IDomainEvent
		{
			_eventsToBeProcessed.Remove(@event);
		}
	}
}
