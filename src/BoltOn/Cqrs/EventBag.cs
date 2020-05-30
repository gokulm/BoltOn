using System.Collections.Generic;

namespace BoltOn.Cqrs
{
    public class EventBag
    {
        public virtual HashSet<ICqrsEvent> EventsToBeProcessed { get; set; } = new HashSet<ICqrsEvent>();
		public virtual HashSet<ICqrsEvent> ProcessedEvents { get; set; } = new HashSet<ICqrsEvent>();
	}
}
