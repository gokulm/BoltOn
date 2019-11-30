using System.Collections.Generic;

namespace BoltOn.Cqrs
{
    public class EventBag
    {
        public List<ICqrsEvent> EventsToBeProcessed { get; set; } = new List<ICqrsEvent>();
		public List<ICqrsEvent> ProcessedEvents { get; set; } = new List<ICqrsEvent>();
	}
}
