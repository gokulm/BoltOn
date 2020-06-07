using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoltOn.Cqrs
{
	public class EventBag
	{
		internal virtual Dictionary<ICqrsEvent, Func<ICqrsEvent, Task>> EventsToBeProcessed { get; set; }
			= new Dictionary<ICqrsEvent, Func<ICqrsEvent, Task>>();

		public virtual void AddEventToBeProcessed(ICqrsEvent cqrsEvent,
			Func<ICqrsEvent, Task> removeEventToBeProcessedHandle)
		{
			if (!EventsToBeProcessed.ContainsKey(cqrsEvent))
				EventsToBeProcessed.Add(cqrsEvent, removeEventToBeProcessedHandle);
		}

		public virtual void RemoveEventToBeProcessed(ICqrsEvent cqrsEvent)
		{
			if (EventsToBeProcessed.ContainsKey(cqrsEvent))
				EventsToBeProcessed.Remove(cqrsEvent);
		}
	}
}
