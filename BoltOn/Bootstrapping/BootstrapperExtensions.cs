using System;
using BoltOn.Bootstrapping;

namespace BoltOn.IoC
{
	public static class BootstrapperExtensions
    {
        public static Bootstrapper ConfigureIoC(this Bootstrapper bootstrapper,
                                                Action<BoltOnIoCOptions> action)
        {
            var options = new BoltOnIoCOptions();
            action(options);
            bootstrapper.AddOptions(options);
            return bootstrapper;
        }
    }
}
