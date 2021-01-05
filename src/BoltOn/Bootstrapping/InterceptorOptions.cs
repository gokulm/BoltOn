namespace BoltOn.Bootstrapping
{
    public class InterceptorOptions
    {
        internal BootstrapperOptions BootstrapperOptions { get; }

        public InterceptorOptions(BootstrapperOptions bootstrapperOptions)
        {
            BootstrapperOptions = bootstrapperOptions;
        }
    }
}