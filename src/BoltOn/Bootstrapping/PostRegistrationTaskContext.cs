using System;

namespace BoltOn.Bootstrapping
{
    public sealed class PostRegistrationTaskContext
    {
        internal PostRegistrationTaskContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }
    }
}