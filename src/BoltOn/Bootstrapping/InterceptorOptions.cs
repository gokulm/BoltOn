namespace BoltOn.Bootstrapping
{
    public class InterceptorOptions
    {
        internal BoltOnOptions BoltOnOptions { get; }

        public InterceptorOptions(BoltOnOptions boltOnOptions)
        {
            BoltOnOptions = boltOnOptions;
        }
    }
}