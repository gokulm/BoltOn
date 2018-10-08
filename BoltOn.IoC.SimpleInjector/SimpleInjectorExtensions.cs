using System;
using BoltOn.Bootstrapping;

namespace BoltOn.IoC.SimpleInjector
{
    public static class SimpleInjectorExtensions
    {
        public static Bootstrapper BoltOnSimpleInjector(this Bootstrapper bootstrapper,
                                                Action<BoltOnIoCOptions> action = null)
        {
            var boltOnIoCOptions = new BoltOnIoCOptions(bootstrapper);
            if (action == null)
            {
                boltOnIoCOptions.Container = new SimpleInjectorContainerAdapter();
            }
            else
            {
                action(boltOnIoCOptions);
                if (boltOnIoCOptions.Container == null)
                {
                    boltOnIoCOptions.Container = new SimpleInjectorContainerAdapter();
                }
            }
            boltOnIoCOptions.RegisterByConvention();
            return bootstrapper;
        }
    }
}
