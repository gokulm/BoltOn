using System;

namespace BoltOn.Bootstrapping
{
    public static class BoltOnServiceLocator
    {
        public static IServiceProvider Current { get; internal set; }
    }
}
