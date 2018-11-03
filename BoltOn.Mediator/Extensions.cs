using System;
using BoltOn.Bootstrapping;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Mediator
{
	public static class Extensions
    {
        public static Bootstrapper ConfigureMediator(this Bootstrapper bootstrapper, Action<MediatorOptions> action)
		{
			Check.Requires(!bootstrapper.IsBolted, "Components are already bolted! Mediator cannot be configured now");
            var options = new MediatorOptions();
			action(options);
			bootstrapper.AddOptions(options);
			return bootstrapper;
		}

		public static IServiceCollection BoltOnMediator(this IServiceCollection serviceCollection, Action<MediatorOptions> action)
		{
			Check.Requires(!Bootstrapper.Instance.IsBolted, "Components are already bolted! Mediator cannot be configured now");
			var options = new MediatorOptions();
			action(options);
			Bootstrapper.Instance.AddOptions(options);
			return serviceCollection;
		}

		public static BoltOn.IoC.BoltOnOptions ConfigureMediator(this BoltOn.IoC.BoltOnOptions boltOnOptions, Action<MediatorOptions> action)
		{
			Check.Requires(!Bootstrapper.Instance.IsBolted, "Components are already bolted! Mediator cannot be configured now");
			var options = new MediatorOptions();
			action(options);
			Bootstrapper.Instance.AddOptions(options);
			return boltOnOptions;
		}
    }
}
