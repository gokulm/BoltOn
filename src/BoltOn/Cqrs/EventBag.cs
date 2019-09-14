using System.Collections.Generic;

namespace BoltOn.Cqrs
{
    public class EventBag
    {
        public List<IEvent> Events { get; set; } = new List<IEvent>();
    }
}
