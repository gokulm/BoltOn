namespace BoltOn.Bootstrapping
{
    public sealed class BoltOnCqrsOptions
    {
        internal bool IsEnabled { get; set; }

        public bool ClearEventsEnabled { get; set; }
    }
}
