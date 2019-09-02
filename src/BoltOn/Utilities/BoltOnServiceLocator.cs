using System;

namespace BoltOn.Utilities
{
	public static class BoltOnServiceLocator
    {
        public static IServiceProvider Current { get; internal set; }
    }
}
