using System;
using BoltOn.Bootstrapping;
using BoltOn.Utilities;

namespace BoltOn.Mediator
{
	public static class BootstrapperExtensions
    {
        public static Bootstrapper ConfigureMediator(this Bootstrapper bootstrapper, Action<MediatorOptions> action)
		{
			Check.Requires(!bootstrapper.IsBolted, "Components are already bolted! Mediator cannot be configured now");
            var options = new MediatorOptions();
			action(options);
			bootstrapper.AddOptions(options);
			return bootstrapper;
        }
    }
}
