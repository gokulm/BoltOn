using System;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.IoC
{
	public static class Extensions
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

		public static BoltOnOptions ConfigureIoC(this BoltOnOptions boltOnOptions, Action<BoltOnIoCOptions> action)
		{
			Check.Requires(!Bootstrapper.Instance.IsBolted, "Components are already bolted! IoC cannot be configured now");
			var options = new BoltOnIoCOptions();
			action(options);
			Bootstrapper.Instance.AddOptions(options);
			return boltOnOptions;
		}

		public static IServiceCollection BoltOn(this IServiceCollection serviceCollection, Action<BoltOnOptions> action)
		{
			Check.Requires(!Bootstrapper.Instance.IsBolted, "Components are already bolted! IoC cannot be configured now");
			var options = new BoltOnOptions();
			action(options);
			options.ServiceCollection = serviceCollection;
			Bootstrapper.Instance.BoltOn(Assembly.GetCallingAssembly());
			return serviceCollection;
		}
    }

	public class BoltOnOptions
	{
		internal IServiceCollection ServiceCollection { get; set; }
	}
}
