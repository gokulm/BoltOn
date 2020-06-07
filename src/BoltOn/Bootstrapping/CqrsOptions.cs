using System;

namespace BoltOn.Bootstrapping
{
    public class CqrsOptions
    {
        public virtual bool PurgeEventsToBeProcessed { get; set; } = true;
        public virtual TimeSpan? PurgeEventsProcessedBefore { get; set; }
    }
}
