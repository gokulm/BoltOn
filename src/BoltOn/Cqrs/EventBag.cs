using System.Collections.Generic;

namespace BoltOn.Cqrs
{
    public class EventBag
    {
        public virtual List<ICqrsEvent> EventsToBeProcessed { get; set; } = new List<ICqrsEvent>();
		public virtual List<ICqrsEvent> ProcessedEvents { get; set; } = new List<ICqrsEvent>();
	}
}
