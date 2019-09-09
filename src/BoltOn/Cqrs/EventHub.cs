using System.Collections.Generic;

namespace BoltOn.Cqrs
{
	public interface IEventHub
	{
		List<IEvent> Events { get; set; }

		void Publish(IEvent @event);
	}

	public class EventHub : IEventHub
    {
		public List<IEvent> Events { get; set; } = new List<IEvent>();

		public void Publish(IEvent @event)
        {
            Events.Add(@event);
        }
    }
}
