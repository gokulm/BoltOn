using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoltOn.Cqrs
{
	public class EventBag
	{
		internal Dictionary<ICqrsEvent, Func<ICqrsEvent, Task>> EventsToBeProcessed
			= new Dictionary<ICqrsEvent, Func<ICqrsEvent, Task>>();

		public virtual void AddEventToBeProcessed(ICqrsEvent cqrsEvent,
			Func<ICqrsEvent, Task> removeEventToBeProcessedHandle)
		{
			if (!EventsToBeProcessed.ContainsKey(cqrsEvent))
				EventsToBeProcessed.Add(cqrsEvent, removeEventToBeProcessedHandle);
		}
	}
}
