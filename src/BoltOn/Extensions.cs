using System;
using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn
{
	public static class Extensions
	{
		public static IServiceCollection BoltOn(this IServiceCollection serviceCollection, Action<BoltOnOptions> action = null)
		{
			var options = new BoltOnOptions();
			action?.Invoke(options);
			Bootstrapper.Instance.BoltOn(serviceCollection, options, Assembly.GetCallingAssembly());
			return serviceCollection;
		}

		public static void TightenBolts(this IServiceProvider serviceProvider)
		{
			Bootstrapper.Instance.RunPostRegistrationTasks(serviceProvider);
		}

		public static BoltOnOptions BoltOnCqrsModule(this BoltOnOptions boltOnOptions, Action<BoltOnCqrsOptions> action = null)
		{
			var options = new BoltOnCqrsOptions();
			action?.Invoke(options);
			options.IsEnabled = true;
			boltOnOptions.CqrsOptions = options;

			return boltOnOptions;
		}
	}
}
