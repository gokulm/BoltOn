using System;
using BoltOn.Bootstrapping;
using BoltOn.Utilities;

namespace BoltOn.IoC
{
	public static class BootstrapperExtensions
    {
        public static Bootstrapper ConfigureIoC(this Bootstrapper bootstrapper,
                                                Action<BoltOnIoCOptions> action)
		{
			Check.Requires(!bootstrapper.IsBolted, "Components are already bolted! IoC cannot be configured now");
            var options = new BoltOnIoCOptions();
            action(options);
            bootstrapper.AddOptions(options);
            return bootstrapper;
        }
    }
}
