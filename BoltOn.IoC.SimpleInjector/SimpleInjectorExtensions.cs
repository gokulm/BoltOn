using System;
using BoltOn.Bootstrapping;

namespace BoltOn.IoC.SimpleInjector
{
    public static class SimpleInjectorExtensions
    {
        public static Bootstrapper BoltOnSimpleInjector(this Bootstrapper bootstrapper,
                                                Action<BoltOnIoCOptions> action = null)
        {
            var options = new BoltOnIoCOptions(bootstrapper);
            if (action == null)
            {
                options.Container = new SimpleInjectorContainerAdapter();
            }
            else
            {
                action(options);
                if (options.Container == null)
                {
                    options.Container = new SimpleInjectorContainerAdapter();
                }
            }
            options.RegisterByConvention();
            return bootstrapper;
        }
    }
}
