using System;
using System.Collections.Generic;
using System.Reflection;

namespace BoltOn.Bootstrapping
{
    public sealed class PostRegistrationTaskContext
    {
        private readonly Bootstrapper _bootstrapper;

        internal PostRegistrationTaskContext(Bootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        public IServiceProvider ServiceProvider => _bootstrapper.ServiceProvider;

        public IReadOnlyList<Assembly> Assemblies => _bootstrapper.Assemblies;
    }
}