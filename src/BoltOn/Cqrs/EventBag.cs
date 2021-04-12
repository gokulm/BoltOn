using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoltOn.Cqrs
{
	// todo: this class can be removed
	[Obsolete]
	public class EventBag
	{
		internal virtual Dictionary<IDomainEvent, Func<IDomainEvent, Task>> EventsToBeProcessed { get; set; }
			= new Dictionary<IDomainEvent, Func<IDomainEvent, Task>>();

		public virtual void AddEventToBeProcessed(IDomainEvent cqrsEvent,
			Func<IDomainEvent, Task> removeEventToBeProcessedHandle)
		{
			if (!EventsToBeProcessed.ContainsKey(cqrsEvent))
				EventsToBeProcessed.Add(cqrsEvent, removeEventToBeProcessedHandle);
		}

		public virtual void RemoveEventToBeProcessed(IDomainEvent cqrsEvent)
		{
			if (EventsToBeProcessed.ContainsKey(cqrsEvent))
				EventsToBeProcessed.Remove(cqrsEvent);
		}
	}
}
