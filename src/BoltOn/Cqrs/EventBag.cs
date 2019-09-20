using System.Collections.Generic;

namespace BoltOn.Cqrs
{
    public class EventBag
    {
        public List<ICqrsEvent> Events { get; set; } = new List<ICqrsEvent>();
    }
}
