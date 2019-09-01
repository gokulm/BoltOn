using System;

namespace BoltOn.Bootstrapping
{
    public static class BoltOnServiceProvider
    {
        public static IServiceProvider Current { get; internal set; }
    }
}
